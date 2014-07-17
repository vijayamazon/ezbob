namespace EzBob.Backend.Strategies.Misc 
{
	using System.Collections.Generic;
	using System.Data;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetCompanyDataForCompanyScore : AStrategy
	{
		private readonly int customerId;
		private readonly string refNumber;

		#region constructor

		public GetCompanyDataForCompanyScore(AConnection oDb, ASafeLog oLog, int customerId, string refNumber)
			: base(oDb, oLog)
		{
			this.customerId = customerId;
			this.refNumber = refNumber;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "GetCompanyDataForCompanyScore"; }
		} // Name

		public CompanyData Data { get; set; }

		#endregion property Name

		#region property Execute

		public override void Execute() {
			Data = new CompanyData {IsLimited = true};

			DataTable dt = DB.ExecuteReader(
				"GetCompanyIsLimited",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("RefNumber", refNumber));

			if (dt.Rows.Count == 1)
			{
				var sr = new SafeReader(dt.Rows[0]);
				Data.IsLimited = sr["IsLimited"];
			}

			if (!Data.IsLimited)
			{
				DataTable nonLimitedDataTable = DB.ExecuteReader(
					"GetNonLimitedDataForCompanyScore",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", customerId),
					new QueryParameter("RefNumber", refNumber));

				if (nonLimitedDataTable.Rows.Count == 1)
				{
					var nonLimitedSafeReader = new SafeReader(nonLimitedDataTable.Rows[0]);
					
					Data.Address1 = nonLimitedSafeReader["Address1"].ToNullString();
					Data.Address2 = nonLimitedSafeReader["Address2"].ToNullString();
					Data.Address3 = nonLimitedSafeReader["Address3"].ToNullString();
					Data.Address4 = nonLimitedSafeReader["Address4"].ToNullString();
					Data.Address5 = nonLimitedSafeReader["Address5"].ToNullString();
					Data.Postcode = nonLimitedSafeReader["Postcode"].ToNullString();
					Data.TelephoneNumber = nonLimitedSafeReader["TelephoneNumber"].ToNullString();
					Data.PrincipalActivities = nonLimitedSafeReader["PrincipalActivities"].ToNullString();
					Data.EarliestKnownDate = nonLimitedSafeReader["EarliestKnownDate"];
					Data.DateOwnershipCommenced = nonLimitedSafeReader["DateOwnershipCommenced"];
					Data.IncorporationDate = nonLimitedSafeReader["IncorporationDate"];
					Data.DateOwnershipCeased = nonLimitedSafeReader["DateOwnershipCeased"];
					Data.LastUpdateDate = nonLimitedSafeReader["LastUpdateDate"];
					Data.BankruptcyCountDuringOwnership = nonLimitedSafeReader["BankruptcyCountDuringOwnership"];
					Data.AgeOfMostRecentBankruptcyDuringOwnershipMonths = nonLimitedSafeReader["AgeOfMostRecentBankruptcyDuringOwnershipMonths"];
					Data.AssociatedBankruptcyCountDuringOwnership = nonLimitedSafeReader["AssociatedBankruptcyCountDuringOwnership"];
					Data.AgeOfMostRecentAssociatedBankruptcyDuringOwnershipMonths = nonLimitedSafeReader["AgeOfMostRecentAssociatedBankruptcyDuringOwnershipMonths"];
					Data.AgeOfMostRecentJudgmentDuringOwnershipMonths = nonLimitedSafeReader["AgeOfMostRecentJudgmentDuringOwnershipMonths"];
					Data.TotalJudgmentCountLast12Months = nonLimitedSafeReader["TotalJudgmentCountLast12Months"];
					Data.TotalJudgmentValueLast12Months = nonLimitedSafeReader["TotalJudgmentValueLast12Months"];
					Data.TotalJudgmentCountLast13To24Months = nonLimitedSafeReader["TotalJudgmentCountLast13To24Months"];
					Data.TotalJudgmentValueLast13To24Months = nonLimitedSafeReader["TotalJudgmentValueLast13To24Months"];
					Data.ValueOfMostRecentAssociatedJudgmentDuringOwnership = nonLimitedSafeReader["ValueOfMostRecentAssociatedJudgmentDuringOwnership"];
					Data.TotalAssociatedJudgmentCountLast12Months = nonLimitedSafeReader["TotalAssociatedJudgmentCountLast12Months"];
					Data.TotalAssociatedJudgmentValueLast12Months = nonLimitedSafeReader["TotalAssociatedJudgmentValueLast12Months"];
					Data.TotalAssociatedJudgmentCountLast13To24Months = nonLimitedSafeReader["TotalAssociatedJudgmentCountLast13To24Months"];
					Data.TotalAssociatedJudgmentValueLast13To24Months = nonLimitedSafeReader["TotalAssociatedJudgmentValueLast13To24Months"];
					Data.TotalJudgmentCountLast24Months = nonLimitedSafeReader["TotalJudgmentCountLast24Months"];
					Data.TotalAssociatedJudgmentCountLast24Months = nonLimitedSafeReader["TotalAssociatedJudgmentCountLast24Months"];
					Data.TotalJudgmentValueLast24Months = nonLimitedSafeReader["TotalJudgmentValueLast24Months"];
					Data.TotalAssociatedJudgmentValueLast24Months = nonLimitedSafeReader["TotalAssociatedJudgmentValueLast24Months"];
					Data.SupplierName = nonLimitedSafeReader["SupplierName"].ToNullString();
					Data.FraudCategory = nonLimitedSafeReader["FraudCategory"].ToNullString();
					Data.FraudCategoryDesc = nonLimitedSafeReader["FraudCategoryDesc"].ToNullString();
					Data.NumberOfAccountsPlacedForCollection = nonLimitedSafeReader["NumberOfAccountsPlacedForCollection"];
					Data.ValueOfAccountsPlacedForCollection = nonLimitedSafeReader["ValueOfAccountsPlacedForCollection"];
					Data.NumberOfAccountsPlacedForCollectionLast2Years = nonLimitedSafeReader["NumberOfAccountsPlacedForCollectionLast2Years"];
					Data.AverageDaysBeyondTermsFor0To100 = nonLimitedSafeReader["AverageDaysBeyondTermsFor0To100"];
					Data.AverageDaysBeyondTermsFor101To1000 = nonLimitedSafeReader["AverageDaysBeyondTermsFor101To1000"];
					Data.AverageDaysBeyondTermsFor1001To10000 = nonLimitedSafeReader["AverageDaysBeyondTermsFor1001To10000"];
					Data.AverageDaysBeyondTermsForOver10000 = nonLimitedSafeReader["AverageDaysBeyondTermsForOver10000"];
					Data.AverageDaysBeyondTermsForLast3MonthsOfDataReturned = nonLimitedSafeReader["AverageDaysBeyondTermsForLast3MonthsOfDataReturned"];
					Data.AverageDaysBeyondTermsForLast6MonthsOfDataReturned = nonLimitedSafeReader["AverageDaysBeyondTermsForLast6MonthsOfDataReturned"];
					Data.AverageDaysBeyondTermsForLast12MonthsOfDataReturned = nonLimitedSafeReader["AverageDaysBeyondTermsForLast12MonthsOfDataReturned"];
					Data.CurrentAverageDebt = nonLimitedSafeReader["CurrentAverageDebt"];
					Data.AverageDebtLast3Months = nonLimitedSafeReader["AverageDebtLast3Months"];
					Data.AverageDebtLast12Months = nonLimitedSafeReader["AverageDebtLast12Months"];
					Data.TelephoneNumberDN36 = nonLimitedSafeReader["TelephoneNumberDN36"].ToNullString();
					Data.RiskScore = nonLimitedSafeReader["RiskScore"];
					Data.SearchType = nonLimitedSafeReader["SearchType"].ToNullString();
					Data.SearchTypeDesc = nonLimitedSafeReader["SearchTypeDesc"].ToNullString();
					Data.CommercialDelphiScore = nonLimitedSafeReader["CommercialDelphiScore"];
					Data.CreditRating = nonLimitedSafeReader["CreditRating"].ToNullString();
					Data.CreditLimit = nonLimitedSafeReader["CreditLimit"].ToNullString();
					Data.ProbabilityOfDefaultScore = nonLimitedSafeReader["ProbabilityOfDefaultScore"];
					Data.StabilityOdds = nonLimitedSafeReader["StabilityOdds"].ToNullString();
					Data.RiskBand = nonLimitedSafeReader["RiskBand"].ToNullString();
					Data.NumberOfProprietorsSearched = nonLimitedSafeReader["NumberOfProprietorsSearched"];
					Data.NumberOfProprietorsFound = nonLimitedSafeReader["NumberOfProprietorsFound"];
					Data.Errors = nonLimitedSafeReader["Errors"].ToNullString();
					int experianNonLimitedResultId = nonLimitedSafeReader["Id"];

					DataTable sicCodesDataTable = DB.ExecuteReader(
						"GetNonLimitedSicCodes",
						CommandSpecies.StoredProcedure,
						new QueryParameter("ExperianNonLimitedResultId", experianNonLimitedResultId));

					if (sicCodesDataTable.Rows.Count > 0)
					{
						Data.SicCodes = new List<string>();
						Data.SicDescs = new List<string>();
						foreach (DataRow row in sicCodesDataTable.Rows)
						{
							var sicCodesSafeReader = new SafeReader(row);

							string sicCode = sicCodesSafeReader["Code"].ToNullString();
							string sicDesc = sicCodesSafeReader["Description"].ToNullString();
							Data.SicCodes.Add(sicCode);
							Data.SicDescs.Add(sicDesc);
						}
					}

					DataTable bakruptcyDetailsDataTable = DB.ExecuteReader(
						"GetNonLimitedBankruptcyDetails",
						CommandSpecies.StoredProcedure,
						new QueryParameter("ExperianNonLimitedResultId", experianNonLimitedResultId));

					if (bakruptcyDetailsDataTable.Rows.Count > 0)
					{
						Data.BankruptcyDetails = new List<BankruptcyDetail>();
						foreach (DataRow row in bakruptcyDetailsDataTable.Rows)
						{
							var bakruptcyDetailsSafeReader = new SafeReader(row);

							var bankruptcyDetail = new BankruptcyDetail();
							bankruptcyDetail.BankruptcyName = bakruptcyDetailsSafeReader["BankruptcyName"].ToNullString();
							bankruptcyDetail.BankruptcyAddr1 = bakruptcyDetailsSafeReader["BankruptcyAddr1"].ToNullString();
							bankruptcyDetail.BankruptcyAddr2 = bakruptcyDetailsSafeReader["BankruptcyAddr2"].ToNullString();
							bankruptcyDetail.BankruptcyAddr3 = bakruptcyDetailsSafeReader["BankruptcyAddr3"].ToNullString();
							bankruptcyDetail.BankruptcyAddr4 = bakruptcyDetailsSafeReader["BankruptcyAddr4"].ToNullString();
							bankruptcyDetail.BankruptcyAddr5 = bakruptcyDetailsSafeReader["BankruptcyAddr5"].ToNullString();
							bankruptcyDetail.PostCode = bakruptcyDetailsSafeReader["PostCode"].ToNullString();
							bankruptcyDetail.GazetteDate = bakruptcyDetailsSafeReader["GazetteDate"];
							bankruptcyDetail.BankruptcyType = bakruptcyDetailsSafeReader["BankruptcyType"].ToNullString();
							bankruptcyDetail.BankruptcyTypeDesc = bakruptcyDetailsSafeReader["BankruptcyTypeDesc"].ToNullString();
							
							Data.BankruptcyDetails.Add(bankruptcyDetail);
						}
					}

					DataTable ccjDetailsDataTable = DB.ExecuteReader(
						"GetNonLimitedCcjDetails",
						CommandSpecies.StoredProcedure,
						new QueryParameter("ExperianNonLimitedResultId", experianNonLimitedResultId));

					if (ccjDetailsDataTable.Rows.Count > 0)
					{
						Data.CcjDetails = new List<CcjDetail>();
						foreach (DataRow row in ccjDetailsDataTable.Rows)
						{
							var ccjDetailsSafeReader = new SafeReader(row);

							var ccjDetail = new CcjDetail();
							ccjDetail.RecordType = ccjDetailsSafeReader["RecordType"].ToNullString();
							ccjDetail.RecordTypeFullName = ccjDetailsSafeReader["RecordTypeFullName"].ToNullString();
							ccjDetail.JudgementDate = ccjDetailsSafeReader["JudgementDate"];
							ccjDetail.SatisfactionFlag = ccjDetailsSafeReader["SatisfactionFlag"].ToNullString();
							ccjDetail.SatisfactionFlagDesc = ccjDetailsSafeReader["SatisfactionFlagDesc"].ToNullString();
							ccjDetail.SatisfactionDate = ccjDetailsSafeReader["SatisfactionDate"];
							ccjDetail.JudgmentType = ccjDetailsSafeReader["JudgmentType"].ToNullString();
							ccjDetail.JudgmentTypeDesc = ccjDetailsSafeReader["JudgmentTypeDesc"].ToNullString();
							ccjDetail.JudgmentAmount = ccjDetailsSafeReader["JudgmentAmount"];
							ccjDetail.Court = ccjDetailsSafeReader["Court"].ToNullString();
							ccjDetail.CaseNumber = ccjDetailsSafeReader["CaseNumber"].ToNullString();
							ccjDetail.NumberOfJudgmentNames = ccjDetailsSafeReader["NumberOfJudgmentNames"].ToNullString();
							ccjDetail.NumberOfTradingNames = ccjDetailsSafeReader["NumberOfTradingNames"].ToNullString();
							ccjDetail.LengthOfJudgmentName = ccjDetailsSafeReader["LengthOfJudgmentName"].ToNullString();
							ccjDetail.LengthOfTradingName = ccjDetailsSafeReader["LengthOfTradingName"].ToNullString();
							ccjDetail.LengthOfJudgmentAddress = ccjDetailsSafeReader["LengthOfJudgmentAddress"].ToNullString();
							ccjDetail.JudgementAddr1 = ccjDetailsSafeReader["JudgementAddr1"].ToNullString();
							ccjDetail.JudgementAddr2 = ccjDetailsSafeReader["JudgementAddr2"].ToNullString();
							ccjDetail.JudgementAddr3 = ccjDetailsSafeReader["JudgementAddr3"].ToNullString();
							ccjDetail.JudgementAddr4 = ccjDetailsSafeReader["JudgementAddr4"].ToNullString();
							ccjDetail.JudgementAddr5 = ccjDetailsSafeReader["JudgementAddr5"].ToNullString();
							ccjDetail.PostCode = ccjDetailsSafeReader["PostCode"].ToNullString();
							int ccjDetailId = ccjDetailsSafeReader["Id"];

							DataTable ccjDetailRegisteredAgainstDataTable = DB.ExecuteReader(
								"GetNonLimitedCcjDetailRegisteredAgainst",
								CommandSpecies.StoredProcedure,
								new QueryParameter("ExperianNonLimitedResultCcjDetailsId", ccjDetailId));

							if (ccjDetailRegisteredAgainstDataTable.Rows.Count > 0)
							{
								ccjDetail.RegisteredAgainst = new List<string>();
								foreach (DataRow registeredAgainstRow in ccjDetailRegisteredAgainstDataTable.Rows)
								{
									var registeredAgainstSafeReader = new SafeReader(registeredAgainstRow);
									string registeredAgainst = registeredAgainstSafeReader["Name"].ToNullString();
									ccjDetail.RegisteredAgainst.Add(registeredAgainst);
								}
							}

							DataTable ccjDetailTradingNamesDataTable = DB.ExecuteReader(
								"GetNonLimitedCcjDetailTradingNames",
								CommandSpecies.StoredProcedure,
								new QueryParameter("ExperianNonLimitedResultCcjDetailsId", ccjDetailId));

							if (ccjDetailTradingNamesDataTable.Rows.Count > 0)
							{
								ccjDetail.TradingNames = new List<TradingName>();
								foreach (DataRow tradingNamesRow in ccjDetailTradingNamesDataTable.Rows)
								{
									var tradingNamesSafeReader = new SafeReader(tradingNamesRow);
									var tradingName = new TradingName();
									tradingName.Name = tradingNamesSafeReader["Name"].ToNullString();
									tradingName.TradingIndicator = tradingNamesSafeReader["TradingIndicator"].ToNullString();
									tradingName.TradingIndicatorDesc = tradingNamesSafeReader["TradingIndicatorDesc"].ToNullString();
									ccjDetail.TradingNames.Add(tradingName);
								}
							}

							Data.CcjDetails.Add(ccjDetail);
						}
					}
				}
			}
		} // Execute

		#endregion property Execute
	} // class GetCompanyDataForCompanyScore
} // namespace
