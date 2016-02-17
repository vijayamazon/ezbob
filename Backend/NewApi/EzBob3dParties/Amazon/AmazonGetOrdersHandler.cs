namespace EzBob3dParties.Amazon {
    using System.Collections.Generic;
    using System.Linq;
    using EzBob3dParties.Amazon.Src.OrdersApi.Model;
    using EzBob3dPartiesApi.Amazon;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobCommon.Utils;
    using EzBobModels.Amazon;
    using EzBobModels.Amazon.Helpers;
    using NServiceBus;

    /// <summary>
    /// Handles the <see cref="AmazonGetOrders3dPartyCommand"/>.
    /// </summary>
    public class AmazonGetOrdersHandler : HandlerBase<AmazonGetOrders3dPartyCommandResponse>, IHandleMessages<AmazonGetOrders3dPartyCommand> {
        [Injected]
        public IAmazonService AmazonService { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public async void Handle(AmazonGetOrders3dPartyCommand command) {

            InfoAccumulator info = new InfoAccumulator();

            ListOrdersRequest request = new ListOrdersRequest {
                MWSAuthToken = command.AuthorizationToken,
                MarketplaceId = new List<string>(command.MarketplaceId),
                SellerId = command.SellerId,
                CreatedAfter = command.DateFrom.ToUniversalTime()
            };

            //get first page
            ListOrdersResponse response = await AmazonService.Orders.ListOrders(request);
            List<IEnumerable<AmazonOrderItemAndPayments>> results = new List<IEnumerable<AmazonOrderItemAndPayments>>();
            if (response.IsSetListOrdersResult()) {
                //process first page
                var orders = CreateOrders(response.ListOrdersResult);
                results.Add(orders);

                //get next pages
                string nextToken = response.ListOrdersResult.NextToken;
                while (StringUtils.IsNotEmpty(nextToken)) {
                    var nextRequest = new ListOrdersByNextTokenRequest {
                        MWSAuthToken = command.AuthorizationToken,
                        NextToken = nextToken,
                        SellerId = command.SellerId
                    };

                    //get next page
                    ListOrdersByNextTokenResponse nextResponse = await AmazonService.Orders.ListOrdersByNextToken(nextRequest);
                    if (nextResponse != null && nextResponse.IsSetListOrdersByNextTokenResult()) {
                        //process next page results
                        var nextOrders = nextResponse.ListOrdersByNextTokenResult.Orders.Select(CreateAmazonOrderItem);
                        results.Add(nextOrders);
                        nextToken = nextResponse.ListOrdersByNextTokenResult.NextToken;
                    } else {
                        nextToken = null;
                    }
                }
            }
            
            SendReply(info, command, resp => resp.OrderPayments = results.SelectMany(o => o).ToArray());
        }

        /// <summary>
        /// Creates the orders.
        /// </summary>
        /// <param name="ordersResult">The orders result.</param>
        /// <returns></returns>
        private IEnumerable<AmazonOrderItemAndPayments> CreateOrders(ListOrdersResult ordersResult) {
            if (ordersResult.Orders.IsEmpty()) {
                return Enumerable.Empty<AmazonOrderItemAndPayments>();
            }

            return ordersResult.Orders.Select(CreateAmazonOrderItem);
        }

        /// <summary>
        /// Creates the amazon order item.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <returns></returns>
        private AmazonOrderItemAndPayments CreateAmazonOrderItem(Src.OrdersApi.Model.Order order) {
            AmazonOrderItemAndPayments orderItemAndPayments = new AmazonOrderItemAndPayments();
            var item = new AmazonOrderItem();

            if (order.IsSetAmazonOrderId()) {
                item.OrderId = order.AmazonOrderId;
            }
            if (order.IsSetSellerOrderId()) {
                item.SellerOrderId = order.SellerOrderId;
            }
            if (order.IsSetPurchaseDate()) {
                item.PurchaseDate = order.PurchaseDate;
            }
            if (order.IsSetLastUpdateDate()) {
                item.LastUpdateDate = order.LastUpdateDate;
            }
            if (order.IsSetOrderStatus()) {
                #region Comment

                //      Pending, The order has been placed but payment has not been authorized. The order is not ready for shipment. Note that for orders with OrderType = Standard, the initial order status is Pending. For orders with OrderType = Preorder (available in JP only), the initial order status is PendingAvailability, and the order passes into the Pending status when the payment authorization process begins.
                //		Unshipped, Payment has been authorized and order is ready for shipment, but no items in the order have been shipped.
                //		PartiallyShipped, One or more (but not all) items in the order have been shipped. 
                //		Shipped, All items in the order have been shipped.
                //		Canceled, The order was canceled.
                //		Unfulfillable, The order cannot be fulfilled. This state applies only to Amazon-fulfilled orders that were not placed on Amazon's retail web site.
                //		PendingAvailability, This status is available for pre-orders only. The order has been placed, payment has not been authorized, and the release date of the item is in the future. The order is not ready for shipment. Note that Preorder is a possible OrderType value in Japan (JP) only.
                //		InvoiceUnconfirmed, All items in the order have been shipped. The seller has not yet given confirmation to Amazon that the invoice has been shipped to the buyer. Note: This value is available only in China (CN).
                //		All default 

                #endregion

                item.OrderStatus = order.OrderStatus;
            }
            if (order.IsSetNumberOfItemsShipped()) {
                item.NumberOfItemsShipped = (int)order.NumberOfItemsShipped;
            }
            if (order.IsSetNumberOfItemsUnshipped()) {
                item.NumberOfItemsUnshipped = (int)order.NumberOfItemsUnshipped;
            }
            if (order.IsSetOrderTotal() && order.OrderTotal.IsSetAmount()) {
                item.OrderTotal = decimal.Parse(order.OrderTotal.Amount);
                if (order.OrderTotal.IsSetCurrencyCode()) {
                    item.OrderTotalCurrency = order.OrderTotal.CurrencyCode;
                } else {
                    Log.Error("got empty currency code");
                }
            }
            if (order.IsSetPaymentExecutionDetail()) {
                orderItemAndPayments.Payments = order.PaymentExecutionDetail
                    .Select(CreatePayment)
                    .ToList();
            }

            orderItemAndPayments.OrderItem = item;
            return orderItemAndPayments;
        }

        /// <summary>
        /// Creates the payment.
        /// </summary>
        /// <param name="payment">The payment.</param>
        /// <returns></returns>
        private AmazonOrderItemPayment CreatePayment(PaymentExecutionDetailItem payment) {
            AmazonOrderItemPayment res = new AmazonOrderItemPayment();
            if (payment.IsSetPayment() && payment.Payment.IsSetAmount()) {
                res.MoneyInfoAmount = decimal.Parse(payment.Payment.Amount);
                res.MoneyInfoCurrency = payment.Payment.CurrencyCode;
                res.SubPaymentMethod = payment.PaymentMethod;
            }

            return res;
        }
    }
}
