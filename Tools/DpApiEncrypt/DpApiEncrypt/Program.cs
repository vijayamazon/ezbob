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
                var parsedArgs = new CommandLineArguments();
                if (!Parser.ParseArgumentsWithUsage(args, parsedArgs))
                {
                    Console.ReadLine();
                    return;
                }
                if (string.IsNullOrEmpty(parsedArgs.Url))
                {
                    Console.WriteLine(parsedArgs.Data.Encrypt());
                }
                else
                {
                    var proxy = new Service {Url = parsedArgs.Url};
                    Console.WriteLine(proxy.Encrypt(parsedArgs.Data));
                }
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

            [Argument(ArgumentType.Required | ArgumentType.AtMostOnce, ShortName = "d", HelpText = "Encrypting data")]
            public string Data;
        }

    }
}
