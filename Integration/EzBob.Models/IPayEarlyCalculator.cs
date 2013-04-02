using EZBob.DatabaseLib.Model.Database.Loans;

namespace EzBob.Web.Areas.Customer.Models
{
    public interface IPayEarlyCalculator
    {
        decimal NextEarlyPayment();
        decimal TotalEarlyPayment();
        decimal PayEarly(decimal amount);
        LoanScheduleItem GetState();
    }
}