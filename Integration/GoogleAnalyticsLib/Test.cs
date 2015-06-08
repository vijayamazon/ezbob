namespace GoogleAnalyticsLib
{
	using System;
	using Ezbob.Logger;
	using NUnit.Framework;

	[TestFixture]
	class Test
	{
		[Test]
		public void TestCertificate()
		{
			//ezbob ga:60953365
			//everline ga:98329566
			GetData("ga:60953365", "ezbob");
			GetData("ga:98329566", "everline");

		}

		private void GetData(string profileID,string name) {
			var ga = new GoogleAnalytics(new ConsoleLog());
			ga.Init(DateTime.Now, "08 a1 90 d7 e7 b6 1e 5c df a6 33 01 e5 28 13 4d 36 99 f0 96", profileID);
			var lastChanged = DateTime.UtcNow;
			var firstOfMonth = new DateTime(lastChanged.Year, lastChanged.Month, 1);
			var todayAnalytics = ga.FetchByCountry(DateTime.Today, lastChanged);
			var monthToDateAnalytics = ga.FetchByCountry(firstOfMonth, lastChanged);
			if (todayAnalytics.ContainsKey("United Kingdom")) {
				Console.WriteLine(name + todayAnalytics["United Kingdom"].Users);
			}

			if (monthToDateAnalytics.ContainsKey("United Kingdom")) {
				Console.WriteLine(name + monthToDateAnalytics["United Kingdom"].Users);
			}
		}
	}
}
