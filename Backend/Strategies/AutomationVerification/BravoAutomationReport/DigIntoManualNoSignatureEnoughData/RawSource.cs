namespace Ezbob.Backend.Strategies.AutomationVerification.BravoAutomationReport.DigIntoManualNoSignatureEnoughData {
	using System;
	using AutomationCalculator.ProcessHistory;
	using DbConstants;
	using Ezbob.Database;

	internal class RawSource : AResultRow {
		public const string SpName = "BAR_LoadManualNoSignatureEnoughDataData";

		public int CustomerID { get; set; }
		public long FirstCashRequestID { get; set; }
		public DateTime DecisionTime { get; set; }

		public int ManualDecisionID {
			get { return (int)ManualDecision; }
			set { ManualDecision = (DecisionActions)value; }
		} // ManualDecisionID

		public string Reason {
			get { return this.reason ?? string.Empty; }
			set { this.reason = (value ?? string.Empty).Trim(); }
		} // Reason

		public string UnderwriterComment { get; set; }
		public int? MpID { get; set; }
		public string MpType { get; set; }
		public Guid? MpTypeInternalID { get; set; }
		public DateTime MpTotalsMonth { get; set; }
		public long AutoApproveTrailID { get; set; }

		public int? DecisionStatusID {
			get {
				if (DecisionStatus == null)
					return null;

				return (int)DecisionStatus.Value;
			} // get
			set {
				if (value == null)
					DecisionStatus = null;
				else
					DecisionStatus = (DecisionStatus)value;
			} // set
		} // DecisionStatusID

		public string TraceName {
			get { return this.traceName ?? string.Empty; }
			set {
				string name = (value ?? string.Empty).Trim();
				int pos = name.LastIndexOf('.');

				this.traceName = (pos < 0) ? string.Empty : name.Substring(pos + 1);
			} // set
		} // TraceName

		public DecisionActions ManualDecision { get; private set; }
		public DecisionStatus? DecisionStatus { get; private set; }

		private string reason;
		private string traceName;
	} // class RawSource
} // namespace
