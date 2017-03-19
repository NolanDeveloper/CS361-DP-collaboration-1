using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Encrypt
{
    interface Cipher
    {
        /// <summary>
        /// Required block size in bytes. All blocks except last one must be 
        /// this bytes long. The last block can be shorter.
        /// </summary>
        int BlockSize { get; }

        /// <summary>
        /// Encrypts block in place.
        /// </summary>
        /// <param name="block">bytes to encrypt</param>
        /// <param name="length">size of block in bytes</param>
        void EncryptBlock(byte[] block, int length);

        /// <summary>
        /// Decrypts block in place.
        /// </summary>
        /// <param name="block">bytes to decrypt</param>
        /// <param name="length">size of block in bytes</param>
        void DecryptBlock(byte[] block, int length);
    }
}
