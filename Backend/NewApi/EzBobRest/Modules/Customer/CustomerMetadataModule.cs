namespace EzBobRest.Modules.Customer {
    using EzBobRest.Modules.Customer.ResponseModels;
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

            AdditionalOwnedProperties = new[] {
                new {
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
            PreviousLivingAddress = new {
                MonthsAtAddress = string.Empty,
                HousingType = 1,
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
            CurrentLivingAddress = new {
                MonthsAtAddress = string.Empty,
                HousingType = 1,
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
                Udprn = string.Empty,
                Mailsortcode = string.Empty //in previous living address this field is before Udprn
                //this is deliberately , because otherwise the schema of this object disappears
            },

            RequestedAmount = 0m
        };

        public CustomerMetadataModule() {
            string tag = "01. Customer API"; //swagger groups descriptions by tags
            DescribeCustomerSignup(tag);
            DescribeCustomerUpdate(tag);
            DescribeGetCustomerDetails(tag);
            DescribeCustomerLogin(tag);
        }

        /// <summary>
        /// Describes the customer login.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void DescribeCustomerLogin(string tag) {
            string customerApiDescription = "Logs in the customer";
            Describe["CustomerLogin"] = desc => new SwaggerRouteMetadata(desc)
                .With(i => i.WithDescription(customerApiDescription, tags: tag))
                .With(i => i.WithResponseModel(HttpStatusCode.BadRequest, new {
                    Errors = new string[] {}
                }.GetType(), "Invalid request format"))
                .With(i => i.WithResponseModel(HttpStatusCode.OK, new {
                    CustomerId = string.Empty
                }.GetType(), "some description"))
                .With(i => i.WithRequestModel(new {
                    EmailAddress = string.Empty,
                    Password = string.Empty,
                }.GetType()));
        }

        /// <summary>
        /// Describes the get customer details.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void DescribeGetCustomerDetails(string tag) {
            string customerApiDescription = "Gets customer's details";
            Describe["GetCustomerDetails"] = desc => new SwaggerRouteMetadata(desc)
                .With(i => i.WithDescription(customerApiDescription, tags: tag))
                .With(i => i.WithResponseModel(HttpStatusCode.BadRequest, new {
                    CustomerId = string.Empty,
                    Errors = new string[] {}
                }.GetType(), "Invalid request format"))
                .With(i => i.WithResponseModel(HttpStatusCode.OK, new {
                    CustomerId = string.Empty,
                    Details = new CustomerGetDetailsResponseModel()
                }.GetType(), "some description"))
                .With(i => i.WithRequestParameter("customerId", "customer id given at sign-up"));
        }

        /// <summary>
        /// Describes the customer update.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void DescribeCustomerUpdate(string tag) {
            string customerApiDescription = "Updates customer's data";
            Describe["UpdateCustomer"] = desc => new SwaggerRouteMetadata(desc)
                .With(i => i.WithDescription(customerApiDescription, tags: tag))
                .With(i => i.WithResponseModel(HttpStatusCode.BadRequest, new {
                    CustomerId = string.Empty,
                    Errors = new string[] {}
                }.GetType(), "Invalid request format"))
                .With(i => i.WithResponseModel(HttpStatusCode.OK, new {
                    CustomerId = string.Empty
                }.GetType(), "some description"))
                .With(i => i.WithRequestParameter("id", "customer id given at sign-up"))
                .With(i => i.WithRequestModel(this.update.GetType(), "body",
                    @"<ul>
                        <li><u>DateOfBirth:</u><br/>'yyyy-MM-ddTHH:mm:ssZ'</li>
                        <li><u>Gender:</u> M,F</li>
                        <li><u>MeritalStatus:</u><br/>Married=0,Single=1,Divorced=2<br/>Widowed=3,LivingTogether=4<br/>Separated=5,Other=6</li>
                        <li><u>HousingType:</u><br/>Renting=1,Social=2,Living with parents=3,Own Property=4</li>
                      </ul>"));
        }

        /// <summary>
        /// Describes the customer sign-up.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void DescribeCustomerSignup(string tag) {
            Describe["SignupCustomer"] = desc => new SwaggerRouteMetadata(desc)
                .With(i => i.WithDescription("Defines customer related operations", tags: tag))
                .With(i => i.WithResponseModel(HttpStatusCode.BadRequest, new {
                    Errors = new string[] {}
                }.GetType(), "Invalid request format"))
                .With(i => i.WithResponseModel(HttpStatusCode.OK, new {
                    CustomerId = string.Empty
                }.GetType(), "some description"))
                .With(i => i.WithRequestModel(new {
                    EmailAddress = string.Empty,
                    Password = string.Empty,
                    PhoneNumber = string.Empty,
                    PhoneNumberValidationCode = string.Empty,
                    SequrityQuestionId = 1,
                    SecurityQuestionAnswer = string.Empty
                }.GetType(), "body", @"SequrityQuestionId:<br/>
                                      <ul>
                                        <li>1 - What is your mother's maiden name?</li>
                                        <li>2 - What was your childhood nickname?</li>
                                        <li>3 - What was the name of your closest childhood friend?</li>
                                      </ul>"));
        }
    }
}