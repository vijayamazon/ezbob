namespace TestRailConsoleClient.Tests.TestsPOC {
    using System.Collections.Generic;
    using System.Linq;
    using TestRailModels.TestRail;

    [TestCaseID(3214)]
	public class EdgeCase : ITest {
		public EdgeCase() {}

		public TestResult Run(List<Configuration> configs = null) {
			return new TestResult {
				Status = ResultStatus.Passed,
				TimeElapsed = 1,
				Comment = configs == null || !configs.Any() ? "No config" : "Configs:" +  configs.Select(x => x.Name).Aggregate((a,b) => a + ", " + b)
			};
		}
	}
}
