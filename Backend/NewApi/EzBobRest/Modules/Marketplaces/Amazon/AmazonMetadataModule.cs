namespace EzBobRest.Modules.Marketplaces.Amazon {
    using EzBobRest.NancySwagger.Core;
    using EzBobRest.NancySwagger.Fluent;
    using Nancy;
    using Nancy.Metadata.Modules;

    /// <summary>
    /// Describes <see cref="AmazonModule"/> in swagger's format.
    /// There is naming convention: metadata module should end with 'MetadataModule'
    /// </summary>
    public class AmazonMetadataModule : MetadataModule<SwaggerRouteMetadata> {
        public AmazonMetadataModule() {
            string tag = "03. Marketplace API"; //swagger groups descriptions by tags
            DescribeRegisterCustomer(tag);
        }

        /// <summary>
        /// Describes the register customer.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void DescribeRegisterCustomer(string tag) {
            string implementationNotes = "Register customer by sending his credential";
            Describe["AmazonRegisterCustomer"] = desc => new SwaggerRouteMetadata(desc)
                .With(i => i.WithDescription(implementationNotes, tags: tag))
                .With(i => i.WithResponseModel(HttpStatusCode.BadRequest, new {
                    CustomerId = string.Empty,
                    Errors = new string[] {}
                }.GetType(), "Invalid request format"))
                .With(i => i.WithResponseModel(HttpStatusCode.OK, new {
                    CustomerId = string.Empty
                }.GetType(), "some description"))
                .With(i => i.WithRequestParameter("customerId", "customer id given at sign-up"))
                .With(i => i.WithRequestModel(new {
                    AuthorizationToken = string.Empty,
                    SellerId = string.Empty,
                    MarketplaceId = new string[] {}
                }.GetType(), "body", @"<ul>
                                            <li><u>AuthorizationToken:</u><br/>Mandatory. Should be provided by customer.</li>
                                            <li><u>SellerId:</u><br/>Mandatory. Should be provided by customer.</li>
                                            <li><u>MarketplaceId:</u><br/>Mandatory. Any Marketplace in which the seller is registered to sell.</li>
                                      </ul>"));
        }
    }
}
