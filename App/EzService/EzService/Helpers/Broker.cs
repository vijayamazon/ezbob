using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Helpers {
    using Ezbob.Backend.Models;
    using Ezbob.Backend.ModelsWithDB;
    using Ezbob.Backend.Strategies.Broker;
    using Ezbob.Backend.Strategies.MailStrategies;
    using Ezbob.Backend.Strategies.Misc;
    using EzService.ActionResults;
    using EzService.Interfaces;

    /// <summary>
    /// handles broker related actions
    /// </summary>
    internal class Broker : Executor, IEzBroker {
        /// <summary>
        /// Initializes a new instance of the <see cref="Broker"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public Broker(EzServiceInstanceRuntimeData data)
            : base(data) {}

        #region BROKER
        /// <summary>
        /// Determines whether the specified contact email is broker.
        /// </summary>
        /// <param name="contactEmail">The contact email.</param>
        /// <param name="uiOrigin">The UI origin.</param>
        /// <returns></returns>
        public BoolActionResult IsBroker(string contactEmail, int uiOrigin)
        {
            BrokerIsBroker oInstance;

            ActionMetaData oResult = ExecuteSync(out oInstance, null, null, contactEmail, uiOrigin);

            return new BoolActionResult
            {
                MetaData = oResult,
                Value = oInstance.IsBroker,
            };
        }

        /// <summary>
        /// Broker's sign-up.
        /// </summary>
        /// <param name="firmName">Name of the firm.</param>
        /// <param name="firmRegNum">The firm reg number.</param>
        /// <param name="contactName">Name of the contact.</param>
        /// <param name="contactEmail">The contact email.</param>
        /// <param name="contactMobile">The contact mobile.</param>
        /// <param name="mobileCode">The mobile code.</param>
        /// <param name="contactOtherPhone">The contact other phone.</param>
        /// <param name="estimatedMonthlyClientAmount">The estimated monthly client amount.</param>
        /// <param name="password">The password.</param>
        /// <param name="firmWebSiteUrl">The firm web site URL.</param>
        /// <param name="estimatedMonthlyApplicationCount">The estimated monthly application count.</param>
        /// <param name="isCaptchEnabled">if set to <c>true</c> [is captcha enabled].</param>
        /// <param name="brokerTermsID">The broker terms identifier.</param>
        /// <param name="referredBy">The referred by.</param>
        /// <param name="FCARegistered">if set to <c>true</c> [FCA registered].</param>
        /// <param name="licenseNumber">The license number.</param>
        /// <param name="uiOriginID">The UI origin identifier.</param>
        /// <returns></returns>
        public BrokerPropertiesActionResult BrokerSignup(
            string firmName,
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
            )
        {
            BrokerSignup brokerSignup;

            ActionMetaData actionMetaData = ExecuteSync(out brokerSignup, null, null,
                firmName,
                firmRegNum,
                contactName,
                contactEmail,
                contactMobile,
                mobileCode,
                contactOtherPhone,
                estimatedMonthlyClientAmount,
                password,
                firmWebSiteUrl,
                estimatedMonthlyApplicationCount,
                isCaptchEnabled,
                brokerTermsID,
                referredBy,
                FCARegistered,
                licenseNumber,
                uiOriginID
                );

            if (brokerSignup != null)
            {
                if (brokerSignup.Properties.BrokerID > 0)
                    Execute<BrokerGreeting>(null, brokerSignup.Properties.BrokerID, brokerSignup.Properties.BrokerID);

                return new BrokerPropertiesActionResult
                {
                    MetaData = actionMetaData,
                    Properties = brokerSignup.Properties,
                };
            }

            return new BrokerPropertiesActionResult
            {
                MetaData = actionMetaData,
                Properties = new BrokerProperties()
            };
        }

        /// <summary>
        /// login.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="password">The password.</param>
        /// <param name="promotionName">Name of the promotion.</param>
        /// <param name="promotionPageVisitTime">The promotion page visit time.</param>
        /// <param name="uiOriginID">The UI origin identifier.</param>
        /// <returns></returns>
        public BrokerPropertiesActionResult BrokerLogin(
            string email,
            Password password,
            string promotionName,
            DateTime? promotionPageVisitTime,
            int uiOriginID
            )
        {
            BrokerLogin brokerLogin;
            ActionMetaData metaData = ExecuteSync(
                out brokerLogin,
                null,
                null,
                email,
                password,
                promotionName,
                promotionPageVisitTime,
                uiOriginID
                );

            return new BrokerPropertiesActionResult
            {
                MetaData = metaData,
                Properties = brokerLogin.Properties
            };
        }

        /// <summary>
        /// restore password.
        /// </summary>
        /// <param name="mobile">The mobile.</param>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public ActionMetaData BrokerRestorePassword(string mobile, string code)
        {
            return ExecuteSync<BrokerRestorePassword>(null, null, mobile, code);
        }

        /// <summary>
        /// accept terms.
        /// </summary>
        /// <param name="termsID">The terms identifier.</param>
        /// <param name="contactEmail">The contact email.</param>
        /// <returns></returns>
        public ActionMetaData BrokerAcceptTerms(int termsID, string contactEmail)
        {
            return Execute<BrokerAcceptTerms>(null, null, termsID, contactEmail);
        }

        /// <summary>
        /// load own properties.
        /// </summary>
        /// <param name="contactEmail">The contact email.</param>
        /// <returns></returns>
        public BrokerPropertiesActionResult BrokerLoadOwnProperties(string contactEmail)
        {
            BrokerLoadOwnProperties brokerLoadOwnProperties;

            ActionMetaData metaData = ExecuteSync(out brokerLoadOwnProperties, null, null, contactEmail, 0);

            return new BrokerPropertiesActionResult
            {
                MetaData = metaData,
                Properties = brokerLoadOwnProperties.Properties,
            };
        }

        /// <summary>
        /// load properties by identifier.
        /// </summary>
        /// <param name="brokerID">The broker identifier.</param>
        /// <returns></returns>
        public BrokerPropertiesActionResult BrokerLoadPropertiesByID(int brokerID)
        {
            BrokerLoadOwnProperties brokerLoadOwnProperties;

            ActionMetaData metaData = ExecuteSync(out brokerLoadOwnProperties, null, null, string.Empty, brokerID);

            return new BrokerPropertiesActionResult
            {
                MetaData = metaData,
                Properties = brokerLoadOwnProperties.Properties,
            };
        }

        /// <summary>
        /// update password.
        /// </summary>
        /// <param name="contactEmail">The contact email.</param>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <returns></returns>
        public ActionMetaData BrokerUpdatePassword(string contactEmail, Password oldPassword, Password newPassword)
        {
            BrokerUpdatePassword brokerUpdatePassword;

            ActionMetaData metaData = ExecuteSync(out brokerUpdatePassword, null, null, contactEmail, oldPassword, newPassword);

            if ((brokerUpdatePassword != null) && (brokerUpdatePassword.BrokerID > 0))
                Execute<BrokerPasswordChanged>(null, brokerUpdatePassword.BrokerID, brokerUpdatePassword.BrokerID, newPassword);

            return metaData;
        }

        /// <summary>
        /// load static data.
        /// </summary>
        /// <param name="loadFilesOnly">if set to <c>true</c> [load files only].</param>
        /// <param name="originID">The origin identifier.</param>
        /// <returns></returns>
        public BrokerStaticDataActionResult BrokerLoadStaticData(bool loadFilesOnly, int originID)
        {
            var result = new BrokerStaticDataActionResult
            {
                MaxPerNumber = 3,
                MaxPerPage = 10,
                Files = new FileDescription[0],
                Terms = "",
                TermsID = 0,
            };

            try
            {
                BrokerLoadMarketingFiles brokerLoadMarketingFiles;

                ActionMetaData metaData = ExecuteSync(out brokerLoadMarketingFiles, null, null, originID);

                if (metaData.Status == ActionStatus.Done)
                    result.Files = brokerLoadMarketingFiles.Files.ToArray();
            }
            catch (Exception e)
            {
                Log.Alert(e, "Failed to retrieve marketing files.");
            }
            if (loadFilesOnly)
                return result;

            try
            {
                BrokerLoadCurrentTerms brokerLoadCurrentTerms;

                ActionMetaData metaData = ExecuteSync(out brokerLoadCurrentTerms, null, null, originID);

                if (metaData.Status == ActionStatus.Done)
                {
                    result.Terms = brokerLoadCurrentTerms.Terms;
                    result.TermsID = brokerLoadCurrentTerms.ID;
                }
            }
            catch (Exception e)
            {
                Log.Alert(e, "Failed to retrieve terms.");
            }

            try
            {
                BrokerLoadSmsCount brokerLoadSmsCount;

                ActionMetaData metaData = ExecuteSync(out brokerLoadSmsCount, null, null);

                if (metaData.Status == ActionStatus.Done)
                {
                    result.MaxPerNumber = brokerLoadSmsCount.MaxPerNumber;
                    result.MaxPerPage = brokerLoadSmsCount.MaxPerPage;
                }
            }
            catch (Exception e)
            {
                Log.Alert(e, "Failed to retrieve SMS counts.");
            }

            try
            {
                CrmLoadLookups crmLoadLookups;

                ActionMetaData metaData = ExecuteSync(out crmLoadLookups, null, null);

                if (metaData.Status == ActionStatus.Done)
                {
                    result.Crm = new CrmStaticModel
                    {
                        CrmActions = crmLoadLookups.Actions,
                        CrmRanks = crmLoadLookups.Ranks,
                        CrmStatuses = crmLoadLookups.Statuses.Where(s => !s.IsBroker.HasValue || !s.IsBroker.Value)
                            .ToList(),
                    };
                }
            }
            catch (Exception e)
            {
                Log.Alert(e, "Failed to retrieve SMS counts.");
            }

            return result;
        }

        /// <summary>
        /// load signed terms.
        /// </summary>
        /// <param name="contactEmail">The contact email.</param>
        /// <returns></returns>
        public StringListActionResult BrokerLoadSignedTerms(string contactEmail)
        {
            BrokerLoadSignedTerms brokerLoadSignedTerms;

            ActionMetaData metaData = ExecuteSync(out brokerLoadSignedTerms, null, null, contactEmail);

            return new StringListActionResult
            {
                MetaData = metaData,
                Records = new List<string> {
                    brokerLoadSignedTerms.Terms,
                    brokerLoadSignedTerms.SignedTime
                }
            };
        }

        /// <summary>
        /// add bank.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public ActionMetaData BrokerAddBank(BrokerAddBankModel model)
        {
            return ExecuteSync<BrokerAddBank>(null, null, model);
        }

        /// <summary>
        /// Changes the broker email.
        /// </summary>
        /// <param name="oldEmail">The old email.</param>
        /// <param name="newEmail">The new email.</param>
        /// <param name="newPassword">The new password.</param>
        /// <returns></returns>
        public ActionMetaData ChangeBrokerEmail(string oldEmail, string newEmail, string newPassword)
        {
            ChangeBrokerEmail instance;
            return ExecuteSync(out instance, 0, 0, oldEmail, newEmail, newPassword);
        }

        /// <summary>
        /// instant offer.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public BrokerInstantOfferResponseActionResult BrokerInstantOffer(BrokerInstantOfferRequest request)
        {
            BrokerInstantOffer brokerInstantOffer;

            ActionMetaData metaData = ExecuteSync(out brokerInstantOffer, null, request.BrokerId, request);

            return new BrokerInstantOfferResponseActionResult
            {
                MetaData = metaData,
                Response = brokerInstantOffer.Response,
            };
        }

        /// <summary>
        /// transfer commission.
        /// </summary>
        /// <returns></returns>
        public ActionMetaData BrokerTransferCommission()
        {
            return Execute<BrokerTransferCommission>(null, null);
        } 
        #endregion

        #region CUSTOMER

        /// <summary>
        /// customer wizard complete.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <returns></returns>
        public ActionMetaData BrokerCustomerWizardComplete(int customerID) {
            return Execute<BrokerCustomerWizardComplete>(customerID, null, customerID);
        }

        /// <summary>
        /// check customer relevance.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="customerEmail">The customer email.</param>
        /// <param name="isAlibaba">if set to <c>true</c> [is alibaba].</param>
        /// <param name="sourceRef">The source reference.</param>
        /// <param name="confirmEmailLink">The confirm email link.</param>
        /// <returns></returns>
        public ActionMetaData BrokerCheckCustomerRelevance(int customerID, string customerEmail, bool isAlibaba, string sourceRef, string confirmEmailLink) {
            return Execute<BrokerCheckCustomerRelevance>(customerID, customerID, customerID, customerEmail, isAlibaba, sourceRef, confirmEmailLink);
        }

        /// <summary>
        /// approve and reset customer password.
        /// </summary>
        /// <param name="underwriterID">The underwriter identifier.</param>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="loanAmount">The loan amount.</param>
        /// <param name="validHours">The valid hours.</param>
        /// <param name="isFirst">if set to <c>true</c> [is first].</param>
        /// <returns></returns>
        public ActionMetaData BrokerApproveAndResetCustomerPassword(int underwriterID, int customerID, decimal loanAmount, int validHours, bool isFirst) {
            return Execute(new ExecuteArguments(customerID, loanAmount, validHours, isFirst) {
                StrategyType = typeof(ApprovedUser),
                CustomerID = customerID,
                UserID = underwriterID,
                OnInit = (stra, amd) => { ((ApprovedUser)stra).SendToCustomer = false; },
            });
        }

        /// <summary>
        /// load customers by identifier.
        /// </summary>
        /// <param name="brokerID">The broker identifier.</param>
        /// <returns></returns>
        public BrokerCustomersActionResult BrokerLoadCustomersByID(int brokerID) {
            BrokerLoadCustomerList brokerLoadCustomerList;

            ActionMetaData result = ExecuteSync(out brokerLoadCustomerList, null, null, string.Empty, brokerID);

            return new BrokerCustomersActionResult {
                MetaData = result,
                Customers = brokerLoadCustomerList.Customers,
            };
        }

        /// <summary>
        /// load customer list.
        /// </summary>
        /// <param name="contactEmail">The contact email.</param>
        /// <returns></returns>
        public BrokerCustomersActionResult BrokerLoadCustomerList(string contactEmail) {
            BrokerLoadCustomerList brokerLoadCustomerList;

            ActionMetaData result = ExecuteSync(out brokerLoadCustomerList, null, null, contactEmail, 0);

            return new BrokerCustomersActionResult {
                MetaData = result,
                Customers = brokerLoadCustomerList.Customers,
            };
        }

        /// <summary>
        /// load customer details.
        /// </summary>
        /// <param name="customerRefNum">The customer reference number.</param>
        /// <param name="contactEmail">The contact email.</param>
        /// <returns></returns>
        public BrokerCustomerDetailsActionResult BrokerLoadCustomerDetails(string customerRefNum, string contactEmail) {
            BrokerLoadCustomerDetails brokerLoadCustomerDetails;

            ActionMetaData result = ExecuteSync(out brokerLoadCustomerDetails, null, null, customerRefNum, contactEmail);

            return new BrokerCustomerDetailsActionResult {
                MetaData = result,
                Data = brokerLoadCustomerDetails.Result,
                PotentialSigners = brokerLoadCustomerDetails.PotentialEsigners,
            };
        }

        /// <summary>
        /// save CRM entry.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="actionID">The action identifier.</param>
        /// <param name="statusID">The status identifier.</param>
        /// <param name="comment">The comment.</param>
        /// <param name="customerRefNum">The customer reference number.</param>
        /// <param name="contactEmail">The contact email.</param>
        /// <returns></returns>
        public StringActionResult BrokerSaveCrmEntry(string type, int actionID, int statusID, string comment, string customerRefNum, string contactEmail) {
            BrokerSaveCrmEntry brokerSaveCrmEntry;

            ActionMetaData result = ExecuteSync(
                out brokerSaveCrmEntry,
                null,
                null,
                type,
                actionID,
                statusID,
                comment,
                customerRefNum,
                contactEmail
                );

            return new StringActionResult {
                MetaData = result,
                Value = brokerSaveCrmEntry.ErrorMsg,
            };
        }

        /// <summary>
        /// load customer files.
        /// </summary>
        /// <param name="customerRefNum">The customer reference number.</param>
        /// <param name="contactEmail">The contact email.</param>
        /// <returns></returns>
        public BrokerCustomerFilesActionResult BrokerLoadCustomerFiles(string customerRefNum, string contactEmail) {
            BrokerLoadCustomerFiles brokerLoadCustomerFiles;

            ActionMetaData result = ExecuteSync(
                out brokerLoadCustomerFiles,
                null,
                null,
                customerRefNum,
                contactEmail
                );

            return new BrokerCustomerFilesActionResult {
                MetaData = result,
                Files = brokerLoadCustomerFiles.Files,
            };
        }

        /// <summary>
        /// download customer file.
        /// </summary>
        /// <param name="customerRefNum">The customer reference number.</param>
        /// <param name="contactEmail">The contact email.</param>
        /// <param name="fileID">The file identifier.</param>
        /// <returns></returns>
        public BrokerCustomerFileContentsActionResult BrokerDownloadCustomerFile(string customerRefNum, string contactEmail, int fileID) {
            BrokerDownloadCustomerFile brokerDownloadCustomerFile;

            ActionMetaData result = ExecuteSync(
                out brokerDownloadCustomerFile,
                null,
                null,
                customerRefNum,
                contactEmail,
                fileID
                );

            return new BrokerCustomerFileContentsActionResult {
                MetaData = result,
                Name = brokerDownloadCustomerFile.FileName,
                Contents = brokerDownloadCustomerFile.Contents,
            };
        }

        /// <summary>
        /// save uploaded customer file.
        /// </summary>
        /// <param name="customerRefNum">The customer reference number.</param>
        /// <param name="contactEmail">The contact email.</param>
        /// <param name="fileContents">The file contents.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public ActionMetaData BrokerSaveUploadedCustomerFile(string customerRefNum, string contactEmail, byte[] fileContents, string fileName) {
            return ExecuteSync<BrokerSaveUploadedCustomerFile>(null, null, customerRefNum, contactEmail, fileContents, fileName);
        }

        /// <summary>
        /// delete customer files.
        /// </summary>
        /// <param name="customerRefNum">The customer reference number.</param>
        /// <param name="contactEmail">The contact email.</param>
        /// <param name="aryFileIDs">The ary file ids.</param>
        /// <returns></returns>
        public ActionMetaData BrokerDeleteCustomerFiles(string customerRefNum, string contactEmail, int[] aryFileIDs) {
            return ExecuteSync<BrokerDeleteCustomerFiles>(null, null, customerRefNum, contactEmail, aryFileIDs);
        }

        /// <summary>
        /// attach customer.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="brokerID">The broker identifier.</param>
        /// <param name="underwriterID">The underwriter identifier.</param>
        /// <returns></returns>
        public ActionMetaData BrokerAttachCustomer(int customerID, int? brokerID, int underwriterID) {
            return ExecuteSync<BrokerAttachCustomer>(customerID, underwriterID, customerID, brokerID, underwriterID);
        }

        /// <summary>
        /// force reset customer password email.
        /// </summary>
        /// <param name="userID">The user identifier.</param>
        /// <param name="customerID">The customer identifier.</param>
        /// <returns></returns>
        public ActionMetaData BrokerForceResetCustomerPasswordEmail(int userID, int customerID) {
            return Execute<BrokerForceResetCustomerPassword>(customerID, userID, customerID);
        }

        #endregion

        #region LEAD

        /// <summary>
        /// add customer lead.
        /// </summary>
        /// <param name="leadFirstName">First name of the lead.</param>
        /// <param name="leadLastName">Last name of the lead.</param>
        /// <param name="leadEmail">The lead email.</param>
        /// <param name="leadAddMode">The lead add mode.</param>
        /// <param name="contactEmail">The contact email.</param>
        /// <returns></returns>
        public ActionMetaData BrokerAddCustomerLead(string leadFirstName, string leadLastName, string leadEmail, string leadAddMode, string contactEmail) {
            return ExecuteSync<BrokerAddCustomerLead>(null, null, leadFirstName, leadLastName, leadEmail, leadAddMode, contactEmail);
        }

        /// <summary>
        /// lead acquire customer.
        /// </summary>
        /// <param name="customerID">The customer identifier.</param>
        /// <param name="leadID">The lead identifier.</param>
        /// <param name="firstName">The first name.</param>
        /// <param name="brokerFillsForCustomer">if set to <c>true</c> [broker fills for customer].</param>
        /// <param name="confirmationToken">The confirmation token.</param>
        /// <returns></returns>
        public ActionMetaData BrokerLeadAcquireCustomer(int customerID, int leadID, string firstName, bool brokerFillsForCustomer, string confirmationToken) {
            return Execute<BrokerLeadAcquireCustomer>(customerID, null, customerID, leadID, firstName, brokerFillsForCustomer, confirmationToken);
        }

        /// <summary>
        /// load lead details.
        /// </summary>
        /// <param name="leadID">The lead identifier.</param>
        /// <param name="contactEmail">The contact email.</param>
        /// <returns></returns>
        public BrokerLeadDetailsDataActionResult BrokerLoadLeadDetails(int leadID, string contactEmail) {
            BrokerLoadLeadDetails brokerLoadLeadDetails;

            ActionMetaData result = ExecuteSync(out brokerLoadLeadDetails, null, null, leadID, contactEmail);

            return new BrokerLeadDetailsDataActionResult {
                MetaData = result,
                BrokerLeadDataModel = brokerLoadLeadDetails.Result
            };
        }

        /// <summary>
        /// lead can fill wizard.
        /// </summary>
        /// <param name="leadID">The lead identifier.</param>
        /// <param name="leadEmail">The lead email.</param>
        /// <param name="contactEmail">The contact email.</param>
        /// <returns></returns>
        public BrokerLeadDetailsActionResult BrokerLeadCanFillWizard(int leadID, string leadEmail, string contactEmail) {
            BrokerLeadCanFillWizard brokerLeadCanFillWizard;

            ActionMetaData result = ExecuteSync(
                out brokerLeadCanFillWizard,
                null,
                null,
                leadID,
                leadEmail,
                contactEmail
                );

            return new BrokerLeadDetailsActionResult {
                LeadID = brokerLeadCanFillWizard.LeadID,
                CustomerID = brokerLeadCanFillWizard.CustomerID,
                LeadEmail = brokerLeadCanFillWizard.LeadEmail,
                FirstName = brokerLeadCanFillWizard.FirstName,
                LastName = brokerLeadCanFillWizard.LastName,
                MetaData = result
            };
        }

        /// <summary>
        /// back from customer wizard.
        /// </summary>
        /// <param name="leadID">The lead identifier.</param>
        /// <returns></returns>
        public StringActionResult BrokerBackFromCustomerWizard(int leadID) {
            BrokerBackFromCustomerWizard brokerBackFromCustomerWizard;

            ActionMetaData oResult = ExecuteSync(out brokerBackFromCustomerWizard, null, null, leadID);

            return new StringActionResult {
                Value = brokerBackFromCustomerWizard.ContactEmail,
                MetaData = oResult
            };
        }

        /// <summary>
        /// lead check token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public BrokerLeadDetailsActionResult BrokerLeadCheckToken(string token) {
            BrokerLeadCheckToken brokerLeadCheckToken;

            ActionMetaData result = ExecuteSync(out brokerLeadCheckToken, null, null, token);

            return new BrokerLeadDetailsActionResult {
                LeadID = brokerLeadCheckToken.LeadID,
                CustomerID = brokerLeadCheckToken.CustomerID,
                LeadEmail = brokerLeadCheckToken.LeadEmail,
                FirstName = brokerLeadCheckToken.FirstName,
                LastName = brokerLeadCheckToken.LastName,
                MetaData = result
            };
        }

        /// <summary>
        /// lead send invitation email.
        /// </summary>
        /// <param name="leadID">The lead identifier.</param>
        /// <param name="brokerContactEmail">The broker contact email.</param>
        /// <returns></returns>
        public ActionMetaData BrokerLeadSendInvitationEmail(int leadID, string brokerContactEmail) {
            return ExecuteSync<BrokerLeadSendInvitation>(null, null, leadID, brokerContactEmail);
        }

        #endregion
    }
}
