namespace Ezbob.Backend.Strategies.MainStrategy {
	using System;
	using Ezbob.Backend.Strategies.MainStrategy.Steps;

	internal class StepHistoryItem {
		public StepHistoryItem(string stepName) {
			this.stepName = stepName;
			this.executed = false;
			this.startTime = DateTime.UtcNow;
		} // constructor

		public void SetResult(StepResults stepResult, string outcum) {
			this.endTime = DateTime.UtcNow;
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
				"[{6}] {0} => {1} ({2}) {5}\n\t[{7}] ---> '{3}' => {4}",
				this.stepName,
				this.executed ? this.result.ToString() : "not executed",
				this.executed ? this.outcome : "not executed",
				NextStepKey == null ? "none" : NextStepKey.ToString(),
				string.IsNullOrWhiteSpace(NextStepName) ? "'N/A'" : NextStepName,
				string.IsNullOrWhiteSpace(Message) ? string.Empty : " - " + Message,
				this.startTime == null ? "not started " : this.startTime.Value.ToString("HH:mm:ss.fff"),
				this.endTime   == null ? "not ended   " : this.endTime.Value.ToString("HH:mm:ss.fff")
			);
		} // ToString

		private readonly string stepName;
		private StepResults result;
		private string outcome;

		private DateTime? startTime;
		private DateTime? endTime;

		private bool executed;
	} // class StepHistoryItem
} // namespace
