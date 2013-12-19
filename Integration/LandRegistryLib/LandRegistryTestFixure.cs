namespace LandRegistryLib
{
	using NUnit.Framework;

	[TestFixture]
	class LandRegistryTestFixure
	{
		private static readonly LandRegistryApi Lr = new LandRegistryApi();

		[Test]
		public void test_enquiry()
		{
			var model = Lr.EnquiryByPropertyDescription(null, null, null, null);
			Assert.NotNull(model.Response);
			Assert.AreEqual(LandRegistryResponseType.Success, model.ResponseType);
		}

		[Test]
		public void test_enquiry_poll()
		{
			var model = Lr.EnquiryByPropertyDescriptionPoll(null);
			Assert.NotNull(model.Response);
			Assert.AreEqual(LandRegistryResponseType.Success, model.ResponseType);
		}

		[Test]
		public void test_res()
		{
			var model = Lr.Res(null);
			Assert.NotNull(model.Response);
			Assert.AreEqual(LandRegistryResponseType.Success, model.ResponseType);
		}

		[Test]
		public void test_res_poll()
		{
			var model = Lr.ResPoll(null);
			Assert.NotNull(model.Response);
			Assert.AreEqual(LandRegistryResponseType.Poll, model.ResponseType);
		}
	}
}
