using System.Collections.Generic;
using System.Threading.Tasks;

namespace EzBob3dParties.Amazon {
    using EzBob3dParties.Amazon.Src.OrdersApi.Model;
    using EzBob3dPartiesApi.Amazon;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobModels.Amazon;
    using NServiceBus;

    /// <summary>
    /// Handles the <see cref="AmazonGetOrdersDetails3PartyCommand"/>.
    /// </summary>
    public class AmazonGetOrdersDetailsHandler : HandlerBase<AmazonGetOrdersDetails3PartyCommandResponse>, IHandleMessages<AmazonGetOrdersDetails3PartyCommand> {

        [Injected]
        internal IAmazonService AmazonService { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public async void Handle(AmazonGetOrdersDetails3PartyCommand command) {
            IDictionary<string, IList<AmazonOrderItemDetail>> orderDetailsByOrderId = await GetOrdersDetailsByOrderId(command);
            InfoAccumulator info = new InfoAccumulator();
            SendReply(info, command, resp => resp.OrderDetailsByOrderId = orderDetailsByOrderId);
        }

        /// <summary>
        /// Gets the orders details by order id.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        private async Task<IDictionary<string, IList<AmazonOrderItemDetail>>> GetOrdersDetailsByOrderId(AmazonGetOrdersDetails3PartyCommand command) {
            Dictionary<string, IList<AmazonOrderItemDetail>> ordersDetails = new Dictionary<string, IList<AmazonOrderItemDetail>>();
            foreach (var request in CreateRequests(command)) {
                //first request
                var response = await AmazonService.Orders.ListOrderItems(request);
                List<AmazonOrderItemDetail> currentOrderDetails = new List<AmazonOrderItemDetail>();
                if (response.IsSetListOrderItemsResult()) {
                    //store parsed response data
                    var orderDetails = ParseOrderDetailsResponse(response.ListOrderItemsResult.OrderItems);
                    currentOrderDetails.AddRange(orderDetails);
                }

                //tries to get next details requests (for the same order)
                string nextToken = response.ListOrderItemsResult.NextToken;
                while (!string.IsNullOrEmpty(nextToken)) {
                    var nextRequest = CreateNextRequest(command, nextToken);
                    //get next response
                    ListOrderItemsByNextTokenResponse nextResponse = await AmazonService.Orders.ListOrderItemsByNextToken(nextRequest);

                    if (nextResponse.IsSetListOrderItemsByNextTokenResult()) {
                        //store parsed response data
                        currentOrderDetails.AddRange(ParseOrderDetailsResponse(nextResponse.ListOrderItemsByNextTokenResult.OrderItems));
                        nextToken = nextResponse.ListOrderItemsByNextTokenResult.NextToken;
                    } else {
                        nextToken = null;
                    }
                }

                //store all order details by AmazonOrderId
                ordersDetails.Add(request.AmazonOrderId, currentOrderDetails);
            }

            return ordersDetails;
        }

        /// <summary>
        /// Parses the order details response.
        /// </summary>
        /// <param name="orderItems">The order items.</param>
        /// <returns></returns>
        private IEnumerable<AmazonOrderItemDetail> ParseOrderDetailsResponse(IEnumerable<OrderItem> orderItems) {
            foreach (var orderItem in orderItems) {
                yield return CreateOrderItemDetail(orderItem);
            }
        }

        /// <summary>
        /// Creates the order item detail.
        /// </summary>
        /// <param name="orderItem">The order item.</param>
        /// <returns></returns>
        private AmazonOrderItemDetail CreateOrderItemDetail(OrderItem orderItem) {
            var details = new AmazonOrderItemDetail {
                ASIN = orderItem.ASIN,
                SellerSKU = orderItem.SellerSKU,
                Title = orderItem.Title
            };

            if (orderItem.IsSetOrderItemId()) {
                details.OrderItemId = int.Parse(orderItem.OrderItemId);
            }

            if (orderItem.IsSetPromotionDiscount()) {
                details.PromotionDiscountCurrency = orderItem.PromotionDiscount.CurrencyCode;
                details.PromotionDiscountPrice = decimal.Parse(orderItem.PromotionDiscount.Amount);
            }

            FillQuantityDetails(orderItem, details);
            FillItemPriceDetails(orderItem, details);
            FillGiftWrapDetails(orderItem, details);
            FillShippingDetails(orderItem, details);
            FillJapanCODFeeDetails(orderItem, details);

            return details;
        }

        /// <summary>
        /// Fills the quantity details.
        /// </summary>
        /// <param name="orderItem">The order item.</param>
        /// <param name="details">The details.</param>
        private void FillQuantityDetails(OrderItem orderItem, AmazonOrderItemDetail details) {
            if (orderItem.IsSetQuantityOrdered())
                details.QuantityOrdered = (int)orderItem.QuantityOrdered;

            if (orderItem.IsSetQuantityShipped())
                details.QuantityShipped = (int)orderItem.QuantityShipped;
        }

        /// <summary>
        /// Fills the gift wrap details.
        /// </summary>
        /// <param name="orderItem">The order item.</param>
        /// <param name="details">The details.</param>
        private void FillGiftWrapDetails(OrderItem orderItem, AmazonOrderItemDetail details) {
            if (orderItem.IsSetGiftWrapLevel()) {
                details.GiftWrapLevel = orderItem.GiftWrapLevel;
            }

            if (orderItem.IsSetGiftWrapPrice()) {
                details.GiftWrapPriceCurrency = orderItem.GiftWrapPrice.CurrencyCode;
                details.GiftWrapPrice = decimal.Parse(orderItem.GiftWrapPrice.Amount);
            }

            if (orderItem.IsSetGiftWrapTax()) {
                details.GiftWrapTaxCurrency = orderItem.GiftWrapTax.CurrencyCode;
                details.GiftWrapTaxPrice = decimal.Parse(orderItem.GiftWrapPrice.Amount);
            }
        }

        /// <summary>
        /// Fills the item price details.
        /// </summary>
        /// <param name="orderItem">The order item.</param>
        /// <param name="details">The details.</param>
        private void FillItemPriceDetails(OrderItem orderItem, AmazonOrderItemDetail details) {
            if (orderItem.IsSetItemPrice()) {
                details.ItemPriceCurrency = orderItem.ItemPrice.CurrencyCode;
                details.ItemPrice = decimal.Parse(orderItem.ItemPrice.Amount);
            }

            if (orderItem.IsSetItemTax()) {
                details.ItemTaxCurrency = orderItem.ItemTax.CurrencyCode;
                details.ItemTaxPrice = decimal.Parse(orderItem.ItemTax.Amount);
            }
        }

        /// <summary>
        /// Fills the shipping details.
        /// </summary>
        /// <param name="orderItem">The order item.</param>
        /// <param name="details">The details.</param>
        private void FillShippingDetails(OrderItem orderItem, AmazonOrderItemDetail details) {
            if (orderItem.IsSetShippingDiscount()) {
                details.ShippingDiscountCurrency = orderItem.ShippingDiscount.CurrencyCode;
                details.ShippingDiscountPrice = decimal.Parse(orderItem.ShippingDiscount.Amount);
            }

            if (orderItem.IsSetShippingPrice()) {
                details.ShippingPriceCurrency = orderItem.ShippingPrice.CurrencyCode;
                details.ShippingPrice = decimal.Parse(orderItem.ShippingPrice.Amount);
            }

            if (orderItem.IsSetShippingTax()) {
                details.ShippingTaxCurrency = orderItem.ShippingTax.CurrencyCode;
                details.ShippingTaxPrice = decimal.Parse(orderItem.ShippingTax.Amount);
            }
        }

        /// <summary>
        /// Fills the japan cod fee details.
        /// </summary>
        /// <param name="orderItem">The order item.</param>
        /// <param name="details">The details.</param>
        private void FillJapanCODFeeDetails(OrderItem orderItem, AmazonOrderItemDetail details) {
            if (orderItem.IsSetCODFee()) {
                details.CODFeeCurrency = orderItem.CODFee.CurrencyCode;
                details.CODFeePrice = decimal.Parse(orderItem.CODFee.Amount);
            }

            if (orderItem.IsSetCODFeeDiscount()) {
                details.CODFeeDiscountCurrency = orderItem.CODFeeDiscount.CurrencyCode;
                details.CODFeeDiscountPrice = decimal.Parse(orderItem.CODFeeDiscount.Amount);
            }
        }

        /// <summary>
        /// Creates the requests.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        private IEnumerable<ListOrderItemsRequest> CreateRequests(AmazonGetOrdersDetails3PartyCommand command) {
            foreach (string orderId in command.OrdersIds) {
                yield return new ListOrderItemsRequest {
                    SellerId = command.SellerId,
                    MWSAuthToken = command.AuthorizationToken,
                    AmazonOrderId = orderId
                };
            }
        }

        /// <summary>
        /// Creates the next request.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="nextToken">The next token.</param>
        /// <returns></returns>
        private ListOrderItemsByNextTokenRequest CreateNextRequest(AmazonGetOrdersDetails3PartyCommand command, string nextToken) {
            ListOrderItemsByNextTokenRequest nextRequest = new ListOrderItemsByNextTokenRequest {
                SellerId = command.SellerId,
                MWSAuthToken = command.AuthorizationToken,
                NextToken = nextToken
            };
            return nextRequest;
        }
    }
}
