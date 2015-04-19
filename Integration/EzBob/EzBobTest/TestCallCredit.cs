namespace EzBobTest {
	using ExperianLib.Tests.Integration;
	using Ezbob.Backend.ModelsWithDB.CallCredit.CallCreditData;
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
		
		public CallCredit TestGetData() {
			var retrievedata = new CallCreditLib.CallCreditGetData();
			return retrievedata.GetSearch07a();
			
		}

		[SetUp]
		public void init() {
			Library.Initialize(this.oLog4NetCfg.Environment, this.m_oDB, this.m_oLog);
		}

		[Test]
		//[Ignore]
		public void TestSaveToDB() {

			ParseCallCredit testsave = new ParseCallCredit(TestGetData(), 1);
			testsave.Execute();
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
