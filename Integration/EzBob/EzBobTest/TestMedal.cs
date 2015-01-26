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

	class OnlineNonLimitedNoBusinessScoreMedalCalculator1NoGathering : OnlineNonLimitedNoBusinessScoreMedalCalculator1 {
		private readonly MedalResult resultsInput;
		public OnlineNonLimitedNoBusinessScoreMedalCalculator1NoGathering(MedalResult resultsInput) {
			this.resultsInput = resultsInput;
		}
	}
	
	class SoleTraderMedalCalculator1NoGathering : SoleTraderMedalCalculator1 {
		private readonly MedalResult resultsInput;
		public SoleTraderMedalCalculator1NoGathering(MedalResult resultsInput) {
			this.resultsInput = resultsInput;
		}
	}
	

	class OfflineLimitedMedalCalculator1NoGathering : LimitedMedalCalculator1 {
		private readonly MedalResult resultsInput;
		public OfflineLimitedMedalCalculator1NoGathering(MedalResult resultsInput) {
			this.resultsInput = resultsInput;
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


		

		[Test]
		public void Test_TurnoverForMedalTest_NH_AV() {
			//DateTime calculationTime = new DateTime(2013, 11, 30);
			//DateTime calculationTime = new DateTime(2013, 08, 31);
			//DateTime calculationTime = new DateTime(2013, 10, 02);
			DateTime calculationTime = new DateTime(2014, 01, 01);
			int customerId = 19834; //211; //19271 ; //1953;  1826;  //  //  171; //348; // 363; //290; // 178; //;363
			// 171: amazon, pp, ebay
			// CustomerId = 211, CalculationTime = 01/01/2014 00:00:00 - have all MP types

			this.m_oLog.Info("START TURNOVER FOR MEDAL customerID: {0}; calculationTime: {1}", customerId, calculationTime.Date);

			this.m_oLog.Info("-------------------OnlineNonLimitedWithBusinessScoreMedalCalculator----------------------");
			// OnlineNonLimitedWithBusinessScoreMedalCalculator
			MedalResult resultsInput = new MedalResult(customerId);
			var calculatorTester = new OnlineNonLimitedWithBusinessScoreMedalCalculator1NoGathering(resultsInput);
			MedalResult resultsOutput = calculatorTester.CalculateMedalScore(customerId, calculationTime);
			//AV
			/*var msc = new OnlineNonLimitedWithBusinessScoreMedalCalculator(this.m_oDB, this.m_oLog);
			var data = msc.GetInputParameters(customerId, calculationTime);
			this.m_oLog.Info("--------###-----------OnlineNonLimitedWithBusinessScoreMedalCalculator----------------------");*/

			/*this.m_oLog.Info("-------------------NonLimitedMedalCalculator----------------------");
			// NonLimitedMedalCalculator
			MedalResult resultsInput1 = new MedalResult(customerId);
			var calculatorTester1 = new NonLimitedMedalCalculator1NoGathering(resultsInput1);
			MedalResult resultsOutput1 = calculatorTester1.CalculateMedalScore(customerId, calculationTime);
			//AV
			var msc1 = new NonLimitedMedalCalculator(this.m_oDB, this.m_oLog);
			var data1 = msc1.GetInputParameters(customerId, calculationTime);
			this.m_oLog.Info("--------###-----------NonLimitedMedalCalculator----------------------");

			this.m_oLog.Info("-------------------OnlineLImitedMedalCalculator----------------------");
			// OnlineLImitedMedalCalculator
			MedalResult resultsInput2 = new MedalResult(customerId);
			var calculatorTester2 = new OnlineLimitedMedalCalculator1NoGathering(resultsInput2);
			MedalResult resultsOutput2 = calculatorTester2.CalculateMedalScore(customerId, calculationTime);
			//AV
			var msc2 = new OnlineLImitedMedalCalculator(this.m_oDB, this.m_oLog);
			var data2 = msc2.GetInputParameters(customerId, calculationTime);
			this.m_oLog.Info("--------###-----------OnlineLImitedMedalCalculator----------------------");

			this.m_oLog.Info("-------------------OnlineNonLimitedNoBusinessScoreMedalCalculator----------------------");
			// OnlineNonLimitedNoBusinessScoreMedalCalculator
			MedalResult resultsInput3 = new MedalResult(customerId);
			var calculatorTester3 = new OnlineNonLimitedNoBusinessScoreMedalCalculator1NoGathering(resultsInput3);
			MedalResult resultsOutput3 = calculatorTester3.CalculateMedalScore(customerId, calculationTime);
			//AV
			var msc3 = new OnlineNonLimitedNoBusinessScoreMedalCalculator(this.m_oDB, this.m_oLog);
			var data3 = msc3.GetInputParameters(customerId, calculationTime);
			this.m_oLog.Info("--------###-----------OnlineNonLimitedNoBusinessScoreMedalCalculator----------------------");

			this.m_oLog.Info("-------------------SoleTraderMedalCalculator----------------------");
			// SoleTraderMedalCalculator
			MedalResult resultsInput4 = new MedalResult(customerId);
			var calculatorTester4 = new SoleTraderMedalCalculator1NoGathering(resultsInput4);
			MedalResult resultsOutput4 = calculatorTester4.CalculateMedalScore(customerId, calculationTime);
			//AV
			var msc4 = new SoleTraderMedalCalculator(this.m_oDB, this.m_oLog);
			var data4 = msc4.GetInputParameters(customerId, calculationTime);
			this.m_oLog.Info("--------###-----------SoleTraderMedalCalculator----------------------");

			this.m_oLog.Info("-------------------OfflineLImitedMedalCalculator----------------------");
			// OfflineLImitedMedalCalculator
			MedalResult resultsInput5 = new MedalResult(customerId);
			var calculatorTester5 = new OfflineLimitedMedalCalculator1NoGathering(resultsInput5);
			MedalResult resultsOutput5 = calculatorTester5.CalculateMedalScore(customerId, calculationTime);
			//AV
			var msc5 = new OfflineLImitedMedalCalculator(this.m_oDB, this.m_oLog);
			var data5 = msc5.GetInputParameters(customerId, calculationTime);
			this.m_oLog.Info("--------###-----------OfflineLImitedMedalCalculator----------------------");*/
		}


		


	} // class TestMedal
} // namespace
