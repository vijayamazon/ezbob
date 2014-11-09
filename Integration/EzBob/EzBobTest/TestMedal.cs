namespace EzBobTest
{
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using EzBob.Backend.Strategies.MedalCalculations;
	using EzServiceAccessor;
	using EzServiceShortcut;
	using NUnit.Framework;
	using StructureMap;

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
			var limitedMedalCalculator1 = new LimitedMedalCalculator1(m_oDB, m_oLog);
			ScoreResult resultsInput = limitedMedalCalculator1.CreateResultWithInitialWeights(customerId, calculationTime);
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

			limitedMedalCalculator1.ReplaceGather(resultsInput, true, -1000, 1, 0, 1);
			ScoreResult result1 = limitedMedalCalculator1.CalculateMedalScore(customerId, calculationTime);
			Assert.AreEqual(result1.NetWorthGrade, 1);
			Assert.AreEqual(result1.EzbobSeniorityGrade, 3);
		}
	}
}