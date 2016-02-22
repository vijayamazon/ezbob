
namespace EzBobTest {
	using System;
	using AutomationCalculator.MedalCalculation;
	using Ezbob.Backend.Strategies;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using EzServiceAccessor;
	using EzServiceShortcut;
	using NUnit.Framework;
	using StructureMap;

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
		public void TestOneMedal() {
			new CalculateMedal(24195, null, null, DateTime.UtcNow, false, true).Execute();
		} // TestOneMedal

		[Test]
		public void Test_TurnoverForMedalTest_NH_AV() {

			DateTime calculationTime = DateTime.UtcNow;
			int customerId = 24517; // 20658; //24609; // 24613; 
			
			//14858; // 20658; not in tail
			
			this.m_oLog.Info("START TURNOVER FOR MEDAL customerID: {0}; calculationTime: {1}", customerId, calculationTime.Date);
			
			//MedalResult resultsInput = new MedalResult(customerId, this.m_oLog);
			//var calculatorTester = new OnlineNonLimitedWithBusinessScoreMedalCalculator1();
			//calculatorTester.Init(customerId, calculationTime, 0, 0, 0, 0, 0, null, null);
			//MedalResult result = calculatorTester.CalculateMedalScore();

			// AV
			var msc = new OnlineNonLimitedWithBusinessScoreMedalCalculator(this.m_oDB, this.m_oLog);
			var model = msc.GetInputParameters(customerId, calculationTime);
		}

	
		[Test]
		public void TestMedalDiscrepancy() {
			new CalculateMedal(3977, null, null, new DateTime(2015, 2, 1, 0, 0, 0, DateTimeKind.Utc), false, true).Execute();
		} // TestMedalDiscrepancy
	} // class TestMedal
} // namespace