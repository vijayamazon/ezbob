namespace EzBobRest.Modules {
    using EzBobRest.NancySwagger.Core;
    using EzBobRest.NancySwagger.Fluent;
    using Nancy;
    using Nancy.Metadata.Modules;

    /// <summary>
    /// Describes <see cref="CustomerModule"/> in swagger's format.
    /// There is naming convention: metadata module should end with 'MetadataModule'
    /// </summary>
    public class CustomerMetadataModule : MetadataModule<SwaggerRouteMetadata> {
        private readonly object update = new {
            Address = new {
                Organization = string.Empty,
                Line1 = string.Empty,
                Line2 = string.Empty,
                Line3 = string.Empty,
                Town = string.Empty,
                County = string.Empty,
                Postcode = string.Empty,
                Country = string.Empty,
                Rawpostcode = string.Empty,
                Deliverypointsuffix = string.Empty,
                Nohouseholds = string.Empty,
                Smallorg = string.Empty,
                Pobox = string.Empty,
                Mailsortcode = string.Empty,
                Udprn = string.Empty
            },

            ContactDetails = new {
                MobilePhone = string.Empty,
                PhoneNumber = string.Empty,
                EmailAddress = string.Empty
            },

            PersonalDetails = new {
                FirstName = string.Empty,
                MiddleName = string.Empty,
                SurName = string.Empty,
                Gender = string.Empty,
                MeritalStatus = 0,
                DateOfBirth = string.Empty
            },

            OwnedProperties = new[] {
                new {
                    IsLivingNow = false,
                    TimeAtAddress = string.Empty,
                    Organization = string.Empty,
                    Line1 = string.Empty,
                    Line2 = string.Empty,
                    Line3 = string.Empty,
                    Town = string.Empty,
                    County = string.Empty,
                    Postcode = string.Empty,
                    Country = string.Empty,
                    Rawpostcode = string.Empty,
                    Deliverypointsuffix = string.Empty,
                    Nohouseholds = string.Empty,
                    Smallorg = string.Empty,
                    Pobox = string.Empty,
                    Mailsortcode = string.Empty,
                    Udprn = string.Empty
                }
            },

            LivingAccommodations = new[] {
                new {
                    IsLivingNow = false,
                    TimeAtAddress = string.Empty,
                    HousingType = string.Empty,
                    Organization = string.Empty,
                    Line1 = string.Empty,
                    Line2 = string.Empty,
                    Line3 = string.Empty,
                    Town = string.Empty,
                    County = string.Empty,
                    Postcode = string.Empty,
                    Country = string.Empty,
                    Rawpostcode = string.Empty,
                    Deliverypointsuffix = string.Empty,
                    Nohouseholds = string.Empty,
                    Smallorg = string.Empty,
                    Pobox = string.Empty,
                    Mailsortcode = string.Empty,
                    Udprn = string.Empty
                }
            }
        };


        public CustomerMetadataModule() {

            Describe["SignupCustomer"] = desc => new SwaggerRouteMetadata(desc)
                .With(i => i.WithDescription("Defines customer related operations", tags: "01. Customer API"))
                .With(i => i.WithResponseModel(HttpStatusCode.BadRequest, new {
                    CustomerId = string.Empty,
                    Errors = new string[] {}
                }.GetType(), "Invalid request format"))
                .With(i => i.WithResponseModel(HttpStatusCode.OK, new {
                    CustomerId = string.Empty
                }.GetType(), "some description"))
                .With(i => i.WithRequestModel(new {
                    Account = new {
                        EmailAddress = ""
                    }
                }.GetType(), "body", "creates new customer"));

            string customerApiDescription = "<u>Living accommodation:</u> should be empty if person lives at his own property<br/>" +
                "<u>All properties are optional.</u> Fill only what you have, omit all others (there is no need to provide default values)";


            Describe["UpdateCustomer"] = desc => new SwaggerRouteMetadata(desc)
                .With(i => i.WithDescription(customerApiDescription, tags: "01. Customer API"))
                .With(i => i.WithResponseModel(HttpStatusCode.BadRequest, new {
                    CustomerId = string.Empty,
                    Errors = new string[] { }
                }.GetType(), "Invalid request format"))
                .With(i => i.WithResponseModel(HttpStatusCode.OK, new
                {
                    CustomerId = string.Empty
                }.GetType(), "some description"))
                .With(i => i.WithRequestParameter("id", "customer id given at sign-up"))
                .With(i => i.WithRequestModel(this.update.GetType(), "body", "<ul><li><u>DateOfBirth:</u><br/>'yyyy-MM-ddTHH:mm:ssZ'</li><li><u>Gender:</u> M,F</li><li><u>MeritalStatus:</u><br/>Married=0,Single=1,Divorced=2<br/>Widowed=3,LivingTogether=4<br/>Separated=5,Other=6</li><li><u>HousingType:</u><br/>Renting=1,Social=2,Parents=3</li></ul>"));
        }
    }
}