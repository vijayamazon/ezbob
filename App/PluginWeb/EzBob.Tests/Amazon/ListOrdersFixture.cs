using System;
using MarketplaceWebServiceOrders.MarketplaceWebServiceOrders;
using MarketplaceWebServiceOrders.MarketplaceWebServiceOrders.Model;
using NUnit.Framework;

namespace EzBob.Tests.Amazon
{
    [TestFixture]
    public class ListOrdersFixture
    {
        private MarketplaceWebServiceOrdersClient _client;

        [SetUp]
        public void SetUp()
        {
            var config = new MarketplaceWebServiceOrdersConfig
            {
                ServiceURL = "https://mws.amazonservices.co.uk/Orders/2011-01-01"
            };

            _client = new MarketplaceWebServiceOrdersClient("C#", "4.0", "AKIAJXUDX6A3XIMZLWFA", "4yQzxltFZjlytmkKmlHhkAAcZTTZUbHpJekTOFj2", config);
        }

        [Test]
        public void real_customer()
        {
            ListOrdersResponse response;
            response = GetOrders("A1F83G8C2ARO7P", "A1OXZLJTRHTZJ3"); //real one
            Assert.That(response.ListOrdersResult.Orders.Order.Count, Is.GreaterThan(0));
        }

        [Test]
        [ExpectedException]
        public void failing_one()
        {
            ListOrdersResponse response;
            response = GetOrders("A1F83G8C2ARO7", "A3A9KK6KZ6IZFN"); //failing one
        }

        [Test]
        public void fixed_failing_one()
        {
            ListOrdersResponse response;
            response = GetOrders("A1F83G8C2ARO7P", "A3A9KK6KZ6IZFN"); //fixed one
            Assert.That(response.ListOrdersResult.Orders.Order.Count, Is.GreaterThan(0));
        }

        private ListOrdersResponse GetOrders(string mpId, string sellerId)
        {
            var listOrdersRequest = new ListOrdersRequest();

            var marketplaceIdList = new MarketplaceIdList();
            marketplaceIdList.WithId(new [] {mpId});

            listOrdersRequest
                .WithSellerId(sellerId)
                .WithCreatedAfter(new DateTime(2012, 6, 4))
                .WithMarketplaceId(marketplaceIdList)
                ;

            return _client.ListOrders(listOrdersRequest);
        }
    }
}