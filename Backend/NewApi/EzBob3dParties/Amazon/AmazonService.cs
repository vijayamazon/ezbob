namespace EzBob3dParties.Amazon {
    using EzBob3dParties.Amazon.RatingScraper;
    using EzBob3dParties.Amazon.Src.CustomerApi;
    using EzBob3dParties.Amazon.Src.OrdersApi;
    using EzBob3dParties.Amazon.Src.ProductsApi;
    using EzBobCommon;

    class AmazonService : IAmazonService {
        [Injected]
        public AmazonConfig Config { get; set; }

        [Injected]
        public IMwsCustomerService Customer { get; set; }

        [Injected]
        public IMwsOrdersService Orders { get; set; }

        [Injected]
        public IMwsProductsService Products { get; set; }

        [Injected]
        public IAmazonCustomerRating Rating { get; set; }

        [PostInject]
        private void Init() {
            Customer.Init(Config.AccessKey, Config.SecretKey, Config.ApplicationName, Config.ApplicationVersion);
            Orders.Init(Config.AccessKey, Config.SecretKey, Config.ApplicationName, Config.ApplicationVersion);
            Products.Init(Config.AccessKey, Config.SecretKey, Config.ApplicationName, Config.ApplicationVersion);
        }
    }
}
