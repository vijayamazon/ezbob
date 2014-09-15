namespace EzBob.AmazonServiceLib
{
	using System;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EZBob.DatabaseLib.DatabaseWrapper.Products;
	using Common;
	using Products;
	using Orders.Model;
	using ServiceCalls;
	using UserInfo;
	using CommonLib;
	using System.Collections.Generic;

	public static class AmazonServiceHelper
	{

		public static AmazonOrdersList GetReportOrders(AmazonServiceConnectionInfo connectionInfo, AmazonOrdersRequestInfo requestInfo, ActionAccessType access) {
			var configurator = AmazonServiceConfigurationFactory.CreateServiceReportsConfigurator(connectionInfo);
			return AmazonServiceReports.GetUserOrders(configurator, requestInfo, access);
		}

		public static RequestsCounterData GetListOrders(AmazonServiceConnectionInfo connectionInfo, AmazonOrdersRequestInfo requestInfo, ActionAccessType access, Func<List<AmazonOrderItem>, bool> func)
		{
			var configurator = AmazonServiceConfigurationFactory.CreateServiceOrdersConfigurator( connectionInfo );
			return AmazonServiceOrders.GetListOrders( configurator, requestInfo, access, func);
		}

		public static AmazonOrderItemDetailsList GetListItemsOrdered( AmazonServiceConnectionInfo connectionInfo, AmazonOrdersItemsRequestInfo requestInfo, ActionAccessType access, RequestsCounterData requestCounter )
		{
			var configurator = AmazonServiceConfigurationFactory.CreateServiceOrdersConfigurator( connectionInfo );
			return AmazonServiceOrders.GetListItemsOrdered( configurator, requestInfo, access, requestCounter );
		}

		public static AmazonUserRatingInfo GetUserStatisticsInfo( AmazonUserInfo request )
		{
			return AmazonRateInfo.GetUserRatingInfo( request );
		}

		public static AmazonProductItemBase GetProductCategories(AmazonServiceConnectionInfo connectionInfo, AmazonProductsRequestInfoBySellerSku requestInfo, ActionAccessType access, RequestsCounterData requestCounter)
		{
			var configurator = AmazonServiceConfigurationFactory.CreateServiceProductsConfigurator( connectionInfo );
			return AmazonServiceProducts.GetProductCategories( configurator, requestInfo, access, requestCounter );
		}
	}
}