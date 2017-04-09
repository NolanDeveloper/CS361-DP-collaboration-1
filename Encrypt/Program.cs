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
        public static readonly string[] CIPEHRS = { "caesar", "vigenere" };

        public static void ShowHelp()
        {
            Console.WriteLine("usage:");
            Console.WriteLine(System.AppDomain.CurrentDomain.FriendlyName + " -c [caesar|vigenere] <key> "
                + "[-i <input file>] [-o <output file>] [-e|-d] [-t <number of threads>]");
            Console.WriteLine("where");
            Console.WriteLine("<key> for caesar cipher is integer value between " + byte.MinValue 
                + " and " + byte.MaxValue);
            Console.WriteLine("<key> for vigener is not empty ascii string");
            Console.WriteLine("if no input or ouput file was provided standard stream will be used");
            Console.WriteLine("-e and -d flags specify desired operation: encrypt or decrypt");
            Console.WriteLine("if no -e or -d were provided encryption will be performed.");
        }

        private class SecondOccurenceException : Exception
        {
            public SecondOccurenceException(string parameter)
                : base(parameter + " parameter must be used only once.")
            { }
        }

        public static void Main(string[] args)
        {
            Stream input = null;
            Stream output = null;
            Cipher cipher = null;
            bool? encode = null;
            int? threadCount = null;
            int i = 0;
            if (0 == args.Length)
            {
                ShowHelp();
                return;
            }
            while (i < args.Length)
            {
                if (args[i] == "-c")
                {
                    if (null != cipher)
                        throw new SecondOccurenceException("-c");
                    if (args.Length <= i + 1)
                        throw new Exception("Expected cipher name after -c. Supported: "
                            + String.Join(" ", CIPEHRS));
                    if (args.Length <= i + 2)
                        throw new Exception("Expected key after cipher name.");
                    String cipherName = args[i + 1];
                    if (cipherName == "caesar")
                    {
                        byte keyValue;
                        if (!byte.TryParse(args[i + 2], out keyValue))
                            throw new Exception("Caesar key must be an integer value between "
                                + byte.MinValue + " and " + byte.MaxValue + ".");
                        cipher = new CaesarCipher(keyValue);
                    }
                    else if (cipherName == "vigenere")
                    {
                        if (0 == args[i + 2].Length)
                            throw new Exception("Vigenere key cannot be empty string.");
                        cipher = new VigenereCipher(args[i + 2]);
                    }
                    else
                        throw new Exception("Unsupported cipher: " + cipherName + "."
                            + Environment.NewLine + ". Choose one of: "
                            + String.Join(" ", CIPEHRS));
                    i += 2;
                }
                else if (args[i] == "-i")
                {
                    if (null != cipher)
                        throw new SecondOccurenceException("-i");
                    if (args.Length <= i + 1)
                        throw new Exception("Expected file name after -i.");
                    input = File.OpenRead(args[i + 1]);
                    ++i;
                }
                else if (args[i] == "-o")
                {
                    if (null != cipher)
                        throw new SecondOccurenceException("-o");
                    if (args.Length <= i + 1)
                        throw new Exception("Expected file name after -o.");
                    output = File.OpenWrite(args[i + 1]);
                    ++i;
                }
                else if (args[i] == "-e" || args[i] == "-d")
                {
                    if (null != cipher)
                        throw new Exception("Only one of -e, -d can be used.");
                    encode = args[i] == "-e";
                    ++i;
                }
                else if (args[i] == "-t")
                {
                    if (null != threadCount)
                        throw new SecondOccurenceException("-t");
                    if (args.Length <= i + 1)
                        throw new Exception("Expected number of threads after -t.");
                    int n;
                    if (!int.TryParse(args[i + 1], out n))
                        throw new Exception("Expected number of threads after -t.");
                    threadCount = n;
                }
                else if (args[i] == "-h")
                {
                    ShowHelp();
                    return;
                }
                else
                    throw new Exception("Unknown parameter: " + args[i] + ".");
            }
            if (null == cipher)
                throw new Exception("No cipher provided.");
            if (null == input)
                input = Console.OpenStandardInput();
            if (null == output)
                output = Console.OpenStandardOutput();
            if (null == encode)
                encode = true;
            if (null == threadCount)
                threadCount = 3;
            StreamEncrypter encrypter = new StreamEncrypter(cipher, threadCount.Value);
            if (encode.Value)
                encrypter.Encrypt(input, output);
            else
                encrypter.Decrypt(input, output);
            input.Close();
            output.Close();
        }
    }
}