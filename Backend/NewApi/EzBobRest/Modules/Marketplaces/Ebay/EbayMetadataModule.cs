namespace EzBobRest.Modules.Marketplaces.Ebay {
    using EzBobRest.NancySwagger.Core;
    using EzBobRest.NancySwagger.Fluent;
    using Nancy;
    using Nancy.Metadata.Modules;

    /// <summary>
    /// Describes <see cref="EbayModule"/> in swagger's format.
    /// There is naming convention: metadata module should end with 'MetadataModule'
    /// </summary>
    public class EbayMetadataModule : MetadataModule<SwaggerRouteMetadata> {
        public EbayMetadataModule() {
            string tag = "03. Marketplace API"; //swagger groups descriptions by tags
            DescribeGetRedirectUrl(tag);
            DescribeRegisterCustomer(tag);
        }

        /// <summary>
        /// Describes the get redirection URL.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void DescribeGetRedirectUrl(string tag) {

            string implementationNotes = "<ol><li>1. Request Ebay redirect url.</li><li>2. Register customer by sending his credentials</li></ol>";

            Describe["EbayRirectUrl"] = desc => new SwaggerRouteMetadata(desc)
                .With(i => i.WithDescription(implementationNotes, tags: tag))
                .With(i => i.WithResponseModel(HttpStatusCode.BadRequest, new {
                    CustomerId = string.Empty,
                    Errors = new string[] {}
                }.GetType(), "Invalid request format"))
                .With(i => i.WithResponseModel(HttpStatusCode.OK, new {
                    CustomerId = string.Empty,
                    RedirectUrl = string.Empty,
                    SessionId = string.Empty
                }.GetType(), "some description"))
                .With(i => i.WithRequestParameter("customerId", "customer id given at sign-up"));
        }

        /// <summary>
        /// Describes the register customer.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void DescribeRegisterCustomer(string tag) {
            string implementationNotes = "<ol><li>1. Request Ebay redirect url.</li><li>2. Register customer by sending his credentials</li></ol>";
            Describe["EbayRegisterCustomer"] = desc => new SwaggerRouteMetadata(desc)
                .With(i => i.WithDescription(implementationNotes, tags: tag))
                .With(i => i.WithResponseModel(HttpStatusCode.BadRequest, new {
                    CustomerId = string.Empty,
                    Errors = new string[] {}
                }.GetType(), "Invalid request format"))
                .With(i => i.WithResponseModel(HttpStatusCode.OK, new {
                    CustomerId = string.Empty
                }.GetType(), "some description"))
                .With(i => i.WithRequestParameter("customerId", "customer id given at sign-up"))
                .With(i => i.WithRequestParameter("sessionId", "sessionId given with redirect url response"))
                .With(i => i.WithRequestModel(new {
                    Token = string.Empty,
                    MarketplaceName = string.Empty
                }.GetType(), "body"));
        }
    }
}
