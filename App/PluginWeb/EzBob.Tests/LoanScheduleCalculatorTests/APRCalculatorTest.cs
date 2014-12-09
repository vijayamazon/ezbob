namespace EzBob.Tests.LoanScheduleCalculatorTests
{
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using NUnit.Framework;
	using PaymentServices.Calculators;

	[TestFixture]
	class APRCalculatorTest
	{
		[Test]
		public void calculateApr1()
		{
			var calc = new APRCalculator();
			var loanSchedule = new List<LoanScheduleItem>
				{
					new LoanScheduleItem
						{
							Date = new DateTime(2013, 6, 7),
							InterestRate = 0.0275M,
							LoanRepayment = 6667,
							Interest = 1100,
							BalanceBeforeRepayment = 40000,
							Balance = 33333,
							AmountDue = 7767
						},
					new LoanScheduleItem
						{
							Date = new DateTime(2013, 7, 7),
							InterestRate = 0.0275M,
							LoanRepayment = 6667,
							Interest = 917,
							BalanceBeforeRepayment = 33333,
							Balance = 26667,
							Fees = 0,
							AmountDue = 7583

						},
					new LoanScheduleItem
						{
							Date = new DateTime(2013, 8, 7),
							InterestRate = 0.0275M,
							LoanRepayment = 6667,
							Interest = 733,
							BalanceBeforeRepayment = 26667,
							Balance = 20000,
							Fees = 0,
							AmountDue = 7400
						},
					new LoanScheduleItem
						{
							Date = new DateTime(2013, 9, 7),
							InterestRate = 0.0275M,
							LoanRepayment = 6667,
							Interest = 550,
							BalanceBeforeRepayment = 20000,
							Balance = 13333,
							Fees = 0,
							AmountDue = 7217
						},
					new LoanScheduleItem
						{
							Date = new DateTime(2013, 10, 7),
							InterestRate = 0.0275M,
							LoanRepayment = 6667,
							Interest = 367,
							BalanceBeforeRepayment = 13333,
							Balance = 6667,
							Fees = 0,
							AmountDue = 7033
						},
					new LoanScheduleItem
						{
							Date = new DateTime(2013, 11, 7),
							InterestRate = 0.0275M,
							LoanRepayment = 6667,
							Interest = 183,
							BalanceBeforeRepayment = 6667,
							Balance = 0,
							Fees = 0,
							AmountDue = 6850
						}
				};
			var apr = calc.Calculate(40000, loanSchedule, 0, new DateTime(2013, 5, 7));
			Assert.That(apr, Is.EqualTo(38.11));
		}

		[Test]
		public void calculateApr2()
		{
			var calc = new APRCalculator();
			var loanSchedule = new List<LoanScheduleItem>
				{
					new LoanScheduleItem
						{
							Date = new DateTime(2012, 9, 1),
							InterestRate = 0.07M,
							LoanRepayment = 500,
							Interest = 210,
							BalanceBeforeRepayment = 3000,
							Balance = 2500,
							Fees = 0,
							AmountDue = 710

						},
					new LoanScheduleItem
						{
							Date = new DateTime(2012, 10, 1),
							InterestRate = 0.07M,
							LoanRepayment = 500,
							Interest = 175,
							BalanceBeforeRepayment = 2500,
							Balance = 2000,
							Fees = 0,
							AmountDue = 675
						},
					new LoanScheduleItem
						{
							Date = new DateTime(2012, 11, 1),
							InterestRate = 0.07M,
							LoanRepayment = 500,
							Interest = 140,
							BalanceBeforeRepayment = 2000,
							Balance = 1500,
							Fees = 0,
							AmountDue = 640
						},
					new LoanScheduleItem
						{
							Date = new DateTime(2012, 12, 1),
							InterestRate = 0.07M,
							LoanRepayment = 500,
							Interest = 105,
							BalanceBeforeRepayment = 1500,
							Balance = 1000,
							Fees = 0,
							AmountDue = 605
						},
					new LoanScheduleItem
						{
							Date = new DateTime(2013, 1, 1),
							InterestRate = 0.07M,
							LoanRepayment = 500,
							Interest = 70,
							BalanceBeforeRepayment = 1000,
							Balance = 500,
							Fees = 0,
							AmountDue = 570
						},
					new LoanScheduleItem
						{
							Date = new DateTime(2013, 2, 1),
							InterestRate = 0.07M,
							LoanRepayment = 500,
							Interest = 35,
							BalanceBeforeRepayment = 500,
							Balance = 0,
							Fees = 0,
							AmountDue = 535
						}
				};
			var apr = calc.Calculate(3000, loanSchedule, 30, new DateTime(2012, 8, 1));
			Assert.That(apr, Is.EqualTo(132.71));
		}

		[Test]
		public void calculateApr3()
		{
			var calc = new APRCalculator();
			var loanSchedule = new List<LoanScheduleItem>
				{
					new LoanScheduleItem
						{
							Date = new DateTime(2012, 8, 30),
							InterestRate = 0.05M,
							LoanRepayment = 2333,
							Interest = 700,
							BalanceBeforeRepayment = 14000,
							Balance = 11667,
							Fees = 0,
							AmountDue = 3033

						},
					new LoanScheduleItem
						{
							Date = new DateTime(2012, 9, 30),
							InterestRate = 0.05M,
							LoanRepayment = 2333,
							Interest = 583,
							BalanceBeforeRepayment = 11667,
							Balance = 9333,
							Fees = 0,
							AmountDue = 2917
						},
					new LoanScheduleItem
						{
							Date = new DateTime(2012, 10, 30),
							InterestRate = 0.05M,
							LoanRepayment = 2333,
							Interest = 467,
							BalanceBeforeRepayment = 9333,
							Balance = 7000,
							Fees = 0,
							AmountDue = 2800
						},
					new LoanScheduleItem
						{
							Date = new DateTime(2012, 11, 30),
							InterestRate = 0.05M,
							LoanRepayment = 2333,
							Interest = 350,
							BalanceBeforeRepayment = 7000,
							Balance = 4667,
							Fees = 0,
							AmountDue = 2683
						},
					new LoanScheduleItem
						{
							Date = new DateTime(2012, 12, 30),
							InterestRate = 0.05M,
							LoanRepayment = 2333,
							Interest = 233,
							BalanceBeforeRepayment = 4667,
							Balance = 2333,
							Fees = 0,
							AmountDue = 2567
						},
					new LoanScheduleItem
						{
							Date = new DateTime(2013, 1, 30),
							InterestRate = 0.05M,
							LoanRepayment = 2333,
							Interest = 117,
							BalanceBeforeRepayment = 2333,
							Balance = 0,
							Fees = 0,
							AmountDue = 2450
						}
				};
			var apr = calc.Calculate(14000, loanSchedule, 112, new DateTime(2012, 7, 30));
			Assert.That(apr, Is.EqualTo(83.87));
		}

		[Test]
		public void calculateAprYoung()
		{
			var calc = new APRCalculator();
			var loanSchedule = new List<LoanScheduleItem>
				{
					new LoanScheduleItem
						{
							Date = new DateTime(2013, 7, 18),
							AmountDue = 2219

						},
					new LoanScheduleItem
						{
							Date = new DateTime(2013, 8, 18),
							AmountDue = 2169.96M

						},
					new LoanScheduleItem
						{
							Date = new DateTime(2013, 9, 18),
							AmountDue = 2124.16M

						},
					new LoanScheduleItem
						{
							Date = new DateTime(2013, 10, 18),
							AmountDue = 2037.85M

						},
					new LoanScheduleItem
						{
							Date = new DateTime(2013, 11, 18),
							AmountDue = 1996.53M

						},
					new LoanScheduleItem
						{
							Date = new DateTime(2013, 12, 18),
							AmountDue = 1955.22M

						},
					new LoanScheduleItem
						{
							Date = new DateTime(2014, 1, 18),
							AmountDue = 1913.9M

						},
					new LoanScheduleItem
						{
							Date = new DateTime(2014, 2, 18),
							AmountDue = 1872.58M

						},
					new LoanScheduleItem
						{
							Date = new DateTime(2014, 3, 18),
							AmountDue = 1814.61M

						},
					new LoanScheduleItem
						{
							Date = new DateTime(2014, 4, 18),
							AmountDue = 1777.46M

						},
					new LoanScheduleItem
						{
							Date = new DateTime(2014, 5, 18),
							AmountDue = 1740.3M

						},
					new LoanScheduleItem
						{
							Date = new DateTime(2014, 5, 18),
							AmountDue = 1703.15M

						},
				};
			var apr = calc.Calculate(20000, loanSchedule, 0, new DateTime(2013, 6, 18));
		//	Assert.That(apr, Is.EqualTo(35.55));
		}

		[Test]
		public void CalculateAprMonthly()
		{
			var calc = new APRCalculator();
			var loanSchedule = new List<LoanScheduleItem>
				{
					new LoanScheduleItem
						{
							Date = new DateTime(2013, 6, 7),
							InterestRate = 0.0275M,
							LoanRepayment = 6667,
							BalanceBeforeRepayment = 40000,
						},
					new LoanScheduleItem
						{
							Date = new DateTime(2013, 7, 7),
							InterestRate = 0.03M,
							LoanRepayment = 6667,
							BalanceBeforeRepayment = 33333,
						},
					new LoanScheduleItem
						{
							Date = new DateTime(2013, 8, 7),
							InterestRate = 0.04M,
							LoanRepayment = 6667,
							BalanceBeforeRepayment = 26667,
						},
					new LoanScheduleItem
						{
							Date = new DateTime(2013, 9, 7),
							InterestRate = 0.05M,
							LoanRepayment = 6667,
							BalanceBeforeRepayment = 20000,
						},
					new LoanScheduleItem
						{
							Date = new DateTime(2013, 10, 7),
							InterestRate = 0.06M,
							LoanRepayment = 6667,
							BalanceBeforeRepayment = 13333,
						},
					new LoanScheduleItem
						{
							Date = new DateTime(2013, 11, 7),
							InterestRate = 0.06M,
							LoanRepayment = 6667,
							BalanceBeforeRepayment = 6667,
						}
				};

			for (int i = 0; i < loanSchedule.Count; i++)
			{
				var aprMonthRate = calc.CalculateMonthly(40000, loanSchedule, i, 0, new DateTime(2013, 5, 7));
				string f = string.Format("month: {0}, APR: {1}", i + 1, aprMonthRate);
				Console.WriteLine("month: {0}, APR: {1}", i + 1, aprMonthRate);
			}

			//Assert.That(aprMonthRate, Is.InRange(42.1,42.2));

			/*
				var AprMonthRate = Math.Floor((Math.Pow((double)loanSchedule[0].InterestRate + 1, 12) - 1) * 100);
				Assert.That(AprMonthRate, Is.EqualTo(42));
			*/

		}
	}
}
