namespace EzBob.Backend.Strategies.Misc 
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using ConfigManager;
	using EzBob.Backend.Strategies.Experian;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using ExperianLib;
	using EzBobIntegration.Web_References.Consumer;
	using StoredProcs;

	public class BackfillConsumerAnalytics : AStrategy
	{
		public BackfillConsumerAnalytics(AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
		}

		public override string Name {
			get { return "BackfillConsumerAnalytics"; }
		}

		public override void Execute()
		{
			DataTable dt = DB.ExecuteReader(
				"GetAllConsumersForBackfill",
				CommandSpecies.StoredProcedure
				);

			int customerId = 0;
			int directorId = 0;
			foreach (DataRow row in dt.Rows)
			{
				try
				{
					var sr = new SafeReader(row);
					customerId = sr["CustomerId"];
					directorId = sr["DirectorId"];
					string firstName = sr["FirstName"];
					string surname = sr["Surname"];
					string gender = sr["Gender"];
					DateTime dateOfBirth = sr["DateOfBirth"];

					Log.Debug("Backfilling customer consumer analytics for customer {0} and director '{1}'...", customerId, directorId);

					var addressLines = new GetCustomerAddresses(customerId, DB, Log).FillFirst<GetCustomerAddresses.ResultRow>();
					InputLocationDetailsMultiLineLocation location = addressLines.GetLocation(AddressCurrency.Current);

					var consumerService = new ConsumerService();
					var result = consumerService.GetConsumerInfo(
						firstName,
						surname,
						gender,
						dateOfBirth,
						null,
						location,
						"PL",
						customerId,
						directorId,
						true,
						directorId != 0,
						false
					);

					if (result == null || result.HasExperianError)
					{
						Log.Debug("Backfilling customer analytics for customer {0} and director {1} Failed!", customerId, directorId);
						continue;
					}

					int? score = result.BureauScore;

					if (directorId == 0)
					{
						UpdateCustomerAnalytics(result, customerId, score);
					}
					else
					{
						UpdateDirectorAnalytics(customerId, directorId, score);
					}

					Log.Debug("Backfilling customer analytics for customer {0} and director {1} complete.", customerId, directorId);
				}
				catch (Exception ex)
				{
					Log.Error("The backfill for customer:{0} and director {1} failed with exception:{2}", customerId, directorId, ex);
				}
			}
		}

		private void UpdateDirectorAnalytics(int customerId, int directorId, int? score)
		{
			Log.Debug("Updating customer analytics director score (customer = {0}, director = {1}, score = {2})", customerId, directorId, score);

			DB.ExecuteNonQuery(
				"CustomerAnalyticsUpdateDirector",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", customerId),
				new QueryParameter("Score", score),
				new QueryParameter("AnalyticsDate", DateTime.UtcNow)
			);
		}

		private void UpdateCustomerAnalytics(ExperianConsumerData data, int customerId, int? score)
		{
			int? nCii = data.CII;
			
			int nNumOfAccounts = 0;
			int nDefaultCount = 0;
			int nLastDefaultCount = 0;

			List<ExperianConsumerDataCais> cais = data.Cais ?? new List<ExperianConsumerDataCais>();
			

			var dThen = DateTime.UtcNow.AddYears(-CurrentValues.Instance.CustomerAnalyticsDefaultHistoryYears);

			foreach (var detail in cais)
			{
					nNumOfAccounts++;

					if (detail.AccountStatus != "F")
						continue;

					nDefaultCount++;

					var settlementDate = detail.LastUpdatedDate ?? detail.SettlementDate;

					if (settlementDate >= dThen)
						nLastDefaultCount++;
			} // for each CAIS datum in CAIS data

			Log.Debug(
				"Updating customer analytics (customer = {0}, " +
				"score = {1}, " +
				"indebtedness index = {2}, " +
				"# accounts = {3}, " +
				"# defaults = {4}, " +
				"# defaults = {5} since {6}" +
				")",
				customerId,
				score,
				nCii,
				nNumOfAccounts,
				nDefaultCount,
				nLastDefaultCount,
				dThen.ToString("MMMM d yyyy", CultureInfo.InvariantCulture)
			);

			DB.ExecuteNonQuery(
				"CustomerAnalyticsUpdate",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", customerId),
				new QueryParameter("Score", score),
				new QueryParameter("IndebtednessIndex", nCii),
				new QueryParameter("NumOfAccounts", nNumOfAccounts),
				new QueryParameter("NumOfDefaults", nDefaultCount),
				new QueryParameter("NumOfLastDefaults", nLastDefaultCount),
				new QueryParameter("AnalyticsDate", DateTime.UtcNow)
			);
		}
	}
}
