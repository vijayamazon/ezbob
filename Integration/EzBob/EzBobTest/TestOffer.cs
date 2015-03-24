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
		public void TestOfferDualCalc() {
			const int customerID = 23350;

			var dates = new List<string> {
				"2015-03-09 11:51:41.0",
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
