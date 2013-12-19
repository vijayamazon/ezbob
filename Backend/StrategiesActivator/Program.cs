namespace StrategiesActivator
{
	using System;
	using Scorto.Configuration.Loader;

	public class Program
    {
		public static void Main(string[] args)
        {
			if (args.Length < 1)
			{
				Console.WriteLine("Usage: StrategiesActivator.exe <StrategyName> [param1] [param2] ... [paramN]");
				return;
			}

			LoadConfigurations();

			var strategiesActivator = new StrategiesActivator(args);
			strategiesActivator.Execute();
        }

		private static void LoadConfigurations()
        {
			EnvironmentConfigurationLoader.AppPathDummy = @"c:\ezbob\app\pluginweb\EzBob.Web\";
        }
    }
}
