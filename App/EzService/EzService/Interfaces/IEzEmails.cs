using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Interfaces
{
    using System.ServiceModel;

    public interface IEzEmails
    {
        [OperationContract]
        ActionMetaData ApprovedUser(int userId, int customerId, decimal loanAmount, int validHours, bool isFirst);

        [OperationContract]
        ActionMetaData CashTransferred(int customerId, decimal amount, string loanRefNum, bool isFirst);

        [OperationContract]
        ActionMetaData Escalated(int customerId, int userId);

        [OperationContract]
        ActionMetaData GetCashFailed(int customerId);

        [OperationContract]
        ActionMetaData LoanFullyPaid(int customerId, string loanRefNum);

        [OperationContract]
        ActionMetaData MoreAmlAndBwaInformation(int userId, int customerId);


        [OperationContract]
        ActionMetaData MoreAmlInformation(int userId, int customerId);

        [OperationContract]
        ActionMetaData MoreBwaInformation(int userId, int customerId);
        
        [OperationContract]
        ActionMetaData PasswordRestored(int customerId);

        [OperationContract]
        ActionMetaData PayEarly(int customerId, decimal amount, string loanRefNum);

        [OperationContract]
        ActionMetaData PayPointAddedByUnderwriter(int customerId, string cardno, string underwriterName, int underwriterId);

        [OperationContract]
        ActionMetaData PayPointNameValidationFailed(int userId, int customerId, string cardHolderName);

        [OperationContract]
        ActionMetaData RejectUser(int userId, int customerId, bool sendToCustomer);

        [OperationContract]
        ActionMetaData EmailRolloverAdded(int userId, int customerId, decimal amount);

        [OperationContract]
        ActionMetaData RenewEbayToken(int userId, int customerId, string marketplaceName, string eBayAddress);

        [OperationContract]
        ActionMetaData RequestCashWithoutTakenLoan(int customerId);

        [OperationContract]
        ActionMetaData TransferCashFailed(int customerId);

        [OperationContract]
        ActionMetaData VipRequest(int customerId, string fullName, string email, string phone);

        [OperationContract]
        ActionMetaData NotifySalesOnNewCustomer(int customerID);

        [OperationContract]
        ActionMetaData EmailHmrcParsingErrors(
            int customerID,
            int customerMarketplaceID,
            SortedDictionary<string, string> errorsToEmail
            );

        [OperationContract]
        ActionMetaData SendEverlineRefinanceMails(int customerId, string customerName, DateTime now, decimal loanAmount, decimal transferedAmount);
    }
}
