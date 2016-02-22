namespace AutomationCalculator.AutoDecision.AutoReApproval {
	using System;
	using AutomationCalculator.ProcessHistory;
	using EZBob.DatabaseLib.Model.Database;
	using Newtonsoft.Json;

	public class AutoReApprovalInputDataModelDb {
		public int FraudStatus { get; set; }
		public DateTime? ManualApproveDate { get; set; }
		public decimal ApprovedAmount { get; set; }
		public bool WasRejected { get; set; }
		public bool WasLate { get; set; }
		public int MaxLateDays { get; set; }
		public bool NewDataSourceAdded { get; set; }
		public int NumOutstandingLoans { get; set; }
		public bool HasLoanCharges { get; set; }
		public decimal TookLoanAmount { get; set; }
		public decimal RepaidPrincipal { get; set; }
		public decimal SetupFee { get; set; }
		public int LacrID { get; set; }

		//configs
		public int AutoReApproveMaxLacrAge { get; set; }
		public int AutoReApproveMaxLatePayment { get; set; }
		public int AutoReApproveMaxNumOfOutstandingLoans { get; set; }
		public int MinLoan { get; set; }
	} // class AutoReApprovalInputDataModelDb

	public class ReApprovalInputData : ITrailInputData {
		public void Init(DateTime dataAsOf, ReApprovalInputData data) {
			DataAsOf = dataAsOf;

			if (data == null)
				return;

			FraudStatus = data.FraudStatus;
			ManualApproveDate = data.ManualApproveDate;
			WasLate = data.WasLate;
			WasRejected = data.WasRejected;
			MaxLateDays = data.MaxLateDays;
			NewDataSourceAdded = data.NewDataSourceAdded;
			NumOutstandingLoans = data.NumOutstandingLoans;
			HasLoanCharges = data.HasLoanCharges;
			ReApproveAmount = data.ReApproveAmount;
			AvaliableFunds = data.AvaliableFunds;
			AutoReApproveMaxLacrAge = data.AutoReApproveMaxLacrAge;
			AutoReApproveMaxLatePayment = data.AutoReApproveMaxLatePayment;
			AutoReApproveMaxNumOfOutstandingLoans = data.AutoReApproveMaxNumOfOutstandingLoans;
			MinLoan = data.MinLoan;
			LacrID = data.LacrID;
		} // Init

		public DateTime DataAsOf { get; private set; }
		public FraudStatus FraudStatus { get; set; }
		public DateTime? ManualApproveDate { get; set; }
		public bool WasRejected { get; set; }
		public bool WasLate { get; set; }
		public int MaxLateDays { get; set; }
		public bool NewDataSourceAdded { get; set; }
		public int NumOutstandingLoans { get; set; }
		public bool HasLoanCharges { get; set; }
		public decimal ReApproveAmount { get; set; }
		public decimal AvaliableFunds { get; set; }
		public long LacrID { get; set; }

		// Configs
		public int AutoReApproveMaxLacrAge { get; set; }
		public int AutoReApproveMaxLatePayment { get; set; }
		public int AutoReApproveMaxNumOfOutstandingLoans { get; set; }
		public int MinLoan { get; set; }

		public string Serialize() {
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		} // Serialize
	} // class ReApprovalInputData

	public class ReApprovalResult {
		public ReApprovalResult(bool isAutoReApproved, int reApproveAmount) {
			IsAutoReApproved = isAutoReApproved;
			ReApproveAmount = reApproveAmount;
		} // constructor

		public bool IsAutoReApproved { get; private set; }
		public int ReApproveAmount { get; private set; }

		public override string ToString() {
			return IsAutoReApproved ? "ReApproved for " + ReApproveAmount : "Not ReApproved";
		} // ToString
	} // class ReApprovalResult
} // namespace
