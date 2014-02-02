namespace LandRegistryLib
{
	using NUnit.Framework;

	[TestFixture]
	class LandRegistryTestFixure
	{
		private static readonly LandRegistryApi Lr = new LandRegistryApi();
		private static readonly LandRegistryTestApi LrTest = new LandRegistryTestApi();
		[Test]
		public void test_prod_enquiry()
		{
			var model = Lr.EnquiryByPropertyDescription(null, null, null, null);
			Assert.NotNull(model.Response);
			Assert.AreEqual(LandRegistryResponseType.Success, model.ResponseType);
		}

		[Test]
		public void test_prod_res()
		{
			/*
			BM253452
			WYK874430
			HW153409
			SGL433128
			SGL410307
			SGL348466
			TGL70137
			*/
			var model = Lr.Res("BM253452", "2348");
			Assert.NotNull(model.Response);
			Assert.AreEqual(LandRegistryResponseType.Success, model.ResponseType);
		}

		[Test]
		public void test_enquiry()
		{
			var model = LrTest.EnquiryByPropertyDescription(null, null, null, null);
			Assert.NotNull(model.Response);
			Assert.AreEqual(LandRegistryResponseType.Success, model.ResponseType);
		}

		[Test]
		public void test_enquiry_poll()
		{
			var model = LrTest.EnquiryByPropertyDescriptionPoll(null);
			Assert.NotNull(model.Response);
			Assert.AreEqual(LandRegistryResponseType.Success, model.ResponseType);
		}

		[Test]
		public void test_res()
		{
			var model = LrTest.Res(null);
			Assert.NotNull(model.Response);
			Assert.AreEqual(LandRegistryResponseType.Success, model.ResponseType);
		}

		[Test]
		public void test_res_poll()
		{
			var model = LrTest.ResPoll(null);
			Assert.NotNull(model.Response);
			Assert.AreEqual(LandRegistryResponseType.Acknowledgement, model.ResponseType);
		}
	}
}
