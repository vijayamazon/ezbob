namespace EzBob.Backend.Strategies.Experian
{
	using System;
	using System.Globalization;
	using ConfigManager;
	using ExperianLib;
	using EzBobIntegration.Web_References.Consumer;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Database;
	using Ezbob.Logger;
	using StoredProcs;

	public class ExperianConsumerCheck : AStrategy
	{
		private readonly int customerId;
		private readonly int? directorId;
		private readonly bool forceCheck;

		private readonly GetCustomerAddresses.ResultRow addressLines;
		private readonly GetPersonalInfoForConsumerCheck.ResultRow personalData;

		public int Score { get; private set; }
		public ExperianConsumerData Result { get; private set; }

		public override string Name
		{
			get { return "Experian consumer check"; }
		}

		public ExperianConsumerCheck(int customerId, int? directorId, bool bForceCheck, AConnection db, ASafeLog log)
			: base(db, log)
		{
			this.customerId = customerId;
			this.directorId = directorId;
			forceCheck = bForceCheck;

			personalData =
				new GetPersonalInfoForConsumerCheck(customerId, directorId, DB, Log)
					.FillFirst<GetPersonalInfoForConsumerCheck.ResultRow>();

			addressLines = new GetCustomerAddresses(customerId, directorId, DB, Log).FillFirst<GetCustomerAddresses.ResultRow>();
		}

		public override void Execute()
		{
			Log.Info("Starting consumer check with parameters: {0} {1}.", personalData, addressLines);

			bool bSuccess = GetConsumerInfoAndSave(AddressCurrency.Current);

			if (!bSuccess && CanUsePrevAddress())
				bSuccess = GetConsumerInfoAndSave(AddressCurrency.Previous);

			UpdateAnalytics();

			Log.Info("Consumer check {2} with parameters: {0} {1}.", personalData, addressLines,
					 bSuccess ? "succeeded" : "failed");
		}

		private bool CanUsePrevAddress()
		{
			return
				(directorId == 0) &&
				(personalData.TimeAtAddress == 1) &&
				!string.IsNullOrEmpty(addressLines[6, AddressCurrency.Previous]);
		}

		private void UpdateAnalytics()
		{
			if ((Result == null) || Result.HasExperianError)
				return;

			if (directorId == 0)
				UpdateCustomerAnalytics();
			else
				UpdateDirectorAnalytics();
		}

		private void UpdateCustomerAnalytics()
		{
			int nCii = Result.CII.HasValue ? Result.CII.Value : 0;

			int nNumOfAccounts = 0;
			int nDefaultCount = 0;
			int nLastDefaultCount = 0;

			var dThen = DateTime.UtcNow.AddYears(-CurrentValues.Instance.CustomerAnalyticsDefaultHistoryYears);

			foreach (var detail in Result.Cais)
			{
				nNumOfAccounts++;

				if (detail.AccountStatus != "F")
					continue;

				nDefaultCount++;

				var settlementDate = detail.SettlementDate ?? detail.LastUpdatedDate;

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
				Score,
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
				new QueryParameter("Score", Score),
				new QueryParameter("IndebtednessIndex", nCii),
				new QueryParameter("NumOfAccounts", nNumOfAccounts),
				new QueryParameter("NumOfDefaults", nDefaultCount),
				new QueryParameter("NumOfLastDefaults", nLastDefaultCount),
				new QueryParameter("AnalyticsDate", DateTime.UtcNow)
				);
		}

		private void UpdateDirectorAnalytics()
		{
			Log.Debug("Updating customer analytics director score (customer = {0}, director = {1}, score = {2})", customerId,
					  directorId, Score);

			DB.ExecuteNonQuery(
				"CustomerAnalyticsUpdateDirector",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", customerId),
				new QueryParameter("Score", Score),
				new QueryParameter("AnalyticsDate", DateTime.UtcNow)
				);
		}

		private bool GetConsumerInfoAndSave(AddressCurrency oAddressCurrency)
		{
			InputLocationDetailsMultiLineLocation location = addressLines.GetLocation(oAddressCurrency);

			var consumerService = new ConsumerService();

			Result = consumerService.GetConsumerInfo(
				personalData.FirstName,
				personalData.Surname,
				personalData.Gender,
				personalData.DateOfBirth,
				null,
				location,
				"PL",
				customerId,
				directorId,
				false,
				directorId != 0,
				forceCheck
				);

			if (!Result.HasExperianError)
				Score = Result.BureauScore.HasValue ? Result.BureauScore.Value : 0;

			return !Result.HasExperianError;
		}

		// ReSharper disable MemberCanBePrivate.Local
		// ReSharper disable UnusedAutoPropertyAccessor.Local
		private class GetPersonalInfoForConsumerCheck : AStoredProcedure
		{
			public GetPersonalInfoForConsumerCheck(int customerId, int? directorId, AConnection db, ASafeLog log)
				: base(db, log)
			{
				CustomerId = customerId;
				DirectorId = directorId;
			}

			public override bool HasValidParameters()
			{
				return CustomerId > 0;
			}

			public int CustomerId { get; set; }

			public int? DirectorId { get; set; }

			public class ResultRow : AResultRow
			{
				public string FirstName { get; set; }
				public string Surname { get; set; }
				public string Gender { get; set; }
				public DateTime DateOfBirth { get; set; }
				public int TimeAtAddress { get; set; }

				public override string ToString()
				{
					return string.Format(
						"FirstName='{0}' Surname='{1}' Gender='{2}' DateOfBirth='{3}' Time at address={4}",
						FirstName, Surname, Gender, DateOfBirth.ToString("d/MMM/yyyy", CultureInfo.InvariantCulture), TimeAtAddress
						);
				}
			}
		}
	}

	internal static class CustomerAddressExt
	{
		public static InputLocationDetailsMultiLineLocation GetLocation(this GetCustomerAddresses.ResultRow oAddr,
																		AddressCurrency oCurrency)
		{
			if (oAddr == null)
				throw new ArgumentNullException("oAddr", "Address row not specified.");

			switch (oCurrency)
			{
				case AddressCurrency.Current:
					return new InputLocationDetailsMultiLineLocation
						{
							LocationLine1 = oAddr.Line1,
							LocationLine2 = oAddr.Line2,
							LocationLine3 = oAddr.Line3,
							LocationLine4 = oAddr.Line4,
							LocationLine5 = oAddr.Line5,
							LocationLine6 = oAddr.Line6,
						};

				case AddressCurrency.Previous:
					return new InputLocationDetailsMultiLineLocation
						{
							LocationLine1 = oAddr.Line1Prev,
							LocationLine2 = oAddr.Line2Prev,
							LocationLine3 = oAddr.Line3Prev,
							LocationLine4 = oAddr.Line4Prev,
							LocationLine5 = oAddr.Line5Prev,
							LocationLine6 = oAddr.Line6Prev,
						};

				default:
					throw new ArgumentOutOfRangeException("oCurrency", "Unsupported value: " + oCurrency.ToString());
			}
		}
	}
}