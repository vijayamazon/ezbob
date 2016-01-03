namespace EzBobService.Customer {
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using EzBobCommon;
    using EzBobCommon.Utils;
    using EzBobModels;
    using EzBobModels.Configurations;
    using EzBobModels.Enums;
    using EzBobPersistence;
    using EzBobPersistence.Alibaba;
    using EzBobPersistence.Customer;
    using EzBobPersistence.Loan;
    using EzBobService.Misc;
    using log4net;

    /// <summary>
    /// encapsulates customer sign-up procedure
    /// </summary>
    public class CustomerProcessor {
        /// <summary>
        /// The user management configuration
        /// </summary>
        private readonly Lazy<UserManagementConfiguration> userManagementConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerProcessor"/> class.
        /// </summary>
        public CustomerProcessor() {
            this.userManagementConfiguration = new Lazy<UserManagementConfiguration>(() => ConfigurationQueries.GetUserManagementConfiguration());
        }

        [Injected]
        public ConfigurationQueries ConfigurationQueries { get; set; }

        [Injected]
        public ICustomerQueries CustomerQueries { get; set; }

        [Injected]
        public ILoanQueries LoanQueries { get; set; }

        [Injected]
        public IAlibabaQueries AlibabaQueries { get; set; }

        [Injected]
        public RefNumberGenerator RefNumGenerator { get; set; }

        [Injected]
        public ILog Log { get; set; }


        /// <summary>
        /// Sign-ups the new customer.
        /// </summary>
        /// <param name="customer">The customer.</param>
        /// <returns>customer id. Negative Id in case of error</returns>
        public int SignupNewCustomer(Customer customer, InfoAccumulator info) {
            using (var unitOfWork = new UnitOfWork()) {
                var loginInfo = customer.LoginInfo;
                User user = CreateUser(loginInfo.Email, loginInfo.Password != null ? loginInfo.Password.Primary : null, loginInfo.PasswordQuestionId, loginInfo.PasswordAnswer, loginInfo.RemoteIp);
                ValidateCreatedUser(user, info);
                if (info.HasErrors) {
                    return -1;
                }

                customer.Id = user.UserId;
                customer.RefNumber = RefNumGenerator.GenerateRefNumber();

                customer.Status = CustomerStatus.Registered.ToString();
                customer.WizardStep = (int)WizardStepType.SignUp;
                customer.CollectionStatus = (int)CollectionStatusNames.Enabled;
                customer.TrustPilotStatusID = (int)TrustPilotStauses.Neither;
                customer.GreetingMailSentDate = DateTime.UtcNow;
                customer.BrokerID = null;
                

                int? isCustomerSaved = CustomerQueries.UpsertCustomer(customer);
                if (!isCustomerSaved.HasValue) {
                    info.AddError("could not save customer");
                    return -1;
                }

                unitOfWork.Commit();
                return customer.Id;
            }
        }

        /// <summary>
        /// Updates customer.
        /// </summary>
        /// <param name="customer">The customer.</param>
        /// <param name="requestedAmount">The requested amount.</param>
        /// <param name="sourceRefList">The source reference list.</param>
        /// <param name="visitTimeList">The visit time list.</param>
        /// <param name="campaignSrcRef">The campaign source reference.</param>
        /// <param name="addresses">The addresses.</param>
        /// <param name="phoneNumbers">The phone numbers.</param>
        /// <returns></returns>
        public InfoAccumulator UpdateCustomer(Customer customer, double requestedAmount, string sourceRefList, string visitTimeList,
            CampaignSourceRef campaignSrcRef, CustomerAddress[] addresses, CustomerPhone[] phoneNumbers) {
            
//            InfoAccumulator info = ValidateCustomer(customer);
//            if (info.HasErrors) {
//                return info;
//            }

            InfoAccumulator info = new InfoAccumulator();

            using (var unitOfWork = new UnitOfWork()) {
                DateTime now = DateTime.UtcNow;

                int? isCustomerSaved = CustomerQueries.UpsertCustomer(customer);
                if (!isCustomerSaved.HasValue) {
                    info.AddError("could not save customer");
                    return info;
                }

                bool isSuccess = true;

                if (requestedAmount > 0) {
                    CustomerRequestedLoan requestedLoan = new CustomerRequestedLoan {
                        CustomerId = customer.Id,
                        Amount = requestedAmount,
                        Created = DateTime.UtcNow
                    };

                    isSuccess = LoanQueries.SaveCustomerRequestedLoan(requestedLoan) ?? true;
                    if (!isSuccess) {
                        info.AddError("could not save requested loan");
                        return info;
                    }
                }


                var session = new CustomerSession() {
                    CustomerId = customer.Id,
                    StartSession = now,
                    Ip = customer.LoginInfo != null ? customer.LoginInfo.RemoteIp ?? "unknown" : "unknown",//TODO: review
                    IsPasswdOk = true,
                    ErrorMessage = "Registration" //TODO: do something with this
                };

                isSuccess = CustomerQueries.SaveCustomerSession(session) ?? true;
                if (!isSuccess) {
                    info.AddError("could not save customer session");
                    return info;
                }

                if (sourceRefList != null && visitTimeList != null) {
                    isSuccess = CustomerQueries.SaveSourceRefHistory(customer.Id, sourceRefList, visitTimeList) ?? true;
                    if (!isSuccess) {
                        info.AddError("could not save customer ref history");
                    }
                }

                if (campaignSrcRef != null) {
                    isSuccess = CustomerQueries.SaveCampaignSourceRef(customer.Id, campaignSrcRef);
                    if (!isSuccess) {
                        info.AddError("could not save campaign source ref for customer: " + customer.Id);
                        return info;
                    }
                }

                if (customer.AlibabaId != null && customer.IsAlibaba) {
                    AlibabaBuyer alibabaBuyer = new AlibabaBuyer {
                        AliId = Convert.ToInt64(customer.AlibabaId),
                        CustomerId = customer.Id
                    };

                    isSuccess = AlibabaQueries.CreateAlibabaBuyer(alibabaBuyer) ?? true;
                    if (!isSuccess) {
                        info.AddError("could not create alibaba buyer");
                    }
                }

                if (customer.CustomerAddress != null) {
                    customer.CustomerAddress.CustomerId = customer.Id;
                    isSuccess = CustomerQueries.SaveCustomerAddress(customer.CustomerAddress) ?? true;
                    if (!isSuccess) {
                        info.AddError("could not save customer address");
                    }
                }

                if (CollectionUtils.IsNotEmpty(addresses)) {
                    foreach (CustomerAddress customerAddress in addresses) {
                        bool? res = CustomerQueries.SaveCustomerAddress(customerAddress);
                        if (res == null || res.Value == false) {
                            info.AddError("could not save customer address");
                            return info;
                        }
                    }
                }

                if (CollectionUtils.IsNotEmpty(phoneNumbers)) {
                    foreach (CustomerPhone phone in phoneNumbers) {
                        bool saveCustomerPhone = CustomerQueries.SaveCustomerPhone(phone);
                        if (!saveCustomerPhone) {
                            info.AddError("could not save customer phone");
                            return info;
                        }
                    }
                }

                unitOfWork.Commit();
                return info;
            }
        }

        /// <summary>
        /// Validates the customer.
        /// </summary>
        /// <param name="customer">The customer.</param>
        /// <returns></returns>
        private InfoAccumulator ValidateCustomer(Customer customer) {
            var loginInfo = customer.LoginInfo;
            var errors = new InfoAccumulator();
            ValidateEmail(loginInfo.Email, this.userManagementConfiguration.Value, errors);
            ValidateNewPassword(loginInfo.Password.Primary, this.userManagementConfiguration.Value, errors);
            return errors;
        }

        /// <summary>
        /// Gets the user identifier by email address.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <returns></returns>
        public ResultInfoAccomulator<long?> GetUserIdByUserName(string emailAddress) {
            var result = new ResultInfoAccomulator<long?>();
            if (string.IsNullOrEmpty(emailAddress)) {
                result.AddError("got empty email address");
                return result;
            }

            long? id = CustomerQueries.GetUserIdByUserName(emailAddress);

            if (id.HasValue) {
                if (id.Value > 0) {
                    result.Result = id.Value;
                    return result;
                }
                result.AddInfo(string.Format("user id is not found for email: {0}", emailAddress));
            } else {
                result.AddError(string.Format("got error when tried to found user id by email: {0}", emailAddress));
            }

            return result;
        }

        /// <summary>
        /// Creates the user in DB using DAL
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="password">The password.</param>
        /// <param name="passwordQuestionId">The password question identifier.</param>
        /// <param name="passwordAnswer">The password answer.</param>
        /// <param name="remoteIp">The remote ip.</param>
        /// <returns></returns>
        private User CreateUser(String email, String password, int passwordQuestionId, string passwordAnswer, string remoteIp) {
            if (string.IsNullOrEmpty(passwordAnswer)) {
                Log.Warn("got empty 'password' on customer sign-up");
            }

            if (string.IsNullOrEmpty(remoteIp)) {
                Log.Warn("go empty 'remoteIp' on customer sign-up");
            }

            return CustomerQueries.CreateUser(email, password, passwordQuestionId, passwordAnswer, remoteIp);
        }

        /// <summary>
        /// Validates the email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="infoAccumulator">The errors accumulator.</param>
        /// <returns></returns>
        private bool ValidateEmail(String email, UserManagementConfiguration configuration, InfoAccumulator infoAccumulator) {

            if (string.IsNullOrEmpty(email)) {
                String msg = "got empty email address";
                infoAccumulator.AddError(msg);
                Log.Error(msg);
                return false;
            }

            email = email.Trim()
                .ToLowerInvariant();

            if (configuration.Underwriters.Contains(email)) {
                return true;
            }

            if (!Regex.IsMatch(email, configuration.LoginValidationStringForWeb)) {
                infoAccumulator.AddError("Login does not conform to the password security policy.");
                return false;
            }

            if (!Regex.IsMatch(email, configuration.LoginValidity)) {
                infoAccumulator.AddError("Can't validate login");
                Log.Warn(string.Format("email: '{0}' does not match the pattern", email));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the new password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="infoAccumulator">The errors accumulator.</param>
        /// <returns></returns>
        private bool ValidateNewPassword(String password, UserManagementConfiguration configuration, InfoAccumulator infoAccumulator) {
            if (string.IsNullOrEmpty(password)) {
                infoAccumulator.AddError("empty password");
                return false;
            }

            if (!Regex.IsMatch(password, configuration.PasswordValidity)) {
                infoAccumulator.AddError("invalid password");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the created user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="info">The errors.</param>
        /// <returns></returns>
        private bool ValidateCreatedUser(User user, InfoAccumulator info) {

            if (user.UserId > 0) {
                return true;
            }

            if (user.UserId == -1) {
                String msg = String.Format("User with email '{0}' already exists.", user.EmailAddress);
                info.AddError(msg);
                Log.Info(msg);
            } else if (user.UserId == -2) {
                String msg = String.Format("Could not find role '{0}'.", user.RoleName);
                info.AddError(msg);
                Log.Error(msg);
            } else {
                String msg = String.Format("CreateWebUser returned unexpected result '{0}' as userId", user.UserId);
                info.AddError(msg);
                Log.Error(msg);
            }

            return false;
        }
    }
}
