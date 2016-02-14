namespace EzBobRest.Modules.DocsUpload {
    using EzBobRest.NancySwagger.Core;
    using EzBobRest.NancySwagger.Fluent;
    using Nancy;
    using Nancy.Metadata.Modules;

    /// <summary>
    /// Describes <see cref="DocsUploadModule"/>.
    /// </summary>
    public class DocsUploadMetadataModule : MetadataModule<SwaggerRouteMetadata> {
        public DocsUploadMetadataModule() {
            string tag = "05. Files upload."; //swagger groups descriptions by tags
            DescribeBankFilesUpload(tag);
            DescribeMiscFilesUpload(tag);
        }

        /// <summary>
        /// Describes the bank files upload.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void DescribeBankFilesUpload(string tag) {
            DescribeFilesUpload(tag, "BankDocsUpload", "Upload bank documents.");
        }

        /// <summary>
        /// Describes the misc files upload.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void DescribeMiscFilesUpload(string tag) {
            DescribeFilesUpload(tag, "BankMiscUpload", "Upload miscellaneous documents.");
        }

        /// <summary>
        /// Describes the bank files upload.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="key">The key.</param>
        /// <param name="comment">The comment.</param>
        private void DescribeFilesUpload(string tag, string key, string comment) {

            Describe[key] = desc => new SwaggerRouteMetadata(desc)
                .With(i => i.WithDescription(comment, tags: tag))
                .With(i => i.WithResponseModel(HttpStatusCode.BadRequest, new {
                    CustomerId = string.Empty,
                    Errors = new string[] {}
                }.GetType(), "Invalid request format"))
                .With(i => i.WithResponseModel(HttpStatusCode.OK, new {
                    CustomerId = string.Empty,
                }.GetType(), "some description"))
                .With(i => i.WithRequestParameter("customerId", "customer id given at sign-up."));
        }
    }
}
