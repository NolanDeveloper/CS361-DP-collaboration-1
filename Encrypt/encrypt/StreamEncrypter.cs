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
        private const int FOURKB = 4 * 1024;
        private PartEncrypter encrypter;
        private Stream input;
        private Stream output;
        private int partSize;
        private int m;

        public StreamEncrypter(PartEncrypter encrypter, int n, Stream input, Stream output)
        {
            byte[] buffer = new byte[FOURKB];
            byte[] outputBuffer = new byte[FOURKB];
            int length = 0;
            int elem;

            while (true)
            {
                elem = input.ReadByte();
                if (elem != -1)
                    break;
                if (length++ == buffer.Length)
                {
                    encrypter.encryptPart(buffer, outputBuffer);
                }

                buffer[length] = (byte)elem;

            }
            output.Write(outputBuffer, 0, outputBuffer.Length);
        }

        public void encrypt()
        {

        }
    }
}
