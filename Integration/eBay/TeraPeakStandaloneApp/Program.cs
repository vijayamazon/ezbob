using System;
using EZBob.DatabaseLib.Model.Database;
using EzBob.TeraPeakServiceLib;
using EzBob.TeraPeakServiceLib.Stub;
using StandaloneInitializer;

namespace TeraPeakStandaloneApp
{
    class Program
    {
        static void Main(string[] args)
        {
            StandaloneApp.Execute<App>(args);
        }

        public class App : StandaloneApp
        {
            public override void Run(string[] args)
            {
                if (args.Length > 2 || args.Length <= 0)
                {
                    Console.WriteLine("Usage: TeraPeakStandaloneApp.exe <umi>|<shop name> [<months>]");
                    return;
                }

                var displayName = GetShop(args[0]);

                int months = 5;

                if (args.Length > 1)
                {
                    months = int.Parse(args[1]);
                }

                var requestInfo = TerapeakRequestInfoBuilder.CreateRequestInfo(displayName, months);

                var data = TeraPeakService.SearchBySeller(requestInfo);
            }

            private string GetShop(string umiOrShop)
            {
                int umi;
                return int.TryParse(umiOrShop, out umi) ? Session.Get<MP_CustomerMarketPlace>(umi).DisplayName : umiOrShop;
            }
        }

        public void Init()
        {
            //NHibernateManager.FluentAssemblies.Add(typeof(e).Assembly);
        }
    }
}
