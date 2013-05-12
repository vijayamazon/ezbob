using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;

namespace Integration.Play {
	public class PlayDatabaseMarketPlace : DatabaseMarketplaceBase<PlayDatabaseFunctionType> {
		public PlayDatabaseMarketPlace() : base(new PlayServiceInfo()) { }

		public override IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper) {
			return new PlayRetriveDataHelper(helper, this);
		} // GetRetrieveDataHelper

		public override IDatabaseFunctionFactory<PlayDatabaseFunctionType> FunctionFactory {
			get { return new PlayDatabaseFunctionFactory(); }
		} // FunctionFactory
	} // class PlayDatabaseMarketPlace
} // namespace
