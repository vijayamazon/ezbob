using System.Collections.Generic;
using System.Linq;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Models;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Areas.Underwriter.Models;

namespace EzBob.Web.Code
{
	using EzBob.Web.Models;
	using log4net;

    public class LoansDetailsBuilder {
        private static readonly ILog Log = LogManager.GetLogger(typeof (LoansDetailsBuilder));

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

			details.Schedule = loan.Schedule.ToModel();
            details.Charges = loan.Charges.Select(LoanChargesModel.FromCharges);

            details.Rollovers = rollovers.Where(x=>x.Status == RolloverStatus.New).Select(x => new RolloverModel
                                      {
                                          ExpiryDate = x.ExpiryDate,
                                          Payment = x.Payment,
                                          Status = x.Status,
                                          LoanScheduleId = x.LoanSchedule.Id,
                                          Id = x.Id
                                      });

            //TODO build loan details using new structure
            Log.InfoFormat("create loan details from new tables for loan {0}", loan.Id);

            return details;
        }

    }
}
