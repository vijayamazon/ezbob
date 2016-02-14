namespace EzBobService.Company {
    using System;
    using System.Linq;
    using EzBobApi.Commands.Company;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobCommon.Utils;
    using EzBobModels;
    using EzBobModels.Company;
    using EzBobModels.Customer;
    using EzBobModels.Enums;
    using EzBobPersistence;
    using EzBobPersistence.Company;
    using EzBobPersistence.Customer;
    using EzBobService.Encryption;
    using NServiceBus;

    /// <summary>
    /// Handles <see cref="UpdateCompanyCommand"/>.
    /// </summary>
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

            int customerId, companyId;
            if (!GetIds(command, info, out customerId, out companyId)) {
                SendReply(info, command);
                return;
            }

            using (UnitOfWork unitOfWork = new UnitOfWork()) {
                if (!this.ValidateCommand(command, info) &&
                    !base.ValidateModel(command.CompanyDetails, info) &&
                    !base.ValidateModel(command.CompanyDetails.Authorities, info) &&
                    !command.CompanyDetails.Authorities.All(o => base.ValidateModel(o, info))) {
                    SendReply(info, command);
                    return;
                }

                Company company = this.CreateCompany(command);
                if (StringUtils.IsNotEmpty(command.CompanyId)) {
                    company.Id = companyId;
                }

                companyId = (int)CompanyQueries.UpsertCompany(company);
                if (companyId < 1) {
                    info.AddError("Some DB error");
                    RegisterError(info, command);
                    return;
                }

                int countId = (int)CompanyQueries.UpsertCompanyEmployeeCount(new CompanyEmployeeCount {
                    Created = DateTime.UtcNow,
                    CustomerId = customerId,
                    CompanyId = companyId,
                    EmployeeCount = command.CompanyDetails.NumberOfEmployees
                });

                if (countId < 1) {
                    string error = string.Format("Could not save CompanyEmployeeCount. CustomerId: {0}, CompanyId {1}", command.CustomerId, command.CompanyId);
                    info.AddError(error);
                    RegisterError(info, command);
                    return;
                }

                Customer customer = this.CreateCustomer(command);
                customer.Id = customerId;
                customer.CompanyId = companyId;

                var customerIdFromQuery = CustomerQueries.UpsertCustomer(customer);
                if (!customerIdFromQuery.HasValue) {
                    info.AddError("Some DB error");
                    RegisterError(info, command);
                    return;
                }

                if (command.ExperianCompanyInfo != null) {
                    CustomerAddress address = this.CreateExperianAddress(command, customerId);
                    var res = CustomerQueries.UpsertCustomerAddress(address);
                    if (!res.HasValue || !res.Value) {
                        info.AddError("could not save customer address");
                        RegisterError(info, command);
                    }
                }

                unitOfWork.Commit();

                SendReply(info, command, response => {
                    response.CustomerId = command.CustomerId;
                    response.CompanyId = command.CompanyId;
                });
            }
        }

        /// <summary>
        /// Gets the ids.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="info">The information.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        private bool GetIds(UpdateCompanyCommand command, InfoAccumulator info, out int customerId, out int companyId) {
            DateTime date;
            try {
                customerId = CustomerIdEncryptor.DecryptCustomerId(command.CustomerId, command.RequestOrigin, out date);
            } catch (Exception ex) {
                Log.Error(ex.Message);
                info.AddError("Invalid customer id.");
                customerId = -1;
                companyId = -1;
                return false;
            }

            try {
                companyId = CompanyIdEncryptor.DecryptCompanyId(command.CustomerId, command.RequestOrigin, out date);
            } catch (Exception ex) {
                Log.Error(ex.Message);
                info.AddError("Invalid company id.");
                customerId = -1;
                companyId = -1;
                return false;
            }

            return true;
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
                IsOffline = (cmd.CompanyDetails.IndustryType == IndustryType.Retail || cmd.CompanyDetails.IndustryType == IndustryType.Online || cmd.CompanyDetails.IndustryType == IndustryType.Online),
                IsDirector = cmd.IsDirectorChecked //TODO: review
            };

            return customer;
        }
    }
}
