namespace EzBobService.Company {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EzBobApi.Commands.Company;
    using EzBobApi.Commands.Customer.Sections;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobModels.Company;
    using EzBobModels.Customer;
    using EzBobPersistence.Company;
    using EzBobPersistence.Customer;
    using EzBobService.Encryption;
    using NServiceBus;

    /// <summary>
    /// Handles <see cref="CompanyGetDetailsCommand"/>.
    /// </summary>
    public class CompanyGetDetailsCommandHandler : HandlerBase<CompanyGetDetailsCommandResponse>, IHandleMessages<CompanyGetDetailsCommand> {

        [Injected]
        public ICompanyQueries CompanyQueries { get; set; }

        [Injected]
        public ICustomerQueries CustomerQueries { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public void Handle(CompanyGetDetailsCommand command) {

            InfoAccumulator info = new InfoAccumulator();
            int customerId, companyId;

            try {
                customerId = CustomerIdEncryptor.DecryptCustomerId(command.CustomerId, command.CommandOriginator);
                companyId = CompanyIdEncryptor.DecryptCompanyId(command.CompanyId, command.CommandOriginator);
            } catch (Exception ex) {
                Log.Error(ex.Message);
                SendReply(info, command, resp => {
                    resp.CustomerId = command.CustomerId;
                    resp.CompanyId = command.CompanyId;
                });

                return;
            }

            var company = CompanyQueries.GetCompanyById(companyId)
                .Value;
            var directors = CompanyQueries.GetDirectors(customerId, companyId)
                .Value;
            var directorsAddresses = CompanyQueries.GetDirectorsAddresses(customerId, companyId)
                .Value;
            var companyEmployeeCount = CompanyQueries.GetCompanyEmployeeCount(customerId, companyId)
                .Value;
            var customer = CustomerQueries.GetCustomerPartiallyById(customerId, o => o.PersonalInfo.IndustryType, o => o.PersonalInfo.OverallTurnOver);

            SendReply(info, command, resp => {
                resp.CompanyDetails = ConvertToCompanyDetails(company, customer, companyEmployeeCount);
                resp.Authorities = GetAuthorities(directors, directorsAddresses);
            });
        }

        /// <summary>
        /// Converts to company details.
        /// </summary>
        /// <param name="company">The company.</param>
        /// <param name="customer">The customer.</param>
        /// <param name="companyEmployeeCount">The company employee count.</param>
        /// <returns></returns>
        public CompanyDetailsInfo ConvertToCompanyDetails(Company company, Customer customer, CompanyEmployeeCount companyEmployeeCount) {
            var companyDetails = new CompanyDetailsInfo();
            if (company != null) {
                companyDetails.TypeOfBusiness = company.TypeOfBusiness;
                companyDetails.RegistrationNumber = company.CompanyNumber;
                companyDetails.BusinessName = company.CompanyName;
                companyDetails.IsVatRegistered = company.VatRegistered;
                companyDetails.MainPhoneNumber = company.BusinessPhone;
            }

            if (customer != null) {
                companyDetails.IndustryType = customer.PersonalInfo.IndustryType;
            }

            if (companyEmployeeCount != null) {
                companyDetails.NumberOfEmployees = companyEmployeeCount.EmployeeCount;
            }

            return companyDetails;
        }

        /// <summary>
        /// Gets the authorities.
        /// </summary>
        /// <param name="directors">The directors.</param>
        /// <param name="addresses">The addresses.</param>
        /// <returns></returns>
        public IEnumerable<AuthorityInfo> GetAuthorities(IEnumerable<Director> directors, IEnumerable<CustomerAddress> addresses) {
            foreach (var director in directors) {
                var address = addresses.FirstOrDefault(o => o.CustomerId == director.CustomerId && o.CompanyId == director.CompanyId);
                yield return ConvertToAuthority(address, director);
            }
        }

        /// <summary>
        /// Converts to authority.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="director">The director.</param>
        /// <returns></returns>
        public AuthorityInfo ConvertToAuthority(CustomerAddress address, Director director) {
            AuthorityInfo authority = new AuthorityInfo();

            if (director != null) {
                authority.ContactDetails = new ContactDetailsInfo {
                    EmailAddress = director.Email,
                    PhoneNumber = director.Phone
                };
                authority.PersonalDetails = new PersonalDetailsInfo {
                    DateOfBirth = director.DateOfBirth,
                    Gender = director.Gender,
                    FirstName = director.Name,
                    MiddleName = director.Middle,
                    SurName = director.Surname
                };
                authority.IsDirector = director.IsDirector ?? false;
                authority.IsShareHolder = director.IsShareholder ?? false;
            }

            if (address != null) {
                authority.AddressInfo = new AddressInfo {
                    Country = address.Country,
                    County = address.County,
                    Deliverypointsuffix = address.Deliverypointsuffix,
                    Line1 = address.Line1,
                    Line2 = address.Line2,
                    Line3 = address.Line3,
                    Mailsortcode = address.Mailsortcode,
                    Nohouseholds = address.Nohouseholds,
                    Organisation = address.Organisation,
                    Pobox = address.Pobox,
                    Postcode = address.Postcode,
                    Rawpostcode = address.Rawpostcode,
                    Smallorg = address.Smallorg,
                    Town = address.Town,
                    Udprn = address.Udprn
                };
            }

            return authority;
        }
    }
}
