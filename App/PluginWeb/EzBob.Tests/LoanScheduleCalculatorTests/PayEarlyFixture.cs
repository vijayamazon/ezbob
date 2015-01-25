using System;
using EZBob.DatabaseLib.Model.Database.Loans;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests
{
    [TestFixture]
    public class PayEarlyFixture
    {
        private LoanScheduleCalculator _calculator;
        private DateTime _date = new DateTime(2012, 1, 1);

        [SetUp]
        public void SetUp()
        {
            _calculator = new LoanScheduleCalculator();
        }

        [Test]
        public void recalculates_schedule()
        {
            var loan = new Loan();
            loan.Date = _date;
            _calculator.Calculate(3000, loan, loan.Date);

            Console.WriteLine(loan);

            MakeEarlyPayment(loan, new DateTime(2012, 1, 15), 1500);
           
            Assert.That(loan.Schedule[0].AmountDue, Is.EqualTo(0));
            Assert.That(loan.Schedule[0].Interest, Is.EqualTo(0));
            Assert.That(loan.Schedule[0].LoanRepayment, Is.EqualTo(0));

            Assert.That(loan.Schedule[1].AmountDue, Is.EqualTo(728.2m));
            Assert.That(loan.Schedule[1].Interest, Is.EqualTo(146.91m));
            Assert.That(loan.Schedule[1].LoanRepayment, Is.EqualTo(581.29));

            Assert.That(loan.Schedule[2].AmountDue, Is.EqualTo(1060));
            Assert.That(loan.Schedule[2].Interest, Is.EqualTo(60));
            Assert.That(loan.Schedule[2].LoanRepayment, Is.EqualTo(1000));
        }

        [Test]
        public void recalculates_schedule_after_payment_on_the_same_day()
        {
            var loan = new Loan();
            loan.Date = _date;
            _calculator.Calculate(3000, loan, loan.Date);

            MakeEarlyPayment(loan, _date, 1000);
            
            Assert.That(loan.Schedule[0].LoanRepayment, Is.EqualTo(0));
            Assert.That(loan.Schedule[1].LoanRepayment, Is.EqualTo(1000));
            Assert.That(loan.Schedule[2].LoanRepayment, Is.EqualTo(1000));
            
            Assert.That(loan.Schedule[0].AmountDue, Is.EqualTo(0));
            Assert.That(loan.Schedule[1].AmountDue, Is.EqualTo(1240));
            Assert.That(loan.Schedule[2].AmountDue, Is.EqualTo(1060));
        }

        [Test]
        public void recalculates_schedule_small_amount()
        {
            var loan = new Loan();
            loan.Date = _date;
            _calculator.Calculate(3000, loan, loan.Date);

            MakeEarlyPayment(loan, new DateTime(2012, 1, 15), 150);
           
            Assert.That(loan.Schedule[0].AmountDue, Is.EqualTo(1027.74m));
            Assert.That(loan.Schedule[1].AmountDue, Is.EqualTo(1120));
            Assert.That(loan.Schedule[2].AmountDue, Is.EqualTo(1060));
            
        }

        [Test]
        public void pay_early_changes_loan_balance()
        {
            var loan = new Loan();
            loan.Date = _date;
            _calculator.Calculate(3000, loan, loan.Date);

            MakeEarlyPayment(loan, new DateTime(2012, 1, 15), 150);

            Assert.That(loan.Principal, Is.EqualTo(2931.29));
        }

        [Test]
        public void pay_early_changes_loan_balance_today()
        {
            var loan = new Loan();
            loan.Date = _date;
            _calculator.Calculate(100, loan, loan.Date);

            MakeEarlyPayment(loan, loan.Date, 30);

            Assert.That(loan.Principal, Is.EqualTo(70));
        }

        [Test]
        [TestCase(100)]
        [TestCase(2300)]
        [TestCase(3000)]
        public void recalculates_schedule_all_amount(int amount)
        {
            var loan = new Loan();
            loan.Date = _date;
            loan.Interest = 0.06M;
            _calculator.Calculate(amount, loan, loan.Date);

            MakeEarlyPayment(loan, _date, amount);

            //Assert.That(savings, Is.EqualTo(1M));
            
            Assert.That(loan.Schedule[0].AmountDue, Is.EqualTo(0));
            Assert.That(loan.Schedule[1].AmountDue, Is.EqualTo(0));
            Assert.That(loan.Schedule[2].AmountDue, Is.EqualTo(0));

            Assert.That(loan.Balance, Is.EqualTo(0));

            Assert.That(loan.TotalEarlyPayment(_date), Is.EqualTo(0));
        }

        [Test]
        public void paying_ontime_is_possible_with_early_payment()
        {
            var loan = new Loan();
            loan.Date = _date;
            _calculator.Calculate(1000, loan, loan.Date);

            Console.WriteLine(loan);

            MakeEarlyPayment(loan, new DateTime(2012, 2, 1), 400);

            Assert.That(loan.Schedule[0].AmountDue, Is.EqualTo(0));
            Assert.That(loan.Schedule[1].AmountDue, Is.EqualTo(366.6));
            Assert.That(loan.Schedule[2].AmountDue, Is.EqualTo(352.98));
        }

        [Test]
        public void paying_ontime_is_possible_with_early_payment_after_several_minutes()
        {
            var loan = new Loan();
            loan.Date = _date;
            _calculator.Calculate(1000, loan, loan.Date);

            MakeEarlyPayment(loan, loan.Schedule[0].Date.AddSeconds(10), 400);

            Assert.That(loan.Schedule[0].AmountDue, Is.EqualTo(0));
            Assert.That(loan.Schedule[1].AmountDue, Is.EqualTo(366.6m));
            Assert.That(loan.Schedule[2].AmountDue, Is.EqualTo(352.98m));
        }

        [Test]
        public void paying_ontime_is_possible_with_early_payment_even_if_it_is_marked_as_late_by_strategy()
        {
            var loan = new Loan();
            loan.Date = _date;
            _calculator.Calculate(1000, loan, loan.Date);

            loan.Schedule[0].Status = LoanScheduleStatus.Late;

            MakeEarlyPayment(loan, new DateTime(2012, 2, 1), 400);

            Assert.That(loan.Schedule[0].AmountDue, Is.EqualTo(0));
            Assert.That(loan.Schedule[1].AmountDue, Is.EqualTo(366.6));
            Assert.That(loan.Schedule[2].AmountDue, Is.EqualTo(352.98));
        }

        [Test]
        public void pay_early_three_times_before_installment()
        {
            var loan = new Loan(){Date = new DateTime(2012, 10, 1)};
            _calculator.Calculate(3000, loan, loan.Date);

            Console.WriteLine(loan.ToString());

            MakeEarlyPayment(loan, new DateTime(2012, 10, 10), 200);
            MakeEarlyPayment(loan, new DateTime(2012, 10, 15), 300);
            MakeEarlyPayment(loan, new DateTime(2012, 10, 25), 400);
        }

        [Test]
        public void pay_early_three_times_before_installment_with_payments_smaller_then_interest()
        {
            var loan = new Loan(){Date = new DateTime(2012, 10, 1)};
            _calculator.Calculate(3000, loan, loan.Date);

            Console.WriteLine(loan.ToString());

            MakeEarlyPayment(loan, new DateTime(2012, 10, 10), 10);
            Assert.That(loan.Schedule[0].Interest, Is.EqualTo(170));

            MakeEarlyPayment(loan, new DateTime(2012, 10, 15), 10);
            Assert.That(loan.Schedule[0].Interest, Is.EqualTo(160));

            MakeEarlyPayment(loan, new DateTime(2012, 10, 25), 10);
            Assert.That(loan.Schedule[0].Interest, Is.EqualTo(150));
        }

        [Test]
        public void four_early_payments()
        {
            var loan = new Loan(){Date = new DateTime(2012, 11, 18)};
            _calculator.Term = 6;
            _calculator.Interest = 0.07M;
            _calculator.Calculate(6300, loan, loan.Date);

            Console.WriteLine(loan.ToString());

            MakeEarlyPayment(loan, new DateTime(2012, 11, 23), 670);
            MakeEarlyPayment(loan, new DateTime(2012, 11, 24), 521);
            MakeEarlyPayment(loan, new DateTime(2012, 11, 24), 500);
            MakeEarlyPayment(loan, new DateTime(2012, 11, 25), 238);

            Assert.That(loan.Schedule[0].AmountDue, Is.EqualTo(0));
            Assert.That(loan.Schedule[1].AmountDue, Is.EqualTo(821.4m));

        }

        private decimal MakeEarlyPayment(Loan loan, DateTime date, int amount)
        {
            Console.WriteLine("Making payment - Date: {0}, Amount: {1}", date.ToString("dd MM yyyy"), amount);
            loan.Transactions.Add(new PaypointTransaction() { Amount = amount, PostDate = date, Status = LoanTransactionStatus.Done });
			var c = new LoanRepaymentScheduleCalculator(loan, date, 0);
            var s = c.RecalculateSchedule();
            Console.WriteLine(loan.ToString());
            return s;
        }
    }
}