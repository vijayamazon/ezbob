using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;

namespace Integration.Volusion {
	public class VolusionDatabaseMarketPlace : DatabaseMarketplaceBase<VolusionDatabaseFunctionType> {
        public VolusionDatabaseMarketPlace() : base(new VolusionServiceInfo()) {}

        public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper) {
            return new VolusionRetriveDataHelper(helper, this);
        } // GetRetrieveDataHelper

        public override IDatabaseFunctionFactory<VolusionDatabaseFunctionType> FunctionFactory { get {
			return new VolusionDatabaseFunctionFactory();
		}} // FunctionFactory
    } // class VolusionDatabaseMarketPlace
} // namespace
