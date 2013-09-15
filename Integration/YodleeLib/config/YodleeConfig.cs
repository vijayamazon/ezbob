namespace YodleeLib.config
{
	using EzBob.Configuration;
	using StructureMap;

    public class YodleeConfig
    {
		public static YodleeEnvConnectionConfig _Config = ConfigurationRootBob.GetConfiguration().YodleeConfig;
    }
}
