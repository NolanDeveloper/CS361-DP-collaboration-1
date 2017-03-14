using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Encrypt
{
    interface PartEncrypter : ICloneable
    {
        int partSize { get; }

        void encryptPart(byte[] part, int size);
    }
}
