namespace TestRailConsoleClient.Tests {
	using TestRail.Types;

	[TestCaseID(3212)]
	public class Positive : ITest {
		public Positive() {}

		public TestResult Run() {
			return new TestResult {
				Status = ResultStatus.Passed,
				TimeElapsed = 1
			};
		}
	}
}
