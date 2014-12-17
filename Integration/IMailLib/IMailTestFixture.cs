namespace IMailLib {
	using NUnit.Framework;

	[TestFixture]
	public class IMailTestFixture {

		[Test]
		public void TestAuthenticate() {
			var api = new IMailApi();
			bool isAuthenticated = api.Authenticate();
			Assert.AreEqual(true, isAuthenticated);
		}
	}
}
