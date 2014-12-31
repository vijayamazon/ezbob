namespace EzBobTest {
	using System;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using EzServiceAccessor;
	using EzServiceShortcut;
	using EZBob.DatabaseLib.Model.Database;
	using NUnit.Framework;
	using StructureMap;
	using MedalType = Ezbob.Backend.Strategies.MedalCalculations.MedalType;

	public class LimitedMedalCalculator1NoGathering : LimitedMedalCalculator1 {
		public LimitedMedalCalculator1NoGathering(MedalResult resultInput) {
			this.resultInput = resultInput;
		}

		protected override void GatherInputData(DateTime calculationTime) {
			Results = this.resultInput;
		}

		private readonly MedalResult resultInput;
	}

	[TestFixture]
	public class TestMedal : BaseTestFixtue {
		[SetUp]
		public new void Init() {
			base.Init();

			ObjectFactory.Configure(x => x.For<IEzServiceAccessor>()
				.Use<EzServiceAccessorShort>());

			Ezbob.Backend.Strategies.Library.Initialize(this.m_oEnv, this.m_oDB, this.m_oLog);
		} // Init

		[Test]
		public void Test_FirstMedalTest() {
			DateTime calculationTime = new DateTime(2014, 11, 06);
			int customerId = 18112;

			MedalResult resultsInput = new MedalResult(customerId);
			resultsInput.CalculationTime = calculationTime;
			resultsInput.MedalType = MedalType.Limited;
			resultsInput.BusinessScore = 0;
			resultsInput.TangibleEquityValue = 0;
			resultsInput.BusinessSeniority = null;
			resultsInput.ConsumerScore = 0;
			resultsInput.MaritalStatus = MaritalStatus.Single;
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
	}
}
