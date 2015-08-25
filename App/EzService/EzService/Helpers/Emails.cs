using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Helpers
{
    using Ezbob.Backend.Strategies.MailStrategies;
    using EzService.Interfaces;

    /// <summary>
    /// Handles emails
    /// </summary>
    internal class Emails : Executor, IEzEmails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Emails"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public Emails(EzServiceInstanceRuntimeData data)
            : base(data) {}

        /// <summary>
        /// Approved user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="loanAmount">The loan amount.</param>
        /// <param name="validHours">The valid hours.</param>
        /// <param name="isFirst">if set to <c>true</c> [is first].</param>
        /// <returns></returns>
        public ActionMetaData ApprovedUser(int userId, int customerId, decimal loanAmount, int validHours, bool isFirst)
        {
            return Execute<ApprovedUser>(customerId, userId, customerId, loanAmount, validHours, isFirst);
        }

        /// <summary>
        /// Cashes the transferred.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="loanRefNum">The loan reference number.</param>
        /// <param name="isFirst">if set to <c>true</c> [is first].</param>
        /// <returns></returns>
        public ActionMetaData CashTransferred(int customerId, decimal amount, string loanRefNum, bool isFirst)
        {
            return Execute<CashTransferred>(customerId, null, customerId, amount, loanRefNum, isFirst);
        }

        /// <summary>
        /// Escalateds the specified customer identifier.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public ActionMetaData Escalated(int customerId, int userId)
        {
            return Execute<Escalated>(customerId, customerId, customerId);
        }

        /// <summary>
        /// Gets the cash failed.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public ActionMetaData GetCashFailed(int customerId)
        {
            return Execute<GetCashFailed>(customerId, null, customerId);
        }

        /// <summary>
        /// Loans the fully paid.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="loanRefNum">The loan reference number.</param>
        /// <returns></returns>
        public ActionMetaData LoanFullyPaid(int customerId, string loanRefNum)
        {
            return Execute<LoanFullyPaid>(customerId, null, customerId, loanRefNum);
        }

        /// <summary>
        /// More the aml and bwa information.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public ActionMetaData MoreAmlAndBwaInformation(int userId, int customerId)
        {
            return Execute<MoreAmlAndBwaInformation>(customerId, userId, customerId);
        }

        /// <summary>
        /// More aml information.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public ActionMetaData MoreAmlInformation(int userId, int customerId)
        {
            return Execute<MoreAmlInformation>(customerId, userId, customerId);
        }

        /// <summary>
        /// More bwa information.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public ActionMetaData MoreBwaInformation(int userId, int customerId)
        {
            return Execute<MoreBwaInformation>(customerId, userId, customerId);
        }

        /// <summary>
        /// Restored password.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public ActionMetaData PasswordRestored(int customerId)
        {
            return Execute<PasswordRestored>(customerId, null, customerId);
        }

        /// <summary>
        /// Pays the early.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="loanRefNum">The loan reference number.</param>
        /// <returns></returns>
        public ActionMetaData PayEarly(int customerId, decimal amount, string loanRefNum)
        {
            return Execute<PayEarly>(customerId, customerId, customerId, amount, loanRefNum);
        }

        /// <summary>
        /// Pays the point added by underwriter.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="cardno">The cardno.</param>
        /// <param name="underwriterName">Name of the underwriter.</param>
        /// <param name="underwriterId">The underwriter identifier.</param>
        /// <returns></returns>
        public ActionMetaData PayPointAddedByUnderwriter(int customerId, string cardno, string underwriterName, int underwriterId)
        {
            return Execute<PayPointAddedByUnderwriter>(customerId, underwriterId, customerId, cardno, underwriterName, underwriterId);
        }

        /// <summary>
        /// Pays the point name validation failed.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="cardHolderName">Name of the card holder.</param>
        /// <returns></returns>
        public ActionMetaData PayPointNameValidationFailed(int userId, int customerId, string cardHolderName)
        {
            return Execute<PayPointNameValidationFailed>(customerId, userId, customerId, cardHolderName);
        }

        /// <summary>
        /// Rejects the user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="sendToCustomer">if set to <c>true</c> [send to customer].</param>
        /// <returns></returns>
        public ActionMetaData RejectUser(int userId, int customerId, bool sendToCustomer)
        {
            return Execute<RejectUser>(customerId, userId, customerId, sendToCustomer);
        }

        /// <summary>
        /// Emails the rollover added.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public ActionMetaData EmailRolloverAdded(int userId, int customerId, decimal amount)
        {
            return Execute<EmailRolloverAdded>(customerId, userId, customerId, amount);
        }

        /// <summary>
        /// Renews the ebay token.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="marketplaceName">Name of the marketplace.</param>
        /// <param name="eBayAddress">The e bay address.</param>
        /// <returns></returns>
        public ActionMetaData RenewEbayToken(int userId, int customerId, string marketplaceName, string eBayAddress)
        {
            return Execute<RenewEbayToken>(customerId, userId, customerId, marketplaceName, eBayAddress);
        }

        /// <summary>
        /// Requests the cash without taken loan.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public ActionMetaData RequestCashWithoutTakenLoan(int customerId)
        {
            return Execute<RequestCashWithoutTakenLoan>(customerId, null, customerId);
        }

        /// <summary>
        /// Transfers the cash failed.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public ActionMetaData TransferCashFailed(int customerId)
        {
            return Execute<TransferCashFailed>(customerId, null, customerId);
        }

        /// <summary>
        /// Vips the request.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="fullName">The full name.</param>
        /// <param name="email">The email.</param>
        /// <param name="phone">The phone.</param>
        /// <returns></returns>
        public ActionMetaData VipRequest(int customerId, string fullName, string email, string phone)
        {
            return Execute<VipRequest>(customerId, null, customerId, fullName, email, phone);
        }

        /// <summary>
        /// Notifies the sales on new customer by mail.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <returns></returns>
        public ActionMetaData NotifySalesOnNewCustomer(int customerID)
        {
            return Execute<NotifySalesOnNewCustomer>(customerID, null, customerID);
        }

        /// <summary>
        /// Emails the HMRC parsing errors.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="customerMarketplaceID">The customer marketplace identifier.</param>
        /// <param name="errorsToEmail">The errors to email.</param>
        /// <returns></returns>
        public ActionMetaData EmailHmrcParsingErrors(int customerID, int customerMarketplaceID, SortedDictionary<string, string> errorsToEmail)
        {
            return Execute<EmailHmrcParsingErrors>(customerID, null, customerID, customerMarketplaceID, errorsToEmail);
        }

        /// <summary>
        /// Sends the everline refinance mails.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="customerName">Name of the customer.</param>
        /// <param name="now">The now.</param>
        /// <param name="loanAmount">The loan amount.</param>
        /// <param name="transferedAmount">The transfered amount.</param>
        /// <returns></returns>
        public ActionMetaData SendEverlineRefinanceMails(int customerId, string customerName, DateTime now, decimal loanAmount, decimal transferedAmount)
        {
            return Execute<SendEverlineRefinanceMails>(customerId, customerId, customerId, customerName, now, loanAmount, transferedAmount);
        }
    }
}
