using System;
using log4net.Config;
using ExperianLib.Ebusiness;
using Newtonsoft.Json;
using StandaloneInitializer;

[assembly: XmlConfigurator(ConfigFileExtension = "log4net", Watch = true)]
namespace ExperianStandAlone
{
	using Ezbob.Database;

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
				if (args.Length < 2)
				{
					Usage("not enough arguments");
				}

				try
				{
			var oLog4NetCfg = new Log4Net().Init();

			m_oLog = new ConsoleLog(new SafeILog(this));

			m_oDB = new SqlConnection(oLog4NetCfg.Environment, m_oLog);

					var service = new EBusinessService();
					var refNum = args[1];
					if (args[0] == "limited")
					{
						var result = service.GetLimitedBusinessData(refNum, 1, false, false);
						Console.WriteLine("Output XML:");
						Console.WriteLine();
						Console.WriteLine(result.OutputXml);
						Console.WriteLine();
						Console.WriteLine("Limited business with ref number = {0}", refNum);
						Console.WriteLine(JsonConvert.SerializeObject(result));
					}

					else if (args[0] == "nonlimited")
					{
						var result = service.GetNotLimitedBusinessData(refNum, 1, false, false);
						Console.WriteLine("Output XML:");
						Console.WriteLine();
						Console.WriteLine(result.OutputXml);
						Console.WriteLine();
						Console.WriteLine("Non Limited business with ref number = {0}", refNum);
						Console.WriteLine(JsonConvert.SerializeObject(result));

					}
					else if (args[0] == "targeting")
					{
						if (args.Length < 4)
						{
							throw new Exception("not enough parameters");
						}

						TargetResults.LegalStatus status = TargetResults.LegalStatus.DontCare;
						if (args[3] == "limited")
						{
							status = TargetResults.LegalStatus.Limited;
						}
						if (args[3] == "nonlimited")
						{
							status = TargetResults.LegalStatus.NonLimited;
						}
						var limitedRefNum = "";
						if (args.Length == 5)
						{
							limitedRefNum = args[4];
						}
						var result = service.TargetBusiness(refNum, args[2], 1, status, limitedRefNum);
						Console.WriteLine("Output XML:");
						Console.WriteLine();
						Console.WriteLine(result.OutStr);
						Console.WriteLine();
						Console.WriteLine("Json Targeting Companies for {0} {1}:", refNum, args[2]);
						Console.WriteLine(JsonConvert.SerializeObject(result));

					}
					else
					{
						Usage("wrong params");
					}
				}
				catch (Exception ex)
				{
					Usage(ex.Message);
				}
			}

			private static void Usage(string message = "")
			{
				Console.WriteLine(message);
				Console.WriteLine(
					@"usage in order to retrieve limited e-series: {0} ExperianStandAlone.exe <'limited'> [refNumber]", Environment.NewLine);
				Console.WriteLine(
					@"usage in order to retrieve non limited e-series: {0} ExperianStandAlone.exe <'nonlimited'> [refNumber]", Environment.NewLine);
				Console.WriteLine(
					@"usage in order to retrieve targeting e-series: {0} ExperianStandAlone.exe <'targeting'> <company name> <post code> <'limited'/'nonlimited'/'all'> [limited refNumber]", Environment.NewLine);
				Environment.Exit(0);
			}
		}

		public static void Init()
		{
			Bootstrap.Init();
		}
	}
}
