namespace EzBob.AmazonServiceLib.ServiceCalls
{
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.DatabaseWrapper.Products;
	using Common;
	using Products;
	using Products.Configurator;
	using CommonLib;
	using CommonLib.TrapForThrottlingLogic;
	using MarketplaceWebServiceProducts;
	using MarketplaceWebServiceProducts.Model;

	internal class AmazonServiceProducts
	{
		private readonly IMarketplaceWebServiceProducts _Service;
		private static readonly ITrapForThrottling RequestGetProductCategoriesTrapForThrottling;

		static AmazonServiceProducts()
		{
			RequestGetProductCategoriesTrapForThrottling = TrapForThrottlingController.Create( "GetProductCategories", 20, 5 );
		}

		private AmazonServiceProducts( IMarketplaceWebServiceProducts service )
		{
			_Service = service;
		}

		public static AmazonProductItemBase GetProductCategories(IAmazonServiceProductsConfigurator configurator, AmazonProductsRequestInfoBySellerSku requestInfo, ActionAccessType access, RequestsCounterData requestCounter)
		{
			var service = configurator.AmazonService;

			var data = new AmazonServiceProducts( service );

			return requestInfo.RequestData( data, access, requestCounter );
			
		}

		internal AmazonProductItemBase GetProductCategoriesBySellerSku( AmazonProductsRequestInfoBySellerSku requestInfo, ActionAccessType access, RequestsCounterData requestCounter )
		{
			var productItem = new AmazonProductItemBySellerSku( requestInfo.SellerSku );

			var request = new GetProductCategoriesForSKURequest
			{
				MarketplaceId = requestInfo.MarketplaceId.First(),
				SellerId = requestInfo.MerchantId,
				SellerSKU = requestInfo.SellerSku,
				MWSAuthToken = requestInfo.MWSAuthToken
			};			

			var response = AmazonWaitBeforeRetryHelper.DoServiceAction(
									requestInfo.ErrorRetryingInfo,
									RequestGetProductCategoriesTrapForThrottling,
									"GetProductCategoriesForSKU",
									access,
									requestCounter,
									() => _Service.GetProductCategoriesForSKU( request ) );

			if (response != null && response.IsSetGetProductCategoriesForSKUResult() && response.GetProductCategoriesForSKUResult.IsSetSelf())
			{
				FillCategories( productItem, response.GetProductCategoriesForSKUResult.Self );

			}

			return productItem;
		}

		private void FillCategories(AmazonProductItemBase productItem, List<Categories> categories)
		{
			foreach (var categorie in categories)
			{
				productItem.Categories.Add( new AmazonProductCategory
				{
					CategoryId = categorie.ProductCategoryId,
					CategoryName = categorie.ProductCategoryName,
					Parent = CreateParentFrom( categorie.Parent )
				} );
			}			
		}

		private AmazonProductCategory CreateParentFrom(Categories parent)
		{
			if ( parent == null )
			{
				return null;
			}

			return new AmazonProductCategory
			{
				CategoryId = parent.ProductCategoryId,
				CategoryName = parent.ProductCategoryName,
				Parent = CreateParentFrom( parent.Parent )
			};
		}
	}
}