namespace IovationLib
{
    using System;
    using System.Collections.Generic;
    using Ezbob.Backend.Models;
    using IovationLib.IovationAddAccountEvidenceNS;
    using IovationLib.IovationCheckTransactionDetailsNS;
    using IovationLib.IovationRetractAccountEvidenceNS;
    using log4net;

    public class IovationAppClient {
        public IovationAppClient(string subscriberId, string subscriberAccount, string subscriberPasscode, string adminPassword, string adminAccountName, string environment) {
            this.subscriberId = subscriberId;
            this.subscriberAccount = subscriberAccount;
            this.subscriberPasscode = subscriberPasscode;
            this.adminPassword = adminPassword;
            this.adminAccountName = adminAccountName;

            this.checkTransactionDetailsClient = new IovationLib.IovationCheckTransactionDetailsNS.PortTypeClient("CheckTransactionDetails" + environment);
            this.addAccountEvidenceClient = new IovationLib.IovationAddAccountEvidenceNS.PortTypeClient("AddAccountEvidence" + environment);
            this.getEvidenceDetailsClient = new IovationLib.IovationGetEvidenceDetailsNS.PortTypeClient("GetEvidenceDetails" + environment);
            this.retractAccountEvidenceClient = new IovationLib.IovationRetractAccountEvidenceNS.PortTypeClient("RetractAccountEvidence" + environment);
        }

        public CheckTransactionDetailsResponse CheckTransactionDetails(IovationCheckModel model) {

            model.MobilePhoneNumber = string.IsNullOrEmpty(model.MobilePhoneNumber) ? "" : "+44" + model.MobilePhoneNumber.Substring(1);

            var properties = new List<CheckTransactionDetailsProperty> {
                new CheckTransactionDetailsProperty {
                    name = "Email",
                    value = model.Email
                },
                new CheckTransactionDetailsProperty {
                    name = "MobilePhoneNumber",
                    value = model.MobilePhoneNumber
                },
                new CheckTransactionDetailsProperty {
                    name = "mobilePhoneSmsEnabled",
                    value = model.mobilePhoneSmsEnabled ? "1" : "0"
                },
                new CheckTransactionDetailsProperty {
                    name = "mobilePhoneVerified",
                    value = model.mobilePhoneVerified ? "1" : "0"
                },
                new CheckTransactionDetailsProperty {
                    name = "eventId",
                    value = model.Origin
                }
            };

            if (model.MoreData != null) {
                model.MoreData.HomePhoneNumber = string.IsNullOrEmpty(model.MoreData.HomePhoneNumber) ? "" : "+44" + model.MoreData.HomePhoneNumber.Substring(1);

                string countryCode = "";
                switch (model.MoreData.BillingCountry) {
                    case "England":
                    case "UK":
                    case "Scotland":
                    case "Northern Ireland":
                    case "United Kingdom":
                        countryCode = "GB";
                        break;
                }

                var moreProperties = new List<CheckTransactionDetailsProperty> {
                    new CheckTransactionDetailsProperty {
                        name = "firstName",
                        value = model.MoreData.FirstName
                    },
                    new CheckTransactionDetailsProperty {
                        name = "lastName",
                        value = model.MoreData.LastName
                    },
                    new CheckTransactionDetailsProperty {
                        name = "BillingCity",
                        value = model.MoreData.BillingCity
                    },
                    new CheckTransactionDetailsProperty {
                        name = "BillingCountry",
                        value = countryCode
                    },
                    new CheckTransactionDetailsProperty {
                        name = "BillingPostalCode",
                        value = model.MoreData.BillingPostalCode
                    },
                    new CheckTransactionDetailsProperty {
                        name = "BillingStreet",
                        value = model.MoreData.BillingStreet
                    },
                    new CheckTransactionDetailsProperty {
                        name = "emailVerified",
                        value = model.MoreData.EmailVerified ? "1" : "0"
                    },
                    new CheckTransactionDetailsProperty {
                        name = "HomePhoneNumber",
                        value = model.MoreData.HomePhoneNumber
                    }
                };

                properties.AddRange(moreProperties);
            }
            
            IovationCheckTransactionDetailsNS.CheckTransactionDetails request = new CheckTransactionDetails {
                subscriberaccount = this.subscriberAccount,
                subscriberid = this.subscriberId,
                subscriberpasscode = this.subscriberPasscode,

                accountcode = model.AccountCode,
                beginblackbox = model.BeginBlackBox,
                enduserip = model.EndUserIp,
                txn_properties = properties.ToArray(),
                type = model.Type
            };

           

            try {
                CheckTransactionDetailsResponse response = this.checkTransactionDetailsClient.CheckTransactionDetails(request);
                this.Log.InfoFormat("CheckTransactionDetails result: {0}", response.result);
                return response;
            } catch (Exception ex) {
                this.Log.ErrorFormat("CheckTransactionDetails failed:\n {0}", ex);
                return new CheckTransactionDetailsResponse() {
                    reason = "Exception: " + ex.Message,
                    result = "U"
                };
            }
        }

        public void AddAccountEvidence(string accountCode, string comment, string evidenceType) {
            AddAccountEvidenceResponse response = this.addAccountEvidenceClient.AddAccountEvidence(new AddAccountEvidence {
                subscriberid = this.subscriberId,
                adminaccountname = this.adminAccountName,
                adminpassword = this.adminPassword,
                
                accountcode = accountCode,
                comment = comment,
                evidencetype = evidenceType
            });

            this.Log.InfoFormat("AddAccountEvidence success: {0}", response.success);
        }

        public void GetEvidenceDetails(string accountCode, string deviceAlias) {
            IovationGetEvidenceDetailsNS.evidence_details details = this.getEvidenceDetailsClient.GetEvidenceDetails(
                subscriberid: this.subscriberId, 
                subscriberaccount: this.subscriberAccount, 
                subscriberpasscode: this.subscriberPasscode, 
                accountcode: accountCode, 
                devicealias: deviceAlias);

            foreach (var evidence in details) {
                this.Log.InfoFormat("source {0} type {1}", evidence.source, evidence.type);
            }
        }

        public void RetractAccountEvidence(string accountCode, string comment, string evidenceType) {
            RetractAccountEvidenceResponse response = this.retractAccountEvidenceClient.RetractAccountEvidence(new RetractAccountEvidence {
                subscriberid = this.subscriberId,
                adminaccountname = this.adminAccountName,
                adminpassword = this.adminPassword,

                accountcode = "",
                comment = "",
                evidencetype = ""
            });

            this.Log.InfoFormat("GetEvidenceDetails success: {0}, retracted account: {1}", response.success, response.retractedcount);
        }

        private readonly IovationCheckTransactionDetailsNS.PortTypeClient checkTransactionDetailsClient;
        private readonly IovationAddAccountEvidenceNS.PortTypeClient addAccountEvidenceClient;
        private readonly IovationGetEvidenceDetailsNS.PortTypeClient getEvidenceDetailsClient;
        private readonly IovationRetractAccountEvidenceNS.PortTypeClient retractAccountEvidenceClient;
        protected ILog Log = LogManager.GetLogger(typeof (IovationAppClient));
        private readonly string subscriberId;
        private readonly string subscriberAccount;
        private readonly string subscriberPasscode;
        private readonly string adminPassword;
        private readonly string adminAccountName;
    }
}
