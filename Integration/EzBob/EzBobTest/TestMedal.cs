// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestMedal.cs" company="">
//     </copyright>
// <summary>
// The limited medal calculator 1 no gathering.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EzBobTest {
	using Ezbob.Backend.Strategies.MedalCalculations;
	using EzServiceAccessor;
	using EzServiceShortcut;
	using NUnit.Framework;
	using StructureMap;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using AutomationCalculator.Common;
	using AutomationCalculator.MedalCalculation;
	using ExcelLibrary.BinaryFileFormat;
	using Ezbob.Backend.Strategies.AutomationVerification;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using NHibernate.Linq;

	
	public class LimitedMedalCalculator1NoGathering : LimitedMedalCalculator1 {
		/*
		 * temprary disabled (elinar)
		 * protected override void GatherInputData(DateTime calculationTime) {
			Results = this.resultInput;
		}*/
		
		private readonly MedalResult resultInput;
		public LimitedMedalCalculator1NoGathering(MedalResult resultInput) {
			this.resultInput = resultInput;
		}
	}
	
	public class OnlineNonLimitedWithBusinessScoreMedalCalculator1NoGathering :
		OnlineNonLimitedWithBusinessScoreMedalCalculator1 {
		private readonly MedalResult resultInput;
		public OnlineNonLimitedWithBusinessScoreMedalCalculator1NoGathering(MedalResult resultInput) {
			this.resultInput = resultInput;
		}
	}
	
	public class NonLimitedMedalCalculator1NoGathering : NonLimitedMedalCalculator1 {
		private readonly MedalResult resultInput;
		public NonLimitedMedalCalculator1NoGathering(MedalResult resultInput) {
			this.resultInput = resultInput;
		}
	}

	public class OnlineLimitedMedalCalculator1NoGathering : OnlineLimitedMedalCalculator1 {
		private readonly MedalResult resultInput;
		public OnlineLimitedMedalCalculator1NoGathering(MedalResult resultInput) {
			this.resultInput = resultInput;
		}
	}
	

	/// <summary>
	/// The test medal.
	/// </summary>
	[TestFixture]
	public class TestMedal : BaseTestFixtue {
		[SetUp]
		public new void Init() {
			base.Init();
			ObjectFactory.Configure(x => x.For<IEzServiceAccessor>().Use<EzServiceAccessorShort>());
			Ezbob.Backend.Strategies.Library.Initialize(this.m_oEnv, this.m_oDB, this.m_oLog);
		} // Init

		
		public void Test_FirstMedalTest() {
			DateTime calculationTime = new DateTime(2014, 01, 01);
			int customerId = 18112;

			MedalResult resultsInput = new MedalResult(customerId);
			resultsInput.CalculationTime = calculationTime;
			resultsInput.MedalType = Ezbob.Backend.Strategies.MedalCalculations.MedalType.Limited;
			resultsInput.BusinessScore = 0;
			resultsInput.TangibleEquityValue = 0;
			resultsInput.BusinessSeniority = null;
			resultsInput.ConsumerScore = 0;
			resultsInput.MaritalStatus = EZBob.DatabaseLib.Model.Database.MaritalStatus.Single;
			resultsInput.FirstRepaymentDatePassed = false;
			resultsInput.EzbobSeniority = new DateTime(2013, 12, 10);
			resultsInput.NumOfLoans = 0;
			resultsInput.NumOfLateRepayments = 0;
			resultsInput.NumOfEarlyRepayments = 0;
			resultsInput.PositiveFeedbacks = 0;
			resultsInput.OnlineAnnualTurnover = 22954;
			resultsInput.BankAnnualTurnover = 22954;
			resultsInput.HmrcAnnualTurnover = 22954;
			resultsInput.ZooplaValue = 55;
			resultsInput.MortgageBalance = 0;

			var calculatorTester = new LimitedMedalCalculator1NoGathering(resultsInput);

			MedalResult resultsOutput = calculatorTester.CalculateMedalScore(customerId, calculationTime);
			Assert.AreEqual(resultsOutput.NetWorthGrade, 1);
			Assert.AreEqual(resultsOutput.EzbobSeniorityGrade, 3);
		}

	
		[Test]
		public void Test_TurnoverForMedalTest() {
			DateTime calculationTime = new DateTime(2013, 11, 30);
			int customerId = 211; //171  // ; //348; // 363; //290; // 178; //;363

			// 171: amazon, pp, ebay
			// CustomerId = 211, CalculationTime = 01/01/2014 00:00:00 - have all MP types

			MedalResult resultsInput = new MedalResult(customerId);
			resultsInput.CalculationTime = calculationTime;
			resultsInput.BusinessScore = 0;
			resultsInput.TangibleEquityValue = 0;
			resultsInput.BusinessSeniority = null;
			resultsInput.ConsumerScore = 0;
			resultsInput.MaritalStatus = EZBob.DatabaseLib.Model.Database.MaritalStatus.Single;
			resultsInput.FirstRepaymentDatePassed = false;
			resultsInput.EzbobSeniority = new DateTime(2013, 12, 10);
			resultsInput.NumOfLoans = 0;
			resultsInput.NumOfLateRepayments = 0;
			resultsInput.NumOfEarlyRepayments = 0;
			resultsInput.PositiveFeedbacks = 0;
			resultsInput.OnlineAnnualTurnover = 22954;
			resultsInput.BankAnnualTurnover = 22954;
			resultsInput.HmrcAnnualTurnover = 22954;
			resultsInput.ZooplaValue = 55;
			resultsInput.MortgageBalance = 0;

			resultsInput.MedalType = Ezbob.Backend.Strategies.MedalCalculations.MedalType.OnlineLimited;
			var calculatorTester = new OnlineNonLimitedWithBusinessScoreMedalCalculator1NoGathering(resultsInput);

			MedalResult resultsOutput = calculatorTester.CalculateMedalScore(customerId, calculationTime);

			resultsInput.MedalType = Ezbob.Backend.Strategies.MedalCalculations.MedalType.NonLimited;
			var calculatorTester1 = new NonLimitedMedalCalculator1NoGathering(resultsInput);

			MedalResult resultsOutput1 = calculatorTester1.CalculateMedalScore(customerId, calculationTime);

			// Assert.AreEqual(resultsOutput.NetWorthGrade, 1); Assert.AreEqual(resultsOutput.EzbobSeniorityGrade, 3);
		}

		[Test]
		public void TestMaamMedalAndPricing() {
			var stra = new MaamMedalAndPricing(1, 16431);

			stra.Execute();
		} // TestMaamMedalAndPricing


		public DateTime getPeriodAgo(DateTime calculationDate, DateTime lastUpdate ) {

			Console.WriteLine("calculationTime: {0}, lastUpdate: {1}", calculationDate, lastUpdate);

			int daysInMonth = DateTime.DaysInMonth(calculationDate.Year, calculationDate.Month);

			if ((daysInMonth - calculationDate.Date.Day) >= 3 && (daysInMonth - lastUpdate.Date.Day) >= 3) {
				return calculationDate.AddMonths(-12);
			}

			return calculationDate.AddMonths(-13);
		}

		[Test]
		public void Test_TurnoverForMedalTest_NH() 
		{
			var rep = ObjectFactory.GetInstance<CustomerMarketPlaceUpdatingHistoryRepository>();
			int customerId = 211; //171 ; //348; // 363; //290; // 178; //;363
			//DateTime calculationTime = new DateTime(2013, 11, 30);
			//DateTime calculationTime = new DateTime(2014, 01, 01);
			DateTime calculationTime = new DateTime(2015, 09, 28);
			DateTime yearAgo = calculationTime.AddMonths(-13);

			//Console.WriteLine("yearAgo: {0}", yearAgo);

			Console.WriteLine("daysInmonth: {0}", DateTime.DaysInMonth(calculationTime.Year, calculationTime.Month));

			var c = rep.GetByCustomerId(customerId);

			var lastUpdate = c.Where(x => x.UpdatingEnd < calculationTime)
				.Where(x => x.Error == null)
				.SelectMany(y => y.AmazonAggregations)
				.OrderByDescending(z => z.CustomerMarketPlaceUpdatingHistory.Id)
				.First();
				//.CustomerMarketPlaceUpdatingHistory.UpdatingEnd;

			DateTime lastUpdateDate = (DateTime)lastUpdate.CustomerMarketPlaceUpdatingHistory.UpdatingEnd;

			DateTime yearAgo1 = this.getPeriodAgo(calculationTime, lastUpdateDate);
			Console.WriteLine(yearAgo1);
			return;

		//	Console.WriteLine("??? {0}", (calculationTime.AddDays(-3).Date == lastUpdate.Date));
		//	Console.WriteLine("calculationTime: {0}, calculationTime-3 days: {1}, lastupd: {2}", calculationTime, calculationTime.AddDays(-3), lastUpdate.CustomerMarketPlaceUpdatingHistory.UpdatingEnd);
			//Console.WriteLine("{0}, {1}, {2}, {3}, {4}", lastUpdate.CustomerMarketPlaceUpdatingHistory.Id, lastUpdate.TheMonth, lastUpdate.Turnover, lastUpdate.AmazonAggregationID,
	//lastUpdate.CustomerMarketPlaceUpdatingHistory.UpdatingEnd);

			var amazons = c.Where(x => x.UpdatingEnd < calculationTime).Where(x => x.Error == null).SelectMany(y => y.AmazonAggregations)
					.Where(z => z.TheMonth >= yearAgo).OrderByDescending(yy => yy.TheMonth).AsEnumerable();
			
			Console.WriteLine("=========all=={0}=======", amazons.Count());
			foreach (var y in amazons)
				Console.WriteLine("{0}, {1}, {2}, {3}, {4}", y.CustomerMarketPlaceUpdatingHistory.Id, y.TheMonth, y.Turnover, y.AmazonAggregationID, y.CustomerMarketPlaceUpdatingHistory.UpdatingEnd);


			if (amazons.Count() > 0) {

				var amazonList = amazons.GroupBy(p => p.TheMonth).Select(g => g.Last());

				Console.WriteLine("=========filtered=={0}=======", amazonList.Count());
				foreach (var y in amazonList) 
	Console.WriteLine("{0}, {1}, {2}, {3}, {4}", y.CustomerMarketPlaceUpdatingHistory.Id, y.TheMonth, y.Turnover, y.AmazonAggregationID, y.CustomerMarketPlaceUpdatingHistory.UpdatingEnd);
			
			
			}

			/*		var amazonHistoryID = amazons.OrderByDescending(x => x.CustomerMarketPlaceUpdatingHistory.Id).First().CustomerMarketPlaceUpdatingHistory.Id;
		    		var amazonList = amazons.Where(x => x.CustomerMarketPlaceUpdatingHistory.Id == amazonHistoryID).Where(z => z.TheMonth >= yearAgo).OrderByDescending(xx => xx.TheMonth);
					Console.WriteLine("=========filtered=={0}=======", amazonList.Count());
					amazonList.ForEach(y => Console.WriteLine("{0}, {1}, {2}, {3}, {4}", y.CustomerMarketPlaceUpdatingHistory.Id, y.TheMonth, y.Turnover, y.AmazonAggregationID, y.CustomerMarketPlaceUpdatingHistory.UpdatingEnd));
			*/
		}


		[Test]
		public void Test_TurnoverForMedalTest_NH_AV() {
			//DateTime calculationTime = new DateTime(2013, 11, 30);
			//DateTime calculationTime = new DateTime(2013, 10, 02);
			DateTime calculationTime = new DateTime(2014, 01, 01);
			int customerId = 171 ;  //211 ; //348; // 363; //290; // 178; //;363
			
			// OnlineNonLimitedWithBusinessScoreMedalCalculator
			// Primary
			MedalResult resultsInput = new MedalResult(customerId);
			var calculatorTester = new OnlineNonLimitedWithBusinessScoreMedalCalculator1NoGathering(resultsInput);
			MedalResult resultsOutput = calculatorTester.CalculateMedalScore(customerId, calculationTime);
			//AV
			var msc = new OnlineNonLimitedWithBusinessScoreMedalCalculator(this.m_oDB, this.m_oLog);
			var data = msc.GetInputParameters(customerId, calculationTime);

			/*// NonLimitedMedalCalculator
			// Primary
			MedalResult resultsInput1 = new MedalResult(customerId);
			var calculatorTester1 = new NonLimitedMedalCalculator1NoGathering(resultsInput1);
			MedalResult resultsOutput1 = calculatorTester1.CalculateMedalScore(customerId, calculationTime);
			//AV
			var msc1 = new NonLimitedMedalCalculator(this.m_oDB, this.m_oLog);
			var data1 = msc.GetInputParameters(customerId, calculationTime);
			 
			// NonLimitedMedalCalculator
			// Primary
			MedalResult resultsInput2 = new MedalResult(customerId);
			var calculatorTester2 = new OnlineLimitedMedalCalculator1NoGathering(resultsInput2);
			MedalResult resultsOutput2 = calculatorTester2.CalculateMedalScore(customerId, calculationTime);
			//AV
			var msc2 = new OfflineLImitedMedalCalculator(this.m_oDB, this.m_oLog);
			var data2 = msc2.GetInputParameters(customerId, calculationTime);*/

			/*resultsInput.MedalType = Ezbob.Backend.Strategies.MedalCalculations.MedalType.NonLimited;
			var calculatorTester1 = new NonLimitedMedalCalculator1NoGathering(resultsInput);
			MedalResult resultsOutput1 = calculatorTester1.CalculateMedalScore(customerId, calculationTime);*/
			// 171: amazon, pp, ebay
			// CustomerId = 211, CalculationTime = 01/01/2014 00:00:00 - have all MP types
		}


		


	} // class TestMedal
} // namespace
