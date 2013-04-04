using System;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests.LoanPaymentFacadeTests
{
    public class MakePaymentFixture : LoanPaymentsTestBase
    {
        private decimal _takenMoney = 3000;
        private DateTime _startDate;
        private Customer _customer;

        protected override void SetUp()
        {
            _loan.Id = 1;
            _loan.Status = LoanStatus.Live;

            var calculator = new LoanScheduleCalculator();
            _startDate = new DateTime(2012, 1, 1);
            calculator.Calculate(_takenMoney, _loan, _startDate);
            _customer = new Customer();
            _customer.Loans.Add(_loan);
            _loan.Customer = _customer;
        }

        [Test]
        public void can_calculate_saved_on_paying_one_loan()
        {
            var result = _facade.MakePayment("transaction", 1000, "", "loan", _loan.Id, _customer, _startDate);
            
            //Assert.That(result.Saved, Is.EqualTo(15M));
            Assert.That(result.PaymentAmount, Is.EqualTo(1000));
        }

        [Test]
        [Ignore]
        public void can_calculate_saved_on_paying_all_loans()
        {

            var loan2 = new Loan() { Id = 2, Status = LoanStatus.Live };
            _customer.Loans.Add(loan2);
            loan2.Customer = _customer;
            var calculator = new LoanScheduleCalculator();
            calculator.Calculate(_takenMoney, loan2, _startDate);

            var result = _facade.MakePayment("transaction", 1000, "", "total", _loan.Id, _customer, _startDate);
            
            //Assert.That(result.Saved, Is.EqualTo(23M));
            Assert.That(result.PaymentAmount, Is.EqualTo(1000));
        }

        [Test]
        public void zero_interest_loan()
        {
            var customer = new Customer();
            var loan = new Loan() { Id = 1, Status = LoanStatus.Live };
            var calculator = new LoanScheduleCalculator();
            calculator.Interest = 0;
            calculator.Calculate(23423, loan, _startDate);
            customer.Loans.Add(loan);
            var result = _facade.MakePayment("transaction", 1000, "", "total", loan.Id, customer, _startDate);
            Assert.That(result.Saved, Is.EqualTo(0));
        }

        [Test]
        public void after_paying_first_installment_total_pay_early_amount_is_calculated()
        {
            var a = _loan.TotalEarlyPayment(_startDate);

            var result = _facade.MakePayment("transaction", 1000, "", "loan", _loan.Id, _customer, _startDate);

            var b = _loan.TotalEarlyPayment(_startDate);

            Assert.That(a, Is.EqualTo(3000));
            Assert.That(b, Is.EqualTo(2000));

        }

        [Test]
        public void saving_after_second_payment()
        {
            PayFastResult result;

            Console.WriteLine(_loan);

            result = _facade.MakePayment("transaction", 1000, "", "loan", _loan.Id, _customer, _startDate);
            Console.WriteLine(_loan); 
            
            Assert.That(result.Saved, Is.GreaterThan(0));
            //Assert.That(result.Saved, Is.EqualTo(15));            

            result = _facade.MakePayment("transaction", 1000, "", "loan", _loan.Id, _customer, _startDate);
            Assert.That(result.Saved, Is.GreaterThan(0));
            //Assert.That(result.Saved, Is.EqualTo(39));

            result = _facade.MakePayment("transaction", 1000, "", "loan", _loan.Id, _customer, _startDate);
            Assert.That(result.Saved, Is.GreaterThan(0));
            //Assert.That(result.Saved, Is.EqualTo(63));
        }

        [Test]
        public void paying_early_add_repayments_to_loan()
        {
            _facade.MakePayment("transaction", 1000, "", "loan", _loan.Id, _customer, _startDate);

            Assert.That(_loan.Repayments, Is.EqualTo(1000));
            Assert.That(_loan.RepaymentsNum, Is.EqualTo(1));

            _facade.MakePayment("transaction", 1000, "", "loan", _loan.Id, _customer, _startDate);

            Assert.That(_loan.Repayments, Is.EqualTo(2000));
            Assert.That(_loan.RepaymentsNum, Is.EqualTo(2));

            _facade.MakePayment("transaction", 1000, "", "loan", _loan.Id, _customer, _startDate);

            Assert.That(_loan.Repayments, Is.EqualTo(3000));
            Assert.That(_loan.RepaymentsNum, Is.EqualTo(3));
        }

        [Test]
        public void pay_loan_from_one_time()
        {
            PayFastResult result;

            result = _facade.MakePayment("transaction", 3000, "", "loan", _loan.Id, _customer, _startDate);

            Assert.That(result.Saved, Is.EqualTo(100));

            Assert.That(_loan.Status, Is.EqualTo(LoanStatus.PaidOff));

            Assert.That(_loan.Schedule.All(s => s.AmountDue == 0), Is.True);
            Assert.That(_loan.Schedule.All(s => s.Interest == 0), Is.True);
            Assert.That(_loan.Schedule.All(s => s.Status == LoanScheduleStatus.PaidEarly), Is.True);
            
        }

        [Test]
        public void pay_first_scheduled_balance_on_first_day_closes_scheduled_item()
        {
            _facade.MakePayment("transaction", 1000, "", "loan", _loan.Id, _customer, _startDate);

            Assert.That(_loan.Schedule[0].AmountDue, Is.EqualTo(0));
            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.PaidEarly));
        }

        [Test]
        public void pay_loan_from_three_times()
        {
            PayFastResult result;

            result = _facade.MakePayment("transaction", 1000, "", "loan", _loan.Id, _customer, _startDate);            
            result = _facade.MakePayment("transaction", 1000, "", "loan", _loan.Id, _customer, _startDate);
            result = _facade.MakePayment("transaction", 1000, "", "loan", _loan.Id, _customer, _startDate);

            Assert.That(_loan.Status, Is.EqualTo(LoanStatus.PaidOff));

            Assert.That(_loan.Schedule.All(s => s.AmountDue == 0), Is.True);
            Assert.That(_loan.Schedule.All(s => s.Interest == 0), Is.True);
            Assert.That(_loan.Schedule.All(s => s.Status == LoanScheduleStatus.PaidEarly), Is.True);            
        }

        [Test]
        public void paying_early_calculates_loan_repayment()
        {
            _facade.MakePayment("transaction", 1000, "", "loan", _loan.Id, _customer, _startDate);

            var tran = (PaypointTransaction)_loan.Transactions.First();

            Assert.That(tran.LoanRepayment, Is.EqualTo(1000));
        }

        [Test]
        public void paying_early_calculates_loan_interest_for_transaction()
        {
            _facade.MakePayment("transaction", 1000, "", "loan", _loan.Id, _customer, _startDate.AddDays(25));

            var tran = (PaypointTransaction)_loan.Transactions.First();

            Assert.That(tran.Interest, Is.EqualTo(145.16));
            Assert.That(tran.LoanRepayment, Is.EqualTo(854.84));
        }

        [Test]
        public void paying_early_calculates_loan_interest_for_installment_and_loan()
        {
            _facade.MakePayment("transaction", 1000, "", "loan", _loan.Id, _customer, _startDate.AddDays(25));

            Assert.That(_loan.Schedule[0].Interest, Is.EqualTo(24.91));
            Assert.That(_loan.Interest, Is.EqualTo(204.91));
        }
    }
}