namespace Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport {
	using System;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class SpLoadCashRequests : AStoredProcedure {
		public SpLoadCashRequests(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public override bool HasValidParameters() {
			return true;
		} // HasValidParameters

		/// <summary>
		/// Returns the name of the stored procedure.
		/// </summary>
		/// <returns>SP name.</returns>
		protected override string GetName() {
			return "BAR_LoadCashRequests";
		} // GetName

		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
	} // class SpLoadCashRequests
} // namespace
