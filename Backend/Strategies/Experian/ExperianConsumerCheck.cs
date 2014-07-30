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
		#region public

		#region constructor

		public ExperianConsumerCheck(int nCustomerID, int nDirectorID, bool bForceCheck, AConnection oDB, ASafeLog oLog)
			: base(oDB, oLog)
		{
			m_nCustomerID = nCustomerID;
			m_nDirectorID = nDirectorID;
			m_bForceCheck = bForceCheck;

			m_oPersonalData =
				new GetPersonalInfoForConsumerCheck(m_nCustomerID, m_nDirectorID, DB, Log)
					.FillFirst<GetPersonalInfoForConsumerCheck.ResultRow>();

			m_oAddressLines = new GetCustomerAddresses(m_nCustomerID, DB, Log).FillFirst<GetCustomerAddresses.ResultRow>();
		}

		// constructor

		#endregion constructor

		#region property Name

		public override string Name
		{
			get { return "Experian consumer check"; }
		}

		// Name

		#endregion property Name

		#region property Score

		public int Score { get; private set; }

		#endregion property Score

		#region method Execute

		public override void Execute()
		{
			Log.Info("Starting consumer check with parameters: {0} {1}.", m_oPersonalData, m_oAddressLines);

			bool bSuccess = GetConsumerInfoAndSave(AddressCurrency.Current);

			if (!bSuccess && CanUsePrevAddress())
				bSuccess = GetConsumerInfoAndSave(AddressCurrency.Previous);

			UpdateAnalytics();

			Log.Info("Consumer check {2} with parameters: {0} {1}.", m_oPersonalData, m_oAddressLines,
					 bSuccess ? "succeeded" : "failed");
		}

		// Execute

		#endregion method Execute

		#endregion public

		#region method CanUsePrevAddress

		private bool CanUsePrevAddress()
		{
			return
				(m_nDirectorID == 0) &&
				(m_oPersonalData.TimeAtAddress == 1) &&
				!string.IsNullOrEmpty(m_oAddressLines[6, AddressCurrency.Previous]);
		}

		// CanUsePrevAddress

		#endregion method CanUsePrevAddress

		#region method UpdateAnalytics

		private void UpdateAnalytics()
		{
			if ((Result == null) || Result.HasExperianError)
				return;

			if (m_nDirectorID == 0)
				UpdateCustomerAnalytics();
			else
				UpdateDirectorAnalytics();
		}

		// UpdateAnalytics

		#endregion method UpdateAnalytics

		#region method UpdateCustomerAnalytics

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
				m_nCustomerID,
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
				new QueryParameter("CustomerID", m_nCustomerID),
				new QueryParameter("Score", Score),
				new QueryParameter("IndebtednessIndex", nCii),
				new QueryParameter("NumOfAccounts", nNumOfAccounts),
				new QueryParameter("NumOfDefaults", nDefaultCount),
				new QueryParameter("NumOfLastDefaults", nLastDefaultCount),
				new QueryParameter("AnalyticsDate", DateTime.UtcNow)
				);
		}

		// UpdateCustomerAnalytics

		#endregion method UpdateCustomerAnalytics

		#region method UpdateDirectorAnalytics

		private void UpdateDirectorAnalytics()
		{
			Log.Debug("Updating customer analytics director score (customer = {0}, director = {1}, score = {2})", m_nCustomerID,
					  m_nDirectorID, Score);

			DB.ExecuteNonQuery(
				"CustomerAnalyticsUpdateDirector",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", m_nCustomerID),
				new QueryParameter("Score", Score),
				new QueryParameter("AnalyticsDate", DateTime.UtcNow)
				);
		}

		// UpdateDirectorAnalytics

		#endregion method UpdateDirectorAnalytics

		#region method GetConsumerInfoAndSave

		private bool GetConsumerInfoAndSave(AddressCurrency oAddressCurrency)
		{
			InputLocationDetailsMultiLineLocation location = m_oAddressLines.GetLocation(oAddressCurrency);

			var consumerService = new ConsumerService();

			Result = consumerService.GetConsumerInfo(
				m_oPersonalData.FirstName,
				m_oPersonalData.Surname,
				m_oPersonalData.Gender,
				m_oPersonalData.DateOfBirth,
				null,
				location,
				"PL",
				m_nCustomerID,
				m_nDirectorID,
				false,
				m_nDirectorID != 0,
				m_bForceCheck
				);

			if (!Result.HasExperianError)
				Score = Result.BureauScore.HasValue ? Result.BureauScore.Value : 0;

			return !Result.HasExperianError;
		}

		// GetConsumerInfoAndSave

		#endregion method GetConsumerInfoAndSave

		#region fields

		private readonly int m_nCustomerID;
		private readonly int m_nDirectorID;
		private readonly bool m_bForceCheck;

		private readonly GetCustomerAddresses.ResultRow m_oAddressLines;
		private readonly GetPersonalInfoForConsumerCheck.ResultRow m_oPersonalData;
		private ExperianConsumerData Result;

		#endregion fields



		#region stored procedure classes

		// ReSharper disable MemberCanBePrivate.Local
		// ReSharper disable UnusedAutoPropertyAccessor.Local

		#region class GetPersonalInfoForConsumerCheck

		private class GetPersonalInfoForConsumerCheck : AStoredProcedure
		{
			public GetPersonalInfoForConsumerCheck(int nCustomerID, int nDirectorID, AConnection oDB, ASafeLog oLog)
				: base(oDB, oLog)
			{
				CustomerID = nCustomerID;
				DirectorID = nDirectorID;
			}

			// constructor

			public override bool HasValidParameters()
			{
				return CustomerID > 0;
			}

			// HasValidParameters

			public int CustomerID { get; set; }

			public int DirectorID { get; set; }

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

				// ToString
			}

			// class ResultRow
		}

		// class GetPersonalInfoForConsumerCheck

		#endregion class GetPersonalInfoForConsumerCheck

		#endregion class ExperianConsumerCheck
	}

	#region class CustomerAddressExt

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
			} // switch
		}

		// GetLocation
	}

	// class CustomerAddressExt

	#endregion class CustomerAddressExt
}	// namespace