namespace YodleeLib.config
{
	using EzBob.Configuration;

	public class YodleeConfig
	{
		public static YodleeEnvConnectionConfig _Config = ConfigurationRootBob.GetConfiguration().YodleeConfig;
	}
}
