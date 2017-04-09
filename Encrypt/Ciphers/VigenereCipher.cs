using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Encrypt.Ciphers
{
    class VigenereCipher : Cipher
    {
        private readonly byte[] key;

        public int BlockSize { get { return key.Length; } }

        public VigenereCipher(byte[] key)
        {
            this.key = key;
        }

        public void EncryptBlock(byte[] block, int length)
        {
            for (int i = 0; i < length; ++i)
                block[i] += key[i];
        }

        public void DecryptBlock(byte[] block, int length)
        {
            for (int i = 0; i < length; ++i)
                block[i] -= key[i];
        }
    }
}
