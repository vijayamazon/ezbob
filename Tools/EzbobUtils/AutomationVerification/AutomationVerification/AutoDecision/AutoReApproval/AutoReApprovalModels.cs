using System;

namespace AutomationCalculator.AutoDecision.AutoReApproval
{
	public class AutoReApprovalInputDataModelDb
	{
		public bool IsFraudSuspect { get; set; }
		public DateTime? ManualApproveDate { get; set; }
		public int OfferedAmount { get; set; }
		public bool WasRejected { get; set; }
		public bool WasLate { get; set; }
		public int MaxLateDays { get; set; }
		public bool NewDataSourceAdded { get; set; }
		public int NumOutstandingLoans { get; set; }
		public bool HasLoanCharges { get; set; }
		public int LoanAmount { get; set; }
		public decimal RepaidPrincipal { get; set; }
		public decimal SetupFee { get; set; }

		//configs
		public int AutoReApproveMaxLacrAge { get; set; }
		public int AutoReApproveMaxLatePayment { get; set; }
		public int AutoReApproveMaxNumOfOutstandingLoans { get; set; }
	}

	public class AutoReApprovalInputDataModel
	{
		public DateTime AsOfDate { get; set; }
		public bool IsFraudSuspect { get; set; }
		public DateTime? ManualApproveDate { get; set; }
		public bool WasRejected { get; set; }
		public bool WasLate { get; set; }
		public int MaxLateDays { get; set; }
		public bool NewDataSourceAdded { get; set; }
		public int NumOutstandingLoans { get; set; }
		public bool HasLoanCharges { get; set; }
		public decimal ReApproveAmount { get; set; }
		public int AvaliableFunds { get; set; }
		//configs
		public int AutoReApproveMaxLacrAge { get; set; }
		public int AutoReApproveMaxLatePayment { get; set; }
		public int AutoReApproveMaxNumOfOutstandingLoans { get; set; }
	}
}
