using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Encrypt
{
    class Block : IComparable<Block>
    {
        private int index;
        private byte[] buffer;
        private int length;

        public int Index { get { return index; } }

        public Block(int index, byte[] buffer, int length)
        {
            this.index = index;
            this.buffer = buffer;
            this.length = length;
        }

        public void ProcessWith(Cipher cipher, bool encrypt)
        {
            if (encrypt)
            {
                cipher.EncryptBlock(buffer, length);
            }
            else
            {
                cipher.DecryptBlock(buffer, length);
            }
        }

        public void WriteTo(Stream output)
        {
            output.Write(buffer, 0, length);
        }

        public int CompareTo(Block other)
        {
            return index - other.index;
        }

        public byte[] ReleaseBuffer()
        {
            var temp = buffer;
            buffer = null;
            return temp;
        }
    }
}
