namespace ZooplaLib
{
	using NUnit.Framework;

	[TestFixture]
	class ZooplaTestFixure
	{
		[Test]
		[Ignore]
		public void test_get_average_prices()
		{
			var postcode = "EC1V4PW";
			var z = new ZooplaApi();
			var res = z.GetAverageSoldPrices(postcode);

		}

		[Test]
		[Ignore]
		public void test_get_graphs()
		{
			var postcode = "NR3 2SY";
			postcode = "a";
			var z = new ZooplaApi();
			var res = z.GetAreaValueGraphs(postcode);
		}
	}
}
