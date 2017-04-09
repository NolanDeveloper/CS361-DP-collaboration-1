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
        private static readonly string[] CIPEHRS = { "caesar", "vigenere" };

        private static void ShowHelp()
        {
            Console.WriteLine("usage:");
            Console.WriteLine(AppDomain.CurrentDomain.FriendlyName + " -c [caesar|vigenere] <key> "
                + "[-i <input file>] [-o <output file>] [-e|-d] [-t <number of threads>]");
            Console.WriteLine("where");
            Console.WriteLine("<key> for caesar cipher is integer value between " + byte.MinValue 
                + " and " + byte.MaxValue);
            Console.WriteLine("<key> for vigener is not empty ascii string");
            Console.WriteLine("if no input or ouput file was provided standard stream will be used");
            Console.WriteLine("-e and -d flags specify desired operation: encrypt or decrypt");
            Console.WriteLine("if no -e or -d were provided encryption will be performed.");
        }

        private static void SecondOccurenceError(string parameter)
        {
            ParameterError(parameter + " parameter must be used only once.");
        }

        private static void ParameterError(string error)
        {
            Console.Error.WriteLine(error);
            Environment.Exit(-1);
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
                        SecondOccurenceError("-c");
                    if (args.Length <= i + 1)
                        ParameterError("Expected cipher name after -c. Supported: "
                            + String.Join(" ", CIPEHRS));
                    if (args.Length <= i + 2)
                        ParameterError("Expected key after cipher name.");
                    String cipherName = args[i + 1];
                    if (cipherName == "caesar")
                    {
                        byte keyValue;
                        if (!byte.TryParse(args[i + 2], out keyValue))
                            ParameterError("Caesar key must be an integer value between "
                                + byte.MinValue + " and " + byte.MaxValue + ".");
                        cipher = new CaesarCipher(keyValue);
                    }
                    else if (cipherName == "vigenere")
                    {
                        if (0 == args[i + 2].Length)
                            ParameterError("Vigenere key cannot be empty string.");
                        cipher = new VigenereCipher(Encoding.ASCII.GetBytes(args[i + 2]));
                    }
                    else
                        ParameterError("Unsupported cipher: " + cipherName
                            + ". Choose one of: " + String.Join(" ", CIPEHRS));
                    i += 3;
                }
                else if (args[i] == "-i")
                {
                    if (null != input)
                        SecondOccurenceError("-i");
                    if (args.Length <= i + 1)
                        ParameterError("Expected file name after -i.");
                    input = File.OpenRead(args[i + 1]);
                    i += 2;
                }
                else if (args[i] == "-o")
                {
                    if (null != output)
                        SecondOccurenceError("-o");
                    if (args.Length <= i + 1)
                        ParameterError("Expected file name after -o.");
                    output = File.OpenWrite(args[i + 1]);
                    i += 2;
                }
                else if (args[i] == "-e" || args[i] == "-d")
                {
                    if (null != encode)
                        ParameterError("Only one of -e, -d can be used.");
                    encode = args[i] == "-e";
                    ++i;
                }
                else if (args[i] == "-t")
                {
                    if (null != threadCount)
                        SecondOccurenceError("-t");
                    if (args.Length <= i + 1)
                        ParameterError("Expected number of threads after -t.");
                    int n;
                    if (!int.TryParse(args[i + 1], out n))
                        ParameterError("Expected number of threads after -t.");
                    threadCount = n;
                    i += 2;
                }
                else if (args[i] == "-h")
                {
                    ShowHelp();
                    return;
                }
                else
                    ParameterError("Unknown parameter: " + args[i] + ".");
            }
            if (null == cipher)
                ParameterError("No cipher provided.");
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