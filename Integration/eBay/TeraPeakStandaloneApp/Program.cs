namespace TeraPeakStandaloneApp
{
	using System;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EzBob.eBayLib;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.TeraPeakServiceLib;
	using EzBob.TeraPeakServiceLib.Stub;
	using StandaloneInitializer;
	using NHibernateWrapper.NHibernate;

	class Program
    {
        static void Main(string[] args) {
	        StandaloneApp.Execute<App>(args);
        }

        public class App : StandaloneApp
        {

            public override void Run(string[] args)
            {
				Init();
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

        public static void Init()
        {
			Bootstrap.Init();
			NHibernateManager.FluentAssemblies.Add(typeof(eBayDatabaseMarketPlace).Assembly);

			new Log4Net().Init();

			var db = DbConnectionGenerator.Get(Log);
			
			EZBob.DatabaseLib.Library.Initialize(db.Env, db, Log);

			CurrentValues.Init(db,Log);
        }

		private static SafeILog Log {
			get {
				if (log == null)
					log = new SafeILog(typeof(Program));

				return log;
			} // get
		} // Log

		private static SafeILog log;
    }
}
