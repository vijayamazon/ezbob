using System;
using CommandLine;
using Master;
using Scorto.Configuration;

namespace DpApiEncrypt
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                string result = "";
                var parsedArgs = new CommandLineArguments();
                if (!Parser.ParseArgumentsWithUsage(args, parsedArgs))
                {
                    Console.ReadLine();
                    return;
                }
                if (string.IsNullOrEmpty(parsedArgs.Url))
                {
                    if (parsedArgs.Decrypt)
                    {
                        result = parsedArgs.Data.Decrypt();
                    }
                    else
                    {
                        result = parsedArgs.Data.Encrypt();
                    }
                }
                else
                {
                    if (parsedArgs.Decrypt)
                    {
                        throw new NotImplementedException("Remote decrypting is not supported.");
                    }

                    var proxy = new Service {Url = parsedArgs.Url};
                    result = proxy.Encrypt(parsedArgs.Data);
                }

                Console.WriteLine(result);

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private class CommandLineArguments
        {
            [Argument(ArgumentType.AtMostOnce, ShortName = "u", HelpText = "Master service URL")]
            public string Url;

            [Argument(ArgumentType.AtMostOnce, HelpText = "Decrypt")]
            public bool Decrypt;

            [Argument(ArgumentType.AtMostOnce, HelpText = "Encrypt")]
            public bool Encrypt;

            [Argument(ArgumentType.Required | ArgumentType.AtMostOnce, ShortName = "d", HelpText = "Encrypting data")]
            public string Data;
        }

    }
}
