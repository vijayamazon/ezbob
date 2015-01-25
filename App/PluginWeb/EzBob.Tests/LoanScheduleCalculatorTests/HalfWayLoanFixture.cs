using System;
using System.Globalization;
using System.Linq;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Models;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests
{
    [TestFixture]
    public class HalfWayLoanFixture
    {
        private LoanScheduleCalculator _calculator;
        private LoanPaymentFacade _facade;

        [SetUp]
        public void SetUp()
        {
            _calculator = new LoanScheduleCalculator();
            _facade = new LoanPaymentFacade();
        }

        [Test]
        public void can_generate_half_way_loan_schedule()
        {
            var loan = new Loan();
            loan.LoanType = new HalfWayLoanType();

            _calculator.Term = 5;

            var schedule = _calculator.Calculate(3000m, loan);

            System.Console.WriteLine(loan);

            Assert.That(schedule.Count, Is.EqualTo(5));

            Assert.That(schedule[0].AmountDue, Is.EqualTo(180));
            Assert.That(schedule[0].Balance, Is.EqualTo(3000));
            Assert.That(schedule[0].Interest, Is.EqualTo(180));
            Assert.That(schedule[0].LoanRepayment, Is.EqualTo(0));

            Assert.That(schedule[1].AmountDue, Is.EqualTo(180));
            Assert.That(schedule[1].Balance, Is.EqualTo(3000));
            Assert.That(schedule[1].Interest, Is.EqualTo(180));
            Assert.That(schedule[1].LoanRepayment, Is.EqualTo(0));

            Assert.That(schedule[2].AmountDue, Is.EqualTo(1180));
            Assert.That(schedule[2].Balance, Is.EqualTo(2000));
            Assert.That(schedule[2].Interest, Is.EqualTo(180));
            Assert.That(schedule[2].LoanRepayment, Is.EqualTo(1000));

            Assert.That(schedule[3].AmountDue, Is.EqualTo(1120));
            Assert.That(schedule[3].Balance, Is.EqualTo(1000));
            Assert.That(schedule[3].Interest, Is.EqualTo(120));
            Assert.That(schedule[3].LoanRepayment, Is.EqualTo(1000));

            Assert.That(schedule[4].AmountDue, Is.EqualTo(1060));
            Assert.That(schedule[4].Balance, Is.EqualTo(0));
            Assert.That(schedule[4].Interest, Is.EqualTo(60));
            Assert.That(schedule[4].LoanRepayment, Is.EqualTo(1000));
        }

        [Test]
        public void half_way_balances()
        {
            var lt = new HalfWayLoanType();
            var balances = lt.GetBalances(1000, 3).ToArray();

            Assert.That(balances.Length, Is.EqualTo(3));

            Assert.That(balances[0], Is.EqualTo(1000));
            Assert.That(balances[1], Is.EqualTo(500));
            Assert.That(balances[2], Is.EqualTo(0));
        }

        [Test]
        public void half_way_balances_even()
        {
            var lt = new HalfWayLoanType();
            var balances = lt.GetBalances(1000, 4).ToArray();

            Assert.That(balances.Length, Is.EqualTo(4));

            Assert.That(balances[0], Is.EqualTo(1000));
            Assert.That(balances[1], Is.EqualTo(1000));
            Assert.That(balances[2], Is.EqualTo(500));
            Assert.That(balances[3], Is.EqualTo(0));
        }

        [Test]
        public void standard_balances()
        {
            var lt = new StandardLoanType();
            var balances = lt.GetBalances(1000, 3).ToArray();

            Assert.That(balances.Length, Is.EqualTo(3));

            Assert.That(balances[0], Is.EqualTo(666));
            Assert.That(balances[1], Is.EqualTo(333));
            Assert.That(balances[2], Is.EqualTo(0));
        }

        [Test]
        public void build_model_for_halfway_loan()
        {
            var now = new DateTime(2013, 01, 01);

            var type = new HalfWayLoanType();            
            var loan = new Loan(){LoanType = type, LoanAmount = 1000, Date = now};
            var schedule = _calculator.Calculate(1000m, loan, now);

            Console.WriteLine(loan);

			var pc = new LoanRepaymentScheduleCalculator(loan, now, 0);

            var model = LoanModel.FromLoan(loan, pc);

            Console.WriteLine(loan);

            Assert.That(schedule[0].Interest, Is.EqualTo(60));
            Assert.That(schedule[1].Interest, Is.EqualTo(60));
        }

        [Test]
        public void next_early_payment_for_halfway_loan()
        {
            var now = new DateTime(2013, 01, 01);

            var type = new HalfWayLoanType();            
            var loan = new Loan(){LoanType = type, LoanAmount = 1000, Date = now};
            var schedule = _calculator.Calculate(1000m, loan, now);

            Console.WriteLine(loan);

			var pc = new LoanRepaymentScheduleCalculator(loan, now, 0);

            var ep = pc.NextEarlyPayment();

            Assert.That(ep, Is.EqualTo(500));
        }

        [Test]
        public void pay_all_installments_ontime()
        {
            var loan = new Loan(){Date = new DateTime(2012, 01, 01)};
            loan.LoanType = new HalfWayLoanType();

            _calculator.Term = 5;

            _calculator.Calculate(3000m, loan, new DateTime(2012, 01, 01));

            Console.WriteLine(loan);

            MakePayment(loan, 180, new DateTime(2012, 02, 01));
            MakePayment(loan, 180, new DateTime(2012, 03, 01));
            MakePayment(loan, 1180, new DateTime(2012, 04, 01));
            MakePayment(loan, 1120, new DateTime(2012, 05, 01));
            MakePayment(loan, 1060, new DateTime(2012, 06, 01));

            Assert.That(loan.Schedule[0].Interest, Is.EqualTo(0));
            Assert.That(loan.Schedule[1].Interest, Is.EqualTo(0));
            Assert.That(loan.Schedule[2].Interest, Is.EqualTo(0));
            Assert.That(loan.Schedule[3].Interest, Is.EqualTo(0));
            Assert.That(loan.Schedule[4].Interest, Is.EqualTo(0));

            Assert.That(loan.Schedule[0].LoanRepayment, Is.EqualTo(0));
            Assert.That(loan.Schedule[1].LoanRepayment, Is.EqualTo(0));
            Assert.That(loan.Schedule[2].LoanRepayment, Is.EqualTo(0));
            Assert.That(loan.Schedule[3].LoanRepayment, Is.EqualTo(0));
            Assert.That(loan.Schedule[4].LoanRepayment, Is.EqualTo(0));

            Assert.That(loan.Schedule[0].AmountDue, Is.EqualTo(0));
            Assert.That(loan.Schedule[1].AmountDue, Is.EqualTo(0));
            Assert.That(loan.Schedule[2].AmountDue, Is.EqualTo(0));
            Assert.That(loan.Schedule[3].AmountDue, Is.EqualTo(0));
            Assert.That(loan.Schedule[4].AmountDue, Is.EqualTo(0));
        }

        [Test]
        public void interest_only_payments_can_be_late()
        {
            var now = Parse("2012-10-27 00:00:00.000");

            var type = new HalfWayLoanType();
            var loan = new Loan() { LoanType = type, LoanAmount = 1000, Date = now };
            _calculator.Calculate(1000m, loan, now);

			var pc = new LoanRepaymentScheduleCalculator(loan, Parse("2013-01-16 00:00:00.000"), 0);
            var ep = pc.NextEarlyPayment();

            Console.WriteLine(loan);

            Assert.That(loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.Late));
            Assert.That(loan.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.Late));
            Assert.That(loan.Schedule[2].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));
        }

        [Test]
        public void paying_late()
        {
            var now = Parse("2012-10-27 00:00:00.000");

            var type = new HalfWayLoanType();
            var loan = new Loan() { LoanType = type, LoanAmount = 1000, Date = now };
            _calculator.Calculate(1000m, loan, now);

            Console.WriteLine(loan);

            MakePayment(loan, 658.71m, Parse("2013-01-16 14:35:25.000"));

            Assert.That(loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.Paid));
            Assert.That(loan.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.Paid));
            Assert.That(loan.Schedule[2].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));
        }

        [Test]
        public void pay_early_376()
        {
            var loan = new Loan() { Date = Parse("2013-01-18 08:54:55.000") };
            loan.LoanType = new HalfWayLoanType();

            _calculator.Term = 6;

            _calculator.Calculate(1000m, loan, loan.Date);

            Console.WriteLine(loan);

            MakePayment(loan, 334, Parse("2013-01-18 08:58:01.000"));

            Assert.That(loan.Schedule[3].LoanRepayment, Is.EqualTo(0));
        }

        private void MakePayment(Loan loan, decimal amount, DateTime date)
        {
            Console.WriteLine("Making payment {0} on {1}", amount, date);
            _facade.PayLoan(loan, "", amount, "", date);
            Console.WriteLine(loan);
        }

        private static DateTime Parse(string date)
        {
            return DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss.000", CultureInfo.InvariantCulture);
        }

    }
}
