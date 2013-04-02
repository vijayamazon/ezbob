using StructureMap;
using StructureMap.Graph;

namespace EZBob.DatabaseLib.Tests
{
	internal class ScannerTest
	{
		public static void Register()
		{
			ObjectFactory.Initialize( x => x.Scan( s =>
			    {
					s.AssemblyContainingType( typeof( TimePeriodDataAggregationFixture ) );
					s.LookForRegistries();
			    }) );
		}

	}
}