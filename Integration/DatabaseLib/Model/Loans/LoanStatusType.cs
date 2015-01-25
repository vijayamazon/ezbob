namespace EZBob.DatabaseLib.Model.Database.Loans 
{
	using System.ComponentModel;
	using NHibernate.Type;

	public enum LoanStatus {
		[Description("Active")]
		Live,
		[Description("Overdue")]
		Late,
		[Description("Paid")]
		PaidOff
	} // enum LoanStatus

	public class LoanStatusType : EnumStringType<LoanStatus> {} // class LoanStatusType

} // namespace EZBob.DatabaseLib.Model.Database.Loans
