using System;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests
{
    [TestFixture]
    public class PayLoanFixture
    {
        private Customer _customer;
        private LoanPaymentFacade _loanRepaymentFacade;
        private LoanScheduleCalculator _calculator;

        [SetUp]
        public void SetUp()
        {
            _customer = new Customer();
            _calculator = new LoanScheduleCalculator();
            _loanRepaymentFacade = new LoanPaymentFacade();
        }

        [Test]
        public void transaction_balances_are_correctly_updated()
        {
            var date = new DateTime(2012, 1, 1);

            var loan1 = new Loan();
            _calculator.Calculate(100, loan1, date);

            _customer.Loans.Add(loan1);

            loan1.Status = LoanStatus.Live;

            _loanRepaymentFacade.PayLoan(loan1, "trans_id", 30, null, date);
            _loanRepaymentFacade.PayLoan(loan1, "trans_id", 30, null, date);
            _loanRepaymentFacade.PayLoan(loan1, "trans_id", 40, null, date);

            Assert.That(loan1.Transactions.Count, Is.EqualTo(3));

            var transactions = loan1.Transactions.OrderBy(t => t.PostDate).Cast<PaypointTransaction>().ToList();

            Assert.That(transactions[0].Amount, Is.EqualTo(30));
            Assert.That(transactions[1].Amount, Is.EqualTo(30));
            Assert.That(transactions[2].Amount, Is.EqualTo(40));
        }

        [Test]
        public void can_pay_loan_early_in_two_times()
        {
            var date = new DateTime(2012, 1, 1);

            var loan1 = new Loan();
            _calculator.Calculate(100, loan1, date);

            _customer.Loans.Add(loan1);

            loan1.Status = LoanStatus.Live;

            _loanRepaymentFacade.PayLoan(loan1, "trans_id", 50, null, date);
            _loanRepaymentFacade.PayLoan(loan1, "trans_id", 50, null, date);

            Assert.That(loan1.Status, Is.EqualTo(LoanStatus.PaidOff));

            Assert.That(loan1.Balance, Is.EqualTo(0));
            Assert.That(loan1.Principal, Is.EqualTo(0));
        }

        [Test]
        public void can_pay_loan_early_in_three_times()
        {
            var date = new DateTime(2012, 1, 1);

            var loan1 = new Loan();
            _calculator.Calculate(100, loan1, date);

            _customer.Loans.Add(loan1);

            loan1.Status = LoanStatus.Live;

            _loanRepaymentFacade.PayLoan(loan1, "trans_id", 30, null, date);
            _loanRepaymentFacade.PayLoan(loan1, "trans_id", 30, null, date);
            _loanRepaymentFacade.PayLoan(loan1, "trans_id", 40, null, date);

            Assert.That(loan1.Status, Is.EqualTo(LoanStatus.PaidOff));

            Assert.That(loan1.Balance, Is.EqualTo(0));
            Assert.That(loan1.Principal, Is.EqualTo(0));
        }
        [Test]
        public void all_loan_late()
        {
            var date = new DateTime(2012, 9, 25);
            var loan1 = new Loan();
            _calculator.Calculate(1000, loan1, date);
            _customer.Loans.Add(loan1);

            var payEarlyCalc = new LoanRepaymentScheduleCalculator(loan1, new DateTime(2012, 12, 26), 0);
            var state = payEarlyCalc.GetState();
            
            Console.Write(state.AmountDue);
        }
    }
}