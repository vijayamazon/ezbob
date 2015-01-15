using System;

namespace EzBob.Web.Areas.Underwriter.Models
{
	using EZBob.DatabaseLib.Model.Database.Loans;

	public class LoanHistoryModel
    {
        public int Id { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal Repayments { get; set; }
        public DateTime DateApplied { get; set; }
        public DateTime? DateClosed { get; set; }
        public string Status { get; set; }
        public decimal Outstanding { get; set; }

		public static LoanHistoryModel Create(Loan loan) {
			return new LoanHistoryModel {
				Id = loan.Id,
				DateApplied = loan.Date,
				LoanAmount = loan.LoanAmount,
				DateClosed = loan.DateClosed,
				Outstanding = loan.Balance,
				Status = loan.Status.ToString(),
				Repayments = loan.Repayments,
			};
		}
	}
}