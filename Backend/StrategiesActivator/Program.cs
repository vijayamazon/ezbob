namespace StrategiesActivator
{
	using System;
	using Scorto.Configuration.Loader;
	using EzBob.Backend.Strategies.MailStrategies;

	public class StrategiesActivator
    {
		public static void Main(string[] args)
        {
			if (args.Length < 1)
			{
				Console.WriteLine("Usage: StrategiesActivator.exe <StrategyName> [param1] [param2] ... [paramN]");
				return;
			}

			LoadConfigurations();

			var g = new Greeting(3060, "dfg");
			g.Execute();
        }

        private static void LoadConfigurations()
        {
			EnvironmentConfigurationLoader.AppPathDummy = @"c:\ezbob\app\pluginweb\EzBob.Web\";
        }
    }
}
