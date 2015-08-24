namespace TestRailConsoleClient.Tests {
    using System.Collections.Generic;
    using TestRailModels.TestRail;

    public interface ITest {
		TestResult Run(List<Configuration> configs = null);
	}
}
