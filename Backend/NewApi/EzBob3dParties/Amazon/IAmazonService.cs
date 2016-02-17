namespace EzBob3dParties.Amazon
{
    using EzBob3dParties.Amazon.RatingScraper;
    using EzBob3dParties.Amazon.Src.CustomerApi;
    using EzBob3dParties.Amazon.Src.OrdersApi;
    using EzBob3dParties.Amazon.Src.ProductsApi;

    public interface IAmazonService
    {
        IMwsCustomerService Customer { get; set; }
        IMwsOrdersService Orders { get; set; }
        IMwsProductsService Products { get; set; }
        IAmazonCustomerRating Rating { get; set; }
    }
}
