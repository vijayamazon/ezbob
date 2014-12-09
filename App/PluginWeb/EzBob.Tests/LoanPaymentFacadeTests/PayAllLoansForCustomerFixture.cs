using System;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests.LoanPaymentFacadeTests
{
    [TestFixture]
    public class PayAllLoansForCustomerFixture
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
        public void pay_all_loans_early()
        {

            var date = new DateTime(2012, 1, 1);

            var loan1 = new Loan();
            _calculator.Calculate(1000, loan1, date);

            var loan2 = new Loan();
            _calculator.Calculate(3000, loan2, date);

            _customer.Loans.Add(loan1);
            _customer.Loans.Add(loan2);

            loan1.Status = LoanStatus.Live;
            loan2.Status = LoanStatus.Live;

            var amount = _customer.TotalEarlyPayment(date);

            _loanRepaymentFacade.PayAllLoansForCustomer(_customer, amount, null, date);

            Assert.That(loan1.Status, Is.EqualTo(LoanStatus.PaidOff));
            Assert.That(loan2.Status, Is.EqualTo(LoanStatus.PaidOff));

            Assert.That(loan1.Balance, Is.EqualTo(0));
            Assert.That(loan2.Balance, Is.EqualTo(0));

            Assert.That(loan1.Repayments, Is.EqualTo(1000));
            Assert.That(loan2.Repayments, Is.EqualTo(3000));

            Assert.That(loan1.TransactionsWithPaypoint.Count, Is.EqualTo(1));
            Assert.That(loan2.TransactionsWithPaypoint.Count, Is.EqualTo(1));
        }

        [Test]
        public void pay_all_loans_early2()
        {

            var date = new DateTime(2012, 1, 1);

            var loan1 = new Loan();
            _calculator.Calculate(1000, loan1, date);

            var loan2 = new Loan();
            _calculator.Calculate(3000, loan2, date);

            _customer.Loans.Add(loan1);
            _customer.Loans.Add(loan2);

            loan1.Status = LoanStatus.Live;
            loan2.Status = LoanStatus.Live;

            var amount = _customer.TotalEarlyPayment(date);

            _loanRepaymentFacade.MakePayment("transid", amount, "10:10:10:10", "total", 0, _customer, date);

            Assert.That(amount, Is.EqualTo(4000));

            Assert.That(loan1.Status, Is.EqualTo(LoanStatus.PaidOff));
            Assert.That(loan2.Status, Is.EqualTo(LoanStatus.PaidOff));

            Assert.That(loan1.Balance, Is.EqualTo(0));
            Assert.That(loan2.Balance, Is.EqualTo(0));

            Assert.That(loan1.Repayments, Is.EqualTo(1000));
            Assert.That(loan2.Repayments, Is.EqualTo(3000));

            Assert.That(loan1.TransactionsWithPaypoint.Count, Is.EqualTo(1));
            Assert.That(loan2.TransactionsWithPaypoint.Count, Is.EqualTo(1));
        }

        [Test]
        public void pay_loan_creates_transaction()
        {

            var date = new DateTime(2012, 1, 1);

            var loan1 = new Loan();
            _calculator.Calculate(1000, loan1, date);

            _customer.Loans.Add(loan1);            

            loan1.Status = LoanStatus.Live;            

            var amount = _customer.TotalEarlyPayment(date);

            _loanRepaymentFacade.PayAllLoansForCustomer(_customer, amount, null, date);

            Assert.That(loan1.Transactions, Is.Not.Empty);

            Assert.That(loan1.Transactions.Count, Is.EqualTo(1));

            Assert.That(loan1.Transactions.First().Amount, Is.EqualTo(amount));
        }

        [Test]
        public void pay_early_change_loan_schedule_status_to_paidontime()
        {
            var date = new DateTime(2012, 1, 1);

            var loan1 = new Loan();
            _calculator.Calculate(100, loan1, date);

            _customer.Loans.Add(loan1);

            loan1.Status = LoanStatus.Live;

            _loanRepaymentFacade.PayAllLoansForCustomer(_customer, 92, null, date);

            Assert.That(loan1.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.PaidEarly));
            Assert.That(loan1.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.PaidEarly));
        }

        [Test]
        public void next_loan_early_payment()
        {

            var date = new DateTime(2012, 1, 1);

            var loan1 = new Loan();
            _calculator.Calculate(100, loan1, date);

            _customer.Loans.Add(loan1);            

            loan1.Status = LoanStatus.Live;            

            _loanRepaymentFacade.PayAllLoansForCustomer(_customer, 92, null, date);

            var totalAmount = _customer.TotalEarlyPayment(date);
            var nextAmount = loan1.NextEarlyPayment(date);

            Assert.That(totalAmount, Is.EqualTo(8));
            Assert.That(nextAmount, Is.EqualTo(8));
        }

        [Test]
        public void pays_small_amount()
        {
            var date = new DateTime(2012, 1, 1);

            var loan1 = new Loan();
            _calculator.Calculate(1000, loan1, date);

            var loan2 = new Loan();
            _calculator.Calculate(1500, loan2, date);

            _customer.Loans.Add(loan1);
            _customer.Loans.Add(loan2);

            loan1.Status = LoanStatus.Live;
            loan2.Status = LoanStatus.Live;

            var amount = _customer.TotalEarlyPayment(date) / 3;

            _loanRepaymentFacade.PayAllLoansForCustomer(_customer, amount, null, date);

            Assert.That(loan1.Status, Is.EqualTo(LoanStatus.Live));
            Assert.That(loan2.Status, Is.EqualTo(LoanStatus.Live));

            Assert.That(loan1.Balance, Is.Not.EqualTo(0));
            Assert.That(loan2.Balance, Is.Not.EqualTo(0));
        }
    }
}
