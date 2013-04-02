using EzBob.CommonLib;
using StructureMap.Configuration.DSL;

namespace EZBob.DatabaseLib.Tests
{
	public class TestRegistry : Registry
	{
		public TestRegistry()
		{
			For<IDatabaseDataHelper>().Use<DatabaseDataHelperTest>();
		}
	}
}