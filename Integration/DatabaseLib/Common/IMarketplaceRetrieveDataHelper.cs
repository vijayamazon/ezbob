namespace EZBob.DatabaseLib.Common {
	using EZBob.DatabaseLib.DatabaseWrapper;

	public interface IMarketplaceRetrieveDataHelper {
		IMarketplaceType Marketplace { get; }

		void CustomerMarketplaceUpdateAction(int nCustomerMarketplaceID);

		IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId);

		void Update(int nCustomerMarketplaceID);
		void UpdateCustomerMarketplaceFirst(int customerMarketPlaceId);
		void UpdateCustomerMarketplaceRegular(int customerMarketPlaceId);
	}
}