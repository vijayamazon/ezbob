namespace EzBob.Web.Code {
	using EZBob.DatabaseLib.Model.Database.Loans;

	public class BuiltLoan {
		public Loan Loan { get; set; }
		public decimal ManualSetupFeePercent { get; set; }
		public decimal BrokerFeePercent { get; set; }
	} // class BuiltLoan
} // namespace
