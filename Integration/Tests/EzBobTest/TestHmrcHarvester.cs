namespace EzBobTest {
	using System;
	using NUnit.Framework;

	[TestFixture]
	class TestHmrcHarvester : BaseTestFixtue {
		[Test]
		public void TestProdCustomer25489() {
			var hmrcID = new Guid("AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA");

			var vi = Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(hmrcID);

			var ad = new Integration.ChannelGrabberConfig.AccountData(vi) {
				Name = "548850187056",
				Login = "548850187056",
				Password = "", // F2IkH6MtsK24NAylsYXEver94cN+TGpJhJFQkEm23tU=
				LimitDays = 0,
				RealmID = 0,
			};

			var harvester = new Ezbob.HmrcHarvester.Harvester(ad, m_oLog);

			if (harvester.Init()) {
				harvester.Run(false);
				harvester.Done();
			} // if
		} // TestProdCustomer
	} // class TestHmrcHarvester
} // namespace
