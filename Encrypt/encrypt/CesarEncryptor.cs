using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Encrypt.encrypt
{
    class CesarEncryptor : PartEncrypter 
    {
        private byte key;
        public void encryptPart(byte[] input, byte[] output)
        {
            for (int i = 0; i < input.Length; ++i)
            {
                output[i] = (byte)(input[i] + key);
            }
        }

        public void decryptPart(byte[] input, byte[] output)
        {
            for (int i = 0; i < input.Length; ++i)
            {
                output[i] = (byte)(input[i] - key); // maybe mistakes
            }
        }

        public CesarEncryptor(byte key)
        {
            this.key = key;
        }
    }
}
