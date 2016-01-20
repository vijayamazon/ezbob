namespace EzBobRest.Modules.Marketplaces.PayPal {
    using EzBobRest.NancySwagger.Core;
    using EzBobRest.NancySwagger.Fluent;
    using Nancy;
    using Nancy.Metadata.Modules;

    /// <summary>
    /// Describes <see cref="PayPalModule"/> in swagger's format.
    /// There is naming convention: metadata module should end with 'MetadataModule'
    /// </summary>
    public class PayPalMetadataModule : MetadataModule<SwaggerRouteMetadata> {
        public PayPalMetadataModule() {
            string tag = "03. Marketplace API"; //swagger groups descriptions by tags
            DescribeGetRedirectonUrl(tag);
            DescribeRegisterCustomer(tag);
        }

        private void DescribeGetRedirectonUrl(string tag) {

            string implementationNotes = "<ol><li>1. Request PayPal redirect url.</li><li>2. Register customer by sending his credentials</li></ol>";

            Describe["PayPalRirectUrl"] = desc => new SwaggerRouteMetadata(desc)
                .With(i => i.WithDescription(implementationNotes, tags: tag))
                .With(i => i.WithResponseModel(HttpStatusCode.BadRequest, new {
                    CustomerId = string.Empty,
                    Errors = new string[] {}
                }.GetType(), "Invalid request format"))
                .With(i => i.WithResponseModel(HttpStatusCode.OK, new {
                    CustomerId = string.Empty,
                    RedirectUrl = string.Empty
                }.GetType(), "some description"))
                .With(i => i.WithRequestParameter("customerId", "customer id given at sign-up"))
                .With(i => i.WithRequestModel(new {
                    Callback = string.Empty
                }.GetType()));
        }

        private void DescribeRegisterCustomer(string tag) {
            string implementationNotes = "<ol><li>1. Request PayPal redirect url.</li><li>2. Register customer by sending his credentials</li></ol>";
            Describe["PayPalRegisterCustomer"] = desc => new SwaggerRouteMetadata(desc)
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
                    CustomerEmailAddress = string.Empty,
                    RequestToken = string.Empty,
                    VerificationToken = string.Empty
                }.GetType()));
        }
    }
}
