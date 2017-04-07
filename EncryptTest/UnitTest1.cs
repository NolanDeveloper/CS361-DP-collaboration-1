using System;
using Encrypt.Ciphers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;

namespace EncryptTest
{
    [TestClass]
    public class CasesarCipherTest
    {
        private String[] testData = { "0", "1", "a", "b", "hello" };
        private String[] plusOneEncrypted = { "1", "2", "b", "c", "ifmmp" };

        [TestMethod]
        public void TestNoEncryption()
        {
            CaesarCipher cipher = new CaesarCipher(0);
            byte[] source = new byte[cipher.BlockSize];
            byte[] encoded = new byte[source.Length];
            byte[] decoded = new byte[source.Length];
            foreach (var data in testData)
            {
                Encoding.ASCII.GetBytes(data, 0, data.Length, source, 0);
                source.CopyTo(encoded, 0);
                cipher.EncryptBlock(encoded, data.Length);
                CollectionAssert.AreEqual(source, encoded);
                encoded.CopyTo(decoded, 0);
                cipher.DecryptBlock(decoded, data.Length);
                CollectionAssert.AreEqual(encoded, decoded);
            }
        }

        [TestMethod]
        public void TestSimple()
        {
            CaesarCipher cipher = new CaesarCipher(1);
            byte[] source = new byte[cipher.BlockSize];
            byte[] encoded = new byte[source.Length];
            byte[] decoded = new byte[source.Length];
            byte[] temp = new byte[source.Length];
            for (int i = 0; i < testData.Length; ++i)
            {
                String sourceData = testData[i];
                String expectedData = plusOneEncrypted[i];
                Encoding.ASCII.GetBytes(sourceData, 0, sourceData.Length, source, 0);
                Encoding.ASCII.GetBytes(expectedData, 0, expectedData.Length, temp, 0);
                source.CopyTo(encoded, 0);
                cipher.EncryptBlock(encoded, sourceData.Length);
                CollectionAssert.AreEqual(temp, encoded);
                encoded.CopyTo(decoded, 0);
                cipher.DecryptBlock(decoded, sourceData.Length);
                CollectionAssert.AreEqual(source, decoded);
            }
        }
    }
}
