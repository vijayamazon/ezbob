namespace EzBob.Models
{
	using System;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;

    public class RepaymentCalculator
    {
        private readonly ChangeLoanDetailsModelBuilder _builder;

        public RepaymentCalculator()
        {
            _builder = new ChangeLoanDetailsModelBuilder();
        }

        public int ReCalculateRepaymentPeriod(CashRequest cashRequest)
        {
            var result = cashRequest.RepaymentPeriod;
            if (!string.IsNullOrEmpty(cashRequest.LoanTemplate))
            {
                var model = EditLoanDetailsModel.Parse(cashRequest.LoanTemplate);
                var loan = _builder.CreateLoan(model);
                var start = DateTime.UtcNow;
                var end = loan.Schedule.Last().Date;
                result = GetTotalMonts(start,  end);
            }
            return result;
        }

        public int CalculateCountRepayment(Loan loan)
        {
            int result;
            if (string.IsNullOrEmpty(loan.CashRequest.LoanTemplate))
            {
                result = loan.Schedule.Count;
            }
            else
            {
                var model = EditLoanDetailsModel.Parse(loan.CashRequest.LoanTemplate);
                var newLoan = _builder.CreateLoan(model);
                result = newLoan.Schedule.Count;
            }
            return result;
        }

        private int GetTotalMonts(DateTime start, DateTime end)
        {
            var compMonth = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
            double daysInEndMonth = (end - end.AddMonths(1)).Days;
            return (int)Math.Ceiling((compMonth + (start.Day - end.Day) / daysInEndMonth));
        }
    }
}
