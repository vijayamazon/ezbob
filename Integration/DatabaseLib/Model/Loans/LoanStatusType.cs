namespace EZBob.DatabaseLib.Model.Database.Loans 
{
	using NHibernate.Type;

	#region enum LoanStatus

	public enum LoanStatus {
		Live,
		Late,
		PaidOff
	} // enum LoanStatus

	public class LoanStatusType : EnumStringType<LoanStatus> {} // class LoanStatusType

	#endregion enum LoanStatus
} // namespace EZBob.DatabaseLib.Model.Database.Loans

namespace EZBob.DatabaseLib.Model.Database.Mapping 
{
	using Loans;

	#region class LoanStatusExtenstions

	public static class LoanStatusExtenstions {
		public static string ToDescription(this LoanStatus status) {
			switch (status) {
			case LoanStatus.Live:
				return "Active";

			case LoanStatus.PaidOff:
				return "Paid";

			case LoanStatus.Late:
				return "Overdue";
			} // switch

			return "";
		} // ToDescription
	} // class LoanStatusExtenstions

	#endregion class LoanStatusExtenstions
} // namespace EZBob.DatabaseLib.Model.Database.Mapping
