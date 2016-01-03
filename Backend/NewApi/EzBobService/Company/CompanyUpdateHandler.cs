namespace EzBobService.Company {
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using EzBobApi.Commands.Company;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobCommon.Utils;
    using EzBobCommon.Utils.Encryption;
    using EzBobModels;
    using EzBobModels.Enums;
    using EzBobPersistence;
    using EzBobPersistence.Company;
    using EzBobPersistence.Customer;
    using NServiceBus;

    public class CompanyUpdateHandler : HandlerBase<UpdateCompanyCommandResponse>, IHandleMessages<UpdateCompanyCommand> {

        [Injected]
        public ICustomerQueries CustomerQueries { get; set; }

        [Injected]
        public ICompanyQueries CompanyQueries { get; set; }

        /// <summary>
        /// Handles a message.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <remarks>
        /// This method will be called when a message arrives on the bus and should contain
        /// the custom logic to execute when the message is received.
        /// </remarks>
        public void Handle(UpdateCompanyCommand command) {
            InfoAccumulator info = new InfoAccumulator();
            using (UnitOfWork unitOfWork = new UnitOfWork()) {
                if (!this.ValidateCommand(command, info) &&
                    !base.ValidateModel(command.CompanyDetails, info) &&
                    !base.ValidateModel(command.CompanyDetails.Authorities, info) &&
                    !command.CompanyDetails.Authorities.All(o => base.ValidateModel(o, info))) {
                    SendReply(info, command);
                    return;
                }

                int customerId = int.Parse(EncryptionUtils.SafeDecrypt(command.CustomerId));

                Company company = this.CreateCompany(command);
                if (StringUtils.IsNotEmpty(command.CompanyId)) {
                    company.Id = int.Parse(EncryptionUtils.SafeDecrypt(command.CompanyId));
                }
                int? companyId = CompanyQueries.SaveCompany(company);
                if (!companyId.HasValue) {
                    info.AddError("Some DB error");
                    RegisterError(info, command);
                    return;
                }

                if (CollectionUtils.IsNotEmpty(command.CompanyDetails.Authorities)) {
                    IEnumerable<Tuple<Director, CustomerAddress>> directors = this.CreateDirectors(command, companyId.Value)
                        .ToArray();
                    IEnumerable<int> directorsIds = CompanyQueries.SaveDirectors(directors.Select(o => o.Item1), companyId.Value);
                    if (directorsIds.Count() != directors.Count()) {
                        info.AddError("Could not save directors");
                        RegisterError(info, command);
                        return;
                    }


                    IEnumerable<CustomerAddress> directorAddresses = directors.Zip(directorsIds, (tuple, id) => {
                        if (tuple.Item2 != null) {
                            tuple.Item2.DirectorId = id;
                            tuple.Item2.CustomerId = customerId;
                        }

                        return tuple.Item2;
                    });

                    if (!CompanyQueries.SaveDirectorAddresses(directorAddresses)) {
                        info.AddError("could not save director address");
                        RegisterError(info, command);
                        return;
                    }
                }

                Customer customer = this.CreateCustomer(command);
                customer.Id = customerId;
                customer.CompanyId = companyId.Value;

                var customerIdFromQuery = CustomerQueries.UpsertCustomer(customer);
                if (!customerIdFromQuery.HasValue)
                {
                    info.AddError("Some DB error");
                    RegisterError(info, command);
                    return;
                }

                if (command.ExperianCompanyInfo != null) {
                    CustomerAddress address = this.CreateExperianAddress(command, customerId);
                    var res = CustomerQueries.SaveCustomerAddress(address);
                    if (!res.HasValue || !res.Value) {
                        info.AddError("could not save customer address");
                        RegisterError(info, command);
                    }
                }

                unitOfWork.Commit();

                SendReply(info, command, response =>
                {
                    response.CustomerId = command.CustomerId;
                    if (StringUtils.IsNotEmpty(command.CompanyId))
                    {
                        response.CompanyId = command.CompanyId;
                    }
                    else
                    {
                        response.CompanyId = EncryptionUtils.SafeEncrypt(companyId.Value.ToString());
                    }
                });
            }
        }

        /// <summary>
        /// Creates the experian address.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        private CustomerAddress CreateExperianAddress(UpdateCompanyCommand command, int customerId) {
            return new CustomerAddress {
                addressType = CustomerAddressType.ExperianCompanyAddress,
                CustomerId = customerId,
                Organisation = command.ExperianCompanyInfo.BusName,
                Line1 = command.ExperianCompanyInfo.AddrLine1,
                Line2 = command.ExperianCompanyInfo.AddrLine2,
                Line3 = command.ExperianCompanyInfo.AddrLine3,
                Town = command.ExperianCompanyInfo.AddrLine4,
                Postcode = command.ExperianCompanyInfo.PostCode
            };
        }

        /// <summary>
        /// Creates the directors.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        private IEnumerable<Tuple<Director, CustomerAddress>> CreateDirectors(UpdateCompanyCommand command, int companyId) {
            foreach (var authorityInfo in command.CompanyDetails.Authorities) {
                var director = new Director {
                    Gender = authorityInfo.PersonalDetails.Gender,
                    CompanyId = companyId,
                    CustomerId = null, //TODO: review
                    DateOfBirth = authorityInfo.PersonalDetails.DateOfBirth,
                    Email = authorityInfo.ContactDetails.EmailAddress,
                    Phone = authorityInfo.ContactDetails.PhoneNumber,
                    IsDirector = authorityInfo.IsDirector,
                    IsShareholder = authorityInfo.IsShareHolder,
                    Surname = authorityInfo.PersonalDetails.SurName,
                    Middle = authorityInfo.PersonalDetails.MiddleName,
                    Name = authorityInfo.PersonalDetails.FirstName,
                    ExperianConsumerScore = null //TODO: review
                };

                CustomerAddress address = null;
                if (authorityInfo.AddressInfo != null) {
                    address = new CustomerAddress {
                        CompanyId = companyId,
                        Country = authorityInfo.AddressInfo.Country,
                        County = authorityInfo.AddressInfo.County,
                        Deliverypointsuffix = authorityInfo.AddressInfo.Deliverypointsuffix,
                        Line1 = authorityInfo.AddressInfo.Line1,
                        Line2 = authorityInfo.AddressInfo.Line2,
                        Line3 = authorityInfo.AddressInfo.Line3,
                        Organisation = authorityInfo.AddressInfo.Organisation,
                        Pobox = authorityInfo.AddressInfo.Pobox,
                        Postcode = authorityInfo.AddressInfo.Postcode,
                        Mailsortcode = authorityInfo.AddressInfo.Mailsortcode,
                        Town = authorityInfo.AddressInfo.Town,
                        Udprn = authorityInfo.AddressInfo.Udprn,
                        Nohouseholds = authorityInfo.AddressInfo.Nohouseholds,
                        addressType = authorityInfo.AddressInfo.addressType,
                        Smallorg = authorityInfo.AddressInfo.Smallorg
                    };
                }

                yield return new Tuple<Director, CustomerAddress>(director, address);
            }
        }

        /// <summary>
        /// Creates the company.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        private Company CreateCompany(UpdateCompanyCommand command) {
            var company = new Company {
                BusinessPhone = command.CompanyDetails.MainPhoneNumber,
                CapitalExpenditure = command.CompanyDetails.TotalMonthlySalaryExpenditure,
                CompanyNumber = command.CompanyDetails.RegistrationNumber,
                CompanyName = command.CompanyDetails.BusinessName,
                PropertyOwnedByCompany = command.OwnsProperty,
                RentMonthLeft = null, //TODO: review
                TypeOfBusiness = command.CompanyDetails.TypeOfBusiness,
                TimeAtAddress = null, //TODO: review
                YearsInCompany = null, //TODO: review
                VatRegistered = command.CompanyDetails.IsVatRegistered,
                VatReporting = null //TODO: review
            };

            if (command.ExperianCompanyInfo != null) {
                company.ExperianCompanyName = command.ExperianCompanyInfo.BusName;
                company.ExperianRefNum = command.ExperianCompanyInfo.BusRefNum == "skip" ? "NotFound" : command.ExperianCompanyInfo.BusRefNum;
            }

            return company;
        }

        /// <summary>
        /// Validates the command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="info">The information.</param>
        /// <returns></returns>
        private bool ValidateCommand(UpdateCompanyCommand command, InfoAccumulator info) {
            if (StringUtils.IsEmpty(command.CustomerId)) {
                info.AddError("invalid customer id");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates the customer.
        /// </summary>
        /// <param name="cmd">The command.</param>
        /// <returns></returns>
        private Customer CreateCustomer(UpdateCompanyCommand cmd) {
            Customer customer = new Customer {
                PromoCode = cmd.PromoCode,
                WizardStep = (int)WizardStepType.CompanyDetails,
                PersonalInfo = {
                    TypeOfBusiness = cmd.CompanyDetails.TypeOfBusiness,
                    IndustryType = cmd.CompanyDetails.IndustryType,
                    OverallTurnOver = cmd.CompanyDetails.TotalAnnualRevenue
                },
                IsOffline = (cmd.CompanyDetails.IndustryType == IndustryType.Retail ||
                    cmd.CompanyDetails.IndustryType == IndustryType.Online ||
                    cmd.CompanyDetails.IndustryType == IndustryType.Online),
                IsDirector = cmd.IsDirectorChecked //TODO: review
            };

            return customer;
        }
    }
}
