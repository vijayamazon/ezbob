using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzService.Helpers {
    using Ezbob.Backend.Models;
    using Ezbob.Backend.ModelsWithDB;
    using Ezbob.Backend.Strategies.Experian;
    using Ezbob.Backend.Strategies.Misc;
    using Ezbob.Database;
    using EzService.Interfaces;

    internal class Experian : Executor, IEzExperian {
        public Experian(EzServiceInstanceRuntimeData data)
            : base(data) {}

        public ActionMetaData BackfillCustomerAnalyticsCompany() {
            return Execute<BackfillCustomerAnalyticsCompany>(null, null);
        }

        public ActionMetaData BackFillExperianNonLtdScoreText() {
            return Execute<BackFillExperianNonLtdScoreText>(null, null);
        }

        public ActionMetaData BackfillExperianDirectors(int? customerID) {
            return Execute<BackfillExperianDirectors>(customerID, null, customerID);
        }

        public ActionMetaData ExperianCompanyCheck(int userId, int customerID, bool forceCheck) {
            return Execute<ExperianCompanyCheck>(customerID, userId, customerID, forceCheck);
        }

        public ActionMetaData ExperianConsumerCheck(int userId, int nCustomerID, int? nDirectorID, bool forceCheck) {
            return Execute<ExperianConsumerCheck>(nCustomerID, userId, nCustomerID, nDirectorID, forceCheck);
        }

        public DateTimeActionResult GetExperianCompanyCacheDate(int userId, string refNumber) {
            DateTime cacheDate = DateTime.UtcNow;
            try {
                SafeReader sr = DB.GetFirst(
                    "GetExperianCompanyCacheDate",
                    CommandSpecies.StoredProcedure,
                    new QueryParameter("RefNumber", refNumber)
                    );

                cacheDate = sr["LastUpdateDate"];
            } catch (Exception e) {
                Log.Error(e, "Exception occurred during execution of GetExperianCompanyCacheDate. userId {0} refNumber {1}", userId, refNumber);
            }

            return new DateTimeActionResult {
                Value = cacheDate
            };
        }

        public ActionMetaData UpdateExperianDirectorDetails(int? customerID, int? underwriterID, Esigner details) {
            return ExecuteSync<UpdateExperianDirectorDetails>(customerID, underwriterID, details);
        }

        public ActionMetaData DeleteExperianDirector(int directorID, int underwriterID) {
            return ExecuteSync<DeleteExperianDirector>(null, underwriterID, directorID);
        }

        public ExperianLtdActionResult ParseExperianLtd(long serviceLogID) {
            ParseExperianLtd oInstance;

            ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, serviceLogID);

            return new ExperianLtdActionResult {
                MetaData = oMetaData,
                Value = oInstance.Result,
            };
        }

        public ActionMetaData BackfillExperianLtd() {
            return Execute<BackfillExperianLtd>(null, null);
        }

        public ActionMetaData BackfillExperianLtdScoreText() {
            return Execute<BackfillExperianLtdScoreText>(null, null);
        }

        public ExperianLtdActionResult LoadExperianLtd(long serviceLogID) {
            LoadExperianLtd oInstance;

            ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, (string)null, serviceLogID);

            return new ExperianLtdActionResult {
                MetaData = oMetaData,
                Value = oInstance.Result,
                History = oInstance.History
            };
        }

        public ExperianLtdActionResult CheckLtdCompanyCache(int userId, string companyRefNum) {
            LoadExperianLtd oInstance;

            ActionMetaData oMetaData = ExecuteSync(out oInstance, null, userId, companyRefNum, 0);

            return new ExperianLtdActionResult {
                MetaData = oMetaData,
                Value = oInstance.Result,
                History = oInstance.History
            };
        }

        public ExperianConsumerActionResult ParseExperianConsumer(long serviceLogId) {
            ParseExperianConsumerData oInstance;

            ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, serviceLogId);

            return new ExperianConsumerActionResult {
                MetaData = oMetaData,
                Value = oInstance.Result,
            };
        }

        public ExperianConsumerActionResult LoadExperianConsumer(int userId, int customerId, int? directorId, long? serviceLogId) {
            LoadExperianConsumerData oInstance;

            ActionMetaData oMetaData = ExecuteSync(out oInstance, customerId, userId, customerId, directorId, serviceLogId);

            return new ExperianConsumerActionResult {
                MetaData = oMetaData,
                Value = oInstance.Result,
            };
        }

        public ActionMetaData BackfillExperianConsumer() {
            return Execute<BackfillExperianConsumer>(null, null);
        }

        public ExperianConsumerMortgageActionResult LoadExperianConsumerMortgageData(int userId, int customerId) {
            LoadExperianConsumerMortgageData oInstance;

            ActionMetaData oMetaData = ExecuteSync(out oInstance, customerId, userId, customerId);

            return new ExperianConsumerMortgageActionResult {
                MetaData = oMetaData,
                Value = oInstance.Result,
            };
        }

        public IntActionResult GetExperianConsumerScore(int customerId) {
            GetExperianConsumerScore oInstance;
            ActionMetaData oMetaData = ExecuteSync(out oInstance, customerId, null, customerId);

            return new IntActionResult {
                MetaData = oMetaData,
                Value = oInstance.Score,
            };
        }

        public CompanyDataForCompanyScoreActionResult GetCompanyDataForCompanyScore(int underwriterId, string refNumber) {
            GetCompanyDataForCompanyScore strategyInstance;
            var result = ExecuteSync(out strategyInstance, null, underwriterId, refNumber);

            return new CompanyDataForCompanyScoreActionResult {
                MetaData = result,
                Data = strategyInstance.Data
            };
        }

        public CompanyDataForCreditBureauActionResult GetCompanyDataForCreditBureau(int underwriterId, string refNumber) {
            GetCompanyDataForCreditBureau strategyInstance;

            var result = ExecuteSync(out strategyInstance, null, underwriterId, refNumber);

            return new CompanyDataForCreditBureauActionResult {
                MetaData = result,
                Result = new CompanyDataForCreditBureau {
                    LastUpdate = strategyInstance.LastUpdate,
                    Score = strategyInstance.Score,
                    Errors = strategyInstance.Errors
                }
            };
        }

        public CompanyCaisDataActionResult GetCompanyCaisDataForAlerts(int underwriterId, int customerId) {
            GetCompanyCaisDataForAlerts strategyInstance;

            var result = ExecuteSync(out strategyInstance, customerId, underwriterId, customerId);

            return new CompanyCaisDataActionResult {
                MetaData = result,
                Accounts = strategyInstance.Accounts,
                NumOfCurrentDefaultAccounts = strategyInstance.NumOfCurrentDefaultAccounts,
                NumOfSettledDefaultAccounts = strategyInstance.NumOfSettledDefaultAccounts
            };
        }

        public ActionMetaData UpdateCustomerAnalyticsOnCompanyChange(int customerID) {
            return Execute<UpdateCustomerAnalyticsOnCompanyChange>(customerID, null, customerID);
        }

        public ExperianTargetingActionResult ExperianTarget(int customerID, int userID, ExperianTargetingRequest request) {
            ExperianTargeting strategyInstance;

            var result = ExecuteSync(out strategyInstance, customerID, userID, request);

            return new ExperianTargetingActionResult {
                MetaData = result,
                CompanyInfos = strategyInstance.Response
            };
        }

        public ActionMetaData WriteToServiceLog(int customerID, int userID, WriteToLogPackage.InputData packageInputData) {
            return Execute<ServiceLogWriter>(customerID, userID, new WriteToLogPackage(packageInputData));
        }

        public IntActionResult GetExperianAccountsCurrentBalance(int customerId, int underwriterId) {
            GetExperianAccountsCurrentBalance instance;

            ActionMetaData result = ExecuteSync(out instance, customerId, underwriterId, customerId);

            return new IntActionResult {
                MetaData = result,
                Value = instance.CurrentBalance
            };
        }
    }
}
