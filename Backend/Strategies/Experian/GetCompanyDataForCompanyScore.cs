namespace Ezbob.Backend.Strategies.Experian {
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;

	public class GetCompanyDataForCompanyScore : AStrategy {
		public GetCompanyDataForCompanyScore(string refNumber) {
			this.refNumber = refNumber;
		} // constructor

		public override string Name {
			get { return "GetCompanyDataForCompanyScore"; }
		} // Name

		public CompanyData Data { get; set; }

		public override void Execute() {
			Data = new CompanyData { IsLimited = true };

			SafeReader sr = DB.GetFirst(
				"GetCompanyIsLimited",
				CommandSpecies.StoredProcedure,
				new QueryParameter("RefNumber", refNumber)
			);

			if (!sr.IsEmpty)
				Data.IsLimited = sr["IsLimited"];

			if (refNumber == "NotFound")
				return;

			if (Data.IsLimited)
				return;

			SafeReader nonLimitedSafeReader = DB.GetFirst(
				"GetNonLimitedDataForCompanyScore",
				CommandSpecies.StoredProcedure,
				new QueryParameter("RefNumber", refNumber)
			);

			if (!nonLimitedSafeReader.IsEmpty) {
				FillBasicData(nonLimitedSafeReader);

				int experianNonLimitedResultId = nonLimitedSafeReader["Id"];

				FillSicCodes(experianNonLimitedResultId);

				FillBankruptcy(experianNonLimitedResultId);

				FillCcj(experianNonLimitedResultId);

				if (Data.CcjDetails != null) {
					FillPreviousSearches(experianNonLimitedResultId);
					FillPaymentPerformanceDetails(experianNonLimitedResultId);
				} // if has CCJ details
			} // if has non limited company data

			FillScoreHistory();
		} // Execute

		private void FillBasicData(SafeReader nonLimitedSafeReader) {
			Data.BusinessName = nonLimitedSafeReader["BusinessName"].ToNullString();
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
			Data.RiskText = nonLimitedSafeReader["RiskText"].ToNullString();
			Data.CreditText = nonLimitedSafeReader["CreditText"].ToNullString();
			Data.ConcludingText = nonLimitedSafeReader["ConcludingText"].ToNullString();
			Data.NocText = nonLimitedSafeReader["NocText"].ToNullString();
			Data.PossiblyRelatedDataText = nonLimitedSafeReader["PossiblyRelatedDataText"].ToNullString();

		} // FillBasicData

		private void FillSicCodes(int experianNonLimitedResultId) {
			DB.ForEachRowSafe(
				(sicCodesSafeReader, bRowsetStart) => {
					if (Data.SicCodes == null)
						Data.SicCodes = new List<string>();

					if (Data.SicDescs == null)
						Data.SicDescs = new List<string>();

					string sicCode = sicCodesSafeReader["Code"].ToNullString();
					string sicDesc = sicCodesSafeReader["Description"].ToNullString();
					Data.SicCodes.Add(sicCode);
					Data.SicDescs.Add(sicDesc);

					return ActionResult.Continue;
				},
				"GetNonLimitedSicCodes",
				CommandSpecies.StoredProcedure,
				new QueryParameter("ExperianNonLimitedResultId", experianNonLimitedResultId)
			);
		} // FillSicCodes

		private void FillBankruptcy(int experianNonLimitedResultId) {
			DB.ForEachRowSafe(
				(bakruptcyDetailsSafeReader, bRowsetStart) => {
					if (Data.BankruptcyDetails == null)
						Data.BankruptcyDetails = new List<BankruptcyDetail>();

					var bankruptcyDetail = new BankruptcyDetail {
						BankruptcyName = bakruptcyDetailsSafeReader["BankruptcyName"].ToNullString(),
						BankruptcyAddr1 = bakruptcyDetailsSafeReader["BankruptcyAddr1"].ToNullString(),
						BankruptcyAddr2 = bakruptcyDetailsSafeReader["BankruptcyAddr2"].ToNullString(),
						BankruptcyAddr3 = bakruptcyDetailsSafeReader["BankruptcyAddr3"].ToNullString(),
						BankruptcyAddr4 = bakruptcyDetailsSafeReader["BankruptcyAddr4"].ToNullString(),
						BankruptcyAddr5 = bakruptcyDetailsSafeReader["BankruptcyAddr5"].ToNullString(),
						PostCode = bakruptcyDetailsSafeReader["PostCode"].ToNullString(),
						GazetteDate = bakruptcyDetailsSafeReader["GazetteDate"],
						BankruptcyType = bakruptcyDetailsSafeReader["BankruptcyType"].ToNullString(),
						BankruptcyTypeDesc = bakruptcyDetailsSafeReader["BankruptcyTypeDesc"].ToNullString(),
					};

					Data.BankruptcyDetails.Add(bankruptcyDetail);

					return ActionResult.Continue;
				},
				"GetNonLimitedBankruptcyDetails",
				CommandSpecies.StoredProcedure,
				new QueryParameter("ExperianNonLimitedResultId", experianNonLimitedResultId)
			);
		} // FillBankruptcy

		private void FillCcj(int experianNonLimitedResultId) {
			DB.ForEachRowSafe(
				(ccjDetailsSafeReader, bRowsetStart) => {
					if (Data.CcjDetails == null)
						Data.CcjDetails = new List<CcjDetail>();

					var ccjDetail = new CcjDetail {
						RecordType = ccjDetailsSafeReader["RecordType"].ToNullString(),
						RecordTypeFullName = ccjDetailsSafeReader["RecordTypeFullName"].ToNullString(),
						JudgementDate = ccjDetailsSafeReader["JudgementDate"],
						SatisfactionFlag = ccjDetailsSafeReader["SatisfactionFlag"].ToNullString(),
						SatisfactionFlagDesc = ccjDetailsSafeReader["SatisfactionFlagDesc"].ToNullString(),
						SatisfactionDate = ccjDetailsSafeReader["SatisfactionDate"],
						JudgmentType = ccjDetailsSafeReader["JudgmentType"].ToNullString(),
						JudgmentTypeDesc = ccjDetailsSafeReader["JudgmentTypeDesc"].ToNullString(),
						JudgmentAmount = ccjDetailsSafeReader["JudgmentAmount"],
						Court = ccjDetailsSafeReader["Court"].ToNullString(),
						CaseNumber = ccjDetailsSafeReader["CaseNumber"].ToNullString(),
						NumberOfJudgmentNames = ccjDetailsSafeReader["NumberOfJudgmentNames"].ToNullString(),
						NumberOfTradingNames = ccjDetailsSafeReader["NumberOfTradingNames"].ToNullString(),
						LengthOfJudgmentName = ccjDetailsSafeReader["LengthOfJudgmentName"].ToNullString(),
						LengthOfTradingName = ccjDetailsSafeReader["LengthOfTradingName"].ToNullString(),
						LengthOfJudgmentAddress = ccjDetailsSafeReader["LengthOfJudgmentAddress"].ToNullString(),
						JudgementAddr1 = ccjDetailsSafeReader["JudgementAddr1"].ToNullString(),
						JudgementAddr2 = ccjDetailsSafeReader["JudgementAddr2"].ToNullString(),
						JudgementAddr3 = ccjDetailsSafeReader["JudgementAddr3"].ToNullString(),
						JudgementAddr4 = ccjDetailsSafeReader["JudgementAddr4"].ToNullString(),
						JudgementAddr5 = ccjDetailsSafeReader["JudgementAddr5"].ToNullString(),
						PostCode = ccjDetailsSafeReader["PostCode"].ToNullString(),
					};

					int ccjDetailId = ccjDetailsSafeReader["Id"];

					DB.ForEachRowSafe(
						(registeredAgainstSafeReader, browsetStart) => {
							if (ccjDetail.RegisteredAgainst == null)
								ccjDetail.RegisteredAgainst = new List<string>();

							string registeredAgainst = registeredAgainstSafeReader["Name"].ToNullString();
							ccjDetail.RegisteredAgainst.Add(registeredAgainst);

							return ActionResult.Continue;
						},
						"GetNonLimitedCcjDetailRegisteredAgainst",
						CommandSpecies.StoredProcedure,
						new QueryParameter("ExperianNonLimitedResultCcjDetailsId", ccjDetailId)
					);

					DB.ForEachRowSafe(
						(tradingNamesSafeReader, bRowsetstart) => {
							if (ccjDetail.TradingNames == null)
								ccjDetail.TradingNames = new List<TradingName>();

							var tradingName = new TradingName {
								Name = tradingNamesSafeReader["Name"].ToNullString(),
								TradingIndicator = tradingNamesSafeReader["TradingIndicator"].ToNullString(),
								TradingIndicatorDesc = tradingNamesSafeReader["TradingIndicatorDesc"].ToNullString(),
							};
							ccjDetail.TradingNames.Add(tradingName);

							return ActionResult.Continue;
						},
						"GetNonLimitedCcjDetailTradingNames",
						CommandSpecies.StoredProcedure,
						new QueryParameter("ExperianNonLimitedResultCcjDetailsId", ccjDetailId)
					);

					Data.CcjDetails.Add(ccjDetail);

					return ActionResult.Continue;
				},
				"GetNonLimitedCcjDetails",
				CommandSpecies.StoredProcedure,
				new QueryParameter("ExperianNonLimitedResultId", experianNonLimitedResultId)
			);
		} // FillCcj

		private void FillPaymentPerformanceDetails(int experianNonLimitedResultId) {
			DB.ForEachRowSafe(
				(paymentPerformanceDetailsSafeReader, bRowsetStart) => {
					if (Data.PaymentPerformanceDetails == null)
						Data.PaymentPerformanceDetails = new List<PaymentPerformanceDetail>();

					var paymentPerformanceDetail = new PaymentPerformanceDetail {
						Code = paymentPerformanceDetailsSafeReader["Code"],
						DaysBeyondTerms = paymentPerformanceDetailsSafeReader["DaysBeyondTerms"],
					};

					Data.PaymentPerformanceDetails.Add(paymentPerformanceDetail);
					return ActionResult.Continue;
				},
				"GetNonLimitedPaymentPerformanceDetails",
				CommandSpecies.StoredProcedure,
				new QueryParameter("ExperianNonLimitedResultId", experianNonLimitedResultId)
			);
		} // FillPaymentPerformanceDetails

		private void FillPreviousSearches(int experianNonLimitedResultId) {
			DB.ForEachRowSafe(
				(previousSearchesSafeReader, bRowsetStart) => {
					if (Data.PreviousSearches == null)
						Data.PreviousSearches = new List<PreviousSearch>();

					var previousSearch = new PreviousSearch {
						PreviousSearchDate = previousSearchesSafeReader["PreviousSearchDate"],
						EnquiryType = previousSearchesSafeReader["EnquiryType"].ToNullString(),
						EnquiryTypeDesc = previousSearchesSafeReader["EnquiryTypeDesc"].ToNullString(),
						CreditRequired = previousSearchesSafeReader["CreditRequired"].ToNullString(),
					};

					Data.PreviousSearches.Add(previousSearch);
					return ActionResult.Continue;
				},
				"GetNonLimitedPreviousSearches",
				CommandSpecies.StoredProcedure,
				new QueryParameter("ExperianNonLimitedResultId", experianNonLimitedResultId)
			);
		} // FillPreviousSearches

		private void FillScoreHistory() {
			var scoreHistory = DB.Fill<ScoreAtDate>(
				"GetCompanyHistory",
				CommandSpecies.StoredProcedure,
				new QueryParameter("RefNumber", refNumber)
			);

			Data.ScoreHistory = scoreHistory;
		} // FillScoreHistory

		private readonly string refNumber;

	} // class GetCompanyDataForCompanyScore
} // namespace
