using EZBob.DatabaseLib.DatabaseWrapper;

namespace EZBob.DatabaseLib.Common
{
	public interface IMarketplaceRetrieveDataHelper
	{		
		void UpdateCustomerMarketplaceFirst( int customerMarketPlaceId );
		void UpdateCustomerMarketplaceRegular( int customerMarketPlaceId );		
		
		IDatabaseMarketplace Marketplace { get; }
		IAnalysisDataInfo GetAnalysisValuesByCustomerMarketPlace( int customerMarketPlaceId );

		IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId);			
	}
}