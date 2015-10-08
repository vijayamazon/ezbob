namespace TestRailConsoleClient.Tests {
    using TestRailModels.TestRail;

    public class TestResult {
		public ResultStatus Status { get; set; }
		public long TimeElapsed { get; set; }
		public string Comment { get; set; }
	}
}
