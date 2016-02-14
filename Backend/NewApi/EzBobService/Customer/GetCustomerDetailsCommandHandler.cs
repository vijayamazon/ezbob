namespace EzBobService.Customer {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EzBobApi.Commands.Customer;
    using EzBobApi.Commands.Customer.Sections;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobModels.Customer;
    using EzBobModels.Enums;
    using EzBobPersistence.Customer;
    using EzBobPersistence.Loan;
    using EzBobService.Encryption;
    using NServiceBus;

    /// <summary>
    /// Handles <see cref="CustomerGetDetailsCommand"/>.
    /// </summary>
    public class GetCustomerDetailsCommandHandler : HandlerBase<CustomerGetDetailsCommandResponse>, IHandleMessages<CustomerGetDetailsCommand> {
        [Injected]
        public ICustomerQueries CustomerQueries { get; set; }

        [Injected]
        public ILoanQueries LoanQueries { get; set; }

        public void Handle(CustomerGetDetailsCommand command) {
            InfoAccumulator info = new InfoAccumulator();

            int customerId;
            try {
                customerId = CustomerIdEncryptor.DecryptCustomerId(command.CustomerId, command.CommandOriginator);
            } catch (Exception ex) {
                Log.Error(ex.Message);
                info.AddError("Invalid customer id.");
                SendReply(info, command);
                return;
            }

            Customer customer = CustomerQueries.GetCustomerById(customerId);
            IEnumerable<CustomerAddress> addresses = CustomerQueries.GetCustomerAddresses(customerId);
            IEnumerable<CustomerPhone> phones = CustomerQueries.GetCustomerPhones(customerId);
            var requesetedLoan = LoanQueries.GetCustomerRequestedLoan(customerId);

            SendReply(info, command, resp => {
                resp.PersonalDetails = GetPersonalDetails(customer);
                resp.ContactDetails = GetContactDetails(phones, customer);
                resp.CurrentLivingAddress = GetCurrentLivingAddress(addresses, customer);
                resp.PreviousLivingAddress = GetPreviousLivingAddress(addresses, customer);
                resp.AdditionalOwnedProperties = GetAdditionalOwnedProperties(addresses, customer)
                    .ToArray();
                resp.RequestedAmount = (decimal)requesetedLoan.Map(o => o.Amount.HasValue ? o.Amount.Value : 0);
            });
        }

        /// <summary>
        /// Gets the personal details.
        /// </summary>
        /// <param name="customer">The customer.</param>
        /// <returns></returns>
        private PersonalDetailsInfo GetPersonalDetails(Customer customer) {
            return new PersonalDetailsInfo {
                DateOfBirth = customer.PersonalInfo.DateOfBirth,
                FirstName = customer.PersonalInfo.FirstName,
                Gender = customer.PersonalInfo.Gender,
                MaritalStatus = customer.PersonalInfo.MaritalStatus.AsString(),
                MiddleName = customer.PersonalInfo.MiddleInitial,
                SurName = customer.PersonalInfo.Surname
            };
        }

        /// <summary>
        /// Gets the contact details.
        /// </summary>
        /// <param name="phones">The phones.</param>
        /// <param name="customer">The customer.</param>
        /// <returns></returns>
        private ContactDetailsInfo GetContactDetails(IEnumerable<CustomerPhone> phones, Customer customer) {

            string mobileNumber = null;
            string dayNumber = null;

            phones.FirstOrDefault(o => o.PhoneType.Equals("Mobile", StringComparison.InvariantCultureIgnoreCase))
                .AsOptional()
                .IfNotEmpty(o => mobileNumber = o.Phone);

            phones.FirstOrDefault(o => o.PhoneType.Equals("Daytime", StringComparison.InvariantCultureIgnoreCase))
                .AsOptional()
                .IfNotEmpty(o => dayNumber = o.Phone);

            return new ContactDetailsInfo {
                EmailAddress = customer.Name,
                MobilePhone = mobileNumber,
                PhoneNumber = dayNumber
            };
        }

        /// <summary>
        /// Gets the other owned properties.
        /// </summary>
        /// <param name="addresses">The addresses.</param>
        /// <param name="customer">The customer.</param>
        /// <returns></returns>
        private IEnumerable<LivingAddressInfo> GetAdditionalOwnedProperties(IEnumerable<CustomerAddress> addresses, Customer customer)
        {
            if (addresses.IsEmpty())
            {
                yield break;
            }

            foreach (var address in addresses.Where(o => o.addressType.HasValue && o.addressType.Value == CustomerAddressType.PersonalAddress)) {
                yield return CreateLivingAddressInfo(address);
            }
        }


        /// <summary>
        /// Gets the previous living address.
        /// </summary>
        /// <param name="addresses">The addresses.</param>
        /// <param name="customer">The customer.</param>
        /// <returns></returns>
        private LivingAddressInfo GetPreviousLivingAddress(IEnumerable<CustomerAddress> addresses, Customer customer) {
            if (addresses.IsEmpty()) {
                return null;
            }


            var address = addresses.FirstOrDefault(o => o.addressType.HasValue && o.addressType.Value == CustomerAddressType.PersonalAddress);
            if (address == null) {
                return null;
            }

            return CreateLivingAddressInfo(address);
        }

        /// <summary>
        /// Gets the current living address.
        /// </summary>
        /// <param name="addresses">The addresses.</param>
        /// <param name="customer">The customer.</param>
        /// <returns></returns>
         private LivingAddressInfo GetCurrentLivingAddress(IEnumerable<CustomerAddress> addresses, Customer customer) {
            if (addresses.IsEmpty()) {
                return null;
            }

            var address = addresses.FirstOrDefault(o => o.addressType.HasValue && o.addressType.Value == CustomerAddressType.PersonalAddress);
            if (address == null) {
                return null;
            }

            var addressInfo = CreateLivingAddressInfo(address);
            //address.Info = HousingType = //TODO: We should change the mapping of customer properties.  Missing at least one possibility 'renting' and have own property

            return addressInfo;
        }


         /// <summary>
         /// Creates the living address information.
         /// </summary>
         /// <param name="address">The address.</param>
         /// <returns></returns>
         private LivingAddressInfo CreateLivingAddressInfo(CustomerAddress address)
         {
             return new LivingAddressInfo
             {
                 Country = address.Country,
                 Deliverypointsuffix = address.Deliverypointsuffix,
                 County = address.County,
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

        //TODO: check it out
        private HousingType DetermineHousingType(CustomerAddressType addressType, CustomerPropertyStatus propertyStatus) {
            if (addressType == CustomerAddressType.PersonalAddress) {
                switch (propertyStatus) {
                case CustomerPropertyStatus.NotYetFilled://should not happen because we require to supply HousingType
                    Log.Error("Got not yet filled");
                    break;
                case CustomerPropertyStatus.IOwnOnlyThisProperty:
                        return HousingType.OwnProperty;
                case CustomerPropertyStatus.IOwnThisPropertyAndOtherProperties:
                        return HousingType.OwnProperty;
                case CustomerPropertyStatus.ILiveInTheAboveAndOwnOtherProperties:
                        // could not determine
                    break;
                case CustomerPropertyStatus.IHomeOwner:
                        return HousingType.OwnProperty;
                case CustomerPropertyStatus.Renting:
                        return HousingType.Renting;
                case CustomerPropertyStatus.SocialHouse:
                        return HousingType.Social;
                case CustomerPropertyStatus.LivingWithParents:
                        return HousingType.LivingWithParents;
                default:
                    throw new ArgumentOutOfRangeException("propertyStatus", propertyStatus, null);
                }
            }

            if (addressType == CustomerAddressType.PrevPersonAddresses) {
                switch (propertyStatus)
                {
                    case CustomerPropertyStatus.NotYetFilled://should not happen because we require to supply HousingType
                        Log.Error("Got not yet filled");
                        break;
                    case CustomerPropertyStatus.IOwnOnlyThisProperty:
                        //could not determine
                        return HousingType.OwnProperty;
                    case CustomerPropertyStatus.IOwnThisPropertyAndOtherProperties:
                        //could not determine
                        return HousingType.OwnProperty;
                    case CustomerPropertyStatus.ILiveInTheAboveAndOwnOtherProperties:
                        // could not determine
                        break;
                    case CustomerPropertyStatus.IHomeOwner:
                        // could not determine
                        return HousingType.OwnProperty;
                    case CustomerPropertyStatus.Renting:
                        // could not determine
                        return HousingType.Renting;
                    case CustomerPropertyStatus.SocialHouse:
                        //could not determine
                        return HousingType.Social;
                    case CustomerPropertyStatus.LivingWithParents:
                        //could not determine
                        return HousingType.LivingWithParents;
                    default:
                        throw new ArgumentOutOfRangeException("propertyStatus", propertyStatus, null);
                }
            }

            return HousingType.OwnProperty;
        }
    }
}