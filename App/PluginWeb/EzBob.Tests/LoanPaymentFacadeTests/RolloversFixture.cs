using System;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Models;
using NUnit.Framework;
using PaymentServices.Calculators;
using System.Linq;

namespace EzBob.Tests.LoanPaymentFacadeTests
{
    public class RolloversFixture : LoanPaymentsTestBase
    {
        [Test]
        [Description("Removed rollover should be ignored")]
        public void pay_after_remove_rollover()
        {
            var loanDate = Parse("2013-01-15 10:10:10.000");
            var rolloverCreateDate = Parse("2013-01-15 10:10:10.000");
            var rolloverExpiryDate = Parse("2013-01-25 10:10:10.000");

            const int rolloverAmount = 50;

            var calculator = new LoanScheduleCalculator { Interest = 0.06M };
            calculator.Calculate(1000, _loan, loanDate);

            var payEarlyCalc = new LoanRepaymentScheduleCalculator(_loan, rolloverCreateDate);
            var state = payEarlyCalc.GetState();

            var rolloverRemoved = new PaymentRollover
            {
                LoanSchedule = _loan.Schedule[0],
                Created = rolloverCreateDate,
                ExpiryDate = rolloverExpiryDate,
                Payment = rolloverAmount,
                Status = RolloverStatus.Removed
            };
            var rolloverActive = new PaymentRollover
            {
                LoanSchedule = _loan.Schedule[0],
                Created = rolloverCreateDate,
                ExpiryDate = rolloverExpiryDate,
                Payment = rolloverAmount,
                Status = RolloverStatus.New
            };
            _loan.Schedule[0].Rollovers.Add(rolloverRemoved);
            _loan.Schedule[0].Rollovers.Add(rolloverActive);

            MakePayment(394 + rolloverAmount, rolloverCreateDate);

            payEarlyCalc = new LoanRepaymentScheduleCalculator(_loan, rolloverCreateDate);

            Console.WriteLine(_loan.ToString());

            state = payEarlyCalc.GetState();
            Assert.That(state.Fees, Is.EqualTo(0));
        }

        [Test]
        public void check_fees_on_experied_rollover()
        {
            const int rolloverAmount = 50;

            var calculator = new LoanScheduleCalculator { Interest = 0.06M };
            calculator.Calculate(1000, _loan, Parse("2013-01-15 10:10:10.000"));

            var rollover = new PaymentRollover
            {
                LoanSchedule = _loan.Schedule[1],
                Created = Parse("2013-01-15 10:10:10.000"),
                ExpiryDate = Parse("2013-01-25 10:10:10.000"),
                Payment = rolloverAmount
            };
            _loan.Schedule[1].Rollovers.Add(rollover);

            var payEarlyCalc = new LoanRepaymentScheduleCalculator(_loan, Parse("2013-01-26 10:10:10.000")); //state for expired rollover
            var state = payEarlyCalc.GetState();

            Assert.That(state.Fees, Is.EqualTo(0));
        }

        [Test]
        public void check_fees_on_experied_rollover2()
        {
            const int rolloverAmount = 50;

            var calculator = new LoanScheduleCalculator { Interest = 0.06M };
            calculator.Calculate(1000, _loan, Parse("2012-12-05 00:00:00.000"));

            var rollover = new PaymentRollover
            {
                LoanSchedule = _loan.Schedule[0],
                Created = Parse("2013-01-08 13:35:59.000"),
                ExpiryDate = Parse("2013-01-09 00:00:00.000"),
                Payment = rolloverAmount
            };
            _loan.Schedule[0].Rollovers.Add(rollover);

            var payEarlyCalc = new LoanRepaymentScheduleCalculator(_loan, Parse("2013-01-11 10:10:10.000")); //state for expired rollover
            var state1 = payEarlyCalc.GetState();
            var state = payEarlyCalc.GetState();

            var model = LoanModel.FromLoan(_loan, payEarlyCalc, payEarlyCalc);

            Assert.That(state.Fees, Is.EqualTo(0));
            Assert.That(state.AmountDue, Is.EqualTo(405.61m));
            Assert.That(state.LateCharges, Is.EqualTo(0));
            Assert.That(state.Interest, Is.EqualTo(405.61m - 334m));

            Assert.That(model.Late, Is.EqualTo(405.61m));
        }

        [Test]
        public void check_fees_on_delete_rollover()
        {
            var loanDate = Parse("2013-01-15 10:10:10.000");
            var rolloverCreateDate = Parse("2013-01-15 10:10:10.000");
            var rolloverExpiryDate = Parse("2013-01-25 10:10:10.000");
            var stateDate = Parse("2013-01-20 10:10:10.000");

            const int rolloverAmount = 50;

            var calculator = new LoanScheduleCalculator { Interest = 0.06M };
            calculator.Calculate(1000, _loan, loanDate);

            var payEarlyCalc = new LoanRepaymentScheduleCalculator(_loan, stateDate);

            var rollover = new PaymentRollover
            {
                LoanSchedule = _loan.Schedule[1],
                Created = rolloverCreateDate,
                ExpiryDate = rolloverExpiryDate,
                Payment = rolloverAmount
            };
            _loan.Schedule[1].Rollovers.Add(rollover);

            var state = payEarlyCalc.GetState();
            var fees = state.Fees;

            foreach (var r in _loan.Schedule[1].Rollovers)
            {
                r.Status = RolloverStatus.Removed;
            }

            state = payEarlyCalc.GetState();
            Assert.That(state.Fees, Is.EqualTo(fees - rolloverAmount));
        }

        [Test]
        public void pay_rollover_with_two_shuffle_mouth()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M };
            calculator.Calculate(1000, _loan, new DateTime(2012, 1, 1));

            var rollover = new PaymentRollover()
            {
                LoanSchedule = _loan.Schedule[1],
                Created = new DateTime(2012, 2, 10),
                ExpiryDate = new DateTime(2012, 2, 15),
                Payment = 100,
                MounthCount = 2
            };
            _loan.Schedule[1].Rollovers.Add(rollover);

            Console.WriteLine(_loan);

            //pay installment, and rollover
            MakePayment(394 + 100, new DateTime(2012, 2, 12));

            Console.WriteLine(_loan);

            Assert.That(_loan.Schedule[1].Date.Date, Is.EqualTo(new DateTime(2012, 5, 1)));
        }

        [Test]
        public void pay_rollover_ontime()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M };
            calculator.Calculate(1000, _loan, Parse("2012-01-01 23:29:35.000"));

            var rollover = new PaymentRollover()
                            {
                                LoanSchedule = _loan.Schedule[1],
                                Created = Parse("2012-01-25 20:15:43.000"),
                                ExpiryDate = Parse("2012-02-10 20:15:43.000"),
                                Payment = 100,
                                MounthCount = 1
                            };
            _loan.Schedule[1].Rollovers.Add(rollover);

            Console.WriteLine(_loan);

            //pay installment, and rollover
            MakePayment(394 + 100, Parse("2012-02-01 10:00:04.000"));

            Console.WriteLine(_loan);

            Assert.That(_loan.Schedule[1].Date.Date, Is.EqualTo(new DateTime(2012, 4, 1)));

            //interest is calculated for 2 moths
            Assert.That(_loan.Schedule[1].Interest, Is.EqualTo(82.68M));
        }

        [Test]
        public void pay_after_expired_rollover()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M };
            calculator.Calculate(1000, _loan, Parse("2012-01-01 23:29:35.000"));

            var rollover = new PaymentRollover()
                            {
                                LoanSchedule = _loan.Schedule[1],
                                Created = Parse("2012-01-25 20:15:43.000"),
                                ExpiryDate = Parse("2012-02-10 20:15:43.000"),
                                Payment = 100                                    
                            };
            _loan.Schedule[1].Rollovers.Add(rollover);

            Console.WriteLine(_loan);

            var state = _facade.GetStateAt(_loan, Parse("2012-02-11 20:15:43.000"));

            Assert.That(state.Fees, Is.EqualTo(0));

            //pay installment, and rollover
            MakePayment(666, Parse("2012-02-11 20:15:43.000"));

            Console.WriteLine(_loan);

            Assert.That(_loan.TransactionsWithPaypoint[0].Rollover, Is.EqualTo(0));
        }

        [Test]
        public void rollover_adds_to_amount_to_pay_on_installment_date()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M };
            calculator.Calculate(1000, _loan, Parse("2012-01-01 23:29:35.000"));

            var rollover = new PaymentRollover()
                            {
                                LoanSchedule = _loan.Schedule[1],
                                Created = Parse("2012-01-25 20:15:43.000"),
                                ExpiryDate = Parse("2012-02-10 20:15:43.000"),
                                Payment = 100                                    
                            };
            _loan.Schedule[1].Rollovers.Add(rollover);

            Console.WriteLine(_loan);

            var state = _facade.GetStateAt(_loan, Parse("2012-02-01 12:29:35.000"));

            Assert.That(state.AmountDue, Is.EqualTo(394+100));

            Assert.That(rollover.Status, Is.EqualTo(RolloverStatus.New));

            Console.WriteLine(_loan);
        }

        [Test]
        public void rollover_expires()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M };
            calculator.Calculate(1000, _loan, Parse("2012-01-01 23:29:35.000"));

            var rollover = new PaymentRollover()
                            {
                                LoanSchedule = _loan.Schedule[1],
                                Created = Parse("2012-01-25 20:15:43.000"),
                                ExpiryDate = Parse("2012-02-10 20:15:43.000"),
                                Payment = 100                                    
                            };
            _loan.Schedule[1].Rollovers.Add(rollover);

            Console.WriteLine(_loan);

            var state = _facade.GetStateAt(_loan, Parse("2012-02-11 12:29:35.000"));

            Assert.That(rollover.Status, Is.EqualTo(RolloverStatus.Expired));

            Console.WriteLine(_loan);
        }

        [Test]
        public void make_paid_half_rollover()
        {
            var startDate = new DateTime(2012, 12, 26);

            var rolloverCreateDate = startDate.AddDays(35);
            var rolloverExpiryDate = rolloverCreateDate.AddDays(5);
            var payentDate = rolloverCreateDate.AddDays(4);

            var calculator = new LoanScheduleCalculator { Interest = 0.06M };
            calculator.Calculate(1000, _loan, startDate);

            var rollover = new PaymentRollover
            {
                LoanSchedule = _loan.Schedule[0],
                Created = rolloverCreateDate,
                ExpiryDate = rolloverExpiryDate,
                Payment = 50,
                Status = RolloverStatus.New
            };
            _loan.Schedule[0].Rollovers.Add(rollover);

            Console.WriteLine(_loan);

            //pay installment, and rollover
            MakePayment(100, payentDate);

            Console.WriteLine(_loan);

            //check for half paid rollover
            rollover = _loan.Schedule[0].Rollovers.FirstOrDefault(x => x.Status == RolloverStatus.New);
            Assert.That(rollover, Is.Not.Null);
            Assert.That(rollover.PaidPaymentAmount, Is.EqualTo(24.52m));
        }

        [Test]
        public void rollover_is_added_to_total_balance()
        {
            var startDate = DateTime.Now;

            var rolloverCreateDate = startDate.AddDays(35);
            var rolloverExpiryDate = rolloverCreateDate.AddDays(5);
            var payentDate = rolloverCreateDate.AddDays(4);

            var calculator = new LoanScheduleCalculator { Interest = 0.06M };
            calculator.Calculate(1000, _loan, startDate);

            var rollover = new PaymentRollover
            {
                LoanSchedule = _loan.Schedule[0],
                Created = rolloverCreateDate,
                ExpiryDate = rolloverExpiryDate,
                Payment = 50,
                Status = RolloverStatus.New
            };
            _loan.Schedule[0].Rollovers.Add(rollover);

            Console.WriteLine(_loan);

            var payment = _loan.TotalEarlyPayment(_loan.Date);

            Assert.That(payment, Is.EqualTo(1050));
        }

        [Test]
        public void loan_732_prod()
        {
            var calculator = new LoanScheduleCalculator();
            calculator.Term = 6;
            calculator.Interest = 0.06m;
            calculator.Calculate(5000, _loan, Parse("2013-04-18 09:26:54.000"));

            var rollover = new PaymentRollover
            {
                LoanSchedule = _loan.Schedule[0],
                Created = Parse("2013-05-18 15:46:54.000"),
                ExpiryDate = Parse("2013-05-21 00:00:00.000"),
                Payment = 50,
                Status = RolloverStatus.New
            };
            _loan.Schedule[0].Rollovers.Add(rollover);

            MakePayment(1262.42m, Parse("2013-05-30 00:00:00.000"));
            MakePayment(1088.77m, Parse("2013-06-26 07:04:11.000"));

            var state = GetStateAt(_loan, Parse("2013-06-26 07:04:11.000"));

            Assert.That(state.LoanRepayment, Is.EqualTo(0));

            Console.WriteLine(_loan);
        }
    }
}
