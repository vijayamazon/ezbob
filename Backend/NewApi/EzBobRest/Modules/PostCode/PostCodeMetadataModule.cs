namespace EzBobRest.Modules.PostCode {
    using System.Linq;
    using EzBobRest.NancySwagger.Core;
    using EzBobRest.NancySwagger.Fluent;
    using Nancy;
    using Nancy.Metadata.Modules;

    public class PostCodeMetadataModule : MetadataModule<SwaggerRouteMetadata> {
        public PostCodeMetadataModule() {
            string tag = "04. Address by postcode API"; //swagger groups descriptions by tags
            DescribeGetAddressesByPostcode(tag);
            DescribeGetAddressDetails(tag);
        }

        /// <summary>
        /// Describes the get addresses by postcode.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void DescribeGetAddressesByPostcode(string tag) {
            string implementationNotes = "Returns addresses associated with provided postal code.<br>Use 'id' of address to get it details.";

            var addressesModel = Enumerable.Repeat(new {
                L = string.Empty,
                Id = string.Empty
            }, 1);

            Describe["GetAddressesByPostcode"] = desc => new SwaggerRouteMetadata(desc)
                .With(i => i.WithDescription(implementationNotes, tags: tag))
                .With(i => i.WithResponseModel(HttpStatusCode.BadRequest, new {
                    CustomerId = string.Empty,
                    Errors = new string[] {}
                }.GetType(), "Invalid request format"))
                .With(i => i.WithResponseModel(HttpStatusCode.OK, new {
                    CustomerId = string.Empty,
                    Count = 0,
                    Addresses = addressesModel
                }.GetType(), "some description"))
                .With(i => i
                    .WithRequestParameter("postcode", "post code for which to return the addresses")
                    .WithRequestParameter("customerId", "customer id given at sign-up."));
        }


        /// <summary>
        /// Describes the get address details.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void DescribeGetAddressDetails(string tag) {
            string implementationNotes = "Returns address details by 'id' of address provided with postal code request";

            var addressModel = new {
                Found = 0,
                Id = string.Empty,
                Organization = string.Empty,
                Line1 = string.Empty,
                Line2 = string.Empty,
                Line3 = string.Empty,
                Town = string.Empty,
                County = string.Empty,
                Postcode = string.Empty,
                Country = string.Empty,
                RawPostcode = string.Empty,
                DeliveryPointSiffix = string.Empty,
                NoHouseholds = 0,
                SmallOrg = string.Empty,
                Pobox = string.Empty,
                MailSortCode = string.Empty,
                Udprn = string.Empty
            };

            Describe["GetAddressDetails"] = desc => new SwaggerRouteMetadata(desc)
                .With(i => i.WithDescription(implementationNotes, tags: tag))
                .With(i => i.WithResponseModel(HttpStatusCode.BadRequest, new {
                    CustomerId = string.Empty,
                    Errors = new string[] {}
                }.GetType(), "Invalid request format"))
                .With(i => i.WithResponseModel(HttpStatusCode.OK, new {
                    CustomerId = string.Empty,
                    Address = addressModel
                }.GetType(), "some description"))
                .With(i => i
                    .WithRequestParameter("addressId", "id of address you get from postal code lookup")
                    .WithRequestParameter("customerId", "customer id given at sign-up."));
        }
    }
}
