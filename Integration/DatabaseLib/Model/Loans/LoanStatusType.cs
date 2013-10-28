using EZBob.DatabaseLib.Model.Database.Loans;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database.Loans {
	#region enum LoanStatus

	public enum LoanStatus {
		Processing,
		Failed,
		Live,
		Late,
		PaidOff,
		WrittenOff,
		Collection,
		Legal
	} // enum LoanStatus

	public class LoanStatusType : EnumStringType<LoanStatus> {} // class LoanStatusType

	#endregion enum LoanStatus
} // namespace EZBob.DatabaseLib.Model.Database.Loans

namespace EZBob.DatabaseLib.Model.Database.Mapping {
	#region class LoanStatusExtenstions

	public static class LoanStatusExtenstions {
		public static string ToDescription(this LoanStatus status) {
			switch (status) {
			case LoanStatus.Processing:
				return "Processing";

			case LoanStatus.Live:
				return "Active";

			case LoanStatus.Failed:
				return "Error";

			case LoanStatus.PaidOff:
				return "Paid";

			case LoanStatus.WrittenOff:
				return "Rolled Over";

			case LoanStatus.Late:
			case LoanStatus.Collection:
			case LoanStatus.Legal:
				return "Overdue";
			} // switch

			return "";
		} // ToDescription
	} // class LoanStatusExtenstions

	#endregion class LoanStatusExtenstions
} // namespace EZBob.DatabaseLib.Model.Database.Mapping
