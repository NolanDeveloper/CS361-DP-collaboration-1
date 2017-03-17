using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Encrypt
{
    interface PartEncrypter
    {
        void encryptPart(byte[] input, byte[] output);
        void decryptPart(byte[] input, byte[] output);
    }
}
