using System;
using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.Model.Database.Loans;

namespace EzBob.Web.Areas.Customer.Models
{
    public class LoanScheduleItemModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DateTime PrevInstallmentDate { get; set; }
        public decimal RepaymentAmount { get; set; }
        public decimal Interest { get; set; }
        public decimal InterestPaid { get; set; }
        public string Status { get; set; }
        public decimal LateCharges { get; set; }
        public decimal AmountDue { get; set; }
        public decimal LoanRepayment { get; set; }
        public string StatusDescription { get; set; }
        public decimal Balance { get; set; }
        public decimal BalanceBeforeRepayment { get; set; }
        public decimal Fees { get; set; }
		public decimal InterestRate { get; set; }

        public static LoanScheduleItemModel FromLoanScheduleItem(LoanScheduleItem s)
        {
            return new LoanScheduleItemModel
                {
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

    public static class LoanScheduleExtension
    {
        public static IEnumerable<LoanScheduleItemModel> ToModel (this IEnumerable<LoanScheduleItem> items)
        {
            return items.Select(LoanScheduleItemModel.FromLoanScheduleItem);
        }
    }
}