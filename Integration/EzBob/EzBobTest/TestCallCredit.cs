namespace EzBobTest {
	using CallCreditLib;
	using ExperianLib.Tests.Integration;
	using Ezbob.Backend.Strategies;
	using NUnit.Framework;
	using Ezbob.Backend.Strategies.CallCreditStrategy;

	[TestFixture]
	public class TestCallCredit : BaseTest {
		[SetUp]
		public void Init() {
			Library.Initialize(this.oLog4NetCfg.Environment, this.m_oDB, this.m_oLog);
		} // Init

		[Test]
		public void Test_Search07a() {
			CallCreditModelBuilder yy = new CallCreditModelBuilder();
			ParseCallCredit zz = new ParseCallCredit(yy.GetSearch07a(),1);
			//yy.GetSearch07a();
			zz.Execute();
		}


	/*	[Test]
		[Ignore]
		public void TestCallCreditBuilder() {
			var a = 5;
			a ++;

			Assert.Greater(a, 5);
			var model = new {Val = 7};

			Assert.IsNotNull(model);
			Assert.AreEqual(5, model.Val);
		}*/
	}
}
