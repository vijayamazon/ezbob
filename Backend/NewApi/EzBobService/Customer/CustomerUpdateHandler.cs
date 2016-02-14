namespace EzBobService.Customer {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Remoting.Messaging;
    using EzBobApi.Commands.Customer;
    using EzBobApi.Commands.Customer.Sections;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobCommon.Utils;
    using EzBobCommon.Utils.Encryption;
    using EzBobModels;
    using EzBobModels.Customer;
    using EzBobModels.Enums;
    using EzBobPersistence.Customer;
    using EzBobService.Encryption;
    using NServiceBus;

    /// <summary>
    /// Handles <see cref="CustomerUpdateCommand"/>.
    /// </summary>
    public class CustomerUpdateHandler : HandlerBase<CustomerUpdateCommandResponse>, IHandleMessages<CustomerUpdateCommand> {
        private static readonly string gotEmptyCustomerID = "got empty customer id";
        private static readonly string gotEmptyAccount = "got empty account";
        private static readonly string gotEmptyEmailAddress = "got empty email address";
        private static readonly string invalidMaritalStatus = "invalid marital status";
        private static readonly string invalidGenderExpectedMorF = "invalid gender: expected M or F";
        private static readonly string gotEmptyEmailaddress = "Got empty emailAddress";
        private static readonly string gotInvalidPassword = "got invalid password";
        private static readonly string expectedToGetOneAddressWherePersonLivesNowAndGot = "expected to get one address where a person lives now and got ";

        /// <summary>
        /// Gets or sets the customer queries.
        /// </summary>
        /// <value>
        /// The customer queries.
        /// </value>
        [Injected] //used for vip check (probably we should get rid of vip)
        public ICustomerQueries CustomerQueries { get; set; }

        /// <summary>
        /// Gets or sets the customer processor.
        /// </summary>
        /// <value>
        /// The customer processor.
        /// </value>
        [Injected]
        public CustomerProcessor CustomerProcessor { get; set; }

        /// <summary>
        /// Handles a message.
        /// </summary>
        /// <param name="command">The command to handle.</param>
        /// <remarks>
        /// This method will be called when a message arrives on the bus and should contain
        ///             the custom logic to execute when the command is received.
        /// </remarks>
        public void Handle(CustomerUpdateCommand command) {

            InfoAccumulator info = new InfoAccumulator();

            if (!ValidateCommand(command, info)) {
                SendReply(info, command, response => response.CustomerId = command.CustomerId);
                return;
            }

            int customerId;

            try {
                customerId = CustomerIdEncryptor.DecryptCustomerId(command.CustomerId, command.CommandOriginator);
            } catch (Exception ex) {
                Log.Error(ex.Message);
                info.AddError("Invalid customer id.");
                SendReply(info, command, resp => resp.CustomerId = command.CustomerId);
                return;
            }

            //requests only PropertyStatusId from DB
            Customer customer = CustomerQueries.GetCustomerPartiallyById(customerId, o => o.PropertyStatusId);

            FillCustomerProperties(customer, command, info);
            if (info.HasErrors) {
                SendReply(info, command, resp => resp.CustomerId = command.CustomerId);
                return;
            }

            IEnumerable<CustomerAddress> addresses = FillCustomerPropertyStatusAndTimeAtAddressAndGetAddresses(command, customer, customerId);
            IEnumerable<CustomerPhone> phones = ExtractPhoneNumbers(command, customerId);

            string referenceSource = null;
            string visitTimes = null;
            if (command.Cookies != null) {
                referenceSource = command.Cookies.ReferenceSource;
                visitTimes = command.Cookies.VisitTimes;
            }

            var errors = CustomerProcessor.UpdateCustomer(customer, command.RequestedAmount, referenceSource, visitTimes, command.CampaignSourceRef, addresses, phones);
            if (errors.HasErrors) {
                if (errors.IsRetry) {
                    RegisterError(info, command);
                }

                return;
            }

            SendReply(errors, command, response => response.CustomerId = command.CustomerId);
        }

        /// <summary>
        /// Validates the command.
        /// </summary>
        /// <param name="cmd">The command.</param>
        /// <param name="info">The information.</param>
        /// <returns></returns>
        private bool ValidateCommand(CustomerUpdateCommand cmd, InfoAccumulator info) {
            if (StringUtils.IsEmpty(cmd.CustomerId)) {
                LogError(gotEmptyCustomerID, info);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Fills the customer's properties.
        /// </summary>
        /// <param name="customer">The customer.</param>
        /// <param name="cmd">The command.</param>
        /// <param name="info">The information.</param>
        private void FillCustomerProperties(Customer customer, CustomerUpdateCommand cmd, InfoAccumulator info) {

            FillPersonalDetails(customer, cmd.PersonalDetails, info);
            FillContactDetails(customer, cmd.ContactDetails);
//            UpdateAddressInfo(customer, command.Address);

            if (info.HasErrors) {
                return;
            }

            if (cmd.AlibabaInfo != null) {
                customer.AlibabaId = cmd.AlibabaInfo.AlibabaId;
                customer.IsAlibaba = cmd.AlibabaInfo.IsAlibaba;
            }

            if (cmd.Cookies != null) {
                customer.GoogleCookie = cmd.Cookies.GoogleCookie;
                customer.ReferenceSource = cmd.Cookies.ReferenceSource;
            }
        }

        /// <summary>
        /// Fills the personal details.
        /// </summary>
        /// <param name="customer">The customer.</param>
        /// <param name="personalDetails">The personal details.</param>
        /// <param name="info">The information.</param>
        private void FillPersonalDetails(Customer customer, PersonalDetailsInfo personalDetails, InfoAccumulator info) {
            if (personalDetails == null) {
                return;
            }

            var personalInfo = customer.PersonalInfo;

            personalInfo.FirstName = personalDetails.FirstName;
            personalInfo.Surname = personalDetails.SurName;
            personalInfo.DateOfBirth = personalDetails.DateOfBirth;

            if (StringUtils.IsNotEmpty(personalDetails.MaritalStatus)) {
                MaritalStatus maritalStatus;
                if (!Enum.TryParse(personalDetails.MaritalStatus, true, out maritalStatus)) {
                    LogError(invalidMaritalStatus, info);
                } else {
                    personalInfo.MaritalStatus = maritalStatus;
                }
            }

            personalInfo.Gender = personalDetails.Gender;
        }

        /// <summary>
        /// Fills the contact details.
        /// </summary>
        /// <param name="customer">The customer.</param>
        /// <param name="contactDetails">The contact details.</param>
        private void FillContactDetails(Customer customer, ContactDetailsInfo contactDetails) {
            if (contactDetails == null) {
                return;
            }

            var personalInfo = customer.PersonalInfo;

            personalInfo.MobilePhone = contactDetails.MobilePhone;
            personalInfo.MobilePhoneVerified = contactDetails.IsMobilePhoneVerified;
        }

        /// <summary>
        /// Extracts the phone numbers.
        /// </summary>
        /// <param name="cmd">The command.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        private IEnumerable<CustomerPhone> ExtractPhoneNumbers(CustomerUpdateCommand cmd, int customerId) {
            if (cmd.ContactDetails != null) {
                if (!string.IsNullOrEmpty(cmd.ContactDetails.MobilePhone)) {
                    yield return new CustomerPhone {
                        CustomerId = customerId,
                        PhoneType = "Mobile",
                        Phone = cmd.ContactDetails.MobilePhone,
                        IsCurrent = true
                    }; //TODO: review (strange type, not all fields are filled, IsCurrent = true)
                }

                if (!string.IsNullOrEmpty(cmd.ContactDetails.PhoneNumber)) {
                    yield return new CustomerPhone {
                        CustomerId = customerId,
                        PhoneType = "Daytime",
                        Phone = cmd.ContactDetails.PhoneNumber,
                        IsCurrent = true
                    }; //TODO: review (strange type, not all fields are filled, IsCurrent = true)
                }
            }
        }

        /// <summary>
        /// Logs the error.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="info">The information.</param>
        private void LogError(string errorMessage, InfoAccumulator info) {
            Log.Error(errorMessage);
            info.AddError(errorMessage);
        }

        /// <summary>
        /// Fills the customer property status and time at address and get addresses.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="customer">The customer.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        private IEnumerable<CustomerAddress> FillCustomerPropertyStatusAndTimeAtAddressAndGetAddresses(CustomerUpdateCommand command, Customer customer, int customerId) {

            bool? isOwnsCurrentAdress = null;
            bool? isOwnsPreviousAddress = null;
            bool? isOwnsAdditionalProperty = null;
            HousingType? currentAddressHousingType = null;

            var curLivingAddress = command.CurrentLivingAddress
                .AsOptional()
                .IfNotEmpty(o => {
                    isOwnsCurrentAdress = o.IsOwns; //Fills time at address
                    customer.PersonalInfo.TimeAtAddress = o.MonthsAtAddress.IsNotEmpty() ? int.Parse(o.MonthsAtAddress) : (int?)null;
                    currentAddressHousingType = o.HousingType;
                })
                .Map(o => ConvertToCustomerAddress(o, customerId))
                .IfNotEmpty(addr => addr.addressType = CustomerAddressType.PersonalAddress);

            var previousLivingAddress = command.PreviousLivingAddress
                .AsOptional()
                .IfNotEmpty(o => isOwnsPreviousAddress = o.IsOwns)
                .Map(o => ConvertToCustomerAddress(o, customerId))
                .IfNotEmpty(addr => addr.addressType = CustomerAddressType.PrevPersonAddresses);

            var additionalOwnedPropertyAddresses = command.AdditionalOwnedProperties
                .AsOptionals()
                .IfAnyNotEmpty(() => isOwnsAdditionalProperty = true)
                .MapMany(o => {
                    var addr = ConvertToCustomerAddress(o, customerId);
                    addr.addressType = CustomerAddressType.OtherPropertyAddress;
                    return addr;
                });

            CustomerPropertyStatus submitted = DetermineSubmittedPropertyStatus(currentAddressHousingType, isOwnsCurrentAdress, isOwnsPreviousAddress, isOwnsAdditionalProperty);
            CustomerPropertyStatus final = DetermineNewCustomerPropertyStatus(submitted, (CustomerPropertyStatus)customer.PersonalInfo.PropertyStatus);

            customer.PersonalInfo.PropertyStatus = (int)final;

            var addresses = curLivingAddress.Concat(previousLivingAddress)
                .Concat(additionalOwnedPropertyAddresses.SelectMany(o => o));
            return addresses;
        }

        /// <summary>
        /// Determines the new customer's property status.
        /// </summary>
        /// <param name="submitted">The submitted.</param>
        /// <param name="current">The current.</param>
        /// <returns></returns>
        private CustomerPropertyStatus DetermineNewCustomerPropertyStatus(CustomerPropertyStatus submitted, CustomerPropertyStatus current) {
            if (submitted == CustomerPropertyStatus.IOwnThisPropertyAndOtherProperties || current == CustomerPropertyStatus.IOwnThisPropertyAndOtherProperties) {
                return CustomerPropertyStatus.IOwnThisPropertyAndOtherProperties;
            }

            if (submitted == CustomerPropertyStatus.IHomeOwner || current == CustomerPropertyStatus.IHomeOwner) {
                if (submitted == CustomerPropertyStatus.ILiveInTheAboveAndOwnOtherProperties ||
                    current == CustomerPropertyStatus.ILiveInTheAboveAndOwnOtherProperties) {
                    return CustomerPropertyStatus.IOwnThisPropertyAndOtherProperties;
                }

                return CustomerPropertyStatus.IHomeOwner;
            }

            if (submitted == CustomerPropertyStatus.ILiveInTheAboveAndOwnOtherProperties ||
                current == CustomerPropertyStatus.ILiveInTheAboveAndOwnOtherProperties) {
                return CustomerPropertyStatus.ILiveInTheAboveAndOwnOtherProperties;
            }

            return CustomerPropertyStatus.NotYetFilled;
        }

        /// <summary>
        /// Determines the submitted property status.
        /// </summary>
        /// <param name="currentAddressHousingType">Type of the current address housing.</param>
        /// <param name="isOwnsCurrentAddress">The is owns current address.</param>
        /// <param name="isOwnsPreviousAddress">The is owns previous address.</param>
        /// <param name="isOwnsAdditionalProperty">The is owns additional property.</param>
        /// <returns></returns>
        private CustomerPropertyStatus DetermineSubmittedPropertyStatus(HousingType? currentAddressHousingType, bool? isOwnsCurrentAddress,
            bool? isOwnsPreviousAddress,
            bool? isOwnsAdditionalProperty) {

            if (!isOwnsCurrentAddress.HasValue && !isOwnsPreviousAddress.HasValue && !isOwnsAdditionalProperty.HasValue) {

                if (currentAddressHousingType.HasValue) {

                    if (currentAddressHousingType == HousingType.Renting) {
                        return CustomerPropertyStatus.Renting;
                    }
                    if (currentAddressHousingType == HousingType.Social) {
                        return CustomerPropertyStatus.SocialHouse;
                    }
                    if (currentAddressHousingType == HousingType.LivingWithParents) {
                        return CustomerPropertyStatus.LivingWithParents;
                    }
                }

                return CustomerPropertyStatus.NotYetFilled;
            }

            if (isOwnsCurrentAddress.IsTrue()) {
                if (!isOwnsAdditionalProperty.IsTrue() && !isOwnsPreviousAddress.IsTrue()) {
                    return CustomerPropertyStatus.IHomeOwner;
                } else {
                    return CustomerPropertyStatus.IOwnThisPropertyAndOtherProperties;
                }
            } else {
                if (!isOwnsAdditionalProperty.IsTrue() && !isOwnsPreviousAddress.IsTrue()) {
                    return CustomerPropertyStatus.NotYetFilled;
                } else {
                    return CustomerPropertyStatus.ILiveInTheAboveAndOwnOtherProperties;
                }
            }
        }

        /// <summary>
        /// Converts to customer address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        private CustomerAddress ConvertToCustomerAddress(LivingAddressInfo address, int customerId) {
            return new CustomerAddress {
                Country = address.Country,
                County = address.County,
                Deliverypointsuffix = address.Deliverypointsuffix,
                Line1 = address.Line1,
                Line2 = address.Line2,
                Line3 = address.Line3,
                Nohouseholds = address.Nohouseholds,
                Mailsortcode = address.Mailsortcode,
                Postcode = address.Postcode,
                Pobox = address.Pobox,
                Organisation = address.Organisation,
                Udprn = address.Udprn,
                Town = address.Town,
                CustomerId = customerId,
                Rawpostcode = address.Rawpostcode,
                Smallorg = address.Smallorg
            };
        }
    }
}
