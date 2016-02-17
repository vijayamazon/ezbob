namespace EzBobAcceptanceTests.Amazon {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using EzBob3dPartiesApi.Amazon;
    using EzBobAcceptanceTests.Customer;
    using EzBobAcceptanceTests.Infra;
    using EzBobAcceptanceTests.Properties;
    using EzBobCommon;
    using EzBobCommon.Utils;
    using EzBobModels.Amazon;
    using EzBobModels.Amazon.Helpers;
    using EzBobModels.MarketPlace;
    using EzBobPersistence.MarketPlace;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTesting.Support;
    using NUnit.Framework;
    using EzBobPersistence.ThirdParty.Amazon;
    using Newtonsoft.Json;
    using StructureMap;

    [TestFixture]
    public class AmazonTests : TestBase {

        private static readonly Guid amazonInternalId = Guid.Parse("A4920125-411F-4BB9-A52D-27E8A00D0A3B");

        private static readonly AmazonSecurityInfo securityInfo = new AmazonSecurityInfo {
            MerchantId = "A4G97F1RP09ZP",
            MWSAuthToken = "amzn.mws.68a5cceb-b60a-b3f6-291c-08d4595ae879",
            MarketplaceId = "A13V1IB3VIYZZH".ToEnumerable()
                .ToArray()
        };

        private class Context : TestContextBase {
            public bool IsSaveOrdersToFile { get; set; }
            public bool IsGetRealOrders { get; set; }
            public bool IsEzBobServiceUp { get; set; }
            public bool IsThirdPartiesUp { get; set; }
            public bool IsDone { get; set; }
        }

        [Test]
        public void TestGetOrdersSendRecieve() {
            Context ctx = new Context() {
                IsGetRealOrders = false,
                IsSaveOrdersToFile = false
            };

            Scenario.Define(ctx)
                .WithEndpoint<ThirdPartiesService>(c => c
                    .Given((bus, context) => {
                        context.IsThirdPartiesUp = true;
                    }))
                .WithEndpoint<EzBobService>(c => c
                    .Given((bus, context) => {
                        context.IsEzBobServiceUp = true;
                    })
                    .When(context => context.IsThirdPartiesUp && context.IsEzBobServiceUp, async (bus, context) => {
                        var container = context.GetContainer(EzBobService.EndpointName);
                        var customerInfo = await GetCustomerInfo(container);
                        var orders = await GetAmazonOrders(container, context);
                        if (context.IsSaveOrdersToFile) {
                            SaveOrdersToFile(orders);
                        }
                        ValidateMarketPlace(container, customerInfo);
                        int marketplaceId = UpsertMarketPlace(container, customerInfo);
                        int marketplaceHistoryId = UpsertMarketplaceHistoryUpdate(container, marketplaceId);
                        int orderId = CreateAmazonOrder(container, marketplaceId, marketplaceHistoryId);
//                        var queries = container.GetInstance<IAmazonOrdersQueries>();
//                        queries.SaveOrdersPayments(response);

                        int kk = 0;
                    }))
                .Done(context => context.IsDone)
                .Run(new RunSettings {
                    TestExecutionTimeout = TimeSpan.FromDays(1)
                });
        }

        private static void SaveOrdersToFile(AmazonGetOrders3dPartyCommandResponse orders) {
            using (var file = System.IO.File.CreateText("orders.json")) {
                using (JsonTextWriter writer = new JsonTextWriter(file)) {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, orders.OrderPayments);
                }
            }
        }

        private static int UpsertMarketplaceHistoryUpdate(IContainer container, int marketPlaceTableId) {
            var updateHistory = new CustomerMarketPlaceUpdateHistory {
                CustomerMarketPlaceId = marketPlaceTableId,
                UpdatingStart = DateTime.UtcNow
            };

            int id = (int)container.GetInstance<IMarketPlaceQueries>()
                .UpsertMarketPlaceUpdatingHistory(updateHistory);

            Assert.True(id > 0, "invalid marketplace history id");
            return id;
        }

        private static int CreateAmazonOrder(IContainer container, int customerMarketPlaceId, int marketPlaceHistoryId) {
            AmazonOrder order = new AmazonOrder {
                Created = DateTime.UtcNow,
                CustomerMarketPlaceId = customerMarketPlaceId,
                CustomerMarketPlaceUpdatingHistoryRecordId = marketPlaceHistoryId
            };

            var amazonOrdersQueries = container.GetInstance<IAmazonOrdersQueries>();
            int orderId = (int)amazonOrdersQueries.SaveOrder(order);
            Assert.IsTrue(orderId > 0, "error inserting order id");
            return orderId;
        }

        private static void ValidateMarketPlace(IContainer container, AmazonGetCustomerInfo3dPartyCommandResponse customerInfoResponse) {
            IMarketPlaceQueries marketPlaceQueries = container.GetInstance<IMarketPlaceQueries>();

            InfoAccumulator info = marketPlaceQueries.ValidateCustomerMarketPlace(amazonInternalId, customerInfoResponse.BusinessName);
            Assert.False(info.HasErrors, "marketplace validation failed");
        }

        private static int UpsertMarketPlace(IContainer container, AmazonGetCustomerInfo3dPartyCommandResponse customerInfoResponse) {
            IMarketPlaceQueries marketPlaceQueries = container.GetInstance<IMarketPlaceQueries>();
            int marketPlaceId = (int)marketPlaceQueries.GetMarketPlaceIdFromTypeId(amazonInternalId);

            CustomerMarketPlace marketPlace = new CustomerMarketPlace {
                CustomerId = 17171717,
                DisplayName = customerInfoResponse.BusinessName,
                MarketPlaceId = marketPlaceId,
                SecurityData = SerializationUtils.SerializeToBinaryXml(securityInfo),
                Created = DateTime.UtcNow
            };

            int id = (int)marketPlaceQueries.UpsertMarketPlace(marketPlace, amazonInternalId);
            Assert.IsTrue(id > 0, "error upserting marketplace");
            return id;
        }

        private static void SaveOrders(IContainer container, AmazonGetOrders3dPartyCommandResponse getOrdersResponse, int orderId) {
            var ordersQueries = container.GetInstance<IAmazonOrdersQueries>();

            bool res = ordersQueries.SaveOrdersPayments(getOrdersResponse.OrderPayments, orderId);
            Assert.True(res, "failed to save orders/payments");
        }

        private static Task<AmazonGetCustomerInfo3dPartyCommandResponse> GetCustomerInfo(IContainer container) {
            var getCustomerInfo = container.GetInstance<AmazonGetCustomerInfoSendRecieve>();
            var command = new AmazonGetCustomerInfo3dPartyCommand {
                SellerId = securityInfo.MerchantId
            };

            return getCustomerInfo.SendAsync(ThirdPartiesService.EndpointName, command);
        }

        /// <summary>
        /// Gets orders from resources. Contains code to get real orders. 
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns></returns>
        private static Task<AmazonGetOrders3dPartyCommandResponse> GetAmazonOrders(IContainer container, Context ctx) {
            if (Resources.AmazonOrdersPayments.IsNotEmpty() && !ctx.IsGetRealOrders) {
                var response = new AmazonGetOrders3dPartyCommandResponse();
                response.OrderPayments = SerializationUtils.DeserializeBinaryJson<IEnumerable<AmazonOrderItemAndPayments>>(Resources.AmazonOrdersPayments);
                return CreateCompletedTask(response);
            }

            var getOrders = container.GetInstance<AmazonGetOrdersSendRecieve>();
            var command = new AmazonGetOrders3dPartyCommand {
                SellerId = securityInfo.MerchantId,
                MarketplaceId = securityInfo.MarketplaceId,
                AuthorizationToken = "amzn.mws.68a5cceb-b60a-b3f6-291c-08d4595ae879",
                DateFrom = DateTime.UtcNow.AddMonths(-1)
            };

            return getOrders.SendAsync(ThirdPartiesService.EndpointName, command);
        }

        [Test]
        public void TestAmazonMarketplaceAddition() {
            Context ctx = new Context();

            Scenario.Define(ctx)
                .WithEndpoint<ThirdPartiesService>(c => c
                    .Given((bus, context) => {
                        context.IsThirdPartiesUp = true;
                    }))
                .WithEndpoint<EzBobService>(c => c
                    .Given((bus, context) => {
                        context.IsEzBobServiceUp = true;
                    }))
                .WithEndpoint<RestService>(c => c
                    .When(context => context.IsRestServerStarted && context.IsEzBobServiceUp && context.IsThirdPartiesUp,
                        (bus, contxt) => {
                            string customerId = SendSignupRequest();
                            Assert.IsNotEmpty(customerId, "Got empty customer id.");
                            int kk = 0;
                        }))
                .Done(context => context.IsDone)
                .Run();
        }

        private string SendSignupRequest() {
            string customerId = null;
            SignupSender.CustomerSignupSendRecieve(request => {
                request
                    .WithRandomEmailAddress()
                    .WithRandomPassword();
            },
                response => {
                    customerId = response.CustomerId;
                    Assert.IsEmpty(response.Errors, "Got errors on signup.");
                });

            return customerId;
        }
    }
}
