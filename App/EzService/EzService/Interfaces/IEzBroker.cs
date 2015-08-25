using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Interfaces {
    using System.ServiceModel;
    using Ezbob.Backend.Models;
    using Ezbob.Backend.ModelsWithDB;
    using EzService.ActionResults;

    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IEzBroker {
        [OperationContract]
        ActionMetaData BrokerAcceptTerms(int termsID, string contactEmail);

        [OperationContract]
        ActionMetaData BrokerAddCustomerLead(
            string leadFirstName,
            string leadLastName,
            string leadEmail,
            string leadAddMode,
            string contactEmail
            );

        [OperationContract]
        ActionMetaData BrokerAddBank(BrokerAddBankModel model);

        [OperationContract]
        ActionMetaData BrokerApproveAndResetCustomerPassword(
            int underwriterID,
            int nCustomerID,
            decimal loanAmount,
            int validHours,
            bool isFirst
            );

        [OperationContract]
        ActionMetaData BrokerAttachCustomer(int nCustomerID, int? brokerID, int underwriterID);

        [OperationContract]
        StringActionResult BrokerBackFromCustomerWizard(int leadID);

        [OperationContract]
        ActionMetaData BrokerCheckCustomerRelevance(
            int nCustomerID,
            string sCustomerEmail,
            bool isAlibaba,
            string sourceRef,
            string confirmEmailLink
            );

        [OperationContract]
        ActionMetaData BrokerCustomerWizardComplete(int nCustomerID);

        [OperationContract]
        ActionMetaData BrokerDeleteCustomerFiles(string customerRefNum, string contactEmail, int[] aryFileIDs);

        [OperationContract]
        BrokerCustomerFileContentsActionResult BrokerDownloadCustomerFile(
            string customerRefNum,
            string contactEmail,
            int fileID
            );

        

        [OperationContract]
        BrokerInstantOfferResponseActionResult BrokerInstantOffer(BrokerInstantOfferRequest request);

        [OperationContract]
        ActionMetaData BrokerLeadAcquireCustomer(
            int customerID,
            int leadID,
            string firstName,
            bool brokerFillsForCustomer,
            string confirmationToken
            );

        [OperationContract]
        BrokerLeadDetailsActionResult BrokerLeadCanFillWizard(int leadID, string leadEmail, string contactEmail);

        [OperationContract]
        BrokerLeadDetailsActionResult BrokerLeadCheckToken(string token);

        [OperationContract]
        ActionMetaData BrokerLeadSendInvitationEmail(int leadID, string brokerContactEmail);

        [OperationContract]
        BrokerCustomerDetailsActionResult BrokerLoadCustomerDetails(string customerRefNum, string contactEmail);

        [OperationContract]
        BrokerLeadDetailsDataActionResult BrokerLoadLeadDetails(int leadID, string contactEmail);

        [OperationContract]
        BrokerCustomerFilesActionResult BrokerLoadCustomerFiles(string customerRefNum, string contactEmail);

        [OperationContract]
        BrokerCustomersActionResult BrokerLoadCustomerList(string contactEmail);

        [OperationContract]
        BrokerCustomersActionResult BrokerLoadCustomersByID(int brokerID);

        [OperationContract]
        BrokerPropertiesActionResult BrokerLoadOwnProperties(string contactEmail);

        [OperationContract]
        BrokerPropertiesActionResult BrokerLoadPropertiesByID(int brokerID);

        [OperationContract]
        StringListActionResult BrokerLoadSignedTerms(string contactEmail);

        [OperationContract]
        BrokerStaticDataActionResult BrokerLoadStaticData(bool loadFilesOnly, int originID);

        [OperationContract]
        BrokerPropertiesActionResult BrokerLogin(
            string email,
            Password password,
            string promotionName,
            DateTime? promotionPageVisitTime,
            int uiOriginID
            );

        [OperationContract]
        ActionMetaData BrokerRestorePassword(string mobile, string code);

        [OperationContract]
        StringActionResult BrokerSaveCrmEntry(
            string type,
            int actionID,
            int statusID,
            string comment,
            string customerRefNum,
            string contactEmail
            );

        [OperationContract]
        ActionMetaData BrokerSaveUploadedCustomerFile(
            string customerRefNum,
            string contactEmail,
            byte[] fileContents,
            string fileName
            );

        [OperationContract]
        BrokerPropertiesActionResult BrokerSignup(
            string sFirmName,
            string firmRegNum,
            string contactName,
            string contactEmail,
            string contactMobile,
            string mobileCode,
            string contactOtherPhone,
            decimal estimatedMonthlyClientAmount,
            Password password,
            string firmWebSiteUrl,
            int estimatedMonthlyApplicationCount,
            bool isCaptchEnabled,
            int brokerTermsID,
            string referredBy,
            bool FCARegistered,
            string licenseNumber,
            int uiOriginID
            );

        [OperationContract]
        ActionMetaData BrokerTransferCommission();

        [OperationContract]
        ActionMetaData BrokerUpdatePassword(string contactEmail, Password oldPassword, Password newPassword);

        [OperationContract]
        ActionMetaData ChangeBrokerEmail(string oldEmail, string newEmail, string newPassword);

        [OperationContract]
        BoolActionResult IsBroker(string contactEmail, int uiOrigin);

        [OperationContract]
        ActionMetaData BrokerForceResetCustomerPasswordEmail(int userID, int nCustomerID);
    }
}
