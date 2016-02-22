namespace AutomationCalculator.AutoDecision.AutoRejection.Models {
	using System.Collections.Generic;

	public class RejectionConfigs {
		public RejectionConfigs() {
			EnabledTraces = new SortedSet<string>();
		} // constructor

		public virtual bool IsTraceEnabled<T>() {
			return EnabledTraces.Contains(typeof(T).FullName);
		} // IsTraceEnabled

		public virtual int AutoRejectConsumerCheckAge { get; set; }
		public virtual int AutoRejectionException_AnualTurnover { get; set; }
		public virtual int AutoRejectionException_CreditScore { get; set; }
		public virtual int RejectionExceptionMaxCompanyScore { get; set; }
		public virtual int RejectionExceptionMaxConsumerScoreForMpError { get; set; }
		public virtual int RejectionExceptionMaxCompanyScoreForMpError { get; set; }
		public virtual int LowCreditScore { get; set; }
		public virtual int RejectionCompanyScore { get; set; }
		public virtual int Reject_Defaults_CreditScore { get; set; }
		public virtual int Reject_Defaults_AccountsNum { get; set; }
		public virtual int Reject_Defaults_Amount { get; set; }
		public virtual int Reject_Defaults_MonthsNum { get; set; }

		public virtual int Reject_Defaults_CompanyScore { get; set; }
		public virtual int Reject_Defaults_CompanyAccountsNum { get; set; }
		public virtual int Reject_Defaults_CompanyMonthsNum { get; set; }
		public virtual int Reject_Defaults_CompanyAmount { get; set; }
		public virtual int Reject_Minimal_Seniority { get; set; }
		public virtual int TotalAnnualTurnover { get; set; }
		public virtual int TotalThreeMonthTurnover { get; set; }
		public virtual int Reject_LateLastMonthsNum { get; set; }
		public virtual int Reject_NumOfLateAccounts { get; set; }
		public virtual int RejectionLastValidLate { get; set; }

		public virtual SortedSet<string> EnabledTraces { get; private set; }
	} // class RejectionConfigs
} // namespace