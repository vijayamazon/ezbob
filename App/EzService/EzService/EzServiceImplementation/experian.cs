namespace EzService.EzServiceImplementation {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using EzBob.Backend.Strategies;
	using Ezbob.Database;
	using Ezbob.Utils;

	partial class EzServiceImplementation {
		public ActionMetaData CheckExperianCompany(int customerId) {
			return Execute(customerId, null, typeof(ExperianCompanyCheck), customerId);
		} // CheckExperianCompany

		public ActionMetaData CheckExperianConsumer(int customerId, int directorId) {
			return Execute(customerId, null, typeof(ExperianConsumerCheck), customerId, directorId);
		} // CheckExperianConsumer

		// TODO: consult Alex - remove ITraversable limitation from CreateTableParameter<T>...
		private class StringWrapper : ITraversable
		{
			public string Value { get; set; }
		}

		public DateTimeActionResult GetExperianCacheDate(List<string> keys)
		{
			var wrappedKeys = keys.Select(key => new StringWrapper {Value = key}).ToList();

			DateTime cacheDate = DateTime.MinValue;
			try
			{
				DataTable dt = DB.ExecuteReader(
					"GetExperianCacheDate",
					CommandSpecies.StoredProcedure,
					DB.CreateTableParameter<StringWrapper>("@Keys", wrappedKeys, stringWrapper =>
					{
						return new object[] { ((StringWrapper)stringWrapper).Value };
					})
				);

				if (dt.Rows.Count > 0)
				{
					var sr = new SafeReader(dt.Rows[0]);
					cacheDate = sr["LastUpdateDate"];
				}
			}
			catch (Exception e)
			{
				Log.Error("Exception occurred during execution of GetExperianCacheDate. The exception:{0}", e);
			}

			return new DateTimeActionResult
			{
				Value = cacheDate
			};
		}
	} // class EzServiceImplementation
} // namespace EzService
