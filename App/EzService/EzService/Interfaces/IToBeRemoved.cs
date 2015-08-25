using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Interfaces
{
    using System.ServiceModel;
    using DbConstants;
    using EchoSignLib;
    using Ezbob.Backend.Models;
    using Ezbob.Backend.Models.ExternalAPI;
    using Ezbob.Backend.ModelsWithDB;
    using Ezbob.Backend.Strategies.PricingModel;
    using Ezbob.Backend.Strategies.UserManagement;
    using EzBob.Backend.Models;
    using EZBob.DatabaseLib.Model.Database;

    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IToBeRemoved : // Add base interfaces in the following lines and in alphabetic order. Please.
        IEzAutomationVerification,
        IEzBroker,
        IEzServiceMainStrategy,
        IEzServiceSalesForce,
        IEzServiceVatReturn,
        IEzServiceNewLoan
    {
        [OperationContract]
        ActionMetaData AddCciHistory(int nCustomerID, int nUnderwriterID, bool bCciMark);

        [OperationContract]
        ActionMetaData BackfillAml();

        [OperationContract]
        ActionMetaData BackfillHmrcBusinessRelevance();

        [OperationContract]
        ActionMetaData BackfillLandRegistry2PropertyLink();

        [OperationContract]
        ActionMetaData BackfillNonLimitedCompanies();

        [OperationContract]
        ActionMetaData BackfillTurnover();

        [OperationContract]
        ActionMetaData BackfillZooplaValue();

        [OperationContract]
        ActionMetaData BackfillBrokerCommissionInvoice();

        [OperationContract]
        ActionMetaData CalculateMedal(int underwriterId, int customerId);

        [OperationContract]
        MarketplacesActionResult CalculateModelsAndAffordability(int userId, int nCustomerID, DateTime? oHistory);

        [OperationContract]
        ActionMetaData CalculateOffer(
            int underwriterId,
            int customerId,
            int amount,
            bool hasLoans,
            Medal medalClassification
            );

        [OperationContract]
        ActionMetaData CheckAml(int customerId, int userId);

        [OperationContract]
        ActionMetaData CheckAmlCustom(
            int userId,
            int customerId,
            string idhubHouseNumber,
            string idhubHouseName,
            string idhubStreet,
            string idhubDistrict,
            string idhubTown,
            string idhubCounty,
            string idhubPostCode
            );

        [OperationContract]
        ActionMetaData CheckBwa(int customerId, int userId);

        [OperationContract]
        ActionMetaData CheckBwaCustom(
            int userId,
            int customerId,
            string idhubHouseNumber,
            string idhubHouseName,
            string idhubStreet,
            string idhubDistrict,
            string idhubTown,
            string idhubCounty,
            string idhubPostCode,
            string idhubBranchCode,
            string idhubAccountNumber
            );
        
        [OperationContract]
        CrmLookupsActionResult CrmLoadLookups();

        [OperationContract]
        StringActionResult CustomerChangePassword(string sEmail, Password oOldPassword, Password oNewPassword);

        [OperationContract]
        UserLoginActionResult CustomerSignup(
            string sEmail,
            Password oPassword,
            int nPasswordQuestion,
            string sPasswordAnswer,
            string sRemoteIp
        );
        
        [OperationContract]
        ActionMetaData DisplayMarketplaceSecurityData(int nCustomerID);

        [OperationContract]
        IntActionResult EmailConfirmationCheckOne(Guid oToken);

        [OperationContract]
        ActionMetaData EmailConfirmationConfirmUser(int nUserID, int nUnderwriterID);

        [OperationContract]
        EmailConfirmationTokenActionResult EmailConfirmationGenerate(int nUserID);

        [OperationContract]
        ActionMetaData EmailConfirmationGenerateAndSend(int nUserID, int underwriterId);

       [OperationContract]
        ActionMetaData EncryptChannelGrabberMarketplaces();
        
        [OperationContract]
        AccountsToUpdateActionResult FindAccountsToUpdate(int nCustomerID);

        [OperationContract]
        ActionMetaData FinishWizard(FinishWizardArgs oArgs, int underwriterId);

        [OperationContract]
        ActionMetaData FirstOfMonthStatusNotifier();

        [OperationContract]
        ActionMetaData FraudChecker(int customerId, FraudMode mode);

       

       
        [OperationContract]
        NullableDateTimeActionResult GetCompanySeniority(int customerId, bool isLimited, int underwriterId); // TODO: remove

        [OperationContract]
        ConfigTableActionResult GetConfigTable(int nUnderwriterID, string sTableName);

        [OperationContract]
        DecimalActionResult GetCurrentCustomerAnnualTurnover(int customerID);

        
        [OperationContract]
        StringActionResult GetCustomerState(int customerId);

      

        [OperationContract]
        DecimalActionResult GetPricingModelDefaultRate(int customerId, int underwriterId, decimal companyShare);

        [OperationContract]
        PricingModelModelActionResult GetPricingModelModel(int customerId, int underwriterId, string scenarioName);

        [OperationContract]
        StringListActionResult GetPricingModelScenarios(int underwriterId);

        [OperationContract]
        PropertyStatusesActionResult GetPropertyStatuses();

        [OperationContract]
        WizardConfigsActionResult GetWizardConfigs();

        [OperationContract]
        ActionMetaData GetZooplaData(int customerId, bool reCheck);

       

        

        [OperationContract]
        ActionMetaData LateBy14Days();

        [OperationContract]
        CustomerDetailsActionResult LoadCustomerByCreatePasswordToken(Guid oToken);

        [OperationContract]
        StringStringMapActionResult LoadCustomerLeadFieldNames();

      

       

        [OperationContract]
        VatReturnPeriodsActionResult LoadManualVatReturnPeriods(int nCustomerID);

       

        [OperationContract]
        ActionMetaData LoanStatusAfterPayment(int userId, int customerID, string customerEmail, int loanID, decimal paymentAmount, decimal balance, bool isPaidOff, bool sendMail);

        [OperationContract]
        ActionMetaData MarketplaceInstantUpdate(int nMarketplaceID);

        [OperationContract]
        ActionMetaData MarkSessionEnded(int nSessionID, string sComment, int? nCustomerId);

        


       

       

      

       

       

       

        [OperationContract]
        ActionMetaData PayPointCharger();

       

        [OperationContract]
        ActionMetaData PostcodeSaveLog(
            string sRequestType,
            string sUrl,
            string sStatus,
            string sResponseData,
            string sErrorMessage,
            int nUserID
            );

        [OperationContract]
        PricingModelModelActionResult PricingModelCalculate(int customerId, int underwriterId, PricingModelModel model);

        [OperationContract]
        QuickOfferActionResult QuickOffer(int customerId, bool saveOfferToDB);

        [OperationContract]
        QuickOfferActionResult QuickOfferWithPrerequisites(int customerId, bool saveOfferToDB);

      

       

       

        

        [OperationContract]
        ActionMetaData ResetPassword123456(int nUnderwriterID, int nTargetID, PasswordResetTarget nTarget);

        [OperationContract]
        BoolActionResult SaveConfigTable(List<ConfigTable> configTableEntries, ConfigTableType configTableType);

        [OperationContract]
        ActionMetaData SavePricingModelSettings(int underwriterId, string scenarioName, PricingModelModel model);

        [OperationContract]
        ActionMetaData SaveSourceRefHistory(
            int nUserID,
            string sSourceRefList,
            string sVisitTimeList,
            CampaignSourceRef campaignSourceRef
        );

        [OperationContract]
        ActionMetaData SendPendingMails(int underwriterId, int customerId);

       

        

        [OperationContract]
        IntActionResult SetCustomerPasswordByToken(string sEmail, Password oPassword, Guid oToken, bool bIsBrokerLead);

        [OperationContract]
        ActionMetaData SetLateLoanStatus();

        [OperationContract]
        ActionMetaData Temp_BackFillMedals();

       

        [OperationContract]
        ActionMetaData UnderwriterSignup(string name, Password password, string role);

        [OperationContract]
        ActionMetaData UpdateConfigurationVariables(int userId);

        [OperationContract]
        ActionMetaData UpdateCurrencyRates();

        

       

        [OperationContract]
        ActionMetaData UpdateGoogleAnalytics(DateTime? oBackfillStartDate, DateTime? oBackfillEndDate);

        [OperationContract]
        ActionMetaData UpdateMarketplace(int customerId, int marketplaceId, bool doUpdateWizardStep, int userId);

        [OperationContract]
        ActionMetaData UpdateTransactionStatus();

        [OperationContract]
        StringActionResult UserChangeEmail(int underwriterId, int nUserID, string sNewEmail);

        [OperationContract]
        StringActionResult UserChangePassword(
            string sEmail,
            Password oOldPassword,
            Password oNewPassword,
            bool bForceChangePassword
        );

        [OperationContract]
        UserLoginActionResult UserLogin(
            string sEmail,
            Password sPassword,
            string sRemoteIp,
            string promotionName,
            DateTime? promotionPageVisitTime
        );

        [OperationContract]
        StringActionResult UserDisable(
            int userID,
            int customerID,
            string email,
            bool unsubscribeFromMailChimp,
            bool changeEmail
        );

        [OperationContract]
        StringActionResult UserResetPassword(string sEmail);

        [OperationContract]
        StringActionResult UserUpdateSecurityQuestion(string sEmail, Password oPassword, int nQuestionID, string sAnswer);

        

       

        [OperationContract]
        ActionMetaData XDaysDue();

       

       

        [OperationContract]
        ActionMetaData DataSharing(int customerID, AlibabaBusinessType businessType, int? uwID);

       

       

       

        [OperationContract]
        ActionMetaData PayPointAddedWithoutOpenLoan(int customerID, int userID, decimal amount, string paypointTransactionID);

       

        
    } // interface IEzService
}
