namespace ZooplaLib
{
	using System;
	using NUnit.Framework;

	[TestFixture]
	class ZooplaTestFixure
	{
		[Test]
        [Ignore("Ignore this fixture")]
		public void test_get_average_prices()
		{
			var postcode = "EC1V4PW";
			var z = new ZooplaApi();
			var res = z.GetAverageSoldPrices(postcode);

		}

		[Test]
        [Ignore("Ignore this fixture")]
		public void test_get_graphs()
		{
			var postcode = "NR3 2SY";
			//postcode = "a";
			var z = new ZooplaApi();
			var res = z.GetAreaValueGraphs(postcode);
		}

		[Test]
        [Ignore("Ignore this fixture")]
		public void test_get_zoopla_estimate()
		{
			var z = new ZooplaEstimate();
			//var estimate = z.GetEstimate("16 Upperkirkgate, Aberdeen AB10 1BA");
			var estimate = z.GetEstimate("Flat+B+2+Upperkirkgate+Aberdeen+AB10+1BA");
			Assert.AreNotEqual("Address not found",estimate);
			Console.WriteLine("estimate {0}", estimate);
			estimate = z.GetEstimate("9 Whitebeam Park Huddersfield HD2 2GZ");
			Console.WriteLine("estimate {0}", estimate);
			Assert.AreNotEqual("Address not found", estimate);
			estimate = z.GetEstimate("14 Weetwood Crescent Leeds West Yorkshire LS16 5NS");
			Console.WriteLine("estimate {0}", estimate);
			Assert.AreNotEqual("Address not found", estimate);
		}

		[Test]
        [Ignore("Ignore this fixture")]
		public void test_get_zoopla_estimate_sorry()
		{
			var z = new ZooplaEstimate();
			var estimate = z.GetEstimate("145 – 157 John Street London EC1V 4PW");
			Assert.AreEqual("Address not found", estimate);

			estimate = z.GetEstimate("11 The Barrows Francis Street Brighton East Sussex BN1 4ZJ");
			Assert.AreEqual("No Estimate", estimate);
		}
	}
}
