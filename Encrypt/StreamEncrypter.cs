using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;

namespace Encrypt
{
    class StreamEncrypter
    {
        private const int TEN_MB = 10 * 1024 * 1024;
        private const int TOTAL_BUFFER_SIZE_LIMIT = TEN_MB;

        private int threadCount;
        private Cipher cipher;

        private LinkedList<Block>[] blocksToProcess;
        private AutoResetEvent[] newBlocksToProcess;

        private SortedSet<Block> blocksToWrite = new SortedSet<Block>();
        private AutoResetEvent newBlocksToWrite = new AutoResetEvent(false);

        private bool finishedReading = false;
        private bool finishedProcessing = false;

        private AutoResetEvent newFreeBuffers = new AutoResetEvent(false);
        private ConcurrentBag<byte[]> bufferPool = new ConcurrentBag<byte[]>();
        private int totalBufferSize = 0;

        public StreamEncrypter(Cipher cipher, int threadCount)
        {
            this.cipher = cipher;
            this.threadCount = threadCount;
            this.blocksToProcess = new LinkedList<Block>[threadCount];
            this.newBlocksToProcess = new AutoResetEvent[threadCount];
            for (int i = 0; i < threadCount; ++i)
            {
                blocksToProcess[i] = new LinkedList<Block>();
                newBlocksToProcess[i] = new AutoResetEvent(false);
            }
        }

        public void WriterThreadTarget(Object state)
        {
            Stream output = (Stream) state;
            int written = 0;
            while (true)
            {
                if (0 == blocksToWrite.Count && finishedProcessing)
                    break;
                if (0 == blocksToWrite.Count)
                    newBlocksToWrite.WaitOne();
                Block block;
                lock (blocksToWrite)
                {
                    if (0 == blocksToWrite.Count)
                        continue;
                    block = blocksToWrite.Min;
                    if (block.Index != written)
                        continue;
                    blocksToWrite.Remove(block);
                }
                block.WriteTo(output);
                releaseBuffer(block.releaseBuffer());
                ++written;
            }
        }

        class ThreadState
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
                if (0 == blocks.Count && finishedReading)
                    break;
                if (0 == blocks.Count)
                    newBlocks.WaitOne();
                Block block;
                lock (blocks)
                {
                    if (0 == blocks.Count)
                        continue;
                    block = blocks.First.Value;
                    blocks.RemoveFirst();
                }
                block.ProcessWith(cipher, threadState.encrypt);
                lock (blocksToWrite)
                    blocksToWrite.Add(block);
                newBlocksToWrite.Set();
            }
        }

        private byte[] obtainBuffer()
        {
            while (true)
            {
                byte[] buffer;
                lock (bufferPool)
                    if (bufferPool.TryTake(out buffer))
                        return buffer;
                if (totalBufferSize + cipher.BlockSize <= TOTAL_BUFFER_SIZE_LIMIT)
                {
                    buffer = new byte[cipher.BlockSize];
                    totalBufferSize += cipher.BlockSize;
                    return buffer;
                }
                else newFreeBuffers.WaitOne();
            }
        }

        private void releaseBuffer(byte[] buffer)
        {
            lock (bufferPool)
                bufferPool.Add(buffer);
            newFreeBuffers.Set();
        }

        private void ProcessData(Stream input, Stream output, bool encrypt)
        {
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
            DateTime lastReport = DateTime.UtcNow;
            while (true)
            {
                // read next block
                byte[] buffer = obtainBuffer();
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
                var blockToProcess = blocksToProcess[threadIndex];
                lock (blockToProcess)
                    blockToProcess.AddLast(newBlock);
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
