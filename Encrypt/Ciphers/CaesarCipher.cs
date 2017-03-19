using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Encrypt.Ciphers
{
    class CaesarCipher : Cipher
    {
        private byte key;

        public int BlockSize { get { return 128; } }

        public CaesarCipher(byte key)
        {
            this.key = key;
        }

        public void EncryptBlock(byte[] buffer, int length)
        {
            for (int i = 0; i < length; ++i)
            {
                buffer[i] = (byte)(buffer[i] + key);
            }
        }

        public void DecryptBlock(byte[] buffer, int length)
        {
            for (int i = 0; i < length; ++i)
            {
                buffer[i] = (byte)(buffer[i] - key);
            }
        }
    }
}
