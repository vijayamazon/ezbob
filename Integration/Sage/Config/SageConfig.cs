namespace Sage.Config
{
	using StructureMap;

	public class SageConfig
    {
		public static ISageConfig _Config = ObjectFactory.GetInstance<ISageConfig>();
    }
}
