using EZBob.DatabaseLib.DatabaseWrapper;

namespace EZBob.DatabaseLib.Common {
	public interface IMarketplaceRetrieveDataHelper {
		void CustomerMarketplaceUpdateAction(int nCustomerMarketplaceID);
		void UpdateCustomerMarketplaceFirst(int customerMarketPlaceId);
		void UpdateCustomerMarketplaceRegular(int customerMarketPlaceId);

		IMarketplaceType Marketplace { get; }
		IAnalysisDataInfo GetAnalysisValuesByCustomerMarketPlace(int customerMarketPlaceId);

		IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId);
	}
}