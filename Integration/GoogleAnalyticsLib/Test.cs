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
			var ga = new GoogleAnalytics(new ConsoleLog());
			ga.Init(DateTime.Now, "08 a1 90 d7 e7 b6 1e 5c df a6 33 01 e5 28 13 4d 36 99 f0 96");
		}
	}
}
