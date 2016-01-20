namespace EzBobRest.Modules.Customer {
    using System;
    using EzBobRest.NancySwagger.Core;
    using EzBobRest.NancySwagger.Fluent;
    using Nancy;
    using Nancy.Metadata.Modules;

    /// <summary>
    /// Describes <see cref="CustomerVerificationModule"/> in swagger's format.
    /// There is naming convention: metadata module should end with 'MetadataModule'
    /// </summary>
    public class CustomerVerificationMetadataModule : MetadataModule<SwaggerRouteMetadata> {
        public CustomerVerificationMetadataModule() {
            string tag = "01. Customer API"; //swagger groups descriptions by tags
            DescribeCustomerSmsVerification(tag);
            DescribeVerificationCodeValidation(tag);
        }

        /// <summary>
        /// Describes the verification code validation.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void DescribeVerificationCodeValidation(string tag) {

            string implementationNotes = @"Validates verification code provided by customer with one that was sent to him.";

            Describe["ValidateVerificationCode"] = desc => new SwaggerRouteMetadata(desc)
                .With(i => i.WithDescription(implementationNotes, tags: tag)
                    .WithRequestParameter("customerId", "customer id given at sign-up")
                    .WithRequestParameter("verificationToken", "this token is being provided after sending sms")
                    .WithRequestModel(new {
                        VerificationCode = String.Empty
                    }.GetType(), "body", "VerificationCode provided by customer")
                    .WithResponseModel(HttpStatusCode.OK, new {
                        CustomerId = string.Empty,
                    }.GetType(), "some description")
                    .WithResponseModel(HttpStatusCode.BadRequest, new {
                        CustomerId = string.Empty,
                        Errors = new string[] {}
                    }.GetType(), "Invalid request format")
                );
        }

        /// <summary>
        /// Describes the customer SMS verification.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void DescribeCustomerSmsVerification(string tag) {

            string description = @"Verifies customer phone number by sending SMS with verification code.
                                    You can customize message text be providing optional header and footer text.
                                    <b>VerificationToken</b> from response should be used to validate verification code.";

            Describe["SendVerificationSms"] = desc => new SwaggerRouteMetadata(desc)
                .With(i => i.WithDescription(description, tags: tag))
                .With(i => i.WithResponseModel(HttpStatusCode.BadRequest, new {
                    CustomerId = string.Empty,
                    Errors = new string[] {}
                }.GetType(), "Invalid request format"))
                .With(i => i.WithResponseModel(HttpStatusCode.OK, new {
                    CustomerId = string.Empty,
                    VerificationToken = string.Empty
                }.GetType(), "some description"))
                .With(i => i.WithRequestParameter("customerId", "customer id given at sign-up"))
                .With(i => i.WithRequestModel(new {
                    PhoneNumber = String.Empty,
                    MessageHeader = String.Empty,
                    MessageFooter = String.Empty
                }.GetType(), "body", @"<ul>
                                            <li><u>PhoneNumber:</u><br/>Mandatory. Verification sms will be sent to this number.</li>
                                            <li><u>MessageHeader:</u><br/>Optional. Placed before verification code.</li>
                                            <li><u>MessageFooter:</u><br/>Optional. Placed after verification code.</li>
                                      </ul>"));
        }
    }
}
