namespace Ezbob.Backend.Strategies.MainStrategy {
	using Ezbob.Backend.Strategies.MainStrategy.Steps;

	internal class StepHistoryItem {
		public StepHistoryItem(string stepName) {
			this.stepName = stepName;
			this.executed = false;
		} // constructor

		public void SetResult(StepResults stepResult, string outcum) {
			this.executed = true;
			this.result = stepResult;
			this.outcome = outcum;
		} // SetResult

		public StepResult NextStepKey { get; set; }

		public string Message { get; set; }

		public string NextStepName { get; set; }

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"{0} => {1} ({2}) ---> '{3}' => {4} {5}",
				this.stepName,
				this.executed ? this.result.ToString() : "not executed",
				this.executed ? this.outcome : "not executed",
				NextStepKey == null ? "none" : NextStepKey.ToString(),
				string.IsNullOrWhiteSpace(NextStepName) ? "'N/A'" : NextStepName,
				string.IsNullOrWhiteSpace(Message) ? string.Empty : " - " + Message
			);
		} // ToString

		private readonly string stepName;
		private StepResults result;
		private string outcome;

		private bool executed;
	} // class StepHistoryItem
} // namespace
