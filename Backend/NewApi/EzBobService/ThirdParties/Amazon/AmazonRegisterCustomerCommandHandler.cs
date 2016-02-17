namespace EzBobService.ThirdParties.Amazon {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EzBob3dPartiesApi.Amazon;
    using EzBobApi.Commands.Amazon;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobModels.Amazon;
    using EzBobPersistence.MarketPlace;
    using EzBobPersistence.ThirdParty.Amazon;
    using EzBobService.Encryption;
    using NServiceBus;

    /// <summary>
    /// Handles <see cref="AmazonRegisterCustomerCommand"/>.
    /// </summary>
    public class AmazonRegisterCustomerCommandHandler : HandlerBase<AmazonRegisterCustomerCommandResponse>, IHandleMessages<AmazonRegisterCustomerCommand> {

        [Injected]
        public AmazonGetOrderDetailsSendReceive GetOrderDetailsSendReceive { get; set; }

        [Injected]
        public AmazonGetOrdersSendReceive GetOrdersSendReceive { get; set; }

        [Injected]
        public AmazonGetProductCategoriesSendReceive GetProductCategoriesSendReceive { get; set; }

        [Injected]
        public ThirdPartyServiceConfig Config { get; set; }

        [Injected]
        public IAmazonOrdersQueries AmazonOrdersQueries { get; set; }

        [Injected]
        public IMarketPlaceQueries MarketPlaceQueries { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public async void Handle(AmazonRegisterCustomerCommand command) {
            InfoAccumulator info = new InfoAccumulator();

            AmazonGetOrders3dPartyCommand request = new AmazonGetOrders3dPartyCommand {
                SellerId = command.SellerId,
                AuthorizationToken = command.AuthorizationToken,
                MarketplaceId = command.MarketplaceId,
                DateFrom = DateTime.UtcNow.AddYears(-1) //TODO
            };

            var response = await GetOrdersSendReceive.SendAsync(Config.Address, request);

            int customerId;
            try {
                customerId = CustomerIdEncryptor.DecryptCustomerId(command.CustomerId, command.CommandOriginator);
            } catch (Exception ex) {
                Log.Error(ex.Message);
                info.AddError("Invalid request");
                SendReply(info, command, resp => resp.CustomerId = command.CustomerId);
                return;
            }


            int customerMarketPlaceId = 0; //TODO
            int marketPlaceHistoryId = 0; //TODO

            AmazonOrder order = new AmazonOrder {
                Created = DateTime.UtcNow,
                CustomerMarketPlaceId = customerMarketPlaceId,
                CustomerMarketPlaceUpdatingHistoryRecordId = marketPlaceHistoryId
            };

            int orderId = (int)AmazonOrdersQueries.SaveOrder(order);
            if (orderId < 1) {
                throw new Exception("could not save amazon order");
            }

            bool res = AmazonOrdersQueries.SaveOrdersPayments(response.OrderPayments, orderId);
            if (!res) {
                throw new Exception("could not save order/order's payments");
            }

            //Get top N order items
            IEnumerable<AmazonOrderItem> items = AmazonOrdersQueries.GetTopNOrderItems(customerMarketPlaceId);

            //Get order details of top N items
            AmazonGetOrdersDetails3PartyCommand detailsRequestCommand = new AmazonGetOrdersDetails3PartyCommand {
                SellerId = command.SellerId,
                AuthorizationToken = command.AuthorizationToken,
                OrdersIds = items.Where(o => o.AmazonOrderId.HasValue)
                    .Select(o => o.AmazonOrderId.ToString())
            };

            AmazonGetOrdersDetails3PartyCommandResponse detailsResponse = await GetOrderDetailsSendReceive.SendAsync(Config.Address, detailsRequestCommand);
            var skuCollection = detailsResponse.OrderDetailsByOrderId.Values
                .SelectMany(o => o)
                .Select(o => o.SellerSKU);

            //Get categories of top N order items
            var getProductCategoriesCommand = new AmazonGetProductCategories3dPartyCommand {
                SellerId = command.SellerId,
                AuthorizationToken = command.AuthorizationToken,
                SellerSKUs = skuCollection
            };

            var productCategoriesResponse = await GetProductCategoriesSendReceive.SendAsync(Config.Address, getProductCategoriesCommand);

            IDictionary<string, IList<AmazonOrderItemDetail>> orderIdToOrderDetails = detailsResponse.OrderDetailsByOrderId;
            //save order item details
            var insertedOrderDetails = AmazonOrdersQueries.OrderDetails.SaveOrderDetails(orderIdToOrderDetails.Values.SelectMany(o => o));


            IDictionary<string, IEnumerable<AmazonProductCategory>> skuToCategory = productCategoriesResponse.CategoriesBySku;
            //upserts categories
            IDictionary<AmazonProductCategory, int> categoryToId = AmazonOrdersQueries.Categories.UpsertCategories(productCategoriesResponse.CategoriesBySku.Values.SelectMany(o => o));

            res = AmazonOrdersQueries.Categories.SaveCategoryOrderDetailsMapping(CreateOrderIdToOrderDetailsIdMappings(insertedOrderDetails, skuToCategory, categoryToId));
            if (!res) {
                throw new Exception("could not save category order details mapping");
            }
        }

        /// <summary>
        /// Creates the order identifier to order details identifier mappings.
        /// </summary>
        /// <param name="orderItemDetails">The order item details.</param>
        /// <param name="skuToCategory">The sku to category map.</param>
        /// <param name="categoryToId">The category to identifier map.</param>
        /// <returns></returns>
        private IEnumerable<AmazonCategoryToOrderDetailsMap> CreateOrderIdToOrderDetailsIdMappings(
            IEnumerable<AmazonOrderItemDetail> orderItemDetails,
            IDictionary<string, IEnumerable<AmazonProductCategory>> skuToCategory,
            IDictionary<AmazonProductCategory, int> categoryToId) {

            foreach (var orderItemDetail in orderItemDetails) {
                IEnumerable<AmazonProductCategory> categories;
                if (!skuToCategory.TryGetValue(orderItemDetail.SellerSKU, out categories)) {
                    Log.ErrorFormat("double not find categories for SKU {0}", orderItemDetail.SellerSKU);
                    continue;
                }

                foreach (var category in categories) {
                    int categoryId;

                    if (!categoryToId.TryGetValue(category, out categoryId)) {
                        Log.ErrorFormat("could not get category id for category", category.CategoryName);
                        continue;
                    }

                    yield return new AmazonCategoryToOrderDetailsMap {
                        AmazonOrderItemDetailId = orderItemDetail.Id,
                        EbayAmazonCategoryId = categoryId
                    };
                }
            }
        }
    }
}
