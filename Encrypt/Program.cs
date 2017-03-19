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
            Stream input = File.Open("input.txt", FileMode.Open, FileAccess.Read);
            Stream output = File.Open("output.txt", FileMode.Create, FileAccess.Write);
            StreamEncrypter encrypter = new StreamEncrypter(new CaesarCipher(42), 3);
            encrypter.Encrypt(input, output);
            input.Close();
            output.Close();
        }
    }
}