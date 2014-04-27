﻿namespace EzService.EzServiceImplementation {
	using System;
	using System.Data;
	using EzBob.Backend.Strategies.Experian;
	using Ezbob.Database;

	partial class EzServiceImplementation {
		public ActionMetaData ExperianCompanyCheck(int nCustomerID, bool bForceCheck) {
			return Execute(nCustomerID, null, typeof(ExperianCompanyCheck), nCustomerID, bForceCheck);
		} // ExperianCompanyCheck

		public ActionMetaData ExperianConsumerCheck(int nCustomerID, int nDirectorID, bool bForceCheck) {
			return Execute(nCustomerID, null, typeof(ExperianConsumerCheck), nCustomerID, nDirectorID, bForceCheck);
		} // ExperianConsumerCheck
		
		public DateTimeActionResult GetExperianConsumerCacheDate(int customerId, int directorId)
		{
			DateTime cacheDate = DateTime.UtcNow;
			try
			{
				DataTable dt = DB.ExecuteReader(
					"GetExperianConsumerCacheDate",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", customerId),
					new QueryParameter("DirectorId", directorId)
				);

				if (dt.Rows.Count > 0)
				{
					var sr = new SafeReader(dt.Rows[0]);
					DateTime tmpCacheDate = sr["LastUpdateDate"];
					if (cacheDate > tmpCacheDate)
					{
						cacheDate = tmpCacheDate;
					}
				}
			}
			catch (Exception e)
			{
				Log.Error("Exception occurred during execution of GetExperianConsumerCacheDate. The exception:{0}", e);
			}

			return new DateTimeActionResult
			{
				Value = cacheDate
			};
		}

		public DateTimeActionResult GetExperianCompanyCacheDate(int customerId)
		{
			DateTime cacheDate = DateTime.UtcNow;
			try
			{
				DataTable dt = DB.ExecuteReader(
					"GetExperianCompanyCacheDate",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", customerId)
				);

				if (dt.Rows.Count > 0)
				{
					var sr = new SafeReader(dt.Rows[0]);
					cacheDate = sr["LastUpdateDate"];
				}
			}
			catch (Exception e)
			{
				Log.Error("Exception occurred during execution of GetExperianCompanyCacheDate. The exception:{0}", e);
			}

			return new DateTimeActionResult
			{
				Value = cacheDate
			};
		}
	} // class EzServiceImplementation
} // namespace EzService
