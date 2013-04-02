using System.Collections.Generic;
using EZBob.DatabaseLib.Model.Loans;

namespace EzBob.Web.Areas.Customer.Models
{
    public class LoanDetails
    {
        public IEnumerable<LoanTransactionModel> Transactions { get; set; }
        public IEnumerable<LoanTransactionModel> PacnetTransactions { get; set; }
        public IEnumerable<LoanScheduleItemModel> Schedule { get; set; }
        public IEnumerable<EzBob.Models.LoanChargesModel> Charges { get; set; }
        public IEnumerable<PaymentRollover> Rollovers { get; set; }
    }
}