namespace AutomationCalculator.AutoDecision.AutoReRejection {
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib.Model.Database;
	using Newtonsoft.Json;

	public class MetaData {
		#region constructor

		public MetaData() {
			ValidationErrors = new List<string>();
		} // constructor

		#endregion constructor

		#region properties read from DB

		[JsonIgnore]
		public string RowType { get; set; }

		public int LmrID { get; set; }
		public DateTime LmrTime { get; set; }

		public int LoanCount { get; set; }

		public decimal TakenLoanAmount { get; set; }
		public decimal RepaidPrincipal { get; set; }
		public decimal SetupFees { get; set; }

		#endregion properties read from DB

		public List<string> ValidationErrors { get; private set; }

		#region property RepaidRatio

		[JsonIgnore]
		public decimal RepaidRatio {
			get {
				if (Math.Abs(TakenLoanAmount) < 0.00000001m)
					return 1;

				return (RepaidPrincipal + SetupFees) / TakenLoanAmount;
			} // get
		} // RepaidRatio

		#endregion property RepaidRatio

		#region method Validate

		public void Validate() {
			if (string.IsNullOrWhiteSpace(RowType))
				throw new Exception("Meta data was not loaded.");
		} // Validate

		#endregion method Validate
	} // class MetaData
} // namespace
