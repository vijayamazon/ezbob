namespace TestRailConsoleClient.Tests {
	using System.Collections.Generic;
	using TestRail.Types;

	public interface ITest {
		TestResult Run(List<Configuration> configs = null);
	}
}
