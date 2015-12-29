namespace Ezbob.Backend.Strategies.NewLoan.Collection {
	using ConfigManager;
	using System;

	/// <summary>
	/// Late Loan Job
	/// </summary>
	public class LateLoanJob : AStrategy {

		public LateLoanJob(DateTime? runTime) {
			if (runTime != null)
				this.nowTime = (DateTime)runTime;
			else
				this.nowTime = DateTime.UtcNow;
		}

		private readonly DateTime nowTime;
		public override string Name { get { return "LateLoanJob"; } }


		public override void Execute() {

			NL_AddLog(LogType.Info, "Strategy Start", this.nowTime, null, null, null);

			if (!CurrentValues.Instance.NewLoanRun) {
				NL_AddLog(LogType.Info, "NL disabled by configuration", null, null, null, null);
				return;
			}

			try {

				NL_AddLog(LogType.Info, "Strategy Start", this.nowTime, null, null, null);

				//For each loan schedule marks it as late, it's loan as late, applies fee if needed
				AStrategy strategy = new SetLateLoanStatus(this.nowTime);
				strategy.Execute();

				//We dont want to send notifications twice for now...
				strategy = new LateLoanNotification(this.nowTime);
				strategy.Execute();

				//We dont want to change customer status twice for now...
				strategy = new LateLoanCured(this.nowTime);
				strategy.Execute();

				NL_AddLog(LogType.Info, "Strategy End", this.nowTime, null, null, null);
			} catch (Exception ex) {
				NL_AddLog(LogType.Error, "Strategy failed", this.nowTime, null, ex.ToString(), ex.StackTrace);
			}
		}//Execute

	}// class CollectionRobot
} // namespace
