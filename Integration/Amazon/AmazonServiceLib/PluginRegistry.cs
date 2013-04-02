using EzBob.AmazonServiceLib.Config;
using StructureMap.Configuration.DSL;

namespace EzBob.AmazonServiceLib
{
	public class PluginRegistry : Registry
	{
		public PluginRegistry()
		{
            For<IAmazonMarketPlaceTypeConnection>().Use(new AmazonMarketPlaceTypeConnection());
		}
	}
}