namespace EzBob.Web.Code {
	using System;
	using System.Linq;
	using ConfigManager;
	using Infrastructure;

	public class LoanLimit {
		public LoanLimit(IWorkplaceContext context) {
			this.context = context;
		} // constructor

		public void Check(double amount) {
			Check((decimal)amount);
		} // Check

		public void Check(decimal amount) {
			int xMinLoan = CurrentValues.Instance.XMinLoan;

			if ((amount < 0) || (amount < xMinLoan) || (amount > GetMaxLimit())) {
				throw new ArgumentException(string.Format(
					"Amount is more then {0} or less then {1}",
					GetMaxLimit(),
					xMinLoan
				));
			} // if
		} // Check

		public int GetMaxLimit() {
			return this.context.UserRoles.Any(r => r == "manager" || r == "Underwriter")
				? CurrentValues.Instance.ManagerMaxLoan
				: CurrentValues.Instance.MaxLoan;
		} // GetMaxLimit

		private readonly IWorkplaceContext context;
	} // class LoanLimit
} // namespace
