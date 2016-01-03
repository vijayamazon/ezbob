using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobRest.Modules {
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
        public CompanyMetadataModule() {
            Describe["UpdateCompany"] = desc => new SwaggerRouteMetadata(desc)
                .With(i => i.WithDescription("Defines company related operations", tags: "02. Company update API"))
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
                .With(i => i.WithRequestParameter("companyId", "id created by first updated company request", "string", null, false))
                .With(i => i.WithRequestModel(new {
                    CompanyDetails = new {
                        IndustryType = IndustryType.Other,
                        TypeOfBusiness = TypeOfBusiness.Entrepreneur,
                        BusinessName = string.Empty,
                        PostCode = string.Empty,
                        MainPhoneNumber = string.Empty,
                        RegistrationNumber = string.Empty,
                        TotalAnnualRevenue = default(decimal),
                        TotalMonthlySalaryExpenditure = default(double),
                        NumberOfEmployees = default(int),
                        IsVatRegistered = default(bool),
                        Authorities = new[] {
                            new {
                                IsShareHolder = default(bool),
                                IsDirector = default(bool),
                                PersonalDetails = new {
                                    FirstName = string.Empty,
                                    MiddleName = string.Empty,
                                    SurName = string.Empty,
                                    Gender = string.Empty,
                                    MeritalStatus = 0,
                                    DateOfBirth = string.Empty
                                },
                                ContactDetails = new {
                                    MobilePhone = string.Empty,
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
                        }
                    }
                }.GetType(), "body", "<ul><li><u>DateOfBirth:</u><br/>'yyyy-MM-ddTHH:mm:ssZ'</li><li><u>Gender:</u> M,F</li><li><u>MeritalStatus:</u><br/>Married=0,Single=1,Divorced=2<br/>Widowed=3,LivingTogether=4<br/>Separated=5,Other=6</li></ul>"));
        }
    }
}
