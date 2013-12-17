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

		[Test]
		[Ignore]
		public void test_get_zoopla_estimate()
		{
			var z = new ZooplaEstimate();
			//var estimate = z.GetEstimate("16 Upperkirkgate, Aberdeen AB10 1BA");
			var estimate = z.GetEstimate("Flat B 2 Upperkirkgate Aberdeen AB10 1BA");
			Assert.NotNull(estimate);
		}

		[Test]
		[Ignore]
		public void test_get_zoopla_estimate_sorry()
		{
			var z = new ZooplaEstimate();
			var estimate = z.GetEstimate("145 – 157 John Street, London EC1V 4PW");
			Assert.NotNull(estimate);
		}
	}
}
