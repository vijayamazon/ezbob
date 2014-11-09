namespace EzBobTest
{
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.Backend.Strategies.MedalCalculations;
	using EzServiceAccessor;
	using EzServiceShortcut;
	using Ezbob.Database;
	using Ezbob.Logger;
	using NUnit.Framework;
	using StructureMap;

	public class LimitedMedalCalculator1NoGathering : LimitedMedalCalculator1
	{
		private readonly ScoreResult resultInput;

		public LimitedMedalCalculator1NoGathering(ScoreResult resultInput, AConnection db, ASafeLog log) : base(db, log)
		{
			this.resultInput = resultInput;
		}

		protected override void GatherInputData(int customerId, DateTime calculationTime)
		{
			Results = resultInput;
		}
	}

	[TestFixture]
	public class TestMedal : BaseTestFixtue
	{
		[SetUp]
		public new void Init() {
			base.Init();

			ObjectFactory.Configure(x => x.For<IEzServiceAccessor>().Use<EzServiceAccessorShort>());

			EzServiceAccessorShort.Set(m_oDB, m_oLog);
		} // Init

		[Test]
		public void Test_FirstMedalTest()
		{
			DateTime calculationTime = new DateTime(2014, 11, 06);
			int customerId = 18112;

			ScoreResult resultsInput = new ScoreResult();
			resultsInput.CustomerId = customerId;
			resultsInput.CalculationTime = calculationTime;
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

			var calculatorTester = new LimitedMedalCalculator1NoGathering(resultsInput, m_oDB, m_oLog);

			ScoreResult resultsOutput = calculatorTester.CalculateMedalScore(customerId, calculationTime);
			Assert.AreEqual(resultsOutput.NetWorthGrade, 1);
			Assert.AreEqual(resultsOutput.EzbobSeniorityGrade, 3);
		}
	}
}