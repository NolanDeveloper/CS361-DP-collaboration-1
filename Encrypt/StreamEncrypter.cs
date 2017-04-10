using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Encrypt
{
    public class StreamEncrypter
    {
        private readonly int threadCount;
        private readonly Cipher cipher;

        private readonly ConcurrentQueue<Block>[] blocksToProcess;
        private readonly AutoResetEvent[] newBlocksToProcess;

        private readonly ConcurrentDictionary<int, Block> blocksToWrite = new ConcurrentDictionary<int, Block>();
        private readonly AutoResetEvent newBlocksToWrite = new AutoResetEvent(false);

        private bool finishedReading;
        private bool finishedProcessing;

        private readonly BufferPool bufferPool;

        public StreamEncrypter(Cipher cipher, int threadCount)
        {
            this.cipher = cipher;
            this.threadCount = threadCount;
            this.blocksToProcess = new ConcurrentQueue<Block>[threadCount];
            this.newBlocksToProcess = new AutoResetEvent[threadCount];
            this.bufferPool = new BufferPool(cipher.BlockSize);
            for (int i = 0; i < threadCount; ++i)
            {
                blocksToProcess[i] = new ConcurrentQueue<Block>();
                newBlocksToProcess[i] = new AutoResetEvent(false);
            }
        }

        public void WriterThreadTarget(Object state)
        {
            Stream output = (Stream) state;
            int written = 0;
            while (true)
            {
                Block block = null;
                bool gotValue;
                while (!(gotValue = blocksToWrite.TryRemove(written, out block)) && !finishedProcessing)
                    newBlocksToWrite.WaitOne();
                if (!gotValue) break;
                block.WriteTo(output);
                bufferPool.ReleaseBuffer(block.ReleaseBuffer());
                ++written;
            }
        }

        private class ThreadState
        {
            public int threadNumber;
            public bool encrypt;
            
            public ThreadState(int threadNumber, bool encrypt)
            {
                this.threadNumber = threadNumber;
                this.encrypt = encrypt;
            }
        }

        public void ProcessorThreadTarget(Object state)
        {
            ThreadState threadState = (ThreadState) state;
            var blocks = blocksToProcess[threadState.threadNumber];
            var newBlocks = newBlocksToProcess[threadState.threadNumber];
            while (true)
            {
                Block block = null;
                bool gotValue;
                while (!(gotValue = blocks.TryDequeue(out block)) && !finishedReading)
                    newBlocks.WaitOne();
                if (!gotValue) break;
                block.ProcessWith(cipher, threadState.encrypt);
                blocksToWrite.TryAdd(block.Index, block);
                newBlocksToWrite.Set();
            }
        }

        private void ProcessData(Stream input, Stream output, bool encrypt)
        {
            finishedReading = false;
            finishedProcessing = false;
            // spawn threads
            Thread[] processorThread = new Thread[threadCount];
            for (int i = 0; i < threadCount; ++i)
            {
                processorThread[i] = new Thread(ProcessorThreadTarget);
                processorThread[i].Name = "Processor thread - " + i;
                processorThread[i].Start(new ThreadState(i, encrypt));
            }
            Thread writerThread = new Thread(WriterThreadTarget);
            writerThread.Name = "Writer thread";
            writerThread.Start(output);
            // keep adding blocks to encription queue
            int threadIndex = 0;
            int blockIndex = 0;
            while (true)
            {
                // read next block
                byte[] buffer = bufferPool.ObtainBuffer();
                int offset = 0;
                while (offset != buffer.Length)
                {
                    int read = input.Read(buffer, offset, buffer.Length - offset);
                    if (0 == read)
                        break;
                    offset += read;
                }
                if (0 == offset)
                    break;
                // send this block to encrypter
                Block newBlock = new Block(blockIndex, buffer, offset);
                blocksToProcess[threadIndex].Enqueue(newBlock);
                newBlocksToProcess[threadIndex].Set();
                threadIndex = (threadIndex + 1) % threadCount;
                ++blockIndex;
            }
            // join threads
            // notify encrypters about end of input
            finishedReading = true;
            foreach (var newBlocksEvent in newBlocksToProcess)
                newBlocksEvent.Set();
            foreach (var thread in processorThread)
                thread.Join();
            finishedProcessing = true;
            newBlocksToWrite.Set();
            writerThread.Join();
            bufferPool.Clean();
        }

        public void Encrypt(Stream input, Stream output)
        {
            ProcessData(input, output, true);
        }

        public void Decrypt(Stream input, Stream output)
        {
            ProcessData(input, output, false);
        }
    }
}
