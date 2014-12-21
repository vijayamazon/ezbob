namespace IMailLib {
	using System;
	using NUnit.Framework;

	[TestFixture]
	public class IMailTestFixture {
		private IMailApi api;
		[SetUp]
		public void SetUp(){
			api = new IMailApi();
		}

		[Test]
		public void TestAuthenticate() {
			bool isAuthenticated = api.Authenticate();
			Assert.AreEqual(true, isAuthenticated);
		}

		[Test]
		public void TestListAttachment() {
			var attachments = api.ListAttachment();
			Assert.Greater(0, attachments.Count);
		}

		[Test]
		public void TestGetReturns() {
			var returns = api.GetReturns();
			Console.WriteLine("returns:\n{0}", returns);
		}
	}
}
