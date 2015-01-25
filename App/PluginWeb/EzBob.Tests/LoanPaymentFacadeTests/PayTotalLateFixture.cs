using System;
using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Models;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests.LoanPaymentFacadeTests
{
    public class PayTotalLateFixture : LoanPaymentsTestBase
    {
        private Customer _customer;

        protected override void SetUp()
        {
            _calculator = new LoanScheduleCalculator();
            _customer = new Customer();
        }

        [Test]
        public void pay_total_late_loans()
        {
            var loan1 = new Loan();
            _calculator.Calculate(1000, loan1, Parse("2012-11-16 13:52:25.000"));
            loan1.Status = LoanStatus.Live;
            
            var loan2 = new Loan();
            _calculator.Calculate(2000, loan2, Parse("2012-11-19 19:52:25.000"));
            loan2.Status = LoanStatus.Live;

            var now = Parse("2013-01-15 13:52:25.000");

            var c1 = new LoanRepaymentScheduleCalculator(loan1, now, 0);
            var lm1 = LoanModel.FromLoan(loan1, c1, c1);
            
            var c2 = new LoanRepaymentScheduleCalculator(loan2, now, 0);
            var lm2 = LoanModel.FromLoan(loan2, c2, c2);

            _customer.Loans.Add(loan1);
            _customer.Loans.Add(loan2);

            Console.WriteLine(loan1);
            Console.WriteLine(loan2);

            var lates = lm1.Late + lm2.Late;

            _facade.MakePayment("", lates, "", "totalLate", 0, _customer, now);

            Console.WriteLine(loan1);
            Console.WriteLine(loan2);

            Assert.That(loan1.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.Paid));
            Assert.That(loan2.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.Paid));
        }

        [Test]
        public void fees_are_added_to_payments()
        {
            var loanAmount = 10000; 
            
            var loan1 = new Loan();
            
            _calculator.Calculate(loanAmount, loan1, Parse("2013-02-28 14:27:14.000"));
            loan1.Status = LoanStatus.Live;

            var charge = new LoanCharge()
                             {
                                 Loan = loan1,
                                 Amount = 20,
                                 Date = Parse("2013-04-10 01:11:12.000")
                             };
            loan1.Charges.Add(charge);

            var now = Parse("2013-02-28 16:40:13.000");

			var c1 = new LoanRepaymentScheduleCalculator(loan1, now, 0);
            var lm1 = LoanModel.FromLoan(loan1, c1, c1);

            Console.WriteLine(loan1);

            Assert.That(lm1.TotalEarlyPayment, Is.EqualTo(loanAmount));
        }

        [Test]
        public void paying_loan_early__without_fees()
        {
            var loanAmount = 10000; 
            
            var loan1 = new Loan();
            
            _calculator.Calculate(loanAmount, loan1, Parse("2013-02-28 14:27:14.000"));
            loan1.Status = LoanStatus.Live;

            var charge = new LoanCharge()
                             {
                                 Loan = loan1,
                                 Amount = 20,
                                 Date = Parse("2013-04-10 01:11:12.000")
                             };
            loan1.Charges.Add(charge);

            var now = Parse("2013-02-28 16:40:13.000");

			var c1 = new LoanRepaymentScheduleCalculator(loan1, now, 0);

            _facade.PayLoan(loan1, "", 10000, "ip", now);

            c1.RecalculateSchedule();

            Console.WriteLine(loan1);
           
            Assert.That(loan1.Status, Is.EqualTo(LoanStatus.PaidOff));
            Assert.That(charge.State, Is.EqualTo("Expired"));
        }

        [Test]
        public void pay_total_late_loans_pays_only_layte_loans()
        {
            var loan1 = new Loan();
            _calculator.Calculate(100, loan1, Parse("2013-01-01 10:34:41.000"));
            loan1.Status = LoanStatus.Live;
            
            var loan2 = new Loan();
            _calculator.Calculate(2000, loan2, Parse("2012-11-19 19:52:25.000"));
            loan2.Status = LoanStatus.Live;

            var now = Parse("2013-01-15 13:52:25.000");

			var c1 = new LoanRepaymentScheduleCalculator(loan1, now, 0);
            var lm1 = LoanModel.FromLoan(loan1, c1, c1);
            
            var c2 = new LoanRepaymentScheduleCalculator(loan2, now, 0);
            var lm2 = LoanModel.FromLoan(loan2, c2, c2);

            _customer.Loans.Add(loan1);
            _customer.Loans.Add(loan2);

            Console.WriteLine(loan1);
            Console.WriteLine(loan2);

            var lates = lm2.Late;

            _facade.MakePayment("", lates, "", "totalLate", 0, _customer, now);

            Console.WriteLine(loan1);
            Console.WriteLine(loan2);

            Assert.That(loan1.Transactions.Count, Is.EqualTo(0));
            Assert.That(loan2.Transactions.Count, Is.EqualTo(1));

            Assert.That(loan1.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));
            Assert.That(loan2.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.Paid));
        }
    }
}