// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestMedal.cs" company="">
//     </copyright>
// <summary>
// The limited medal calculator 1 no gathering.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EzBobTest {
	using Ezbob.Backend.Strategies.MedalCalculations;
	using EZBob.DatabaseLib.Model.Database;
	using EzServiceAccessor;
	using EzServiceShortcut;
	using NUnit.Framework;
	using StructureMap;
	using System;
	using Ezbob.Backend.Strategies.AutomationVerification;

	/// <summary>
	/// The limited medal calculator 1 no gathering.
	/// </summary>
	public class LimitedMedalCalculator1NoGathering : LimitedMedalCalculator1 {
		/*
		 * temprary disabled (elinar)
		 * protected override void GatherInputData(DateTime calculationTime) {
			Results = this.resultInput;
		}*/

		/// <summary>
		/// The result input.
		/// </summary>
		private readonly MedalResult resultInput;

		/// <summary>
		/// Initializes a new instance of the <see cref="LimitedMedalCalculator1NoGathering"/> class.
		/// </summary>
		/// <param name="resultInput">The result input.</param>
		public LimitedMedalCalculator1NoGathering(MedalResult resultInput) {
			this.resultInput = resultInput;
		}
	}

	/// <summary>
	/// The online non limited with business score medal calculator 1 no gathering.
	/// </summary>
	public class OnlineNonLimitedWithBusinessScoreMedalCalculator1NoGathering :
		OnlineNonLimitedWithBusinessScoreMedalCalculator1 {
		/// <summary>
		/// The result input.
		/// </summary>
		private readonly MedalResult resultInput;

		/// <summary>
		/// Initializes a new instance of the <see cref="OnlineNonLimitedWithBusinessScoreMedalCalculator1NoGathering"/> class.
		/// </summary>
		/// <param name="resultInput">The result input.</param>
		public OnlineNonLimitedWithBusinessScoreMedalCalculator1NoGathering(MedalResult resultInput) {
			this.resultInput = resultInput;
		}
	}

	/// <summary>
	/// The non limited medal calculator 1 no gathering.
	/// </summary>
	public class NonLimitedMedalCalculator1NoGathering : NonLimitedMedalCalculator1 {
		/// <summary>
		/// The result input.
		/// </summary>
		private readonly MedalResult resultInput;

		/// <summary>
		/// Initializes a new instance of the <see cref="NonLimitedMedalCalculator1NoGathering"/> class.
		/// </summary>
		/// <param name="resultInput">The result input.</param>
		public NonLimitedMedalCalculator1NoGathering(MedalResult resultInput) {
			this.resultInput = resultInput;
		}
	}

	/// <summary>
	/// The test medal.
	/// </summary>
	[TestFixture]
	public class TestMedal : BaseTestFixtue {
		/// <summary>
		/// The init.
		/// </summary>
		[SetUp]
		public new void Init() {
			base.Init();

			ObjectFactory.Configure(x => x.For<IEzServiceAccessor>().Use<EzServiceAccessorShort>());

			Ezbob.Backend.Strategies.Library.Initialize(this.m_oEnv, this.m_oDB, this.m_oLog);
		} // Init

		// [Test]
		/// <summary>
		/// The test_ first medal test.
		/// </summary>
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

		/// <summary>
		/// The test_ turnover for medal test.
		/// </summary>
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
	} // class TestMedal
} // namespace
