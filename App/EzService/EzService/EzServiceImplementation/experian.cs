namespace EzService.EzServiceImplementation {
	using System;
	using EzBob.Backend.Strategies.Experian;
	using Ezbob.Backend.Models;
	using Ezbob.Database;

	partial class EzServiceImplementation
	{
		#region method BackfillExperianDirectors

		public ActionMetaData BackfillExperianDirectors(int? nCustomerID) {
			return Execute<BackfillExperianDirectors>(nCustomerID, null, nCustomerID);
		} // BackfillExperianDirectors

		#endregion method BackfillExperianDirectors

		#region method ExperianCompanyCheck

		public ActionMetaData ExperianCompanyCheck(int nCustomerID, bool bForceCheck) {
			return Execute(nCustomerID, null, typeof(ExperianCompanyCheck), nCustomerID, bForceCheck);
		} // ExperianCompanyCheck
		
		#endregion method ExperianCompanyCheck

		#region method ExperianConsumerCheck

		public ActionMetaData ExperianConsumerCheck(int nCustomerID, int nDirectorID, bool bForceCheck) {
			return Execute(nCustomerID, null, typeof(ExperianConsumerCheck), nCustomerID, nDirectorID, bForceCheck);
		} // ExperianConsumerCheck

		#endregion method ExperianConsumerCheck

		#region method GetExperianConsumerCacheDate

		public DateTimeActionResult GetExperianConsumerCacheDate(int customerId, int directorId) {
			DateTime cacheDate = DateTime.UtcNow;
			try {
				SafeReader sr = DB.GetFirst(
					"GetExperianConsumerCacheDate",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", customerId),
					new QueryParameter("DirectorId", directorId)
				);

				DateTime tmpCacheDate = sr["LastUpdateDate"];
				if (cacheDate > tmpCacheDate)
					cacheDate = tmpCacheDate;
			}
			catch (Exception e) {
				Log.Error("Exception occurred during execution of GetExperianConsumerCacheDate. The exception:{0}", e);
			}

			return new DateTimeActionResult {
				Value = cacheDate
			};
		}
		#endregion method GetExperianConsumerCacheDate

		#region method GetExperianCompanyCacheDate

		public DateTimeActionResult GetExperianCompanyCacheDate(string refNumber)
		{
			DateTime cacheDate = DateTime.UtcNow;
			try {
				SafeReader sr = DB.GetFirst(
					"GetExperianCompanyCacheDate",
					CommandSpecies.StoredProcedure,
					new QueryParameter("RefNumber", refNumber)
				);

				cacheDate = sr["LastUpdateDate"];
			}
			catch (Exception e) {
				Log.Error("Exception occurred during execution of GetExperianCompanyCacheDate. The exception:{0}", e);
			}

			return new DateTimeActionResult {
				Value = cacheDate
			};
		}
		#endregion method GetExperianCompanyCacheDate

		#region method UpdateExperianDirectorDetails

		public ActionMetaData UpdateExperianDirectorDetails(int? nCustomerID, int? nUnderwriterID, Esigner oDetails) {
			return ExecuteSync<UpdateExperianDirectorDetails>(nCustomerID, nUnderwriterID, oDetails);
		} // UpdateExperianDirectorDetails

		#endregion method UpdateExperianDirectorDetails

		#region method DeleteExperianDirector

		public ActionMetaData DeleteExperianDirector(int nDirectorID, int nUnderwriterID) {
			return ExecuteSync<DeleteExperianDirector>(null, nUnderwriterID, nDirectorID);
		} // DeleteExperianDirector

		#endregion method DeleteExperianDirector

		#region method ParseExperianLtd

		public ExperianLtdActionResult ParseExperianLtd(long nServiceLogID) {
			ParseExperianLtd oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, nServiceLogID);

			return new ExperianLtdActionResult {
				MetaData = oMetaData,
				Value = oInstance.Result,
			};
		} // ParseExperianLtd

		#endregion method ParseExperianLtd

		#region method BackfillExperianLtd

		public ActionMetaData BackfillExperianLtd() {
			return Execute<BackfillExperianLtd>(null, null);
		} // BackfillExperianLtd

		#endregion method BackfillExperianLtd

		#region method LoadExperianLtd

		public ExperianLtdActionResult LoadExperianLtd(long nServiceLogID) {
			LoadExperianLtd oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, (string)null, nServiceLogID);

			return new ExperianLtdActionResult {
				MetaData = oMetaData,
				Value = oInstance.Result,
			};
		} // LoadExperianLtd

		#endregion method LoadExperianLtd

		#region method CheckLtdCompanyCache

		public ExperianLtdActionResult CheckLtdCompanyCache(string sCompanyRefNum) {
			LoadExperianLtd oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, sCompanyRefNum, 0);

			return new ExperianLtdActionResult {
				MetaData = oMetaData,
				Value = oInstance.Result,
			};
		} // CheckLtdCompanyCache

		#endregion method CheckLtdCompanyCache

		#region method ParseExperianConsumer

		public ExperianConsumerActionResult ParseExperianConsumer(long nServiceLogId)
		{
			ParseExperianConsumerData oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, nServiceLogId);

			return new ExperianConsumerActionResult
			{
				MetaData = oMetaData,
				Value = oInstance.Result,
			};

		}

		#endregion method ParseExperianConsumer

		#region method LoadExperianConsumer

		public ExperianConsumerActionResult LoadExperianConsumer(int customerId, int? directorId, long? nServiceLogId)
		{
			LoadExperianConsumerData oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, customerId, null, customerId, directorId, nServiceLogId);

			return new ExperianConsumerActionResult
			{
				MetaData = oMetaData,
				Value = oInstance.Result,
			};
		}

		#endregion method LoadExperianConsumer

		#region method BackfillExperianConsumer

		public ActionMetaData BackfillExperianConsumer()
		{
			return Execute<BackfillExperianConsumer>(null, null);
		}

		#endregion method BackfillExperianConsumer

		public ExperianConsumerMortgageActionResult LoadExperianConsumerMortgageData(int customerId)
		{
			LoadExperianConsumerMortgageData oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, customerId, null, customerId);

			return new ExperianConsumerMortgageActionResult
			{
				MetaData = oMetaData,
				Value = oInstance.Result,
			};
		}

		public IntActionResult GetExperianConsumerScore(int customerId)
		{
			GetExperianConsumerScore oInstance;
			ActionMetaData oMetaData = ExecuteSync(out oInstance, customerId, null, customerId);

			return new IntActionResult
			{
				MetaData = oMetaData,
				Value = oInstance.Score,
			};
		}
	} // class EzServiceImplementation
} // namespace EzService
