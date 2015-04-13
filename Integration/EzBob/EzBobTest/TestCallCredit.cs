namespace EzBobTest {
	using NUnit.Framework;

	[TestFixture]
	public class TestCallCredit {



		[Test]
		public void TestCallCreditBuilder() {
			var a = 5;
			a ++;

			Assert.Greater(a, 5);
			var model = new {Val = 7};

			Assert.IsNotNull(model);
			Assert.AreEqual(5, model.Val);
		}
	}
}
