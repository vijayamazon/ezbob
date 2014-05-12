namespace Ezbob.Backend.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;
	using EZBob.DatabaseLib.Model.Database.Loans;

	[DataContract]
	public class LoanScheduleItemModel {
		[DataMember]
		public int Id { get; set; }

		[DataMember]
		public DateTime Date { get; set; }

		[DataMember]
		public DateTime PrevInstallmentDate { get; set; }

		[DataMember]
		public decimal RepaymentAmount { get; set; }

		[DataMember]
		public decimal Interest { get; set; }

		[DataMember]
		public decimal InterestPaid { get; set; }

		[DataMember]
		public string Status { get; set; }

		[DataMember]
		public decimal LateCharges { get; set; }

		[DataMember]
		public decimal AmountDue { get; set; }

		[DataMember]
		public decimal LoanRepayment { get; set; }

		[DataMember]
		public string StatusDescription { get; set; }

		[DataMember]
		public decimal Balance { get; set; }

		[DataMember]
		public decimal BalanceBeforeRepayment { get; set; }

		[DataMember]
		public decimal Fees { get; set; }

		[DataMember]
		public decimal InterestRate { get; set; }

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
				StatusDescription = s.Status.ToDescription(),
				LoanRepayment = s.LoanRepayment,
				Balance = s.Balance,
				BalanceBeforeRepayment = s.BalanceBeforeRepayment,
				Fees = s.Fees,
				InterestRate = s.InterestRate
			};
		}
	}

	public static class LoanScheduleExtension {
		public static IEnumerable<LoanScheduleItemModel> ToModel(this IEnumerable<LoanScheduleItem> items) {
			return items.Select(LoanScheduleItemModel.FromLoanScheduleItem);
		}
	}
}