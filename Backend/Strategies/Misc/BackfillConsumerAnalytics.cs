namespace EzBob.Backend.Strategies.Misc 
{
	using System;
	using System.Data;
	using System.Globalization;
	using ConfigManager;
	using EzBob.Backend.Strategies.Experian;
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
					ConsumerServiceResult consumerServiceResult = consumerService.GetConsumerInfo(
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

					if (consumerServiceResult == null || consumerServiceResult.Data.HasExperianError)
					{
						Log.Debug("Backfilling customer analytics for customer {0} and director {1} Failed!", customerId, directorId);
						continue;
					}

					int score = (int)consumerServiceResult.Data.BureauScore;

					if (directorId == 0)
					{
						UpdateCustomerAnalytics(consumerServiceResult, customerId, score);
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

		private void UpdateDirectorAnalytics(int customerId, int directorId, int score)
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

		private void UpdateCustomerAnalytics(ConsumerServiceResult consumerServiceResult, int customerId, int score)
		{
			string sCii = consumerServiceResult.Output.Output.ConsumerSummary.PremiumValueData.CII.NDSPCII;

			int nCii;
			if (!int.TryParse(sCii, out nCii))
				Log.Alert("Failed to parse customer indebtedness index '{0}' (integer expected).", sCii);

			OutputFullConsumerDataConsumerDataCAIS[] cais = null;
			int nNumOfAccounts = 0;
			int nDefaultCount = 0;
			int nLastDefaultCount = 0;

			try
			{
				cais = consumerServiceResult.Output.Output.FullConsumerData.ConsumerData.CAIS;
			}
			catch (Exception e)
			{
				Log.Debug(e, "Could not extract CAIS data from Experian output - this is a thin file.");
			} // try

			if (cais == null)
				cais = new OutputFullConsumerDataConsumerDataCAIS[0];

			var dThen = DateTime.UtcNow.AddYears(-CurrentValues.Instance.CustomerAnalyticsDefaultHistoryYears);

			foreach (var caisData in cais)
			{
				if (caisData.CAISDetails == null)
					continue;

				foreach (OutputFullConsumerDataConsumerDataCAISCAISDetails detail in caisData.CAISDetails)
				{
					nNumOfAccounts++;

					if (detail.AccountStatus != "F")
						continue;

					nDefaultCount++;

					int relevantYear, relevantMonth, relevantDay;

					if (detail.SettlementDate != null)
					{
						relevantYear = detail.SettlementDate.CCYY;
						relevantMonth = detail.SettlementDate.MM;
						relevantDay = detail.SettlementDate.DD;
					}
					else
					{
						relevantYear = detail.LastUpdatedDate.CCYY;
						relevantMonth = detail.LastUpdatedDate.MM;
						relevantDay = detail.LastUpdatedDate.DD;
					} // if

					var settlementDate = new DateTime(relevantYear, relevantMonth, relevantDay);

					if (settlementDate >= dThen)
						nLastDefaultCount++;
				} // for each detail in CAIS datum
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
