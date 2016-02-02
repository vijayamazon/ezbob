namespace EzBobRest.Modules.Marketplaces.Yodlee {
    using EzBobRest.NancySwagger.Core;
    using EzBobRest.NancySwagger.Fluent;
    using Nancy;
    using Nancy.Metadata.Modules;

    /// <summary>
    /// Describes <see cref="YodleeModule"/>
    /// </summary>
    public class YodleeMetadataModule : MetadataModule<SwaggerRouteMetadata> {
        public YodleeMetadataModule() {
            string tag = "03. Marketplace API"; //swagger groups descriptions by tags
            DescribeGetFastlinkUrl(tag);
            DescribeAccountAddedNotification(tag);
        }

        /// <summary>
        /// Describes the account added notification.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void DescribeAccountAddedNotification(string tag) {
            string implementationNotes = "Notification should be sent when you know that customer finished to add an account.";
            Describe["OnAccountAdded"] = desc => new SwaggerRouteMetadata(desc)
                .With(i => i.WithDescription(implementationNotes, tags: tag))
                .With(i => i.WithResponseModel(HttpStatusCode.BadRequest, new {
                    CustomerId = string.Empty,
                    Errors = new string[] {}
                }.GetType(), "Invalid request format"))
                .With(i => i.WithResponseModel(HttpStatusCode.OK, new {
                    CustomerId = string.Empty
                }.GetType(), "some description"))
                .With(i => i.WithRequestParameter("customerId", "customer id given at sign-up."));
        }

        /// <summary>
        /// Describes the get fastlink URL.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void DescribeGetFastlinkUrl(string tag) {

            string implementationNotes = "Creates fast link url.";

            Describe["GetFastLink"] = desc => new SwaggerRouteMetadata(desc)
                .With(i => i.WithDescription(implementationNotes, tags: tag))
                .With(i => i.WithResponseModel(HttpStatusCode.BadRequest, new {
                    CustomerId = string.Empty,
                    Errors = new string[] {}
                }.GetType(), "Invalid request format"))
                .With(i => i.WithResponseModel(HttpStatusCode.OK, new {
                    CustomerId = string.Empty,
                    FastlinkUrl = string.Empty
                }.GetType(), "some description"))
                .With(i => i.WithRequestParameter("customerId", "customer id given at sign-up")
                    .WithRequestParameter("contentServiceId", "represents certain bank"));
        }
    }
}
