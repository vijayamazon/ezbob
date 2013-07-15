using EZBob.DatabaseLib.Model.Database.Loans;

namespace EzBob.Web.Areas.Customer.Models
{
    public interface ILoanRepaymentScheduleCalculator
    {
        decimal NextEarlyPayment();
        decimal TotalEarlyPayment();
        decimal RecalculateSchedule();
        LoanScheduleItem GetState();
    }
}