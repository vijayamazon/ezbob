namespace EzBob.Web.Models {
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Backend.Models;
	using Ezbob.Utils.Extensions;
	using EZBob.DatabaseLib.Model.Database.Loans;

	public static class LoanScheduleExtention {
		public static LoanScheduleItemModel FromLoanScheduleItem(LoanScheduleItem s) {
			return new LoanScheduleItemModel {
				Id = s.Id,
				AmountDue = s.AmountDue,
				Date = s.Date,
				PrevInstallmentDate = s.PrevInstallmentDate,
				Interest = s.Interest,
				InterestPaid = s.InterestPaid,
				LateCharges = s.LateCharges,
				RepaymentAmount = s.RepaymentAmount,
				Status = s.Status.ToString(),
				StatusDescription = s.Status.DescriptionAttr(),
				LoanRepayment = s.LoanRepayment,
				Balance = s.Balance,
				BalanceBeforeRepayment = s.BalanceBeforeRepayment,
				Fees = s.Fees,
				InterestRate = s.InterestRate
			};
		}

		public static IEnumerable<LoanScheduleItemModel> ToModel(this IEnumerable<LoanScheduleItem> items) {
			return items.Select(LoanScheduleExtention.FromLoanScheduleItem);
		}
	}
}