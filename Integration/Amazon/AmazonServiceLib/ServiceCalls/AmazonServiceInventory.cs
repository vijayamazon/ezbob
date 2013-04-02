using System;
using System.Linq;
using EzBob.AmazonServiceLib.Inventory.Configurator;
using EzBob.AmazonServiceLib.Inventory.Model;
using FBAInventoryServiceMWS.Service;
using FBAInventoryServiceMWS.Service.Model;

namespace EzBob.AmazonServiceLib.ServiceCalls
{

	[Obsolete("Not used", true)]
	class AmazonServiceInventory
	{
		public static AmazonInventorySupplyList GetInventoryList(IAmazonServiceInventoryConfigurator configurator, AmazonInventoryRequestInfo requestInfo)
		{
			var service = configurator.AmazonService;
			

			var inv = new AmazonServiceInventory( service );

			inv.RetrieveData( requestInfo );

			return inv.InventoryList;
		}

		private readonly IFbaInventoryServiceMws _Service;
		private readonly AmazonInventorySupplyList _InventoryList;


		private AmazonServiceInventory( IFbaInventoryServiceMws service )
		{
			_Service = service;
			_InventoryList = new AmazonInventorySupplyList();
		}

		private void RetrieveData(AmazonInventoryRequestInfo requestInfo)
		{
			var marketplaceIds = requestInfo.MarketplaceId;

			if ( marketplaceIds.Count > 0 )
			{
				foreach ( var marketplaceId in marketplaceIds )
				{
					var request = new ListInventorySupplyRequest
					{
						SellerId = requestInfo.MerchantId,
						Marketplace = marketplaceId,
					};

					if ( requestInfo.StartDate.HasValue )
					{
						request.QueryStartDateTime = requestInfo.StartDate.Value.ToUniversalTime();
					}

					RetrieveData( request );
				}
			}
			else
			{
				var request = new ListInventorySupplyRequest
				{
					SellerId = requestInfo.MerchantId,
				};

				if ( requestInfo.StartDate.HasValue )
				{
					request.QueryStartDateTime = requestInfo.StartDate.Value.ToUniversalTime();
				}

				RetrieveData( request );
			}
		}

		private void RetrieveData(ListInventorySupplyRequest request)
		{
			var response = _Service.ListInventorySupply( request );
			if ( response.IsSetListInventorySupplyResult() )
			{
				var result = response.ListInventorySupplyResult;

				if ( result.IsSetInventorySupplyList() )
				{
					ParceInventoryInfos( result.InventorySupplyList );
				}

				if ( result.IsSetNextToken() )
				{
					GetInventoryByNextToken( result.NextToken, request.SellerId );
				}
			}
		}

		private void GetInventoryByNextToken(string nextToken, string sellerId)
		{
			var req = new ListInventorySupplyByNextTokenRequest()
			{
				NextToken = nextToken,
				SellerId = sellerId
			};
			var response = _Service.ListInventorySupplyByNextToken( req );

			if ( response.IsSetListInventorySupplyByNextTokenResult() )
			{
				var result = response.ListInventorySupplyByNextTokenResult;
				if (result.IsSetInventorySupplyList())
				{
					ParceInventoryInfos( result.InventorySupplyList );
				}

				if (result.IsSetNextToken())
				{
					GetInventoryByNextToken(result.NextToken, sellerId);
				}
			}
		}

		private void ParceInventoryInfos(InventorySupplyList data)
		{
			data.member.AsParallel().ForAll( _InventoryList.Add );
		}

		private AmazonInventorySupplyList InventoryList
		{
			get { return _InventoryList; }
		}
	}
}
