using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Encrypt
{
    class StreamEncrypter
    {
        private PartEncrypter[] encrypters;
        private Stream input;
        private Stream output;
        private int partSize;
        private int m;

        public StreamEncrypter(PartEncrypter encrypter, int n, Stream input, Stream output)
        {
            this.encrypters = new PartEncrypter[n];
            this.encrypters[0] = encrypter;
            for (int i = 1; i < n; ++i)
            {
                this.encrypters[i] = (PartEncrypter) encrypter.Clone();
            }
            this.input = input;
            this.output = output;
            this.partSize = encrypter.partSize;
            this.m = (int) input.Length / partSize + 1;
        }

        public void encrypt()
        {
            byte[] part = new byte[partSize];
            for (int i = 0; i < m; ++i)
            {
                int size = input.Read(part, i * partSize, partSize);
                encrypters[i].encryptPart(part, size);
            }
        }
    }
}
