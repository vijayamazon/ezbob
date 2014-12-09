namespace EzBob.AmazonServiceLib.ServiceCalls {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using Common;
	using MarketplaceWebServiceOrders.Model;
	using Orders.Configurator;
	using Orders.Model;
	using CommonLib;
	using CommonLib.TrapForThrottlingLogic;
	using MarketplaceWebServiceOrders;
	internal class AmazonServiceOrders {
		private readonly MarketplaceWebServiceOrders _Service;
		private readonly static ITrapForThrottling ListOrdersTrapForThrottling;
		private readonly static ITrapForThrottling ListOrderItemsTrapForThrottling;

		static AmazonServiceOrders() {
			ListOrdersTrapForThrottling = TrapForThrottlingController.Create("List Orders", 6);
			ListOrderItemsTrapForThrottling = TrapForThrottlingController.Create("List Order Items", 30, 2);
		}

		public static RequestsCounterData GetListOrders(IAmazonServiceOrdersConfigurator configurator, AmazonOrdersRequestInfo requestInfo, ActionAccessType access, Func<List<AmazonOrderItem>, bool> func) {
			var service = configurator.AmazonService;

			WriteToLog(string.Format("GetListOrders - SellerId: {0}, CreatedAfter (UTC) {1}, amazon mps id {2}, customer id {3} ", requestInfo.MerchantId, requestInfo.StartDate, requestInfo.GetMarketPlacesString(), requestInfo.CustomerId));

			return new AmazonServiceOrders(service).GetListOrders(requestInfo, access, func);
		}

		public static AmazonOrderItemDetailsList GetListItemsOrdered(IAmazonServiceOrdersConfigurator configurator, AmazonOrdersItemsRequestInfo requestInfo, ActionAccessType access, RequestsCounterData requestCounter) {
			return new AmazonServiceOrders(configurator.AmazonService).GetListItemsOrdered(requestInfo, access, requestCounter);
		}

		private AmazonServiceOrders(MarketplaceWebServiceOrders service) {
			_Service = service;
		}

		private RequestsCounterData GetListOrders(AmazonOrdersRequestInfo requestInfo, ActionAccessType access, Func<List<AmazonOrderItem>, bool> func) {

			var request = new ListOrdersRequest {
				MarketplaceId = requestInfo.MarketplaceId,
				SellerId = requestInfo.MerchantId,
			};

			if (requestInfo.StartDate.HasValue) {
				request.CreatedAfter = requestInfo.StartDate.Value.ToUniversalTime();
			}
			var responseCounter = new RequestsCounterData();
			var response = AmazonWaitBeforeRetryHelper.DoServiceAction(
									requestInfo.ErrorRetryingInfo,
									ListOrdersTrapForThrottling,
									"ListOrders",
									access,
									responseCounter,
									() => _Service.ListOrders(request));

			if (response == null || response.ListOrdersResult == null || !response.IsSetListOrdersResult()) {
				return null;
			}

			var result = response.ListOrdersResult;

			if (result.IsSetOrders()) {
				var ordersList = ParceOrdersInfo(result.Orders, request.SellerId, access, requestInfo.CustomerId);
				func(ordersList);
			}

			if (result.IsSetNextToken()) {
				GetOrdersByNextToken(requestInfo, result.NextToken, access, responseCounter, func);
			}

			return responseCounter;
		}

		private void GetOrdersByNextToken(AmazonOrdersRequestInfo requestInfo, string nextToken, ActionAccessType access, RequestsCounterData responseCounter, Func<List<AmazonOrderItem>, bool> func) {
			var sellerId = requestInfo.MerchantId;

			var req = new ListOrdersByNextTokenRequest {
				NextToken = nextToken,
				SellerId = sellerId
			};
			var response = AmazonWaitBeforeRetryHelper.DoServiceAction(
									requestInfo.ErrorRetryingInfo,
									ListOrdersTrapForThrottling,
									"ListOrdersByNextToken",
									access,
									responseCounter,
									() => _Service.ListOrdersByNextToken(req));

			if (response != null && response.IsSetListOrdersByNextTokenResult()) {
				var result = response.ListOrdersByNextTokenResult;
				if (result.IsSetOrders()) {
					var ordersList = ParceOrdersInfo(result.Orders, sellerId, access, requestInfo.CustomerId);
					func(ordersList);
				}

				if (result.IsSetNextToken()) {
					GetOrdersByNextToken(requestInfo, result.NextToken, access, responseCounter, func);
				}
			}
		}

		private List<AmazonOrderItem> ParceOrdersInfo(List<Order> orders, string sellerId, ActionAccessType access, int customerId) {
			if (orders.Any(x => x.IsSetPurchaseDate())) {
				var firstDate = orders.Min(x => x.PurchaseDate);
				var lastDate = orders.Max(x => x.PurchaseDate);
				WriteToLog(
					string.Format(
						"Amazon ParceOrdersInfo customerId {2}, sellerId {0} number of orders {1}, first order date {3} last order date {4}",
						sellerId, orders.Count, customerId, firstDate, lastDate));
			} else {
				WriteToLog(string.Format("Amazon ParceOrdersInfo customerId {1}, sellerId {0} number of orders 0", sellerId, customerId));
			}
			var ordersList = new List<AmazonOrderItem>();
			orders.AsParallel().ForAll(o => ordersList.Add(ParceOrder(o)));
			return ordersList;
		}

		private AmazonOrderItem ParceOrder(Order order) {
			var orderInfo = new AmazonOrderItem();

			if (order.IsSetAmazonOrderId()) {
				orderInfo.OrderId = order.AmazonOrderId;
			}
			if (order.IsSetSellerOrderId()) {
				orderInfo.SellerOrderId = order.SellerOrderId;
			}
			if (order.IsSetPurchaseDate()) {
				orderInfo.PurchaseDate = order.PurchaseDate;
			}
			if (order.IsSetLastUpdateDate()) {
				orderInfo.LastUpdateDate = order.LastUpdateDate;
			}
			if (order.IsSetOrderStatus()) {
				orderInfo.OrderStatus = ConvertOrderStatus(order.OrderStatus);
			}

			orderInfo.OrderTotal = ConvertToAmount(order.IsSetOrderTotal(), order.OrderTotal);

			if (order.IsSetNumberOfItemsShipped()) {
				orderInfo.NumberOfItemsShipped = (int)order.NumberOfItemsShipped;
			}
			if (order.IsSetNumberOfItemsUnshipped()) {
				orderInfo.NumberOfItemsUnshipped = (int)order.NumberOfItemsUnshipped;
			}

			if (order.IsSetPaymentExecutionDetail()) {
				orderInfo.PaymentsInfo = new AmazonOrderItem2PaymentsInfoList();

				foreach (PaymentExecutionDetailItem paymentExecutionDetailItem in order.PaymentExecutionDetail) {
					var orderPayment = new AmazonOrderItem2PaymentInfoListItem();

					orderPayment.MoneyInfo = ConvertToAmount(paymentExecutionDetailItem.IsSetPayment(), paymentExecutionDetailItem.Payment);
					if (paymentExecutionDetailItem.IsSetPaymentMethod()) {
						orderPayment.PaymentMethod = paymentExecutionDetailItem.PaymentMethod;
					}

					orderInfo.PaymentsInfo.Add(orderPayment);
				}
			}

			return orderInfo;
		}

		private AmountInfo ConvertToAmount(bool needConvert, Money money) {
			var info = new AmountInfo {
				CurrencyCode = CurrencyConvertor.BaseCurrency,
				Value = 0
			};

			if (needConvert) {
				if (money.IsSetCurrencyCode()) {
					info.CurrencyCode = money.CurrencyCode;
				}

				if (money.IsSetAmount()) {
					double amount;
					if (double.TryParse(money.Amount, out amount)) {
						info.Value = amount;
					}
				}
			}

			return info;
		}

		private AmazonOrdersList2ItemStatusType ConvertOrderStatus(string orderStatus) {
			switch (orderStatus) {
				case "Canceled":
					return AmazonOrdersList2ItemStatusType.Canceled;

				case "PartiallyShipped":
					return AmazonOrdersList2ItemStatusType.PartiallyShipped;

				case "Pending":
					return AmazonOrdersList2ItemStatusType.Pending;

				case "Shipped":
					return AmazonOrdersList2ItemStatusType.Shipped;

				case "Unfulfillable":
					return AmazonOrdersList2ItemStatusType.Unfulfillable;

				case "Unshipped":
					return AmazonOrdersList2ItemStatusType.Unshipped;

				case "PendingAvailability":
					return AmazonOrdersList2ItemStatusType.PendingAvailability;

				case "InvoiceUnconfirmed":
					return AmazonOrdersList2ItemStatusType.InvoiceUnconfirmed;

				case "All":
					return AmazonOrdersList2ItemStatusType.All;
				default:
					WriteToLog("Amazon order: Unknown order status : " + orderStatus, WriteLogType.Error);
					return AmazonOrdersList2ItemStatusType.All;
			}
		}

		private AmazonOrderItemDetailsList GetListItemsOrdered(AmazonOrdersItemsRequestInfo requestInfo, ActionAccessType access, RequestsCounterData requestCounter) {
			var orderId = requestInfo.OrderId;
			var sellerId = requestInfo.MerchantId;
			try {
				var req = new ListOrderItemsRequest {
					AmazonOrderId = orderId,
					SellerId = sellerId
				};

				var response = AmazonWaitBeforeRetryHelper.DoServiceAction(
									requestInfo.ErrorRetryingInfo,
									ListOrderItemsTrapForThrottling,
									"ListOrderItems",
									access,
									requestCounter,
									() => _Service.ListOrderItems(req));

				var list = new AmazonOrderItemDetailsList(DateTime.UtcNow) {
					RequestsCounter = requestCounter
				};

				if (response == null) {
					return list;
				}
				var result = response.ListOrderItemsResult;

				if (result.IsSetOrderItems()) {
					ParseOrderItems(list, result.OrderItems);
				}

				if (result.IsSetNextToken()) {
					GetOrderItemsByNextToken(list, requestInfo, result.NextToken, access, requestCounter);
				}

				return list;
			} catch (MarketplaceWebServiceOrdersException) {
				return null;
			}
		}

		private void GetOrderItemsByNextToken(AmazonOrderItemDetailsList list, AmazonOrdersItemsRequestInfo requestInfo, string nextToken, ActionAccessType access, RequestsCounterData requestCounter) {
			var sellerId = requestInfo.MerchantId;
			var req = new ListOrderItemsByNextTokenRequest {
				NextToken = nextToken,
				SellerId = sellerId
			};

			var response = AmazonWaitBeforeRetryHelper.DoServiceAction(
									requestInfo.ErrorRetryingInfo,
									ListOrderItemsTrapForThrottling,
									"ListOrderItemsByNextToken",
									access,
									requestCounter,
									() => _Service.ListOrderItemsByNextToken(req));

			if (response == null) {
				return;
			}

			var result = response.ListOrderItemsByNextTokenResult;

			if (result.IsSetOrderItems()) {
				ParseOrderItems(list, result.OrderItems);
			}

			if (result.IsSetNextToken()) {
				GetOrderItemsByNextToken(list, requestInfo, result.NextToken, access, requestCounter);
			}
		}

		private void ParseOrderItems(AmazonOrderItemDetailsList list, IEnumerable<OrderItem> orderItemInfo) {
			orderItemInfo.AsParallel().ForAll(orderItem => list.Add(ParceOrderItem(orderItem)));
		}

		private AmazonOrderItemDetailInfo ParceOrderItem(OrderItem orderItem) {
			var item = new AmazonOrderItemDetailInfo();
			if (orderItem.IsSetASIN()) {
				item.ASIN = orderItem.ASIN;
			}

			item.CODFee = ConvertToAmount(orderItem.IsSetCODFee(), orderItem.CODFee);
			item.CODFeeDiscount = ConvertToAmount(orderItem.IsSetCODFeeDiscount(), orderItem.CODFeeDiscount);

			if (orderItem.IsSetGiftMessageText()) {
				item.GiftMessageText = orderItem.GiftMessageText;
			}

			if (orderItem.IsSetGiftWrapLevel()) {
				item.GiftWrapLevel = orderItem.GiftWrapLevel;
			}
			item.GiftWrapPrice = ConvertToAmount(orderItem.IsSetGiftWrapPrice(), orderItem.GiftWrapPrice);
			item.GiftWrapTax = ConvertToAmount(orderItem.IsSetGiftWrapTax(), orderItem.GiftWrapTax);
			item.ItemPrice = ConvertToAmount(orderItem.IsSetItemPrice(), orderItem.ItemPrice);
			item.ItemTax = ConvertToAmount(orderItem.IsSetItemTax(), orderItem.ItemTax);

			if (orderItem.IsSetOrderItemId()) {
				item.OrderItemId = orderItem.OrderItemId;
			}

			item.PromotionDiscount = ConvertToAmount(orderItem.IsSetPromotionDiscount(), orderItem.PromotionDiscount);

			/*if ( orderItem.IsSetPromotionIds() )
			{
				item.PromotionIds = orderItem.PromotionIds;
			}*/

			if (orderItem.IsSetQuantityOrdered()) {
				item.QuantityOrdered = orderItem.QuantityOrdered;
			}

			if (orderItem.IsSetQuantityShipped()) {
				item.QuantityShipped = orderItem.QuantityShipped;
			}

			if (orderItem.IsSetSellerSKU()) {
				item.SellerSKU = orderItem.SellerSKU;
			}

			item.ShippingDiscount = ConvertToAmount(orderItem.IsSetShippingDiscount(), orderItem.ShippingDiscount);

			item.ShippingPrice = ConvertToAmount(orderItem.IsSetShippingPrice(), orderItem.ShippingPrice);

			item.ShippingTax = ConvertToAmount(orderItem.IsSetShippingTax(), orderItem.ShippingTax);

			if (orderItem.IsSetTitle()) {
				item.Title = orderItem.Title;
			}

			return item;
		}

		private static void WriteToLog(string message, WriteLogType messageType = WriteLogType.Info, Exception ex = null) {
			WriteLoggerHelper.Write(message, messageType, null, ex);
			Debug.WriteLine(message);
		}
	}
}
