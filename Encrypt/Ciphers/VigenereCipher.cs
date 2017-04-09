using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Encrypt.Ciphers
{
    class VigenereCipher : Cipher
    {
        private string key;

        public int BlockSize
        {
            get { return key.Length; }
        }

        public VigenereCipher(string key)
        {
            this.key = key;
        }

        public void DecryptBlock(byte[] block, int length)
        {
            throw new NotImplementedException();
        }

        public void EncryptBlock(byte[] block, int length)
        {
            throw new NotImplementedException();
        }
    }
}
