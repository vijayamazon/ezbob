namespace EzBob.PayPal {
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EzBob.PayPalServiceLib;

	public class PayPalDatabaseMarketPlace : DatabaseMarketplaceBaseBase {
		public PayPalDatabaseMarketPlace()
			: base(new PayPalServiceInfo()) {
		}

		public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper) {
			return new PayPalRetriveDataHelper(helper, this);
		}
	}
}
