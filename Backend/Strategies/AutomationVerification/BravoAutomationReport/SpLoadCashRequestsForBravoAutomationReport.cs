namespace Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class SpLoadCashRequestsForBravoAutomationReport : AStoredProc {
		public SpLoadCashRequestsForBravoAutomationReport(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public override bool HasValidParameters() {
			return true;
		} // HasValidParameters

		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
	} // class SpLoadCashRequestsForBravoAutomationReport
} // namespace
