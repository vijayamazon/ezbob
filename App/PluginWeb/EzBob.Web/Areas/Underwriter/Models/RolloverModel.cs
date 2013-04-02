using EZBob.DatabaseLib.Model.Loans;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class RolloverModel : PaymentRollover
    {
        public int LoanScheduleId { get; set; }
        public int LoanId { get; set; }
        public decimal RolloverPayValue { get; set; }
    }
}