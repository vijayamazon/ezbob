namespace EzBobRest.Modules.Company {
    using System;
    using System.Linq;
    using EzBobModels.Enums;
    using EzBobRest.NancySwagger.Core;
    using EzBobRest.NancySwagger.Fluent;
    using Nancy;
    using Nancy.Metadata.Modules;

    /// <summary>
    /// Describes <see cref="CompanyModule"/> in swagger's format.
    /// There is naming convention: metadata module should end with 'MetadataModule'
    /// </summary>
    public class CompanyMetadataModule : MetadataModule<SwaggerRouteMetadata> {

        private static readonly string targetingDescription = @"Suggests companies based on provided search parameters.<br/><u><b>Rules:</b></u><b>
                                                                                                            <ul>
                                                                                                                <li>For Limited company: registration number is mandatory</li>
                                                                                                                <li>For Not limited company: company name is mandatory</li>
                                                                                                                <li>Post code: optional, but if provided greatly reduce number of suggestions</li>
                                                                                                            </ul>
                                                                                                           </b>";

        public class CompanyInfo {
            public string LegalStatus { get; set; }
            public string BusinessStatus { get; set; }
            public string MatchScore { get; set; }
            public string BusRefNum { get; set; }
            public string BusName { get; set; }
            public string AddrLine1 { get; set; }
            public string AddrLine2 { get; set; }
            public string AddrLine3 { get; set; }
            public string AddrLine4 { get; set; }
            public string PostCode { get; set; }
            public string SicCodeType { get; set; }
            public string SicCode { get; set; }
            public string SicCodeDesc { get; set; }
            public string MatchedBusName { get; set; }
            public string MatchedBusNameType { get; set; }
        }

        public CompanyMetadataModule() {
            string tag = "02. Company API";
            DescribeCompanyUpdate(tag);
            DescribeCompanyTargeting(tag);
            DescribeDirectorUpsert(tag);
            DescribeShareHolderUpsert(tag);
            DescribeGetCompanyDetails(tag);
        }

        /// <summary>
        /// Describes the get company details.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void DescribeGetCompanyDetails(string tag) {
            string implementationDetails = "Gets company's details.";
            Describe["GetCompanyDetails"] = desc => new SwaggerRouteMetadata(desc)
                .With(i => i.WithDescription(implementationDetails, tags: tag))
                .With(i => i.WithResponseModel(HttpStatusCode.BadRequest, new {
                    CustomerId = string.Empty,
                    CompanyId = string.Empty,
                    Errors = new string[] {}
                }.GetType(), "Invalid request format"))
                .With(i => i.WithResponseModel(HttpStatusCode.OK, new {
                    CustomerId = string.Empty,
                    CompanyId = string.Empty,
                    Details = new {
                        CompanyDetails = GetCompanyDetails(),
                        Authorities = Enumerable.Repeat(GetDirectorModel(), 1)
                            .ToArray()
                    }
                }.GetType(), "some description"))
                .With(i => i.WithRequestParameter("customerId", "customer id given at sign-up"))
                .With(i => i.WithRequestParameter("companyId", "id created by first 'update company' request"));

        }

        /// <summary>
        /// Describes the director upsert.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void DescribeDirectorUpsert(string tag) {
            string implementationDetails = "Updates existing director if authorityId is specified, or creates a new shareholder if authorityId is not specified";
            Describe["UpsertDirector"] = desc => new SwaggerRouteMetadata(desc)
                .With(i => i.WithDescription(implementationDetails, tags: tag))
                .With(i => i.WithResponseModel(HttpStatusCode.BadRequest, new {
                    CustomerId = string.Empty,
                    CompanyId = string.Empty,
                    AuthorityId = string.Empty,
                    Errors = new string[] {}
                }.GetType(), "Invalid request format"))
                .With(i => i.WithResponseModel(HttpStatusCode.OK, new {
                    CustomerId = string.Empty,
                    CompanyId = string.Empty,
                    AuthorityId = string.Empty
                }.GetType(), "some description"))
                .With(i => i.WithRequestParameter("customerId", "customer id given at sign-up"))
                .With(i => i.WithRequestParameter("companyId", "id created by first 'update company' request"))
                .With(i => i.WithRequestParameter("authorityId", "if specified designates update and if not, designates create", "string", null, false))
                .With(i => i.WithRequestModel(GetDirectorModel().GetType(), "body", @"<ul>
                                                                                                <li><u>DateOfBirth:</u><br/>'yyyy-MM-ddTHH:mm:ssZ'</li>
                                                                                                <li><u>Gender:</u> M,F</li>                                                                                              
                                                                                            </ul>"));

            //  <li><u>MeritalStatus:</u><br/>Married=0,Single=1,Divorced=2<br/>Widowed=3,LivingTogether=4<br/>Separated=5,Other=6</li>

        }

        /// <summary>
        /// Describes the share holder upsert.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void DescribeShareHolderUpsert(string tag) {
            string implementationDetails = "Updates existing shareholder if authorityId is specified, or creates a new shareholder if authorityId is not specified";
            Describe["UpsertShareHolder"] = desc => new SwaggerRouteMetadata(desc)
                .With(i => i.WithDescription(implementationDetails, tags: tag))
                .With(i => i.WithResponseModel(HttpStatusCode.BadRequest, new {
                    CustomerId = string.Empty,
                    CompanyId = string.Empty,
                    AuthorityId = string.Empty,
                    Errors = new string[] {}
                }.GetType(), "Invalid request format"))
                .With(i => i.WithResponseModel(HttpStatusCode.OK, new {
                    CustomerId = string.Empty,
                    CompanyId = string.Empty,
                    AuthorityId = string.Empty
                }.GetType(), "some description"))
                .With(i => i.WithRequestParameter("customerId", "customer id given at sign-up"))
                .With(i => i.WithRequestParameter("companyId", "id created by first 'update company' request"))
                .With(i => i.WithRequestParameter("authorityId", "if specified designates update and if not, designates create", "string", null, false))
                .With(i => i.WithRequestModel(GetShareHolderRequestModelType(), "body", @"<ul>
                                                                                                <li><u>DateOfBirth:</u><br/>'yyyy-MM-ddTHH:mm:ssZ'</li>
                                                                                                <li><u>Gender:</u> M,F</li>
                                                                                                <li><u>MeritalStatus:</u><br/>Married=0,Single=1,Divorced=2<br/>Widowed=3,LivingTogether=4<br/>Separated=5,Other=6</li>
                                                                                            </ul>"));

        }

        /// <summary>
        /// Describes the company targeting.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void DescribeCompanyTargeting(string tag) {
            Describe["TargetCompany"] = desc => new SwaggerRouteMetadata(desc)
                .With(i => i.WithDescription(targetingDescription, tags: tag)
                    .WithRequestParameter("customerId", "customer id given at sign-up")
                    .WithRequestModel(new {
                        IsLimited = false,
                        CompanyName = string.Empty,
                        RegistrationNumber = string.Empty,
                        PostCode = string.Empty
                    }.GetType())
                    .WithResponseModel(HttpStatusCode.BadRequest, new {
                        CustomerId = string.Empty,
                        Errors = new string[] {}
                    }.GetType(), "Invalid request format")
                    .WithResponseModel(HttpStatusCode.OK, new {
                        CustomerId = string.Empty,
                        Suggestions = new CompanyInfo[] {}
                    }.GetType())
                );
        }

        /// <summary>
        /// Describes the company update.
        /// </summary>
        private void DescribeCompanyUpdate(string tag) {
            Describe["UpdateCompany"] = desc => new SwaggerRouteMetadata(desc)
                .With(i => i.WithDescription("Defines company related operations", tags: tag))
                .With(i => i.WithResponseModel(HttpStatusCode.BadRequest, new {
                    CustomerId = string.Empty,
                    CompanyId = string.Empty,
                    Errors = new string[] {}
                }.GetType(), "Invalid request format"))
                .With(i => i.WithResponseModel(HttpStatusCode.OK, new {
                    CustomerId = string.Empty,
                    CompanyId = string.Empty
                }.GetType(), "some description"))
                .With(i => i.WithRequestParameter("customerId", "customer id given at sign-up"))
                .With(i => i.WithRequestParameter("companyId", "id created by first 'update company' request", "string", null, false))
                .With(i => i.WithRequestModel(GetCompanyDetails().GetType()));
        }


        /// <summary>
        /// Gets the company details.
        /// </summary>
        /// <returns></returns>
        private static object GetCompanyDetails() {
            return new {
                CompanyDetails = new {
                    IndustryType = IndustryType.Other,
                    TypeOfBusiness = TypeOfBusiness.Entrepreneur,
                    BusinessName = string.Empty,
                    MainPhoneNumber = string.Empty,
                    RegistrationNumber = string.Empty,
                    TotalAnnualRevenue = default(decimal),
                    TotalMonthlySalaryExpenditure = default(double),
                    NumberOfEmployees = default(int),
                    IsVatRegistered = default(bool)
                }
            };
        }

        /// <summary>
        /// Gets the director model.
        /// </summary>
        /// <returns></returns>
        private static object GetDirectorModel() {
            return new {
                Authority = new {
                    IsShareHolder = default(bool),
                    PersonalDetails = new {
                        FirstName = string.Empty,
                        MiddleName = string.Empty,
                        SurName = string.Empty,
                        Gender = string.Empty,
                        MeritalStatus = 0,
                        DateOfBirth = string.Empty
                    },
                    ContactDetails = new {
//                        MobilePhone = string.Empty,
                        PhoneNumber = string.Empty,
                        EmailAddress = string.Empty
                    },
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
                    }
                }
            };
        }



        /// <summary>
        /// Gets the type of the share holder request model.
        /// </summary>
        /// <returns></returns>
        private static Type GetShareHolderRequestModelType() {
            return new {
                Authority = new {
                    PersonalDetails = new {
                        FirstName = string.Empty,
                        MiddleName = string.Empty,
                        SurName = string.Empty,
                        Gender = string.Empty,
//                        MeritalStatus = 0,
                        DateOfBirth = string.Empty
                    },
                    ContactDetails = new {
//                        MobilePhone = string.Empty,
                        PhoneNumber = string.Empty,
                        EmailAddress = string.Empty
                    },
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
                    }
                }
            }.GetType();

        }
    }
}
