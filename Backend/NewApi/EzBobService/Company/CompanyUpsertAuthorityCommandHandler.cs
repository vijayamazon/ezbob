using System;

namespace EzBobService.Company {
    using EzBobApi.Commands.Company;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobModels;
    using EzBobModels.Company;
    using EzBobModels.Customer;
    using EzBobModels.Enums;
    using EzBobPersistence.Company;
    using EzBobService.Encryption;
    using NServiceBus;

    /// <summary>
    /// Handles <see cref="CompanyUpdateAuthorityCommand"/>
    /// </summary>
    public class CompanyUpsertAuthorityCommandHandler : HandlerBase<CompanyUpdateAuthorityCommandResponse>, IHandleMessages<CompanyUpdateAuthorityCommand> {

        [Injected]
        public ICompanyQueries CompanyQueries { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <exception cref="System.Exception">
        /// </exception>
        public void Handle(CompanyUpdateAuthorityCommand command) {
            InfoAccumulator info = new InfoAccumulator();

            int customerId;
            int companyId;
            int authorityId = -1;
            if (!GetIds(command, info, out customerId, out companyId, ref authorityId)) {
                SendReply(info, command);
                return;
            }

            if (command.Authority != null) {
                Director director = CreateDirector(command, companyId, customerId, authorityId);
                int directorId = (int)CompanyQueries.UpsertDirector(director);
                if (directorId < 1) {
                    string error = string.Format("could not upsert director. customerId:{0}, companyId:{1}", customerId, companyId, directorId);
                    info.AddError(error);
                    Log.Error(error);
                    RegisterError(info, command);
                    throw new Exception(error);
                }

                Optional<TypeOfBusiness> companyTypeOfBusiness = CompanyQueries.GetCompanyBusinessType(companyId);
                if (!companyTypeOfBusiness.HasValue) {
                    string errMessage = string.Format("Failed to query company business type for customerId: {0}, companyId {1}", customerId, companyId);
                    Log.Error(errMessage);
                    throw new Exception(errMessage);
                }

                CustomerAddress customerAddress = CreateCustomerAddress(companyId, customerId, directorId, companyTypeOfBusiness.Value, command.Authority);
                if (customerAddress != null) {
                    int id = (int)CompanyQueries.UpsertDirectorAddress(customerAddress);
                    if (id < 1) {
                        string error = string.Format("could not upsert customerAddress. customerId:{0}, companyId:{1}, directorId:{2}", customerId, companyId, directorId);
                        info.AddError(error);
                        Log.Error(error);
                        RegisterError(info, command);
                        throw new Exception(error);
                    }
                }

                SendReply(info, command, resp => {
                    resp.CompanyId = command.CompanyId;
                    resp.CustomerId = command.CustomerId;
                    resp.AuthorityId = authorityId > 0 ? command.AuhorityId : AuthorityIdEncryptor.EncryptAuthorityId(directorId, command.CommandOriginator, command.Authority.IsDirector);
                });
            } else {
                Log.ErrorFormat("Got empty authority for customerId: {0}, companyId {0}", customerId, companyId);
                info.AddError("Got empty authority");
                SendReply(info, command);
            }
        }

        /// <summary>
        /// Creates the directors.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="authorityId">The authority identifier.</param>
        /// <returns></returns>
        private Director CreateDirector(CompanyUpdateAuthorityCommand command, int companyId, int customerId, int authorityId) {
            var authorityInfo = command.Authority;

            return new Director {
                id = authorityId > 0 ? authorityId : (int?)null,
                Gender = authorityInfo.PersonalDetails.Gender,
                CompanyId = companyId,
                CustomerId = customerId,
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
        }

        /// <summary>
        /// Creates the customer address.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="directorId">The director identifier.</param>
        /// <param name="typeOfBusiness">The type of business.</param>
        /// <param name="authorityInfo">The authority information.</param>
        /// <returns></returns>
        private CustomerAddress CreateCustomerAddress(int companyId, int customerId, int directorId, TypeOfBusiness typeOfBusiness, AuthorityInfo authorityInfo) {
            CustomerAddress address = null;
            if (authorityInfo.AddressInfo != null) {
                address = new CustomerAddress {
                    CompanyId = companyId,
                    CustomerId = customerId,
                    DirectorId = directorId,
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
                    addressType = DeduceCustomerAddressType(typeOfBusiness, authorityInfo.IsDirector),
                    Smallorg = authorityInfo.AddressInfo.Smallorg
                };
            }
            return address;
        }

        /// <summary>
        /// Gets the ids.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="info">The information.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="authorityId">The authority identifier.</param>
        /// <returns></returns>
        private bool GetIds(CompanyUpdateAuthorityCommand command, InfoAccumulator info, out int customerId, out int companyId, ref int authorityId) {
            try {
                customerId = CustomerIdEncryptor.DecryptCustomerId(command.CustomerId, command.CommandOriginator);
                companyId = CompanyIdEncryptor.DecryptCompanyId(command.CompanyId, command.CommandOriginator);
                if (command.AuhorityId.IsNotEmpty()) {
                    authorityId = AuthorityIdEncryptor.DecryptAuthorityId(command.AuhorityId, command.CommandOriginator, command.Authority.IsDirector);
                }
            } catch (Exception ex) {
                Log.Error(ex.Message);
                info.AddError(ex.Message);
                customerId = -1;
                companyId = -1;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Deduces the type of the customer address.
        /// </summary>
        /// <param name="typeOfBusiness">The type of business.</param>
        /// <param name="isDirector">if set to <c>true</c> [is director].</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">typeOfBusiness;null</exception>
        private CustomerAddressType DeduceCustomerAddressType(TypeOfBusiness typeOfBusiness, bool isDirector) {
            switch (typeOfBusiness) {
            case TypeOfBusiness.Entrepreneur:
                    return CustomerAddressType.PersonalAddress;
            case TypeOfBusiness.LLP:
                return CustomerAddressType.LimitedDirectorHomeAddress;
            case TypeOfBusiness.PShip3P:
                return CustomerAddressType.NonLimitedCompanyAddress;
            case TypeOfBusiness.PShip:
                return CustomerAddressType.NonLimitedCompanyAddress;
            case TypeOfBusiness.Limited:
                return CustomerAddressType.LimitedDirectorHomeAddress;
            default:
                throw new ArgumentOutOfRangeException("typeOfBusiness", typeOfBusiness, null);
            }
        }
    }
}
