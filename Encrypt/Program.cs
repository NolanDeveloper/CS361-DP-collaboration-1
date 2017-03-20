using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Encrypt.Ciphers;

namespace Encrypt
{
    class Program
    {
        enum Сipher
        {
            Cesar,
            Vigener,
            Permutation
        }

        static void Main(string[] args)
        {
            Stream input = File.OpenRead("output.txt");
            Stream output = File.OpenWrite("output1.txt");
            StreamEncrypter encrypter = new StreamEncrypter(new CaesarCipher(42), 3);
            encrypter.Decrypt(input, output);
            input.Close();
            output.Close();
        }
    }
}