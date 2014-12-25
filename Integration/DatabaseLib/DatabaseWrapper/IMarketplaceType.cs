namespace EZBob.DatabaseLib.DatabaseWrapper {
	using EZBob.DatabaseLib.Common;
	using EzBob.CommonLib;

	public interface IMarketplaceType : IMarketplaceInfo {
		IMarketplaceRetrieveDataHelper GetRetrieveDataHelper(DatabaseDataHelper helper);
	}
}