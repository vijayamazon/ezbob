
namespace EzBobTest {
	using System;
	using AutomationCalculator.MedalCalculation;
	using Ezbob.Backend.Strategies;
	using Ezbob.Backend.Strategies.AutomationVerification;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using EzServiceAccessor;
	using EzServiceShortcut;
	using NUnit.Framework;
	using StructureMap;
	using System.Collections;
	using System.IO;
	using System.Security.Cryptography;
	using System.Text;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Reject;
	using Ezbob.Utils;

	/// <summary>
	/// The test medal.
	/// </summary>
	[TestFixture]
	public class TestMedal : BaseTestFixtue {
		[SetUp]
		public new void Init() {
			base.Init();
			ObjectFactory.Configure(x => x.For<IEzServiceAccessor>().Use<EzServiceAccessorShort>());
			Library.Initialize(this.m_oEnv, this.m_oDB, this.m_oLog);
		} // Init




		[Test]
		public void TestMaamMedalAndPricing() {
			var stra = new MaamMedalAndPricing(1, 16431);
			stra.Execute();
		} // TestMaamMedalAndPricing


		[Test]
		public void Test_TurnoverForRejectThrougtMarketPlaceTurnoverView() {
			int customerId = 415; // 9582; //6500; //  5935; // 6500; // 9582; //10331;
			DateTime calculationTime = new DateTime(2015, 02, 02);
			Agent agent = new Agent(customerId, this.m_oDB, this.m_oLog);
			agent.Init();
			agent.CalculateTurnoverForReject(customerId, calculationTime);
		}


		[Test]
		public void Test_TurnoverForMedalTest_NH_AV() {
			DateTime calculationTime = DateTime.UtcNow; //new DateTime(2015, 01, 26);
			int customerId = 20366; //19271; // 739; //19856; // 211; // 1871; // //19271 ; //1953;  1826;  //  //  171; //348; // 363; //290; // 178; //;363 // 171: amazon, pp, ebay
			// CustomerId = 211, CalculationTime = 01/01/2014 00:00:00 - have all MP types
			this.m_oLog.Info("START TURNOVER FOR MEDAL customerID: {0}; calculationTime: {1}", customerId, calculationTime.Date);
			;
			MedalResult resultsInput = new MedalResult(customerId, this.m_oLog);
			/*var calculatorTester = new OnlineNonLimitedWithBusinessScoreMedalCalculator1NoGathering(resultsInput);
			MedalResult result = calculatorTester.CalculateMedalScore(customerId, calculationTime);*/
			// both
			//	new CalculateMedal(customerId, calculationTime, false, true).Execute();
			var msc = new OnlineNonLimitedWithBusinessScoreMedalCalculator(this.m_oDB, this.m_oLog);
			//var model = msc.GetInputParameters(customerId, calculationTime);
		}

	
		[Test]
		public void TestMedalDiscrepancy() {
			new CalculateMedal(3977, new DateTime(2015, 2, 1, 0, 0, 0, DateTimeKind.Utc), false, true).Execute();
		} // TestMedalDiscrepancy
	} // class TestMedal
} // namespace
