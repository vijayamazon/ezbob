namespace EzBobService.Customer {
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Common.Logging;
    using EzBobCommon;
    using EzBobCommon.Utils;
    using EzBobModels;
    using EzBobModels.Configurations;
    using EzBobModels.Customer;
    using EzBobModels.Enums;
    using EzBobPersistence;
    using EzBobPersistence.Alibaba;
    using EzBobPersistence.Customer;
    using EzBobPersistence.Loan;
    using EzBobService.Misc;

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
        public ILog Log { get; set; }


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
        public InfoAccumulator UpdateCustomer(Customer customer, decimal requestedAmount, string sourceRefList, string visitTimeList,
            CampaignSourceRef campaignSrcRef, IEnumerable<CustomerAddress> addresses, IEnumerable<CustomerPhone> phoneNumbers) {
            
            InfoAccumulator info = new InfoAccumulator();

            using (var unitOfWork = new UnitOfWork()) {
                DateTime now = DateTime.UtcNow;

                int customerId = (int)CustomerQueries.UpsertCustomer(customer);
                if (customerId < 1) {
                    info.AddError("could not save customer");
                    return info;
                }

                bool isSuccess = true;

                if (requestedAmount > 0) {
                    CustomerRequestedLoan requestedLoan = new CustomerRequestedLoan {
                        CustomerId = customerId,
                        Amount = requestedAmount,
                        Created = DateTime.UtcNow
                    };

                    int id = (int)LoanQueries.UpsertCustomerRequestedLoan(requestedLoan);
                    if (id < 1) {
                        info.AddError("could not save requested loan");
                        return info;
                    }
                }


//                var session = new CustomerSession() {
//                    CustomerId = customer.Id,
//                    StartSession = now,
//                    Ip = customer.LoginInfo != null ? customer.LoginInfo.RemoteIp ?? "unknown" : "unknown",//TODO: review
//                    IsPasswdOk = true,
//                    ErrorMessage = "Registration" //TODO: do something with this
//                };
//
//                isSuccess = CustomerQueries.SaveCustomerSession(session) ?? true;
//                if (!isSuccess) {
//                    info.AddError("could not save customer session");
//                    return info;
//                }

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
                    isSuccess = CustomerQueries.UpsertCustomerAddress(customer.CustomerAddress) ?? true;
                    if (!isSuccess) {
                        info.AddError("could not save customer address");
                    }
                }

                if (CollectionUtils.IsNotEmpty(addresses)) {
                    foreach (CustomerAddress customerAddress in addresses) {
                        bool? res = CustomerQueries.UpsertCustomerAddress(customerAddress);
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
        /// Gets the user identifier by email address.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <returns></returns>
        public Optional<int> GetUserIdByUserName(string emailAddress) {
            if (emailAddress.IsEmpty()) {
                throw new ArgumentException("Got empty email address");
            }

            return CustomerQueries.GetUserIdByUserName(emailAddress);
        }
    }
}
