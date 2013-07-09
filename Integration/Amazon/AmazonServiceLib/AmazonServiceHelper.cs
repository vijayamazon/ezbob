using System.Collections.Generic;
using EZBob.DatabaseLib.DatabaseWrapper.Order;
using EZBob.DatabaseLib.DatabaseWrapper.Products;
using EzBob.AmazonServiceLib.Common;
using EzBob.AmazonServiceLib.Inventory.Configurator;
using EzBob.AmazonServiceLib.Inventory.Model;
using EzBob.AmazonServiceLib.MarketWebService.Configurator;
using EzBob.AmazonServiceLib.MarketWebService.Model;
using EzBob.AmazonServiceLib.Orders.Configurator;
using EzBob.AmazonServiceLib.Orders.Model;
using EzBob.AmazonServiceLib.ServiceCalls;
using EzBob.AmazonServiceLib.UserInfo;
using EzBob.CommonLib;

namespace EzBob.AmazonServiceLib
{
	public static class AmazonServiceHelper
	{
		public static AmazonOrdersList2 GetListOrders( AmazonServiceConnectionInfo connectionInfo, AmazonOrdersRequestInfo requestInfo, ActionAccessType access )
		{
			var configurator = AmazonServiceConfigurationFactory.CreateServiceOrdersConfigurator( connectionInfo );
			return AmazonServiceOrders.GetListOrders( configurator, requestInfo, access );
		}

		public static AmazonOrderItemDetailsList GetListItemsOrdered( AmazonServiceConnectionInfo connectionInfo, AmazonOrdersItemsRequestInfo requestInfo, ActionAccessType access, RequestsCounterData requestCounter )
		{
			var configurator = AmazonServiceConfigurationFactory.CreateServiceOrdersConfigurator( connectionInfo );
			return AmazonServiceOrders.GetListItemsOrdered( configurator, requestInfo, access, requestCounter );
		}

		/*public static AmazonInventorySupplyList GetInventoryList( AmazonServiceConnectionInfo connectionInfo, AmazonInventoryRequestInfo requestInfo )
		{
			return AmazonServiceInventory.GetInventoryList( configurator, requestInfo );
		}*/

		public static AmazonUserRatingInfo GetUserStatisticsInfo( AmazonUserInfo request )
		{
			return AmazonRateInfo.GetUserRatingInfo( request );
		}

		/*public static AmazonOrdersList GetOrdersReport( AmazonServiceConnectionInfo connectionInfo, AmazonOrdersRequestInfo requestInfo, ActionAccessType access )
		{
			return AmazonServiceReports.GetUserOrders( configurator, requestInfo, access );
		}*/

		public static AmazonInventoryData GetUserInventorList( AmazonServiceConnectionInfo connectionInfo, AmazonInventoryRequestInfo requestInfo, ActionAccessType access )
		{
			var configurator = AmazonServiceConfigurationFactory.CreateServiceReportsConfigurator( connectionInfo );
			return AmazonServiceReports.GetUserInventory( configurator, requestInfo, access );
		}

		public static AmazonProductItemBase GetProductCategories( AmazonServiceConnectionInfo connectionInfo, AmazonProductsRequestBase requestInfo, ActionAccessType access, RequestsCounterData requestCounter )
		{
			var configurator = AmazonServiceConfigurationFactory.CreateServiceProductsConfigurator( connectionInfo );
			return AmazonServiceProducts.GetProductCategories( configurator, requestInfo, access, requestCounter );
		}

	}

	
}