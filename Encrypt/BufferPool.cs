using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Encrypt
{
    public class BufferPool
    {
        private const int TEN_MB = 10 * 1024 * 1024;
        private const int TOTAL_BUFFER_SIZE_LIMIT = TEN_MB;

        private readonly AutoResetEvent newFreeBuffers = new AutoResetEvent(false);
        private readonly int blockSize;
        private ConcurrentBag<byte[]> buffers = new ConcurrentBag<byte[]>();
        private int totalBufferSize = 0;

        public BufferPool(int blockSize) {
            this.blockSize = blockSize;
        }

        public byte[] ObtainBuffer()
        {
            byte[] buffer;
            if (buffers.TryTake(out buffer))
                return buffer;
            if (totalBufferSize + blockSize <= TOTAL_BUFFER_SIZE_LIMIT)
            {
                buffer = new byte[blockSize];
                totalBufferSize += blockSize;
                return buffer;
            }
            while (!buffers.TryTake(out buffer))
                newFreeBuffers.WaitOne();
            return buffer;
        }

        public void ReleaseBuffer(byte[] buffer)
        {
            buffers.Add(buffer);
            newFreeBuffers.Set();
        }

        public void Clean()
        {
            buffers = new ConcurrentBag<byte[]>();
        }
    }
}
