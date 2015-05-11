namespace Ezbob.Backend.CalculateLoan.Models.Extensions {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.CalculateLoan.Models.Helpers;

	public static class RepaymentListExt {
		public static bool ContainsDate(this List<Repayment> lst, DateTime date) {
			return (lst != null) && lst.Any(s => s.Date == date.Date);
		} // ContainsDate
	} // class RepaymentListExt
} // namespace
