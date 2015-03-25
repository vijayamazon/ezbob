namespace EzBobTest {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Backend.Strategies.Extensions;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Backend.Strategies.OfferCalculation;
	using EzServiceAccessor;
	using EzServiceShortcut;
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
			var odc = new OfferDualCalculator(
				18164,
				new DateTime(2015, 3, 20, 15, 18, 37, DateTimeKind.Utc),
				14100,
				true,
				EZBob.DatabaseLib.Model.Database.Medal.Silver,
				false
			);

			odc.CalculateOffer();
		} // TestOneOffer

		[Test]
		public void TestOfferDualCalc() {
			const int customerID = 18164;

			var dates = new List<string> {
				"2014-10-27 10:09:24.0",
				"2014-10-27 13:51:40.0",
				"2014-10-27 14:42:09.0",
				"2015-03-20 15:03:27.0",
			};

			var res = new SortedDictionary<DateTime, MedalOffer>();

			foreach (var aDate in dates) {
				DateTime now = DateTime.ParseExact(
					aDate,
					"yyyy-MM-dd HH:mm:ss.f",
					CultureInfo.InvariantCulture
				);

				var instance = new CalculateMedal(customerID, now, false, false);
				instance.Execute();

				var offerCalc = new OfferDualCalculator(
					customerID,
					now,
					instance.Result.RoundOfferedAmount(),
					instance.Result.NumOfLoans > 0,
					instance.Result.MedalClassification,
					false
				);

				if (instance.Result.RoundOfferedAmount() > 0)
					offerCalc.CalculateOffer();

				res[now] = new MedalOffer(instance.Result, offerCalc);
			} // for each

			foreach (var pair in res) {
				m_oLog.Info(
					"TestOfferDualCalc:\n" +
					"\tOffer for customer {0} at {1}:\n" +
					"\t\tPrimary: {5}\n" +
					"\t\tBy seek: {6}\n" +
					"\t\tBy boundary: {7}\n" +
					"\tMedal for customer {0} at {1}:\n" +
					"\t\tAmount: {2}\n" +
					"\t\tHas loans: {3}\n" +
					"\t\tMedal class: {4}\n",
					customerID,
					pair.Key.MomentStr(),
					pair.Value.Medal.RoundOfferedAmount(),
					pair.Value.Medal.NumOfLoans > 0,
					pair.Value.Medal.MedalClassification,
					pair.Value.Offer.Primary,
					pair.Value.Offer.VerifySeek,
					pair.Value.Offer.VerifyBoundaries
				);
			} // for each
		} // TestOfferDualCalc

		private class MedalOffer {
			public MedalOffer(MedalResult m, OfferDualCalculator o) {
				Medal = m;
				Offer = o;
			} // constructor

			public MedalResult Medal { get; private set; }
			public OfferDualCalculator Offer { get; private set; }
		} // MedalOffer
	} // class TestOffer
} // namespace
