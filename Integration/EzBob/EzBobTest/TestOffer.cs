namespace EzBobTest {
	using System;
	using System.Collections.Generic;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation.AutoDecisions.Approval;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Backend.Strategies.OfferCalculation;
	using EzServiceAccessor;
	using EzServiceShortcut;
	using EZBob.DatabaseLib.Model.Database;
	using NUnit.Framework;
	using StructureMap;

	[TestFixture]
	public class TestOffer : BaseTestFixtue {
		[SetUp]
		public new void Init() {
			base.Init();

			ObjectFactory.Configure(x => x.For<IEzServiceAccessor>().Use<EzServiceAccessorShort>());

			Ezbob.Backend.Strategies.Library.Initialize(m_oEnv, m_oDB, m_oLog);
		} // Init

		[Test]
		public void TestMedalScoreMatrix() {
			/*
			const int customerID = 8596;

			DateTime now = DateTime.ParseExact(
				"2015-03-24 01:02:03.0",
				"yyyy-MM-dd HH:mm:ss.f",
				CultureInfo.InvariantCulture
			);

			var companyScores = new List<int> { 0, 5, 15, 25, 35, 96, };
			const long cacID = 17396;

			var customerScores = new List<int> { 0, 400, 550, 700, 693, };
			const long ecdID = 20382;

			foreach (int businessScore in companyScores) {
				m_oDB.ExecuteNonQuery(string.Format(
					"UPDATE CustomerAnalyticsCompany SET Score = {0} WHERE CustomerAnalyticsCompanyID = {1}",
					businessScore,
					cacID
				));

				foreach (int consumerScore in customerScores) {
					m_oDB.ExecuteNonQuery(string.Format(
						"UPDATE ExperianConsumerData SET BureauScore = {0} WHERE Id = {1}",
						consumerScore,
						ecdID
					));

					var instance = new CalculateMedal(customerID, now, false, false);
					instance.Execute();
				} // for each consumer score
			} // for each business score
			*/
		} // TestMedalScoreMatrix

		[Test]
		public void TestOneOffer() {
			// List of { loan amount, has loans }.
			Tuple<int, bool>[] testCases = {
				new Tuple<int, bool>(6000, false),
				new Tuple<int, bool>(6000, true),
				new Tuple<int, bool>(16000, false),
				new Tuple<int, bool>(16000, true),
			};

			foreach (var tc in testCases) {
				var odc = new OfferDualCalculator(
					31,
					DateTime.UtcNow,
					tc.Item1,
					tc.Item2,
					EZBob.DatabaseLib.Model.Database.Medal.Silver,
					3,
					15,
					false
				);

				odc.CalculateOffer();
			} // for each test case
		} // TestOneOffer

		[Test]
		public void TestOfferDualCalc() {
		    var customers = new int[] {31,47,54,56,57,89,185};
		    var now = DateTime.UtcNow;
		    bool passed = true;
			foreach (var customer in customers) {
			    int amount = new Random().Next(1, 50) * 1000;	
				var offerCalc = new OfferDualCalculator(
					customer,
					now,
                    amount,
                    amount > 25000,
					Medal.Gold,
                    3,
                    15,
					true
				);

				var res = offerCalc.CalculateOffer();

			    if (res != null) {
			        m_oLog.Info(@"TestOfferDualCalc
                               Customer {0}
                               Medal class: {1}
                               Amount: {2}
                               Primary: {3}
                               By seek: {4}
                               Error: {5}",
			            customer,
			            res.MedalClassification,
			            res.Amount,
			            offerCalc.Primary,
			            offerCalc.VerifySeek,
			            res.IsError
			            );
                } else {
			        passed = false;
			    }

			} // for each

		    Assert.AreEqual(true, passed);
		} // TestOfferDualCalc
	} // class TestOffer
} // namespace
