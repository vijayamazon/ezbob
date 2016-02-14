namespace EzBobPersistence.Customer {
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using EzBobCommon;
    using EzBobModels;
    using EzBobModels.Customer;

    public interface ICustomerQueries {

        /// <summary>
        /// Creates the security user.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="password">The password.</param>
        /// <param name="passwordQuestion">The password question.</param>
        /// <param name="passwordAnswer">The password answer.</param>
        /// <param name="remoteIp">The remote ip.</param>
        /// <returns></returns>
        SecurityUser CreateSecurityUser(string email, string password, int passwordQuestion, string passwordAnswer, string remoteIp);
        /// <summary>
        /// Gets user identifier by user name.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        Optional<int> GetUserIdByUserName(string userName);

        /// <summary>
        /// Creates or updates the customer.
        /// </summary>
        /// <param name="customer">The customer.</param>
        /// <returns>customerId</returns>
        Optional<int> UpsertCustomer(Customer customer);
        /// <summary>
        /// Gets the customer by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        Customer GetCustomerById(int id);
        /// <summary>
        /// Saves the customer requested loan.
        /// </summary>
        /// <param name="requestedLoan">The requested loan.</param>
        Optional<int> SaveCustomerRequestedLoan(CustomerRequestedLoan requestedLoan);
        /// <summary>
        /// Saves the customer session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        bool? SaveCustomerSession(CustomerSession session);
        /// <summary>
        /// Saves the source reference history.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="sourceRefList">The source reference list.</param>
        /// <param name="visitTimeList">The visit time list.</param>
        /// <returns>true - success, false - failure, null - was nothing to save  in db</returns>
        bool? SaveSourceRefHistory(int userId, string sourceRefList, string visitTimeList);
        /// <summary>
        /// Saves the campaign source reference.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="campaignSrcRef">The campaign source reference.</param>
        /// <returns></returns>
        bool SaveCampaignSourceRef(int userId, CampaignSourceRef campaignSrcRef);
        /// <summary>
        /// Gets the customer by email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        Customer GetCustomerByEmail(string email);

        /// <summary>
        /// Upserts the customer address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>true - success, false - failure, null - was nothing to save  in db</returns>
        bool? UpsertCustomerAddress(CustomerAddress address);

        /// <summary>
        /// Determines whether [customer exists by reference number] [the specified reference number].
        /// </summary>
        /// <param name="refNumber">The reference number.</param>
        /// <returns>true - exists, false - does not exist, null - was some problem with db</returns>
        bool? IsCustomerExistsByRefNumber(string refNumber);

        /// <summary>
        /// Determines whether the specified customer is vip.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <returns></returns>
        bool? IsVip(string emailAddress);

        /// <summary>
        /// Saves the customer phone.
        /// </summary>
        /// <param name="customerPhone">The customer phone.</param>
        /// <returns></returns>
        bool SaveCustomerPhone(CustomerPhone customerPhone);

        /// <summary>
        /// Gets the customer addresses.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        IEnumerable<CustomerAddress> GetCustomerAddresses(int customerId);

        /// <summary>
        /// Gets the customer phones.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        IEnumerable<CustomerPhone> GetCustomerPhones(int customerId);

        /// <summary>
        /// Gets the customer partially by identifier.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="selectProperties">The selectProperties.</param>
        /// <returns></returns>
        Customer GetCustomerPartiallyById(int customerId, params Expression<Func<Customer, object>>[] selectProperties);

        /// <summary>
        /// Determines whether the user exists.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        bool IsUserExists(string emailAddress, string password);

        /// <summary>
        /// Gets the customer partially by property.
        /// </summary>
        /// <param name="wherePropertyAccess">The property access.</param>
        /// <param name="wherePropertyValue">The property value.</param>
        /// <param name="selectProperties">The selectProperties.</param>
        /// <returns></returns>
        Customer GetCustomerPartiallyByProperty(Expression<Func<Customer, object>> wherePropertyAccess, object wherePropertyValue, params Expression<Func<Customer, object>>[] selectProperties);
    }
}