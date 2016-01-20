namespace EzBobRest.Modules.Marketplaces.Hmrc {
    using EzBobRest.NancySwagger.Core;
    using EzBobRest.NancySwagger.Fluent;
    using Nancy;
    using Nancy.Metadata.Modules;

    /// <summary>
    /// Describes <see cref="HmrcModule"/>
    /// </summary>
    public class HmrcMetadataModule : MetadataModule<SwaggerRouteMetadata> {
        public HmrcMetadataModule() {
            string tag = "03. Marketplace API"; //swagger groups descriptions by tags
            DescribeHrmcRegistration(tag);
        }

        private void DescribeHrmcRegistration(string tag) {
            string implementationNotes = "Registers HMRC";

            Describe["RegisterHmrc"] = desc => new SwaggerRouteMetadata(desc)
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
                    UserName = string.Empty,
                    Password = string.Empty
                }.GetType(), "body"));
        }
    }
}
