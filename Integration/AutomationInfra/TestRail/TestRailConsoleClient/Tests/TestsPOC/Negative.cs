namespace TestRailConsoleClient.Tests.TestsPOC {
    using System.Collections.Generic;
    using System.Linq;
    using TestRailModels.TestRail;

    [TestCaseID(3213)]
	public class Negative : ITest {
		public Negative() { }

		public TestResult Run(List<Configuration> configs = null) {
			return new TestResult {
				Comment = "failed to run this test\n" + (configs == null || !configs.Any() ? "No config" : "Configs:" + configs.Select(x => x.Name).Aggregate((a, b) => a + ", " + b)),
				Status = ResultStatus.Failed,
				TimeElapsed = 1
			};
		}
	}
}
