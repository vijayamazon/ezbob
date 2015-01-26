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
	using Ezbob.Utils.Extensions;
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
			DateTime calculationTime = new DateTime(2015, 01, 26);
			calculationTime = DateTime.UtcNow;
			int customerId = 19970; // 211; // 1871; // //19271 ; //1953;  1826;  //  //  171; //348; // 363; //290; // 178; //;363 // 171: amazon, pp, ebay
			// CustomerId = 211, CalculationTime = 01/01/2014 00:00:00 - have all MP types

			this.m_oLog.Info("START TURNOVER FOR MEDAL customerID: {0}; calculationTime: {1}", customerId, calculationTime.Date);
			this.m_oLog.Info("-------------------OnlineNonLimitedWithBusinessScoreMedalCalculator----------------------");

			/*MedalResult resultsInput = new MedalResult(customerId);
			var calculatorTester = new OnlineNonLimitedWithBusinessScoreMedalCalculator1NoGathering(resultsInput);
			MedalResult result = calculatorTester.CalculateMedalScore(customerId, calculationTime);*/
	
			// both
			new CalculateMedal(customerId, calculationTime, false, true).Execute();

			//AV
			/*var msc = new OnlineNonLimitedWithBusinessScoreMedalCalculator(this.m_oDB, this.m_oLog);
			var model = msc.GetInputParameters(customerId, calculationTime);
			
			this.m_oLog.Info("AV : AnnualTurnover: {0}, HmrcAnnualTurnover: {1}, YodleeAnnualTurnover:{2}, OnlineAnnualTurnover: {3}, Type: {4}", model.AnnualTurnover, model.HmrcAnnualTurnover, model.YodleeAnnualTurnover, model.OnlineAnnualTurnover, model.TurnoverType);
			this.m_oLog.Info("--------###-----------OnlineNonLimitedWithBusinessScoreMedalCalculator----------------------");
			*/

			/*this.m_oLog.Info("-------------------NonLimitedMedalCalculator----------------------");
			MedalResult resultsInput1 = new MedalResult(customerId);
			var calculatorTester1 = new NonLimitedMedalCalculator1NoGathering(resultsInput1);
			MedalResult result1 = calculatorTester1.CalculateMedalScore(customerId, calculationTime);
	//		this.m_oLog.Info("Base : AnnualTurnover: {0}, HmrcAnnualTurnover: {1}, YodleeAnnualTurnover:{2}, OnlineAnnualTurnover: {3}, Type: {4}", result1.AnnualTurnover, result1.HmrcAnnualTurnover, result1.BankAnnualTurnover, result1.OnlineAnnualTurnover, result1.TurnoverType);
			//AV
			var msc1 = new NonLimitedMedalCalculator(this.m_oDB, this.m_oLog);
			var model1 = msc1.GetInputParameters(customerId, calculationTime);
			this.m_oLog.Info("AV : AnnualTurnover: {0}, HmrcAnnualTurnover: {1}, YodleeAnnualTurnover:{2}, OnlineAnnualTurnover: {3}, Type: {4}", model1.AnnualTurnover, model1.HmrcAnnualTurnover, model1.YodleeAnnualTurnover, model1.OnlineAnnualTurnover, model1.TurnoverType);
			this.m_oLog.Info("--------###-----------NonLimitedMedalCalculator----------------------");

			this.m_oLog.Info("-------------------OnlineLImitedMedalCalculator----------------------");
			MedalResult resultsInput2 = new MedalResult(customerId);
			var calculatorTester2 = new OnlineLimitedMedalCalculator1NoGathering(resultsInput2);
			MedalResult result2 = calculatorTester2.CalculateMedalScore(customerId, calculationTime);
		//	this.m_oLog.Info("Base : AnnualTurnover: {0}, HmrcAnnualTurnover: {1}, YodleeAnnualTurnover:{2}, OnlineAnnualTurnover: {3}, Type: {4}", result2.AnnualTurnover, result2.HmrcAnnualTurnover, result2.BankAnnualTurnover, result2.OnlineAnnualTurnover, result2.TurnoverType);
			//AV
			var msc2 = new OnlineLImitedMedalCalculator(this.m_oDB, this.m_oLog);
			var model2 = msc2.GetInputParameters(customerId, calculationTime);
			this.m_oLog.Info("AV : AnnualTurnover: {0}, HmrcAnnualTurnover: {1}, YodleeAnnualTurnover:{2}, OnlineAnnualTurnover: {3}, Type: {4}", model2.AnnualTurnover, model2.HmrcAnnualTurnover, model2.YodleeAnnualTurnover, model2.OnlineAnnualTurnover, model2.TurnoverType);
			this.m_oLog.Info("--------###-----------OnlineLImitedMedalCalculator----------------------");

			this.m_oLog.Info("-------------------OnlineNonLimitedNoBusinessScoreMedalCalculator----------------------");
			MedalResult resultsInput3 = new MedalResult(customerId);
			var calculatorTester3 = new OnlineNonLimitedNoBusinessScoreMedalCalculator1NoGathering(resultsInput3);
			MedalResult result3 = calculatorTester3.CalculateMedalScore(customerId, calculationTime);
		//	this.m_oLog.Info("Base : AnnualTurnover: {0}, HmrcAnnualTurnover: {1}, YodleeAnnualTurnover:{2}, OnlineAnnualTurnover: {3}, Type: {4}", result3.AnnualTurnover, result3.HmrcAnnualTurnover, result3.BankAnnualTurnover, result3.OnlineAnnualTurnover, result3.TurnoverType);
			//AV
			var msc3 = new OnlineNonLimitedNoBusinessScoreMedalCalculator(this.m_oDB, this.m_oLog);
			var model3 = msc3.GetInputParameters(customerId, calculationTime);
			this.m_oLog.Info("AV : AnnualTurnover: {0}, HmrcAnnualTurnover: {1}, YodleeAnnualTurnover:{2}, OnlineAnnualTurnover: {3}, Type: {4}", model3.AnnualTurnover, model3.HmrcAnnualTurnover, model3.YodleeAnnualTurnover, model3.OnlineAnnualTurnover, model3.TurnoverType);
			this.m_oLog.Info("--------###-----------OnlineNonLimitedNoBusinessScoreMedalCalculator----------------------");

			this.m_oLog.Info("-------------------SoleTraderMedalCalculator----------------------");
			MedalResult resultsInput4 = new MedalResult(customerId);
			var calculatorTester4 = new SoleTraderMedalCalculator1NoGathering(resultsInput4);
			MedalResult result4 = calculatorTester4.CalculateMedalScore(customerId, calculationTime);
		//	this.m_oLog.Info("Base : AnnualTurnover: {0}, HmrcAnnualTurnover: {1}, YodleeAnnualTurnover:{2}, OnlineAnnualTurnover: {3}, Type: {4}", result4.AnnualTurnover, result4.HmrcAnnualTurnover, result4.BankAnnualTurnover, result4.OnlineAnnualTurnover, result4.TurnoverType);
			//AV
			var msc4 = new SoleTraderMedalCalculator(this.m_oDB, this.m_oLog);
			var model4 = msc4.GetInputParameters(customerId, calculationTime);
			this.m_oLog.Info("AV : AnnualTurnover: {0}, HmrcAnnualTurnover: {1}, YodleeAnnualTurnover:{2}, OnlineAnnualTurnover: {3}, Type: {4}", model4.AnnualTurnover, model4.HmrcAnnualTurnover, model4.YodleeAnnualTurnover, model4.OnlineAnnualTurnover, model4.TurnoverType);
			this.m_oLog.Info("--------###-----------SoleTraderMedalCalculator----------------------");

			this.m_oLog.Info("-------------------OfflineLImitedMedalCalculator----------------------");
			MedalResult resultsInput5 = new MedalResult(customerId);
			var calculatorTester5 = new OfflineLimitedMedalCalculator1NoGathering(resultsInput5);
			MedalResult result5 = calculatorTester5.CalculateMedalScore(customerId, calculationTime);
		//	this.m_oLog.Info("Base : AnnualTurnover: {0}, HmrcAnnualTurnover: {1}, YodleeAnnualTurnover:{2}, OnlineAnnualTurnover: {3}, Type: {4}", result5.AnnualTurnover, result5.HmrcAnnualTurnover, result5.BankAnnualTurnover, result5.OnlineAnnualTurnover, result5.TurnoverType);
			//AV
			var msc5 = new OfflineLImitedMedalCalculator(this.m_oDB, this.m_oLog);
			var model5 = msc5.GetInputParameters(customerId, calculationTime);
			this.m_oLog.Info("AV : AnnualTurnover: {0}, HmrcAnnualTurnover: {1}, YodleeAnnualTurnover:{2}, OnlineAnnualTurnover: {3}, Type: {4}", model5.AnnualTurnover, model5.HmrcAnnualTurnover, model5.YodleeAnnualTurnover, model5.OnlineAnnualTurnover, model5.TurnoverType);
			this.m_oLog.Info("--------###-----------OfflineLImitedMedalCalculator----------------------");*/

		}


		


	} // class TestMedal
} // namespace
