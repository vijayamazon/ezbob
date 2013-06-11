namespace FreeAgent.Config
{
	using StructureMap;

	public class FreeAgentConfig
    {
		public static IFreeAgentConfig _Config = ObjectFactory.GetInstance<IFreeAgentConfig>();
    }
}
