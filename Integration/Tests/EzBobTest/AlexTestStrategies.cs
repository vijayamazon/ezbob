namespace EzBobTest {
	using System;
	using Ezbob.Backend.Strategies.LogicalGlue;
	using Ezbob.Backend.Strategies.MedalCalculations;
	using Ezbob.Integration.LogicalGlue;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Utils;
	using NUnit.Framework;

	[TestFixture]
	public class AlexTestStrategies : BaseTestFixtue {
		[Test]
		public void TestCaisAccountIsBad() {
			const int tailLength = 3;

			DateTime july1 = new DateTime(2015, 7, 1, 0, 0, 0, DateTimeKind.Utc);
			DateTime june28 = new DateTime(2015, 6, 28, 0, 0, 0, DateTimeKind.Utc);
			DateTime june29 = new DateTime(2015, 6, 29, 0, 0, 0, DateTimeKind.Utc);
			DateTime june20 = new DateTime(2015, 6, 20, 0, 0, 0, DateTimeKind.Utc);
			DateTime may20 = new DateTime(2015, 5, 20, 0, 0, 0, DateTimeKind.Utc);
			DateTime april20 = new DateTime(2015, 4, 20, 0, 0, 0, DateTimeKind.Utc);
			DateTime february20 = new DateTime(2015, 2, 20, 0, 0, 0, DateTimeKind.Utc);
			DateTime january20 = new DateTime(2015, 1, 20, 0, 0, 0, DateTimeKind.Utc);
			DateTime november20 = new DateTime(2014, 11, 20, 0, 0, 0, DateTimeKind.Utc);

			DateTime lastJuly1 = new DateTime(2014, 7, 1, 0, 0, 0, DateTimeKind.Utc);
			DateTime lastJune1 = new DateTime(2014, 6, 1, 0, 0, 0, DateTimeKind.Utc);
			DateTime lastMay1 = new DateTime(2014, 5, 1, 0, 0, 0, DateTimeKind.Utc);
			DateTime lastMarch1 = new DateTime(2014, 3, 1, 0, 0, 0, DateTimeKind.Utc);

			Assert.AreEqual(lastJuly1, MiscUtils.GetPeriodAgo(june29, june28, tailLength, null));
			Assert.AreEqual(lastJune1, MiscUtils.GetPeriodAgo(june29, june20, tailLength, null));
			Assert.AreEqual(lastJune1, MiscUtils.GetPeriodAgo(june20, june29, tailLength, null));
			Assert.AreEqual(lastJune1, MiscUtils.GetPeriodAgo(june20, june20, tailLength, null));

			Assert.AreEqual(lastJuly1, MiscUtils.GetPeriodAgo(june29, june28, tailLength, july1));
			Assert.AreEqual(lastJune1, MiscUtils.GetPeriodAgo(june29, june20, tailLength, july1));
			Assert.AreEqual(lastJune1, MiscUtils.GetPeriodAgo(june20, june29, tailLength, july1));
			Assert.AreEqual(lastJune1, MiscUtils.GetPeriodAgo(june20, june20, tailLength, july1));

			Assert.AreEqual(lastJune1, MiscUtils.GetPeriodAgo(june29, june28, tailLength, may20));
			Assert.AreEqual(lastJune1, MiscUtils.GetPeriodAgo(june29, june20, tailLength, may20));
			Assert.AreEqual(lastJune1, MiscUtils.GetPeriodAgo(june20, june29, tailLength, may20));
			Assert.AreEqual(lastJune1, MiscUtils.GetPeriodAgo(june20, june20, tailLength, may20));

			Assert.AreEqual(lastMay1, MiscUtils.GetPeriodAgo(june29, june28, tailLength, april20));
			Assert.AreEqual(lastMay1, MiscUtils.GetPeriodAgo(june29, june20, tailLength, april20));
			Assert.AreEqual(lastMay1, MiscUtils.GetPeriodAgo(june20, june29, tailLength, april20));
			Assert.AreEqual(lastMay1, MiscUtils.GetPeriodAgo(june20, june20, tailLength, april20));

			Assert.AreEqual(lastJuly1, MiscUtils.GetPeriodAgo(june29, june28, tailLength, february20));
			Assert.AreEqual(lastMarch1, MiscUtils.GetPeriodAgo(june29, june20, tailLength, february20));
			Assert.AreEqual(lastMarch1, MiscUtils.GetPeriodAgo(june20, june29, tailLength, february20));
			Assert.AreEqual(lastMarch1, MiscUtils.GetPeriodAgo(june20, june20, tailLength, february20));

			Assert.AreEqual(lastJuly1, MiscUtils.GetPeriodAgo(june29, june28, tailLength, january20));
			Assert.AreEqual(lastJune1, MiscUtils.GetPeriodAgo(june29, june20, tailLength, january20));
			Assert.AreEqual(lastJune1, MiscUtils.GetPeriodAgo(june20, june29, tailLength, january20));
			Assert.AreEqual(lastJune1, MiscUtils.GetPeriodAgo(june20, june20, tailLength, january20));

			Assert.AreEqual(lastJuly1, MiscUtils.GetPeriodAgo(june29, june28, tailLength, november20));
			Assert.AreEqual(lastJune1, MiscUtils.GetPeriodAgo(june29, june20, tailLength, november20));
			Assert.AreEqual(lastJune1, MiscUtils.GetPeriodAgo(june20, june29, tailLength, november20));
			Assert.AreEqual(lastJune1, MiscUtils.GetPeriodAgo(june20, june20, tailLength, november20));
		} // TestCaisAccountIsBad

		[Test]
		public void TestMedal() {
			var x = new CalculateMedal(29486, null, null, DateTime.UtcNow, false, false);
			x.Execute();
		} // TestMedal

		[Test]
		public void TestGetLatestKnownLogicalGlue() {
			var lgEngine = InjectorStub.GetEngine();
			lgEngine.GetInference(1417, 0, false, GetInferenceMode.DownloadIfOld);

			var stra = new GetLatestKnownInference(1417, null, false);
			stra.Execute();
			m_oLog.Debug("Inference is: {0}", stra.Inference);
		} // TestGetLatestKnownLogicalGlue

		[Test]
		public void TestGetLogicalGlueHistory() {
			var stra = new GetHistoryInferences(3406);
			stra.Execute();
			foreach (var inference in stra.Inferences) {
				m_oLog.Debug(inference);
			}
		} // TestGetLatestKnownLogicalGlue
	} // class AlexTestStrategies
} // namespace