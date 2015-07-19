namespace TestRailConsoleClient.Tests {
	using TestRail.Types;

	[TestCaseID(3214)]
	public class EdgeCase : ITest {
		public EdgeCase() {}

		public TestResult Run() {
			return new TestResult {
				Status = ResultStatus.Passed,
				TimeElapsed = 1
			};
		}
	}
}
