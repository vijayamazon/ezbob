using System;
using System.Linq;
using EZBob.DatabaseLib.Model.Database.Loans;
using EZBob.DatabaseLib.Model.Loans;
using EzBob.Models;
using NUnit.Framework;
using PaymentServices.Calculators;

namespace EzBob.Tests.LoanPaymentFacadeTests
{

    public class NegativeInterestInSchedule : LoanPaymentsTestBase
    {
        [Test]
        [Description("Modeling loan case 37")]
        public void loan_37()
        {
            var calculator = new LoanScheduleCalculator();
            calculator.Calculate(1000, _loan, new DateTime(2012, 9, 18, 2, 11, 45));

            Console.WriteLine(_loan);

            MakePayment(400, new DateTime(2012, 10, 18, 10, 58, 57));
            MakePayment(380, new DateTime(2012, 11, 19, 02, 38, 35));

            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.PaidOnTime));
            Assert.That(_loan.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.Paid));
            Assert.That(_loan.Schedule[2].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));

            Assert.That(_loan.DateClosed, Is.Null);

            Assert.That(_loan.Schedule.All(i => i.Interest >= 0), Is.True);

            Assert.That(_loan.OnTimeNum, Is.EqualTo(1));
        }

        [Test]
        [Description("Modeling loan case 38")]
        [Ignore()]
        public void loan_38()
        {
            var calculator = new LoanScheduleCalculator();
            calculator.Calculate(1500, _loan, new DateTime(2012, 09, 18, 09, 57, 26));

            Console.WriteLine(_loan);

            //MakePayment(590, new DateTime(2012, 10, 23)); //late payment
            MakePayment(590, new DateTime(2012, 10, 18));
            MakePayment(560, new DateTime(2012, 11, 18, 20, 00, 22));

            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.PaidOnTime));
            Assert.That(_loan.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.PaidOnTime));
            Assert.That(_loan.Schedule[2].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));

            Assert.That(_loan.DateClosed, Is.Null);

            Assert.That(_loan.Schedule.All(i => i.Interest >= 0), Is.True);
        }

        [Test]
        [Description("Modeling loan case 37")]
        public void paying_small_amount_after_second_installment_make_it_late()
        {
            var calculator = new LoanScheduleCalculator();
            calculator.Calculate(1000, _loan, new DateTime(2012, 9, 18, 2, 11, 45));

            Console.WriteLine(_loan);

            MakePayment(400, new DateTime(2012, 10, 18, 10, 58, 57));
            MakePayment(1,   new DateTime(2012, 11, 19, 02, 38, 35));

            Assert.That(_loan.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.Late));
            Assert.That(_loan.DateClosed, Is.Null);
            Assert.That(_loan.Status, Is.EqualTo(LoanStatus.Late));
        }

        [Test]
        [Description("Modeling loan case 36")]
        public void loan_36()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.05M};
            calculator.Calculate(3500, _loan, new DateTime(2012, 09, 19, 17, 33, 21));

            Console.WriteLine(_loan);

            MakePayment(1343,    new DateTime(2012, 10, 19, 19, 00, 26));
            MakePayment(1282.6M, new DateTime(2012, 11, 19, 20, 07, 35));

            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.PaidOnTime));
            Assert.That(_loan.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.PaidOnTime));
            Assert.That(_loan.Schedule[2].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));

            Assert.That(_loan.DateClosed, Is.Null);
        }

        [Test]
        [Description("Modeling loan case 39")]
        public void loan_39()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.06M};
            calculator.Calculate(600, _loan, new DateTime(2012, 09, 19, 10, 02, 54));

            Console.WriteLine(_loan);

            MakePayment(231,    new DateTime(2012, 10, 15, 06, 31, 10));
            MakePayment(208M, new DateTime(2012, 10, 26, 12, 41, 06));

            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.Paid));
            Assert.That(_loan.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));
            Assert.That(_loan.Schedule[2].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));

            Assert.That(_loan.DateClosed, Is.Null);
        }

        [Test]
        [Description("Modeling loan case 40")]
        public void loan_40()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.06M};
            calculator.Calculate(300, _loan, new DateTime(2012, 09, 20, 07, 53, 45));

            Console.WriteLine(_loan);

            MakePayment(118, new DateTime(2012, 10, 20, 19, 00, 24));
            MakePayment(112, new DateTime(2012, 11, 20, 08, 00, 28));

            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.PaidOnTime));
            Assert.That(_loan.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.PaidOnTime));
            Assert.That(_loan.Schedule[2].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));

            Assert.That(_loan.DateClosed, Is.Null);
        }

        [Test]
        [Description("Modeling loan case 41")]
        public void loan_41()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.05M};
            calculator.Calculate(18000, _loan, new DateTime(2012, 09, 20, 11, 00, 10));

            Console.WriteLine(_loan);

            MakePayment(6780, new DateTime(2012, 10, 16, 11, 09, 35));

            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.PaidEarly));
            Assert.That(_loan.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));
            Assert.That(_loan.Schedule[2].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));

            Assert.That(_loan.DateClosed, Is.Null);

            MakePayment(12665, new DateTime(2012, 11, 19, 09, 04, 18));

            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.PaidEarly));
            Assert.That(_loan.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.PaidEarly));
            Assert.That(_loan.Schedule[2].Status, Is.EqualTo(LoanScheduleStatus.PaidEarly));

            Assert.That(_loan.DateClosed, Is.Not.Null);
        }

        [Test]
        [Description("Modeling loan case 147")]
        public void loan_147()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.07M};
            calculator.Calculate(400, _loan, new DateTime(2012, 11, 24, 19, 57, 02));

            Console.WriteLine(_loan);

            MakePayment(100, new DateTime(2012, 12, 02, 10, 0, 0));
            MakePayment(100, new DateTime(2012, 12, 02, 11, 0, 0));

            Assert.That(_loan.Schedule[1].Interest, Is.EqualTo(25.17m));
            Assert.That(_loan.Schedule[1].LoanRepayment, Is.EqualTo(74.47m));

            MakePayment(100, new DateTime(2012, 12, 02, 12, 0, 0));
            MakePayment(55,  new DateTime(2012, 12, 02, 13, 0, 0));

            Assert.That(_loan.TransactionsWithPaypointSuccesefull[0].Interest, Is.EqualTo(7.47m));

            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.PaidEarly));
            Assert.That(_loan.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.PaidEarly));
            Assert.That(_loan.Schedule[2].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));

            Assert.That(_loan.DateClosed, Is.Null);
        }

        [Test]
        [Description("Modeling loan case 67")]
        public void loan_67()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.06M};
            calculator.Calculate(2000, _loan, Parse("2012-10-15 19:18:25.000"));

            Console.WriteLine(_loan);

            MakePayment(240, Parse("2012-11-09 13:04:40.000"));
            MakePayment(550, Parse("2012-11-13 22:23:49.000"));
            MakePayment(100, Parse("2012-11-14 19:33:30.000"));
            MakePayment(142, Parse("2012-11-14 19:35:05.000"));

            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.PaidEarly));
            Assert.That(_loan.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));
            Assert.That(_loan.Schedule[2].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));

            Assert.That(_loan.DateClosed, Is.Null);

            Assert.That(_loan.Schedule[0].AmountDue, Is.EqualTo(0m));
            Assert.That(_loan.Schedule[1].AmountDue, Is.EqualTo(482.7m));
            Assert.That(_loan.Schedule[2].AmountDue, Is.EqualTo(705.96m));

        }

        [Test]
        [Description("Modeling loan case 121")]
        public void loan_121()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.07M, Term = 6};
            calculator.Calculate(6300, _loan, Parse("2012-11-18 11:56:41.000"));

            Console.WriteLine(_loan);

            MakePayment(670, Parse("2012-11-23 14:23:14.000"));
            MakePayment(521, Parse("2012-11-23 22:31:21.000"));
            MakePayment(500, Parse("2012-11-24 10:21:15.000"));
            MakePayment(238, Parse("2012-11-25 12:16:00.000"));

            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.PaidEarly));
            Assert.That(_loan.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));
            Assert.That(_loan.Schedule[2].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));
            Assert.That(_loan.Schedule[3].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));
            Assert.That(_loan.Schedule[4].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));
            Assert.That(_loan.Schedule[5].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));

            Assert.That(_loan.DateClosed, Is.Null);

            Assert.That(_loan.Schedule[0].AmountDue, Is.EqualTo(0m));
            Assert.That(_loan.Schedule[1].AmountDue, Is.EqualTo(820.03m));
            Assert.That(_loan.Schedule[2].AmountDue, Is.EqualTo(1344.0m));
            Assert.That(_loan.Schedule[3].AmountDue, Is.EqualTo(1270.5m));
            Assert.That(_loan.Schedule[4].AmountDue, Is.EqualTo(1197.0m));
            Assert.That(_loan.Schedule[5].AmountDue, Is.EqualTo(1123.5m));

        }

        [Test]
        [Description("Modeling loan case 147")]
        public void loan_147_interest_after_200()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.07M};
            calculator.Calculate(400, _loan, new DateTime(2012, 11, 24, 19, 57, 02));

            Console.WriteLine(_loan);

            MakePayment(200, new DateTime(2012, 12, 02, 10, 0, 0));

            Assert.That(_loan.Schedule[1].Interest, Is.EqualTo(25.17m));
            Assert.That(_loan.Schedule[1].LoanRepayment, Is.EqualTo(74.47m));
            Assert.That(_loan.TransactionsWithPaypointSuccesefull[0].Interest, Is.EqualTo(7.47m));
        }

        [Test]
        [Description("Modeling loan case 42. RollOver")]
        public void loan_42()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.06M};
            calculator.Calculate(5000, _loan, Parse("2012-09-23 23:29:35.000"));

            Console.WriteLine(_loan);

            MakePayment(200, Parse("2012-10-24 10:03:01.000"));
            MakePayment(400, Parse("2012-10-29 10:44:22.000"));
            MakePayment(200, Parse("2012-10-29 13:30:18.000"));

            Assert.That(_loan.Schedule[1].LoanRepayment, Is.EqualTo(1666m));

            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.Late));
            Assert.That(_loan.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));
            Assert.That(_loan.Schedule[2].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));

            Assert.That(_loan.DateClosed, Is.Null);
        }

        [Test]
        [Description("Modeling loan case 54")]
        public void loan_54()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.05M};
            calculator.Calculate(3000, _loan, Parse("2012-10-03 13:40:37.000"));

            Console.WriteLine(_loan);

            MakePayment(2300, Parse("2012-10-31 19:55:37.000"));
            MakePayment(839, Parse("2012-11-27 16:25:02.000"));
            MakePayment(34, Parse("2012-11-27 16:28:20.000"));

            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.PaidEarly));
            Assert.That(_loan.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.PaidEarly));
            Assert.That(_loan.Schedule[2].Status, Is.EqualTo(LoanScheduleStatus.PaidEarly));

            Assert.That(_loan.DateClosed, Is.Not.Null);
        }

        [Test]
        [Description("Modeling loan case 55")]
        public void loan_55()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.06M};
            calculator.Calculate(5000, _loan, Parse("2012-10-03 17:15:27.000"));

            Console.WriteLine(_loan);

            MakePayment(600, Parse("2012-10-26 15:17:42.000"));
            MakePayment(1550, Parse("2012-11-01 07:39:04.000"));
            MakePayment(3419, Parse("2012-11-14 08:40:33.000"));

            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.PaidEarly));
            Assert.That(_loan.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.PaidEarly));
            Assert.That(_loan.Schedule[2].Status, Is.EqualTo(LoanScheduleStatus.PaidEarly));

            Assert.That(_loan.DateClosed, Is.Not.Null);
        }

        [Test]
        [Description("Modeling loan case 56")]
        public void loan_56()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.06M};
            calculator.Calculate(1500, _loan, Parse("2012-10-05 17:52:52.000"));

            Console.WriteLine(_loan);

            MakePayment(590, Parse("2012-11-05 19:13:37.000"));

            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.PaidOnTime));
            Assert.That(_loan.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));
            Assert.That(_loan.Schedule[2].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));

            Assert.That(_loan.DateClosed, Is.Null);
        }

        [Test]
        [Description("Modeling loan case 59")]
        public void loan_59()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.04M};
            calculator.Calculate(20000, _loan, Parse("2012-10-09 13:32:23.000"));

            Console.WriteLine(_loan);

            MakePayment(14910, Parse("2012-11-08 00:00:00.000"));

            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.PaidEarly));
            Assert.That(_loan.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.PaidEarly));
            Assert.That(_loan.Schedule[2].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));

            Assert.That(_loan.DateClosed, Is.Null);

            Assert.That(_loan.Schedule[2].AmountDue, Is.EqualTo(6340.9));
        }

        [Test]
        [Description("trello card 14")]
        //https://trello.com/c/MeFaqtiZ
        public void trello_card_14()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.05M};
            calculator.Calculate(7500, _loan, Parse("2012-10-17 13:32:23.000"));

            Console.WriteLine(_loan);

            MakePayment(2890, Parse("2012-11-20 00:00:00.000"));

            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.Late));
            Assert.That(_loan.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));
            Assert.That(_loan.Schedule[2].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));

            Assert.That(_loan.DateClosed, Is.Null);
        }

        [Test]
        [Description("loan 331. trello card 122")]
        //https://trello.com/c/MeFaqtiZ
        public void trello_card_122()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.06M};
            calculator.Calculate(1110, _loan, Parse("2012-12-14 11:11:38.000"));
            _loan.Status = LoanStatus.Live;

            Console.WriteLine(_loan);

            MakePayment(378.59m, Parse("2012-12-18 14:47:08.000"));
            MakePayment(731.41m, Parse("2012-12-18 15:01:07.000"));

            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.PaidEarly));
            Assert.That(_loan.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.PaidEarly));
            Assert.That(_loan.Schedule[2].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));

            Assert.That(_loan.DateClosed, Is.Null);
            Assert.That(_loan.Status, Is.EqualTo(LoanStatus.Live));
        }

        [Test]
        [Description("loan 331. trello card 122. TotalEarlyPayment")]
        //https://trello.com/c/MeFaqtiZ
        public void trello_card_122_totalearlypayment()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.06M};
            calculator.Calculate(1110, _loan, Parse("2012-12-14 11:11:38.000"));
            _loan.Status = LoanStatus.Live;

            MakePayment(378.59m, Parse("2012-12-18 14:47:08.000"));
            MakePayment(731.41m, Parse("2012-12-18 15:01:07.000"));

            Console.WriteLine(_loan);

			var calc = new LoanRepaymentScheduleCalculator(_loan, Parse("2012-12-21 11:11:38.000"), 0);

            var payment = calc.TotalEarlyPayment();
            var next = calc.NextEarlyPayment();

            Console.WriteLine(_loan);

            Assert.That(payment, Is.EqualTo(8.64m));
            Assert.That(next, Is.EqualTo(8.64m));
        }

        [Test]
        [Description("loan 173. trello card 130.")]
        //https://trello.com/c/IhYey6dp
        public void trello_card_130()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.06M, Term = 10};
            calculator.Calculate(2721650, _loan, Parse("2012-08-25 08:20:35.000"));
            _loan.Status = LoanStatus.Live;

            Console.WriteLine(_loan);

            MakePayment(435464.00m, Parse("2012-09-25 13:27:46.000"));
            MakePayment(419134.00m, Parse("2012-10-25 12:15:23.000"));
            MakePayment(402804.00m, Parse("2012-11-25 06:00:48.000"));
            MakePayment(1409923.59m, Parse("2012-12-19 13:27:01.000"));

			var calc = new LoanRepaymentScheduleCalculator(_loan, Parse("2012-12-21 11:11:38.000"), 0);

            var payment = calc.TotalEarlyPayment();
            var next = calc.NextEarlyPayment();
        }

        [Test]
        [Description("trello card 244. late amount after exactly 2 moths")]
        //https://trello.com/c/x2e8DXJ8
        public void trello_card_244()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M, Term = 3 };
            calculator.Calculate(1000, _loan, Parse("2012-11-08 00:00:00.000"));
            _loan.Status = LoanStatus.Live;

            Console.WriteLine(_loan);

			var payEarlyCalc = new LoanRepaymentScheduleCalculator(_loan, Parse("2013-01-08 16:03:42.000"), 0);
            var state = payEarlyCalc.GetState();

            var earlyPayment = payEarlyCalc.TotalEarlyPayment();

            var model = LoanModel.FromLoan(_loan, payEarlyCalc, payEarlyCalc);

            Assert.That(state.AmountDue, Is.EqualTo(787));
            Assert.That(earlyPayment, Is.EqualTo(1120m));

            Assert.That(model.Late, Is.EqualTo(454));
        }

        [Test]
        [Description("trello card 245. one pound mistake")]
        //https://trello.com/c/NHwkJCpE
        public void trello_card_245_one_pound_mistake()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M, Term = 3 };
            calculator.Calculate(1000, _loan, Parse("2012-12-09 00:00:00.000"));
            _loan.Status = LoanStatus.Live;

            Console.WriteLine(_loan);

			var payEarlyCalc = new LoanRepaymentScheduleCalculator(_loan, Parse("2013-01-09 00:00:00.000"), 0);

            var earlyPayment = payEarlyCalc.NextEarlyPayment();

            Assert.That(earlyPayment, Is.EqualTo(394));
        }

        [Test]
        [Description("loan 432. trello card 162.")]
        //https://trello.com/c/IhYey6dp
        public void trello_card_162()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.06M, Term = 3};
            calculator.Calculate(1000, _loan, Parse("2012-11-25 00:00:00.000"));
            _loan.Status = LoanStatus.Live;

            var rollover = new PaymentRollover()
            {
                Created = Parse("2012-12-22 16:59:04.000"),
                ExpiryDate = Parse("2012-12-25 00:00:00.000"),
                Status = RolloverStatus.New,
                LoanSchedule = _loan.Schedule[1],
                Payment = 50
            };

            _loan.Schedule[1].Rollovers.Add(rollover);

            Console.WriteLine(_loan);

            MakePayment(54.00m, Parse("2012-12-22 15:00:15.000"));
            MakePayment(36.00m, Parse("2012-12-22 15:01:21.000"));

            Assert.That(_loan.TransactionsWithPaypoint[0].Interest, Is.EqualTo(54m));

            Assert.That(rollover.PaidPaymentAmount, Is.EqualTo(36m));
            Assert.That(_loan.TransactionsWithPaypoint[1].Rollover, Is.EqualTo(36m));
        }

        [Test]
        [Description("loan 452. trello card 191.")]
        //https://trello.com/c/IhYey6dp
        public void trello_card_191()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.06M, Term = 3};
            calculator.Calculate(1000, _loan, Parse("2012-11-20 00:00:00.000"));
            _loan.Status = LoanStatus.Live;

            var rollover = new PaymentRollover()
            {
                Created = Parse("2012-12-24 17:25:49.000"),
                ExpiryDate = Parse("2012-12-27 00:00:00.000"),
                Status = RolloverStatus.New,
                LoanSchedule = _loan.Schedule[1],
                Payment = 50
            };

            _loan.Schedule[1].Rollovers.Add(rollover);

            Console.WriteLine(_loan);

            MakePayment(54.00m, Parse("2012-12-24 15:26:17.000"));
            MakePayment(36.00m, Parse("2012-12-24 15:26:53.000"));
            MakePayment(30.00m, Parse("2012-12-24 15:27:41.000"));

            Assert.That(rollover.PaidPaymentAmount, Is.EqualTo(50m));
            Assert.That(_loan.TransactionsWithPaypoint[2].Rollover, Is.EqualTo(27.74m));
        }

        [Test]
        [Description("loan 517. incorrect interest for rollover repayment")]
        public void loan_517()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.06M, Term = 3};
            calculator.Calculate(1000, _loan, Parse("2012-10-26 00:00:00.000"));
            _loan.Status = LoanStatus.Live;

            var rollover = new PaymentRollover()
            {
                Created = Parse("2012-12-26 18:02:54.000"),
                ExpiryDate = Parse("2012-12-29 00:00:00.000"),
                Status = RolloverStatus.New,
                LoanSchedule = _loan.Schedule[0],
                Payment = 50
            };

            _loan.Schedule[0].Rollovers.Add(rollover);

            Console.WriteLine(_loan);

            MakePayment(170m, Parse("2012-12-26 16:03:42.000"));

			var payEarlyCalc = new LoanRepaymentScheduleCalculator(_loan, Parse("2012-12-26 16:03:42.000"), 0);
            var state = payEarlyCalc.GetState();

            Assert.That(rollover.PaidPaymentAmount, Is.EqualTo(50m));
            Assert.That(_loan.TransactionsWithPaypoint[0].Rollover, Is.EqualTo(50m));
            Assert.That(_loan.TransactionsWithPaypoint[0].Interest, Is.EqualTo(120m));
            Assert.That(_loan.TransactionsWithPaypoint[0].LoanRepayment, Is.EqualTo(0m));
        }

        [Test]
        [Description("loan 582. incorrect payment amount for rollover")]
        public void loan_582_Taras()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.06M, Term = 3};
            calculator.Calculate(1000, _loan, Parse("2012-10-30 00:00:00.000"));
            _loan.Status = LoanStatus.Live;

            var rollover = new PaymentRollover()
            {
                Created = Parse("2012-12-28 16:00:31.000"),
                ExpiryDate = Parse("2012-12-31 00:00:00.000"),
                Status = RolloverStatus.New,
                LoanSchedule = _loan.Schedule[1],
                Payment = 50
            };

            _loan.Schedule[1].Rollovers.Add(rollover);

            Console.WriteLine(_loan);

			var payEarlyCalc = new LoanRepaymentScheduleCalculator(_loan, Parse("2012-12-28 14:00:53.000"), 0);
            var state = payEarlyCalc.GetState();

            Assert.That(state.Fees + state.Interest, Is.EqualTo(116m + 50m));
        }

        [Test]
        [Description("loan 533. rollover and late fee")]
        public void loan_533_Taras()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.06M, Term = 3};
            calculator.Calculate(1000, _loan, Parse("2012-10-12 00:00:00.000"));
            _loan.Status = LoanStatus.Live;

            var rollover = new PaymentRollover()
            {
                Created = Parse("2012-12-27 15:18:30.000"),
                ExpiryDate = Parse("2012-12-30 00:00:00.000"),
                Status = RolloverStatus.New,
                LoanSchedule = _loan.Schedule[0],
                Payment = 50
            };

            _loan.Schedule[0].Rollovers.Add(rollover);

            var charge = new LoanCharge() {Amount = 75, ChargesType = null, Date = Parse("2012-12-27 13:15:29.000"), Loan = _loan};
            _loan.Charges.Add(charge);

            Console.WriteLine(_loan);

            var now = Parse("2012-12-27 13:19:00.000");
            MakePayment(274.03m, now);

            //var model = LoanModel.FromLoan(_loan, new LoanRepaymentScheduleCalculator(_loan, now), new LoanRepaymentScheduleCalculator(_loan, now));

			var payEarlyCalc = new LoanRepaymentScheduleCalculator(_loan, now, 0);
            var state = payEarlyCalc.GetState();

            Console.WriteLine(_loan);

            Assert.That(_loan.Schedule[0].Interest, Is.EqualTo(0));
        }

        [Test]
        [Description("loan 579. paying after late installment should not substract from last installment")]
        public void loan_579_Taras()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.06M, Term = 3};
            calculator.Calculate(1000, _loan, Parse("2012-11-09 00:00:00.000"));
            _loan.Status = LoanStatus.Live;

            Console.WriteLine(_loan);

            MakePayment(100m, Parse("2012-12-28 13:41:37.000"));

            Console.WriteLine(_loan);

            Assert.That(_loan.Schedule[1].LoanRepayment, Is.EqualTo(333));
            Assert.That(_loan.Schedule[2].LoanRepayment, Is.EqualTo(333));
        }

        [Test]
        [Description("loan 579.no payments, first installment is missed. last should be 333")]
        public void loan_581_Taras_no_payments()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.06M, Term = 3};
            calculator.Calculate(1000, _loan, Parse("2012-10-28 00:00:00.000"));
            _loan.Status = LoanStatus.Live;

            Console.WriteLine(_loan);

			var payEarlyCalc = new LoanRepaymentScheduleCalculator(_loan, Parse("2012-12-28 09:26:50.000"), 0);
            var payment = payEarlyCalc.TotalEarlyPayment();

            Console.WriteLine(_loan);

            Assert.That(_loan.Schedule[0].LoanRepayment, Is.EqualTo(334));
            Assert.That(_loan.Schedule[1].LoanRepayment, Is.EqualTo(333));
            Assert.That(_loan.Schedule[2].LoanRepayment, Is.EqualTo(333));
        }

        [Test]
        [Description("total early payment should not add expired rollover")]
        public void total_early_payment_expired_rollover()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.06M, Term = 3};
            calculator.Calculate(1000, _loan, Parse("2012-12-27 18:43:59.000"));
            _loan.Status = LoanStatus.Live;

            var rollover = new PaymentRollover()
            {
                Created = Parse("2012-12-27 20:46:25.000"),
                ExpiryDate = Parse("2012-12-28 00:00:00.000"),
                Status = RolloverStatus.New,
                LoanSchedule = _loan.Schedule[0],
                Payment = 50
            };

            _loan.Schedule[0].Rollovers.Add(rollover);

            Console.WriteLine(_loan);

            MakePayment(335.94m, Parse("2012-12-28 08:58:02.000"));

			var payEarlyCalc = new LoanRepaymentScheduleCalculator(_loan, Parse("2012-12-28 09:26:50.000"), 0);
            var payment = payEarlyCalc.TotalEarlyPayment();

            Assert.That(payment, Is.EqualTo(666m));

            MakePayment(666m, Parse("2012-12-28 09:26:50.000"));

            Console.WriteLine(_loan);

            Assert.That(_loan.Status, Is.EqualTo(LoanStatus.PaidOff));
        }

        [Test]
        [Description("double principal bug")]
        [Ignore]
        public void double_principal_bug()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.06M, Term = 3};
            calculator.Calculate(1000, _loan, Parse("2012-10-27 18:43:59.000"));
            _loan.Status = LoanStatus.Live;

            var rollover = new PaymentRollover()
            {
                Created = Parse("2012-12-27 20:46:25.000"),
                ExpiryDate = Parse("2012-12-28 00:00:00.000"),
                Status = RolloverStatus.New,
                LoanSchedule = _loan.Schedule[0],
                Payment = 50
            };

            _loan.Schedule[0].Rollovers.Add(rollover);

            Console.WriteLine(_loan);

            MakePayment(170m, Parse("2012-12-27 08:58:02.000"));

            //var payEarlyCalc = new LoanRepaymentScheduleCalculator(_loan, Parse("2012-12-27 08:58:02.000"));
            //var state = payEarlyCalc.GetState();

            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));
            Assert.That(_loan.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));
            Assert.That(_loan.Schedule[2].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));

            Console.WriteLine(_loan);
        }

        [Test]
        [Description("loan 478. can pay two rollovers on one day.")]
        public void loan_478_roman()
        {
            var calculator = new LoanScheduleCalculator(){Interest = 0.06M, Term = 3};
            calculator.Calculate(1000, _loan, Parse("2012-05-26 00:00:00.000"));
            _loan.Status = LoanStatus.Live;

            var rollover1 = new PaymentRollover()
            {
                Created      = Parse("2012-12-26 12:35:42.000"),
                ExpiryDate   = Parse("2012-12-29 00:00:00.000"),
                Status       = RolloverStatus.New,
                LoanSchedule = _loan.Schedule[0],
                Payment      = 50
            };

            var rollover2 = new PaymentRollover()
            {
                Created      = Parse("2012-12-26 12:39:12.000"),
                ExpiryDate   = Parse("2012-12-29 00:00:00.000"),
                Status       = RolloverStatus.New,
                LoanSchedule = _loan.Schedule[0],
                Payment      = 50
            };

            _loan.Schedule[0].Rollovers.Add(rollover1);
            _loan.Schedule[0].Rollovers.Add(rollover2);

            Console.WriteLine(_loan);

            var state = _facade.GetStateAt(_loan, Parse("2012-12-26 10:36:26.000"));

            var interest = 420m;

            Assert.That(state.Interest, Is.EqualTo(interest));

            MakePayment(interest + 50m, Parse("2012-12-26 10:36:26.000"));
            Assert.That(_loan.TransactionsWithPaypoint[0].Interest, Is.EqualTo(interest));
            Assert.That(_loan.TransactionsWithPaypoint[0].Rollover, Is.EqualTo(50));
            Assert.That(rollover1.PaidPaymentAmount, Is.EqualTo(50));

            MakePayment(50m,   Parse("2012-12-26 10:54:48.000"));

            Assert.That(rollover2.PaidPaymentAmount, Is.EqualTo(50));
            Assert.That(_loan.TransactionsWithPaypoint[1].Rollover, Is.EqualTo(50));
            MakePayment(1000.00m, Parse("2012-12-26 10:57:31.000"));

            Assert.That(_loan.Status, Is.EqualTo(LoanStatus.PaidOff));

            Assert.That(rollover1.PaidPaymentAmount, Is.EqualTo(50));
            Assert.That(rollover2.PaidPaymentAmount, Is.EqualTo(50));

            Console.WriteLine(_loan);            
        }

        [Test]
        [Description("installment should be late only on the next day of it's payment date.")]
        public void loan_512()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M, Term = 3 };

            var now = Parse("2012-12-26 15:22:45.000");

            calculator.Calculate(1000, _loan, Parse("2012-10-26 00:00:00.000"));
            _loan.Status = LoanStatus.Live;

            Console.WriteLine(_loan);

			var payEarlyCalc = new LoanRepaymentScheduleCalculator(_loan, now, 0);
            var state = payEarlyCalc.GetState();

            Assert.That(_loan.Schedule[0].Status, Is.EqualTo(LoanScheduleStatus.Late));
            Assert.That(_loan.Schedule[1].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));
            Assert.That(_loan.Schedule[2].Status, Is.EqualTo(LoanScheduleStatus.StillToPay));
        }

        [Test]
        [Description("payment for two late installments")]
        public void loan_late_payment_amount()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M, Term = 3 };

            var now = Parse("2012-12-26 15:22:45.000");

            calculator.Calculate(1000, _loan, Parse("2012-10-26 00:00:00.000"));
            _loan.Status = LoanStatus.Live;

            Console.WriteLine(_loan);

			var payEarlyCalc = new LoanRepaymentScheduleCalculator(_loan, now, 0);
            var state = payEarlyCalc.GetState();

            Assert.That(state.Interest, Is.EqualTo(120m));

            MakePayment(120, now);

            Assert.That(_loan.TransactionsWithPaypoint[0].Interest, Is.EqualTo(120));
        }

        [Test]
        public void check_total_early_payment()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M, Term = 3 };

            var now = Parse("2012-12-26 01:11:16.000");

            calculator.Calculate(1000, _loan, Parse("2012-12-22 13:52:25.000"));
            _loan.Status = LoanStatus.Live;

            Console.WriteLine(_loan);

            MakePayment(1003.87m, Parse("2012-12-24 14:56:16.000"));

			var model = LoanModel.FromLoan(_loan, new LoanRepaymentScheduleCalculator(_loan, now, 0), new LoanRepaymentScheduleCalculator(_loan, now, 0));

            Assert.That(_loan.TotalEarlyPayment(now), Is.EqualTo(0));
            Assert.That(model.TotalEarlyPayment, Is.EqualTo(0));
        }

        [Test]
        [Description("loan 187")]
        public void loan_187()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M, Term = 6 };

            var now = Parse("2012-12-12 10:23:04.000");

            calculator.Calculate(400, _loan, Parse("2012-12-12 10:23:04.000"));
            _loan.Status = LoanStatus.Live;

            Console.WriteLine(_loan);

            MakePayment(94.0m, Parse("2013-01-14 20:00:04.000"));

            Console.WriteLine(_loan);

			var payEarlyCalc = new LoanRepaymentScheduleCalculator(_loan, Parse("2013-01-20 20:00:04.000"), 0);
            var state = payEarlyCalc.GetState();

            Console.WriteLine(_loan);
        }

        [Test]
        [Description("loan 196")]
        public void loan_196()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M, Term = 6 };

            calculator.Calculate(12200, _loan, Parse("2012-12-16 07:47:33.000"));
            _loan.Status = LoanStatus.Live;

            Console.WriteLine(_loan);

            MakePayment(12625.0m, Parse("2013-01-03 09:13:21.000"));

			var payEarlyCalc = new LoanRepaymentScheduleCalculator(_loan, Parse("2013-01-23 20:00:04.000"), 0);
            var state = payEarlyCalc.GetState();

            //Assert.That(state.AmountDue, Is.EqualTo(0.04m));

            //Assert.That(_loan.Status, Is.EqualTo(LoanStatus.PaidOff));
            //Assert.That(_loan.Schedule.All(s => s.Status == LoanScheduleStatus.PaidEarly), Is.True);
        }

        [Test]
        [Description("loan 34")]
        public void loan_34()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M, Term = 3 };

            calculator.Calculate(400, _loan, Parse("2012-09-12 15:05:27.000"));
            _loan.Status = LoanStatus.Live;

            Console.WriteLine(_loan);

            MakePayment(158.0m, Parse("2012-10-12 10:50:44.000"));
            MakePayment(146.0m, Parse("2012-11-07 12:23:19.000"));
            MakePayment(136.0m, Parse("2012-11-21 16:33:08.000"));
            MakePayment(0.13m, Parse("2013-01-22 20:00:11.000"));

			var payEarlyCalc = new LoanRepaymentScheduleCalculator(_loan, Parse("2013-01-23 20:00:11.000"), 0);
            var state = payEarlyCalc.GetState();

            Assert.That(state.AmountDue, Is.EqualTo(1.08m));

            //Assert.That(state.AmountDue, Is.EqualTo(0.04m));
            //Assert.That(_loan.Status, Is.EqualTo(LoanStatus.PaidOff));
            //Assert.That(_loan.Schedule.All(s => s.Status == LoanScheduleStatus.PaidEarly), Is.True);
        }

        [Test]
        [Description("loan 85")]
        public void loan_85()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.05M, Term = 3 };

            calculator.Calculate(2000, _loan, Parse("2012-10-25 12:56:27.000"));
            _loan.Status = LoanStatus.Live;

            _loan.Charges.Add(new LoanCharge() { Amount = 20m, Date = Parse("2013-02-01 00:16:32.000") });
            _loan.Charges.Add(new LoanCharge() { Amount = 45m, Date = Parse("2013-02-08 00:16:30.000") });

            Console.WriteLine(_loan);

            MakePayment(768.0m, Parse("2012-11-25 09:05:32.000"));
            MakePayment(50.00m, Parse("2013-01-01 12:11:16.000"));
            MakePayment(50.00m, Parse("2013-01-02 13:33:06.000"));
            MakePayment(50.00m, Parse("2013-01-03 14:13:29.000"));
            MakePayment(50.00m, Parse("2013-01-04 11:26:40.000"));
            MakePayment(50.00m, Parse("2013-01-14 09:53:03.000"));
            MakePayment(50.00m, Parse("2013-01-24 16:55:00.000"));
            MakePayment(51.00m, Parse("2013-01-27 11:29:31.000"));
            MakePayment(50.00m, Parse("2013-01-28 09:46:23.000"));
            MakePayment(52.00m, Parse("2013-01-29 09:55:05.000"));
            MakePayment(60.00m, Parse("2013-01-30 09:15:14.000"));
            MakePayment(56.00m, Parse("2013-01-31 09:35:16.000"));
            MakePayment(57.00m, Parse("2013-02-01 08:00:55.000"));
            MakePayment(50.00m, Parse("2013-02-04 08:42:51.000"));
            MakePayment(36.94m, Parse("2013-03-02 00:00:00.000"));
            MakePayment(25.00m, Parse("2013-03-02 00:00:00.000"));
            MakePayment(50.00m, Parse("2013-03-02 00:00:00.000"));
            MakePayment(50.00m, Parse("2013-03-02 00:00:00.000"));
            MakePayment(20.00m, Parse("2013-03-02 00:00:00.000"));
            MakePayment(100.00m, Parse("2013-03-04 00:00:00.000"));
            MakePayment(100.00m, Parse("2013-03-06 00:00:00.000"));
            MakePayment(192.05m, Parse("2013-03-17 00:00:00.000"));

			var payEarlyCalc = new LoanRepaymentScheduleCalculator(_loan, Parse("2013-03-19 20:00:11.000"), 0);
            var state = payEarlyCalc.GetState();

            //Assert.That(state.AmountDue, Is.EqualTo(0m));

            //Assert.That(_loan.Balance, Is.EqualTo(0));
            //Assert.That(_loan.Principal, Is.EqualTo(0));
        }

        [Test]
        [Description("loan 236")]
        public void loan_236()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M, Term = 6 };

            calculator.Calculate(1800, _loan, Parse("2012-12-28 17:52:37.000"));
            _loan.Status = LoanStatus.Live;

            Console.WriteLine(_loan);

            MakePayment(1000.0m, Parse("2013-01-11 23:16:28.000"));
        }

        [Test]
        [Description("EZ-488. Incorrect total interest. LoanId 151 on UAT")]
        public void ez488()
        {
            var calculator = new LoanScheduleCalculator() { Interest = 0.06M, Term = 12 };
            calculator.Calculate(1000, _loan, new DateTime(2013, 5, 8));

            Console.WriteLine(_loan);

            _loan.Charges.Add(new LoanCharge(){Amount = 75, Loan = _loan, Date = new DateTime(2013, 6, 25)});

             var state = _facade.GetStateAt(_loan, new DateTime(2013, 7, 10));

            Console.WriteLine("Interest: {0}", state.Interest);

            Console.WriteLine(_loan);
        }

        [Test]
        [Description("3 pound less and get amount to pay")]
        public void loanid_1196_trunk()
        {
            _calculator.Interest = 0.069m;
            _calculator.Term = 6;

            Console.WriteLine(_loan);

            CreateLoan(Parse("2013-07-28 00:00:00.000"), 1000);

            MakePayment(236.0m, Parse("2013-08-28 00:00:00.000"));

            var state = GetStateAt(_loan, Parse("2013-08-28 00:00:00.000"));

            Assert.That(state.AmountDue, Is.EqualTo(3));
            Assert.That(_loan.MaxDelinquencyDays, Is.EqualTo(0));

            Console.WriteLine(_loan);
        }

    }
}
