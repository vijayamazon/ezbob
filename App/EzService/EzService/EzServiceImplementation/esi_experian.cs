namespace EzService.EzServiceImplementation {
	using System;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Backend.Strategies.Experian;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Database;

	partial class EzServiceImplementation {
		public ActionMetaData BackFillExperianNonLtdScoreText() {
			return Execute<BackFillExperianNonLtdScoreText>(null, null);
		} // BackFillExperianNonLtdScoreText

		public ActionMetaData BackfillExperianDirectors(int? nCustomerID) {
			return Execute<BackfillExperianDirectors>(nCustomerID, null, nCustomerID);
		} // BackfillExperianDirectors

		public ActionMetaData ExperianCompanyCheck(int userId, int nCustomerID, bool bForceCheck) {
			return Execute<ExperianCompanyCheck>(nCustomerID, userId, nCustomerID, bForceCheck);
		} // ExperianCompanyCheck

		public ActionMetaData ExperianConsumerCheck(int userId, int nCustomerID, int? nDirectorID, bool bForceCheck) {
			return Execute<ExperianConsumerCheck>(nCustomerID, userId, nCustomerID, nDirectorID, bForceCheck);
		} // ExperianConsumerCheck

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

		public ActionMetaData UpdateExperianDirectorDetails(int? nCustomerID, int? nUnderwriterID, Esigner oDetails) {
			return ExecuteSync<UpdateExperianDirectorDetails>(nCustomerID, nUnderwriterID, oDetails);
		} // UpdateExperianDirectorDetails

		public ActionMetaData DeleteExperianDirector(int nDirectorID, int nUnderwriterID) {
			return ExecuteSync<DeleteExperianDirector>(null, nUnderwriterID, nDirectorID);
		} // DeleteExperianDirector

		public ExperianLtdActionResult ParseExperianLtd(long nServiceLogID) {
			ParseExperianLtd oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, nServiceLogID);

			return new ExperianLtdActionResult {
				MetaData = oMetaData,
				Value = oInstance.Result,
			};
		} // ParseExperianLtd

		public ActionMetaData BackfillExperianLtd() {
			return Execute<BackfillExperianLtd>(null, null);
		} // BackfillExperianLtd

        public ActionMetaData BackfillExperianLtdScoreText()
        {
            return Execute<BackfillExperianLtdScoreText>(null, null);
        } // BackfillExperianLtdScoreText

		public ExperianLtdActionResult LoadExperianLtd(long nServiceLogID) {
			LoadExperianLtd oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, (string)null, nServiceLogID);

			return new ExperianLtdActionResult {
				MetaData = oMetaData,
				Value = oInstance.Result,
				History = oInstance.History
			};
		} // LoadExperianLtd

		public ExperianLtdActionResult CheckLtdCompanyCache(int userId, string sCompanyRefNum) {
			LoadExperianLtd oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, userId, sCompanyRefNum, 0);

			return new ExperianLtdActionResult {
				MetaData = oMetaData,
				Value = oInstance.Result,
				History = oInstance.History,
				CompaniesHouse = oInstance.CompaniesHouseResult
			};
		} // CheckLtdCompanyCache

		public ExperianConsumerActionResult ParseExperianConsumer(long nServiceLogId) {
			ParseExperianConsumerData oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, nServiceLogId);

			return new ExperianConsumerActionResult {
				MetaData = oMetaData,
				Value = oInstance.Result,
			};
		}

		public ExperianConsumerActionResult LoadExperianConsumer(int userId, int customerId, int? directorId, long? nServiceLogId) {
			LoadExperianConsumerData oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, customerId, userId, customerId, directorId, nServiceLogId);

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

	    public ExperianTargetingActionResult ExperianTarget(int customerID, int userID, ExperianTargetingRequest request) {
            ExperianTargeting strategyInstance;

            var result = ExecuteSync(out strategyInstance, customerID, userID, request);

            return new ExperianTargetingActionResult
            {
                MetaData = result,
                CompanyInfos = strategyInstance.Response
            };
        }//ExperianTarget

        public ActionMetaData WriteToServiceLog(int customerID, int userID, WriteToLogPackage.InputData packageInputData) {
            return Execute<ServiceLogWriter>(customerID, userID, new WriteToLogPackage(packageInputData));
        }
	} // class EzServiceImplementation
} // namespace EzService
