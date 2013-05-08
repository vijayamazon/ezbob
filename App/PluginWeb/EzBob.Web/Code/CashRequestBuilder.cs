using System;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Infrastructure;
using PaymentServices.Calculators;

namespace EzBob.Web.Code
{
    public class CashRequestBuilder
    {
        private readonly IEzbobWorkplaceContext _context;
        private readonly LoanScheduleCalculator _calculator;
        private readonly APRCalculator _aprCalc;

        public CashRequestBuilder(IEzbobWorkplaceContext context)
        {
            _context = context;
            _calculator = new LoanScheduleCalculator();
            _aprCalc = new APRCalculator();
        }

        public CashRequest CreateCashRequest(int amount)
        {
            var schedule = _calculator.Calculate(amount);
            var apr = _aprCalc.Calculate(amount, schedule);

            var cashRequest = new CashRequest()
            {
                CreationDate = DateTime.UtcNow,
                Customer = _context.Customer,
                APR = (decimal)apr,
                InterestRate = _calculator.Interest
            };

            return cashRequest;
        }
    }
}