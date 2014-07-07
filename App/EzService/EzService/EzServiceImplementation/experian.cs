namespace EzService.EzServiceImplementation {
	using System;
	using System.Data;
	using EzBob.Backend.Strategies.Experian;
	using Ezbob.Backend.Models;
	using Ezbob.Database;

	partial class EzServiceImplementation {
		public ActionMetaData BackfillExperianDirectors(int? nCustomerID) {
			return Execute<BackfillExperianDirectors>(nCustomerID, null, nCustomerID);
		} // BackfillExperianDirectors

		public ActionMetaData ExperianCompanyCheck(int nCustomerID, bool bForceCheck) {
			return Execute(nCustomerID, null, typeof(ExperianCompanyCheck), nCustomerID, bForceCheck);
		} // ExperianCompanyCheck

		public ActionMetaData ExperianConsumerCheck(int nCustomerID, int nDirectorID, bool bForceCheck) {
			return Execute(nCustomerID, null, typeof(ExperianConsumerCheck), nCustomerID, nDirectorID, bForceCheck);
		} // ExperianConsumerCheck

		public DateTimeActionResult GetExperianConsumerCacheDate(int customerId, int directorId) {
			DateTime cacheDate = DateTime.UtcNow;
			try {
				DataTable dt = DB.ExecuteReader(
					"GetExperianConsumerCacheDate",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", customerId),
					new QueryParameter("DirectorId", directorId)
				);

				if (dt.Rows.Count > 0) {
					var sr = new SafeReader(dt.Rows[0]);
					DateTime tmpCacheDate = sr["LastUpdateDate"];
					if (cacheDate > tmpCacheDate) {
						cacheDate = tmpCacheDate;
					}
				}
			}
			catch (Exception e) {
				Log.Error("Exception occurred during execution of GetExperianConsumerCacheDate. The exception:{0}", e);
			}

			return new DateTimeActionResult {
				Value = cacheDate
			};
		}

		public DateTimeActionResult GetExperianCompanyCacheDate(int customerId) {
			DateTime cacheDate = DateTime.UtcNow;
			try {
				DataTable dt = DB.ExecuteReader(
					"GetExperianCompanyCacheDate",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", customerId)
				);

				if (dt.Rows.Count > 0) {
					var sr = new SafeReader(dt.Rows[0]);
					cacheDate = sr["LastUpdateDate"];
				}
			}
			catch (Exception e) {
				Log.Error("Exception occurred during execution of GetExperianCompanyCacheDate. The exception:{0}", e);
			}

			return new DateTimeActionResult {
				Value = cacheDate
			};
		}

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
	} // class EzServiceImplementation
} // namespace EzService
