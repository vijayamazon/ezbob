using System.Collections.Generic;
using System.Linq;
using EzBob.Models;
using EzBob.Web.Areas.Customer.Models;
using PaymentServices.Calculators;

namespace EzBob.Web.Areas.Underwriter.Models
{
	using ConfigManager;

	public class LoansAndOffers
    {
        public List<LoanModel> loans { get; set; }
        public List<CashRequestModel> offers { get; set; }

        public LoansAndOffers(List<LoanModel> loans, List<CashRequestModel> offers)
        {
            this.loans = loans;
            this.offers = offers;
        }

        public LoansAndOffers(EZBob.DatabaseLib.Model.Database.Customer customer)
        {
			loans = customer.Loans.Select(l => LoanModel.FromLoan(l, new LoanRepaymentScheduleCalculator(l, null, CurrentValues.Instance.AmountToChargeFrom))).ToList();
            offers = customer.CashRequests
                .OrderBy(c => c.CreationDate)
                .Select(c => CashRequestModel.Create(c))
                .ToList();
        }
    }
}