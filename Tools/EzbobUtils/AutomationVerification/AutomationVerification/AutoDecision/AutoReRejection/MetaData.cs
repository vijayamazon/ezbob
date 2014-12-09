namespace AutomationCalculator.AutoDecision.AutoReRejection {
	using System;
	using System.Collections.Generic;
	using Newtonsoft.Json;

	public class MetaData {

		public MetaData() {
			ValidationErrors = new List<string>();
		} // constructor

		[JsonIgnore]
		public string RowType { get; set; }

		public bool LastDecisionWasReject { get; set; }
		public DateTime? LastDecisionDate { get; set; }

		public DateTime? LastRejectDate { get; set; }
		public int NumOfOpenLoans { get; set; }

		public int LoanCount { get; set; }

		public decimal TakenLoanAmount { get; set; }
		public decimal RepaidPrincipal { get; set; }
		public decimal SetupFees { get; set; }

		public List<string> ValidationErrors { get; private set; }

		[JsonIgnore]
		public decimal RepaidRatio {
			get {
				if (Math.Abs(TakenLoanAmount) < 0.00000001m)
					return 1;

				return (RepaidPrincipal + SetupFees) / TakenLoanAmount;
			} // get
		} // RepaidRatio

		public void Validate() {
			if (string.IsNullOrWhiteSpace(RowType))
				throw new Exception("Meta data was not loaded.");
		} // Validate

	} // class MetaData
} // namespace
