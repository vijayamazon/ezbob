using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Models;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Areas.Underwriter.Models;

namespace EzBob.Web.Code
{
	using Ezbob.Backend.Models;

	public class LoansDetailsBuilder 
    {
        public LoanDetails Build(Loan loan, IEnumerable<PaymentRollover> rollovers )
        {
            var details = new LoanDetails();

            details.Transactions = loan.TransactionsWithPaypoint
                .Select(t => new LoanTransactionModel()
                {
                    Id = t.Id,
                    Amount = t.Amount,
                    Description = t.Description,
                    PostDate = t.PostDate,
                    Status = t.Status.ToString(),
                    StatusDescription = t.Status.ToDescription(),
                    Principal = t.Principal,
                    Interest = t.Interest,
                    Fees = t.Fees,
                    Rollover = t.Rollover,
                    Balance = t.Balance,
                    LoanRepayment =  t.LoanRepayment
                });

            details.PacnetTransactions = loan.PacnetTransactions.Select(t => new LoanTransactionModel()
            {
                Id = t.Id,
                Amount = t.Amount,
                Description = t.Description,
                PostDate = t.PostDate,
                Status = t.Status.ToString(),
                StatusDescription = t.Status.ToDescription(),
                Fees = t.Fees
            });
			List<LoanScheduleItemModel> l = new List<LoanScheduleItemModel>();
			foreach (var s in loan.Schedule)
			{
				LoanScheduleItemModel n = new LoanScheduleItemModel
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

				l.Add(n);
			}
            details.Schedule = l;
            details.Charges = loan.Charges.Select(LoanChargesModel.FromCharges);

            details.Rollovers = rollovers.Where(x=>x.Status == RolloverStatus.New).Select(x => new RolloverModel
                                      {
                                          ExpiryDate = x.ExpiryDate,
                                          Payment = x.Payment,
                                          Status = x.Status,
                                          LoanScheduleId = x.LoanSchedule.Id,
                                          Id = x.Id
                                      });

            return details;
        }

    }
}
