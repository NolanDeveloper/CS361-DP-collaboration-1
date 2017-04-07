using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Encrypt.Ciphers;

namespace Encrypt
{
    public class Program
    {
        enum Сipher
        {
            Cesar,
            Vigener,
            Permutation
        }

        static void Main(string[] args)
        {
            Stream input = File.OpenRead("input.txt");
            Stream output = File.OpenWrite("output.txt");

            StreamEncrypter encrypter = new StreamEncrypter(new CaesarCipher(2), 3);

            encrypter.Encrypt(input, output);

            input.Close();
            output.Close();

            input = File.OpenRead("output.txt");
            output = File.OpenWrite("output_what.txt");

            encrypter.Decrypt(input, output);

            input.Close();
            output.Close();
        }
    }
}