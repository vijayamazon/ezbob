namespace EzBob.AmazonServiceLib.ServiceCalls
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using Common;
	using Orders.Configurator;
	using Orders.Model;
	using CommonLib;
	using CommonLib.TrapForThrottlingLogic;
	using MarketplaceWebServiceOrders.MarketplaceWebServiceOrders;
	using MarketplaceWebServiceOrders.MarketplaceWebServiceOrders.Model;

	internal class AmazonServiceOrders
	{
		private readonly IMarketplaceWebServiceOrders _Service;
		private readonly static ITrapForThrottling ListOrdersTrapForThrottling;
		private readonly static ITrapForThrottling ListOrderItemsTrapForThrottling;

		static AmazonServiceOrders()
		{
			ListOrdersTrapForThrottling = TrapForThrottlingController.Create( "List Orders", 6 );
			ListOrderItemsTrapForThrottling = TrapForThrottlingController.Create( "List Order Items", 30, 2 );
		}

		public static AmazonOrdersList2 GetListOrders( IAmazonServiceOrdersConfigurator configurator, AmazonOrdersRequestInfo requestInfo, ActionAccessType access )
		{
			var service = configurator.AmazonService;
			
			WriteToLog( string.Format( "GetListOrders - SellerId: {0}, CreatedAfter (UTC) {1}, amazon mps id {2}, customer id {3} ", requestInfo.MerchantId, requestInfo.StartDate, requestInfo.GetMarketPlacesString(), requestInfo.CustomerId ) );

			return new AmazonServiceOrders( service ).GetListOrders( requestInfo, access );
		}

		public static AmazonOrderItemDetailsList GetListItemsOrdered( IAmazonServiceOrdersConfigurator configurator, AmazonOrdersItemsRequestInfo requestInfo, ActionAccessType access, RequestsCounterData requestCounter )
		{			
			return	new AmazonServiceOrders( configurator.AmazonService ).GetListItemsOrdered( requestInfo, access, requestCounter);
		}

		private AmazonServiceOrders( IMarketplaceWebServiceOrders service )
		{
			_Service = service;
		}

		private AmazonOrdersList2 GetListOrders( AmazonOrdersRequestInfo requestInfo, ActionAccessType access )
		{
			
			var request = new ListOrdersRequest
			{
				MarketplaceId = new MarketplaceIdList
				{
					Id = requestInfo.MarketplaceId
				},
				SellerId = requestInfo.MerchantId,

			};
			
			if ( requestInfo.StartDate.HasValue )
			{
				request.CreatedAfter = requestInfo.StartDate.Value.ToUniversalTime();
			}
			var responseCounter = new RequestsCounterData();
			var response = AmazonWaitBeforeRetryHelper.DoServiceAction(
									requestInfo.ErrorRetryingInfo,
									ListOrdersTrapForThrottling,
									"ListOrders",
									access,
									responseCounter,
									() => _Service.ListOrders( request ) );

			if (response == null || response.ListOrdersResult == null || !response.IsSetListOrdersResult())
			{
				return null;
			}

			var result = response.ListOrdersResult;

			var ordersList = new AmazonOrdersList2( response.ListOrdersResult.CreatedBefore );

			if ( result.IsSetOrders() )
			{
				ParceOrdersInfo( ordersList, result.Orders, request.SellerId, access, requestInfo.CustomerId );
			}

			if ( result.IsSetNextToken() )
			{
				GetOrdersByNextToken( ordersList, requestInfo, result.NextToken, access, responseCounter );
			}
			ordersList.RequestsCounter = responseCounter;
			return ordersList;
		}

		private void GetOrdersByNextToken(AmazonOrdersList2 ordersList, AmazonOrdersRequestInfo requestInfo, string nextToken, ActionAccessType access, RequestsCounterData responseCounter)
		{
			var sellerId = requestInfo.MerchantId;

			var req = new ListOrdersByNextTokenRequest
			          	{
			          		NextToken = nextToken,
			          		SellerId = sellerId
			          	};
			var response = AmazonWaitBeforeRetryHelper.DoServiceAction(
									requestInfo.ErrorRetryingInfo,
									ListOrdersTrapForThrottling,
									"ListOrdersByNextToken",
									access,
									responseCounter,
									() => _Service.ListOrdersByNextToken( req ) );

			if ( response != null && response.IsSetListOrdersByNextTokenResult() )
			{
				var result = response.ListOrdersByNextTokenResult;
				if (result.IsSetOrders())
				{
					ParceOrdersInfo( ordersList, result.Orders, sellerId, access, requestInfo.CustomerId );
				}

				if (result.IsSetNextToken())
				{
					GetOrdersByNextToken( ordersList, requestInfo, result.NextToken, access, responseCounter);
				}
			}
		}

		private void ParceOrdersInfo( AmazonOrdersList2 ordersList, OrderList orders, string sellerId, ActionAccessType access, int customerId)
		{
			if (orders.Order.Any())
			{
				DateTime? firstDate = orders.Order.First().IsSetPurchaseDate() ? orders.Order[0].PurchaseDate : (DateTime?) null;
				DateTime? lastDate = orders.Order.Last().IsSetPurchaseDate() ? orders.Order[0].PurchaseDate : (DateTime?) null;
				WriteToLog(
					string.Format(
						"Amazon ParceOrdersInfo customerId {2}, sellerId {0} number of orders {1}, first order date {3} last order date {4}",
						sellerId, ordersList.Count, customerId, firstDate, lastDate));
			}
			else
			{
				WriteToLog(
					string.Format("Amazon ParceOrdersInfo customerId {1}, sellerId {0} number of orders 0", sellerId, customerId));
			}
			orders.Order.AsParallel().ForAll( o => ordersList.Add( ParceOrder( o ) ) );
		}

		private AmazonOrderItem2 ParceOrder( Order order )
		{
			var orderInfo = new AmazonOrderItem2();
			
			if ( order.IsSetAmazonOrderId() )
			{
				orderInfo.AmazonOrderId = order.AmazonOrderId;
			}
			if ( order.IsSetSellerOrderId() )
			{
				orderInfo.SellerOrderId = order.SellerOrderId;
			}
			if ( order.IsSetPurchaseDate() )
			{
				orderInfo.PurchaseDate = order.PurchaseDate;
			}
			if ( order.IsSetLastUpdateDate() )
			{
				orderInfo.LastUpdateDate = order.LastUpdateDate;
			}
			if ( order.IsSetOrderStatus() )
			{
				orderInfo.OrderStatus = ConvertOrderStatus(order.OrderStatus);
			}
			
			orderInfo.OrderTotal = ConvertToAmount( order.IsSetOrderTotal(), order.OrderTotal );

			if ( order.IsSetNumberOfItemsShipped() )
			{
				orderInfo.NumberOfItemsShipped = (int)order.NumberOfItemsShipped;
			}
			if ( order.IsSetNumberOfItemsUnshipped() )
			{
				orderInfo.NumberOfItemsUnshipped = (int)order.NumberOfItemsUnshipped;
			}
			
			if ( order.IsSetPaymentExecutionDetail() )
			{
				orderInfo.PaymentsInfo = new AmazonOrderItem2PaymentsInfoList();

				foreach ( PaymentExecutionDetailItem paymentExecutionDetailItem in order.PaymentExecutionDetail.PaymentExecutionDetailItem )
				{
					var orderPayment = new AmazonOrderItem2PaymentInfoListItem();

					orderPayment.MoneyInfo = ConvertToAmount( paymentExecutionDetailItem.IsSetPayment(), paymentExecutionDetailItem.Payment );
					if ( paymentExecutionDetailItem.IsSetSubPaymentMethod() )
					{
						orderPayment.SubPaymentMethod = paymentExecutionDetailItem.SubPaymentMethod;
					}

					orderInfo.PaymentsInfo.Add( orderPayment );
				}
			}
			
			return orderInfo;
		}
		
		private AmountInfo ConvertToAmount( bool needConvert, Money money )
		{
			var info = new AmountInfo
				{
					CurrencyCode = CurrencyConvertor.BaseCurrency,
					Value = 0
				};

			if (needConvert)
			{
				if (money.IsSetCurrencyCode())
				{
					info.CurrencyCode = money.CurrencyCode;
				}

				if (money.IsSetAmount())
				{
					double amount;
					if (double.TryParse(money.Amount, out amount))
					{
						info.Value = amount;
					}
				}
			}

			return info;
		}

		private AmazonOrdersList2ItemStatusType ConvertOrderStatus(OrderStatusEnum orderStatus)
		{
			switch (orderStatus)
			{
				case OrderStatusEnum.Canceled:
					return AmazonOrdersList2ItemStatusType.Canceled;

				case OrderStatusEnum.PartiallyShipped:
					return AmazonOrdersList2ItemStatusType.PartiallyShipped;

				case OrderStatusEnum.Pending:
					return AmazonOrdersList2ItemStatusType.Pending;

				case OrderStatusEnum.Shipped:
					return AmazonOrdersList2ItemStatusType.Shipped;

				case OrderStatusEnum.Unfulfillable:
					return AmazonOrdersList2ItemStatusType.Unfulfillable;

				case OrderStatusEnum.Unshipped:
					return AmazonOrdersList2ItemStatusType.Unshipped;

				default:
					throw new NotImplementedException();
			}
		}

		private AmazonOrderItemDetailsList GetListItemsOrdered( AmazonOrdersItemsRequestInfo requestInfo, ActionAccessType access, RequestsCounterData requestCounter )
		{
			var orderId = requestInfo.OrderId;
			var sellerId = requestInfo.MerchantId;
			try
			{
				var req = new ListOrderItemsRequest
							{
								AmazonOrderId = orderId,
								SellerId = sellerId
							};

				var response = AmazonWaitBeforeRetryHelper.DoServiceAction(
									requestInfo.ErrorRetryingInfo,
									ListOrderItemsTrapForThrottling,
									"ListOrderItems",
									access,
									requestCounter,
									() => _Service.ListOrderItems( req ) );


				var list = new AmazonOrderItemDetailsList(DateTime.UtcNow)
					{
						RequestsCounter = requestCounter
					};

				if (response == null)
				{
					return list;
				}
				var result = response.ListOrderItemsResult;

				

				if ( result.IsSetOrderItems() )
				{					
					ParseOrderItems( list, result.OrderItems );
				}

				if ( result.IsSetNextToken() )
				{
					GetOrderItemsByNextToken( list, requestInfo, result.NextToken, access, requestCounter );
				}

				
				return list;
			}
			catch ( MarketplaceWebServiceOrdersException  )
			{
				return null;
			}
		}

		private void GetOrderItemsByNextToken( AmazonOrderItemDetailsList list, AmazonOrdersItemsRequestInfo requestInfo, string nextToken, ActionAccessType access, RequestsCounterData requestCounter )
		{
			var sellerId = requestInfo.MerchantId;
			var req = new ListOrderItemsByNextTokenRequest
			{
				NextToken = nextToken,
				SellerId = sellerId
			};

			var response = AmazonWaitBeforeRetryHelper.DoServiceAction(
									requestInfo.ErrorRetryingInfo,
									ListOrderItemsTrapForThrottling,
									"ListOrderItemsByNextToken",
									access,
									requestCounter,
									() => _Service.ListOrderItemsByNextToken( req ) );

			if (response == null)
			{
				return;
			}

			var result = response.ListOrderItemsByNextTokenResult;

			if ( result.IsSetOrderItems() )
			{
				ParseOrderItems( list, result.OrderItems );
			}

			if ( result.IsSetNextToken() )
			{
				GetOrderItemsByNextToken( list, requestInfo, result.NextToken, access, requestCounter);
			}
		}

		private void ParseOrderItems( AmazonOrderItemDetailsList list, OrderItemList orderItemInfo )
		{
			orderItemInfo.OrderItem.AsParallel().ForAll( orderItem => list.Add( ParceOrderItem( orderItem ) ) );
		}

		private AmazonOrderItemDetailInfo ParceOrderItem(OrderItem orderItem)
		{
			var item = new AmazonOrderItemDetailInfo();
			if ( orderItem.IsSetASIN() )
			{
				item.ASIN = orderItem.ASIN;
			}

			item.CODFee = ConvertToAmount( orderItem.IsSetCODFee(), orderItem.CODFee );
			item.CODFeeDiscount = ConvertToAmount( orderItem.IsSetCODFeeDiscount(), orderItem.CODFeeDiscount );

			if ( orderItem.IsSetGiftMessageText() )
			{
				item.GiftMessageText = orderItem.GiftMessageText;
			}

			if ( orderItem.IsSetGiftWrapLevel() )
			{
				item.GiftWrapLevel = orderItem.GiftWrapLevel;
			}
			item.GiftWrapPrice = ConvertToAmount( orderItem.IsSetGiftWrapPrice(), orderItem.GiftWrapPrice );
			item.GiftWrapTax = ConvertToAmount( orderItem.IsSetGiftWrapTax(), orderItem.GiftWrapTax );
			item.ItemPrice = ConvertToAmount( orderItem.IsSetItemPrice(), orderItem.ItemPrice );
			item.ItemTax = ConvertToAmount( orderItem.IsSetItemTax(), orderItem.ItemTax );

			if ( orderItem.IsSetOrderItemId() )
			{
				item.OrderItemId = orderItem.OrderItemId;
			}

			item.PromotionDiscount = ConvertToAmount( orderItem.IsSetPromotionDiscount(), orderItem.PromotionDiscount );

			/*if ( orderItem.IsSetPromotionIds() )
			{
				item.PromotionIds = orderItem.PromotionIds;
			}*/

			if ( orderItem.IsSetQuantityOrdered() )
			{
				item.QuantityOrdered = orderItem.QuantityOrdered;
			}

			if ( orderItem.IsSetQuantityShipped() )
			{
				item.QuantityShipped = orderItem.QuantityShipped;
			}

			if ( orderItem.IsSetSellerSKU() )
			{
				item.SellerSKU = orderItem.SellerSKU;
			}

			item.ShippingDiscount = ConvertToAmount( orderItem.IsSetShippingDiscount(), orderItem.ShippingDiscount );
			
			item.ShippingPrice = ConvertToAmount( orderItem.IsSetShippingPrice(), orderItem.ShippingPrice );
	
			item.ShippingTax = ConvertToAmount( orderItem.IsSetShippingTax(), orderItem.ShippingTax );

			if ( orderItem.IsSetTitle() )
			{
				item.Title = orderItem.Title;
			}

			return item;
		}

		private static void WriteToLog( string message, WriteLogType messageType = WriteLogType.Info, Exception ex = null )
		{
			WriteLoggerHelper.Write(message, messageType, null, ex);
			Debug.WriteLine( message );
		}
	}
}