namespace TestRailConsoleClient.Tests {
	using TestRail.Types;

	[TestCaseID(3213)]
	public class Negative : ITest {
		public Negative() { }

		public TestResult Run() {
			return new TestResult {
				Comment = "failed to run this test",
				Status = ResultStatus.Failed,
				TimeElapsed = 1
			};
		}
	}
}
