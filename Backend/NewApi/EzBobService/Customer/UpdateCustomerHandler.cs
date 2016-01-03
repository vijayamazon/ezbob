namespace EzBobService.Customer {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using EzBobApi.Commands.Customer;
    using EzBobApi.Commands.Customer.Sections;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobCommon.Utils;
    using EzBobCommon.Utils.Encryption;
    using EzBobModels;
    using EzBobModels.Enums;
    using EzBobPersistence.Customer;
    using NServiceBus;

    /// <summary>
    /// Handles CustomerUpdateCommand
    /// </summary>
    public class UpdateCustomerHandler : HandlerBase<CustomerUpdateCommandResponse>, IHandleMessages<CustomerUpdateCommand> {
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
        /// <param name="cmd">The command to handle.</param>
        /// <remarks>
        /// This method will be called when a message arrives on the bus and should contain
        ///             the custom logic to execute when the command is received.
        /// </remarks>
        public void Handle(CustomerUpdateCommand cmd) {

            InfoAccumulator info = new InfoAccumulator();

            if (!ValidateCommand(cmd, info)) {
                SendReply(info, cmd, response => response.CustomerId = cmd.CustomerId);
                return;
            }

            //fills customer's properties
            Customer customer = CreateCustomer(cmd, info);
            if (info.HasErrors) {
                SendReply(info, cmd, resp => resp.CustomerId = cmd.CustomerId);
                return;
            }

            string referenceSource = null;
            string visitTimes = null;
            if (cmd.Cookies != null) {
                referenceSource = cmd.Cookies.ReferenceSource;
                visitTimes = cmd.Cookies.VisitTimes;
            }

            CustomerAddress[] addresses = ExtractCustomerAddresses(cmd, customer.Id)
                .ToArray();
            CustomerPhone[] phones = ExtractPhoneNumbers(cmd, customer.Id)
                .ToArray();

            var errors = CustomerProcessor.UpdateCustomer(customer, cmd.RequestedAmount, referenceSource, visitTimes, cmd.CampaignSourceRef, addresses, phones);
            if (errors.HasErrors) {
                if (errors.IsRetry) {
                    RegisterError(info, cmd);
                }

                return;
            }

            SendReply(errors, cmd, response => response.CustomerId = cmd.CustomerId);
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

            if (!ValidateAddresses(cmd, info)) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the addresses.
        /// </summary>
        /// <param name="cmd">The command.</param>
        /// <param name="info">The information.</param>
        /// <returns></returns>
        private bool ValidateAddresses(CustomerUpdateCommand cmd, InfoAccumulator info) {
            //it's ok if nothing was sent
            if (CollectionUtils.IsEmpty(cmd.LivingAccommodations) && CollectionUtils.IsEmpty(cmd.OwnedProperties)) {
                return true;
            }

            if (CollectionUtils.IsNotEmpty(cmd.LivingAccommodations) && CollectionUtils.IsEmpty(cmd.OwnedProperties)) {
                return ValidateOneLivingAddressExistence(cmd.LivingAccommodations, info);
            }

            if (CollectionUtils.IsNotEmpty(cmd.OwnedProperties) && CollectionUtils.IsEmpty(cmd.LivingAccommodations)) {
                return ValidateOneLivingAddressExistence(cmd.OwnedProperties, info);
            }

            return ValidateOneLivingAddressExistence(cmd.LivingAccommodations.Concat(cmd.OwnedProperties.Cast<LivingAddressInfo>()), info);
        }

        /// <summary>
        /// Validates the one living address existence.
        /// </summary>
        /// <param name="livingAddresses">The living addresses.</param>
        /// <param name="info">The information.</param>
        /// <returns></returns>
        private bool ValidateOneLivingAddressExistence(IEnumerable<LivingAddressInfo> livingAddresses, InfoAccumulator info) {
            bool res = true;

            int livingCount = livingAddresses.Count(o => o.IsLivingNow);

            if (livingCount != 1) {
                LogError(expectedToGetOneAddressWherePersonLivesNowAndGot + livingCount, info);
                res = false;
            }

            return res;
        }

        /// <summary>
        /// Creates the customer.
        /// </summary>
        /// <param name="cmd">The command.</param>
        /// <returns></returns>
        private Customer CreateCustomer(CustomerUpdateCommand cmd, InfoAccumulator info) {

            Customer customer = new Customer();
            customer.Id = int.Parse(EncryptionUtils.SafeDecrypt(cmd.CustomerId));
            customer.PersonalInfo.PropertyStatus = GetPropertyStatus(cmd);

            UpdatePersonalDetails(customer, cmd.PersonalDetails, info);
            UpdateAccount(customer, cmd.Account, info, string.IsNullOrEmpty(cmd.CustomerOrigin)); //TODO: logic to determine ezbob origin
            UpdateContactDetails(customer, cmd.ContactDetails);
            UpdateAddressInfo(customer, cmd.Address);

            if (info.HasErrors) {
                return null;
            }

            if (cmd.AlibabaInfo != null) {
                customer.AlibabaId = cmd.AlibabaInfo.AlibabaId;
                customer.IsAlibaba = cmd.AlibabaInfo.IsAlibaba;
            }

            if (cmd.Cookies != null) {
                customer.GoogleCookie = cmd.Cookies.GoogleCookie;
                customer.ReferenceSource = cmd.Cookies.ReferenceSource;
            }

            return customer;
        }

        /// <summary>
        /// Gets the property status.
        /// </summary>
        /// <param name="cmd">The command.</param>
        /// <returns></returns>
        private int GetPropertyStatus(CustomerUpdateCommand cmd) {
            int ownedProperties = 0;
            int livingAccomotdations = 0;

            int propertyStatus = -1;

            if (cmd.LivingAccommodations != null) {
                livingAccomotdations = cmd.LivingAccommodations.Length;
            }

            if (cmd.OwnedProperties != null) {
                ownedProperties = cmd.OwnedProperties.Length;
            }

            if (ownedProperties == 0 && livingAccomotdations > 0) {
                var livingAccommodationInfo = cmd.LivingAccommodations
                    .First(o => o.IsLivingNow);
                var housingType = (HousingType)livingAccommodationInfo.HousingType;
                switch (housingType) {
                case HousingType.Social:
                    propertyStatus = 6;
                    break;
                case HousingType.LivingWithParents:
                    propertyStatus = 7;
                    break;
                case HousingType.Renting:
                    propertyStatus = 5;
                    break;
                default:
                    propertyStatus = -1;
                    Log.Error("got unexpected housing type: " + livingAccommodationInfo.HousingType);
                    break;
                }
            } else if (ownedProperties > 0 && livingAccomotdations > 0) {
                propertyStatus = 3; //I live in the above and own other properties
            } else if (ownedProperties > 0 && livingAccomotdations == 0) {
                if (ownedProperties == 1) {
                    propertyStatus = 1; //I own only this property
                } else {
                    propertyStatus = 2; //I own this property and other properties
                }
            }

            return propertyStatus;
        }

        /// <summary>
        /// Updates the address information.
        /// </summary>
        /// <param name="customer">The customer.</param>
        /// <param name="address">The address.</param>
        private void UpdateAddressInfo(Customer customer, AddressInfo address) {
            if (address == null) {
                return;
            }
            customer.CustomerAddress.Organisation = address.Organisation;
            customer.CustomerAddress.Line1 = address.Line1;
            customer.CustomerAddress.Line2 = address.Line2;
            customer.CustomerAddress.Line3 = address.Line3;
            customer.CustomerAddress.Town = address.Town;
            customer.CustomerAddress.County = address.County;
            customer.CustomerAddress.Country = address.Country;
            customer.CustomerAddress.Postcode = address.Postcode;
            customer.CustomerAddress.Rawpostcode = address.Rawpostcode;
            customer.CustomerAddress.Deliverypointsuffix = address.Deliverypointsuffix;
            customer.CustomerAddress.Nohouseholds = address.Nohouseholds;
            customer.CustomerAddress.Smallorg = address.Smallorg;
            customer.CustomerAddress.Pobox = address.Pobox;
            customer.CustomerAddress.Mailsortcode = address.Mailsortcode;
            customer.CustomerAddress.Udprn = address.Udprn;
        }

        /// <summary>
        /// Updates the personal details.
        /// </summary>
        /// <param name="customer">The customer.</param>
        /// <param name="personalDetails">The personal details.</param>
        /// <param name="info">The information.</param>
        private void UpdatePersonalDetails(Customer customer, PersonalDetailsInfo personalDetails, InfoAccumulator info) {
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
        /// Updates the contact details.
        /// </summary>
        /// <param name="customer">The customer.</param>
        /// <param name="contactDetails">The contact details.</param>
        private void UpdateContactDetails(Customer customer, ContactDetailsInfo contactDetails) {
            if (contactDetails == null) {
                return;
            }

            var personalInfo = customer.PersonalInfo;

            personalInfo.MobilePhone = contactDetails.MobilePhone;
            personalInfo.MobilePhoneVerified = contactDetails.IsMobilePhoneVerified;
        }

        /// <summary>
        /// Updates the account.
        /// </summary>
        /// <param name="customer">The customer.</param>
        /// <param name="account">The account.</param>
        private void UpdateAccount(Customer customer, AccountInfo account, InfoAccumulator info, bool isEzBobOrigin) {
            if (account == null) {
                return;
            }

            if (StringUtils.IsEmpty(account.EmailAddress)) {
                LogError(gotEmptyEmailaddress, info);
                return;
            }

            customer.Name = account.EmailAddress;
            customer.Vip = CustomerQueries.IsVip(account.EmailAddress) ?? false; //TODO: look again in this flow, how we get vip

            var loginInfo = customer.LoginInfo;

            if (isEzBobOrigin) {
                if (StringUtils.IsEmpty(account.Password)) {
                    LogError(gotInvalidPassword, info);
                    return;
                }

                loginInfo.Password = new Password(account.Password, account.Password); //TODO: clean password
            }

            loginInfo.PasswordAnswer = account.SecretAnswer;
            loginInfo.PasswordQuestionId = account.SecretQuestion.HasValue ? account.SecretQuestion.Value : -1;
            loginInfo.Email = account.EmailAddress;
            loginInfo.RemoteIp = account.RemoteIp;
        }

        /// <summary>
        /// Extracts the customer addresses.
        /// </summary>
        /// <param name="cmd">The command.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        private IEnumerable<CustomerAddress> ExtractCustomerAddresses(CustomerUpdateCommand cmd, int customerId) {
            if (CollectionUtils.IsNotEmpty(cmd.LivingAccommodations)) {
                foreach (var accommodation in cmd.LivingAccommodations) {
                    yield return ConvertLivingAccommodationToCustomerAddress(accommodation, customerId);
                }
            }

            if (CollectionUtils.IsNotEmpty(cmd.OwnedProperties)) {
                foreach (var ownedProperty in cmd.OwnedProperties) {
                    yield return ConvertOwnedPropertyToCustomerAddress(ownedProperty, customerId);
                }
            }
        }

        /// <summary>
        /// Converts the living property to customer address.
        /// </summary>
        /// <param name="accommodation">The accommodation.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        private CustomerAddress ConvertLivingAccommodationToCustomerAddress(LivingAccommodationInfo accommodation, int customerId) {
            CustomerAddress address = new CustomerAddress {
                CustomerId = customerId,
                addressType = accommodation.IsLivingNow ? CustomerAddressType.PersonalAddress : CustomerAddressType.PrevPersonAddresses
            };

            FillCustomerAddressByAddressInfo(address, accommodation);

            return address;
        }

        /// <summary>
        /// Converts the owned property to customer address.
        /// </summary>
        /// <param name="ownedProperty">The owned property.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        private CustomerAddress ConvertOwnedPropertyToCustomerAddress(OwnedPropertyAddressInfo ownedProperty, int customerId) {
            CustomerAddress address = new CustomerAddress {
                CustomerId = customerId,
                addressType = ownedProperty.IsLivingNow ? CustomerAddressType.PersonalAddress : CustomerAddressType.PrevPersonAddresses
            };

            FillCustomerAddressByAddressInfo(address, ownedProperty);

            return address;
        }

        /// <summary>
        /// Fills the customer address by address information.
        /// </summary>
        /// <param name="customerAddress">The customer address.</param>
        /// <param name="address">The address.</param>
        private void FillCustomerAddressByAddressInfo(CustomerAddress customerAddress, AddressInfo address) {
            customerAddress.Country = address.Country;
            customerAddress.County = address.County;
            customerAddress.Deliverypointsuffix = address.Deliverypointsuffix;
            customerAddress.Line1 = address.Line1;
            customerAddress.Line2 = address.Line2;
            customerAddress.Line3 = address.Line3;
            customerAddress.Nohouseholds = address.Nohouseholds;
            customerAddress.Mailsortcode = address.Mailsortcode;
            customerAddress.Postcode = address.Postcode;
            customerAddress.Pobox = address.Pobox;
            customerAddress.Organisation = address.Organisation;
            customerAddress.Udprn = address.Udprn;
            customerAddress.Town = address.Town;
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
                        Phone = cmd.ContactDetails.MobilePhone,
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
    }
}
