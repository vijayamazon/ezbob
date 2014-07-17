namespace EzBob.Web.Models
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using System.Text.RegularExpressions;
	using Areas.Underwriter.Models;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Database;
	using Ezbob.ExperianParser;
	using EZBob.DatabaseLib;
	using Ezbob.Logger;
	using log4net;

	public class CompanyScoreModel
	{
		public bool IsLimited { get; set; }
		public string BusinessName { get; set; }
		public string Address1 { get; set; }
		public string Address2 { get; set; }
		public string Address3 { get; set; }
		public string Address4 { get; set; }
		public string Address5 { get; set; }
		public string Postcode { get; set; }
		public string TelephoneNumber { get; set; }
		public string PrincipalActivities { get; set; }
		public DateTime? EarliestKnownDate { get; set; }
		public DateTime? DateOwnershipCommenced { get; set; }
		public DateTime? IncorporationDate { get; set; }
		public DateTime? DateOwnershipCeased { get; set; }
		public DateTime? LastUpdateDate { get; set; }
	
		public int? BankruptcyCountDuringOwnership { get; set; }
		public int? AgeOfMostRecentBankruptcyDuringOwnershipMonths { get; set; }
		public int? AssociatedBankruptcyCountDuringOwnership { get; set; }
		public int? AgeOfMostRecentAssociatedBankruptcyDuringOwnershipMonths { get; set; }

		public int? AgeOfMostRecentJudgmentDuringOwnershipMonths { get; set; }
		public int? TotalJudgmentCountLast12Months { get; set; }
		public int? TotalJudgmentValueLast12Months { get; set; }
		public int? TotalJudgmentCountLast13To24Months { get; set; }
		public int? TotalJudgmentValueLast13To24Months { get; set; }
		public int? ValueOfMostRecentAssociatedJudgmentDuringOwnership { get; set; }
		public int? TotalAssociatedJudgmentCountLast12Months { get; set; }
		public int? TotalAssociatedJudgmentValueLast12Months { get; set; }
		public int? TotalAssociatedJudgmentCountLast13To24Months { get; set; }
		public int? TotalAssociatedJudgmentValueLast13To24Months { get; set; }
		public int? TotalJudgmentCountLast24Months { get; set; }
		public int? TotalAssociatedJudgmentCountLast24Months { get; set; }
		public int? TotalJudgmentCountValue24Months { get; set; }
		public int? TotalAssociatedJudgmentValueLast24Months { get; set; }

		public string SupplierName { get; set; }
		public string FraudCategory { get; set; }
		public string FraudCategoryDesc { get; set; }
		public int? NumberOfAccountsPlacedForCollection { get; set; }
		public int? ValueOfAccountsPlacedForCollection { get; set; }
		public int? NumberOfAccountsPlacedForCollectionLast2Years { get; set; }
		public int? AverageDaysBeyondTermsFor0To100 { get; set; }
		public int? AverageDaysBeyondTermsFor101To1000 { get; set; }
		public int? AverageDaysBeyondTermsFor1001To10000 { get; set; }
		public int? AverageDaysBeyondTermsForOver10000 { get; set; }
		public int? AverageDaysBeyondTermsForLast3MonthsOfDataReturned { get; set; }
		public int? AverageDaysBeyondTermsForLast6MonthsOfDataReturned { get; set; }
		public int? AverageDaysBeyondTermsForLast12MonthsOfDataReturned { get; set; }
		public int? CurrentAverageDebt { get; set; }
		public int? AverageDebtLast3Months { get; set; }
		public int? AverageDebtLast12Months { get; set; }
		public string TelephoneNumberDN36 { get; set; }
		public int? RiskScore { get; set; }
		public string SearchType { get; set; }
		public string SearchTypeDesc { get; set; }
		public int? CommercialDelphiScore { get; set; }
		public string CreditRating { get; set; }
		public string CreditLimit { get; set; }
		public decimal? ProbabilityOfDefaultScore { get; set; }
		public string StabilityOdds { get; set; }
		public string RiskBand { get; set; }
		public int? NumberOfProprietorsSearched { get; set; }
		public int? NumberOfProprietorsFound { get; set; }
		public string Errors { get; set; }

		public const string Ok = "ok";

		public string result { get; set; }

		public Dictionary<string, ParsedData> dataset { get; set; }

		public string company_name { get; set; }

		public string company_ref_num { get; set; }

		public ComapanyDashboardModel DashboardModel { get; set; }
		public CompanyScoreModel[] Owners { get { return ReferenceEquals(m_oOwners, null) ? null : m_oOwners.ToArray(); } }

		public void AddOwner(CompanyScoreModel oOwner)
		{
			if (ReferenceEquals(m_oSavedOwners, null))
			{
				m_oSavedOwners = new SortedSet<string>();
				m_oOwners = new List<CompanyScoreModel>();
			} // if

			if (m_oSavedOwners.Contains(oOwner.company_ref_num))
				return;

			m_oSavedOwners.Add(oOwner.company_ref_num);
			m_oOwners.Add(oOwner);
		} // AddOwner

		private SortedSet<string> m_oSavedOwners;
		private List<CompanyScoreModel> m_oOwners;
	}

	public class ComapanyDashboardModel
	{
		public bool IsLimited { get; set; }
		public string CompanyName { get; set; }
		public string CompanyRefNum { get; set; }
		public int Score { get; set; }
		public string ScoreColor { get; set; }
		public decimal CaisBalance { get; set; }
		public int CaisAccounts { get; set; }
		public FinDataModel LastFinData { get; set; }
		public int Ccjs { get; set; }
		public int CcjMonths { get; set; }
		public int DefaultAccounts { get; set; }
		public decimal DefaultAmount { get; set; }
		public int LateAccounts { get; set; }
		public string LateStatus { get; set; }
		public List<FinDataModel> FinDataHistories { get; set; }
		public List<NonLimScoreHistory> NonLimScoreHistories { get; set; }
		public string Error { get; set; }
	}

	public class NonLimScoreHistory
	{
		public int Score { get; set; }
		public DateTime ScoreDate { get; set; }
	}

	public class FinDataModel
	{
		public decimal TangibleEquity { get; set; }
		public decimal AdjustedProfit { get; set; }
	}

	public class CompanyScoreModelBuilder
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(CompanyScoreModelBuilder));
		private readonly AConnection db;
		private readonly SafeILog log;

		public CompanyScoreModelBuilder()
		{
			log = new SafeILog(LogManager.GetLogger(typeof(CompanyScoreModelBuilder)));
			var env = new Ezbob.Context.Environment(log);
			db = new SqlConnection(env, log);
		}

		public CompanyScoreModel Create(Customer customer)
		{
			ExperianParserOutput oOutput = customer.ParseExperian(ExperianParserFacade.Target.Company);

			CompanyScoreModel oResult = BuildFromParseResult(oOutput, customer.Id, customer.Company.ExperianRefNum);

			if (oResult.result != CompanyScoreModel.Ok)
				return oResult;

			if (oOutput.TypeOfBusinessReduced == TypeOfBusinessReduced.Limited)
			{
				AddOwners(customer,
					oResult,
					"Limited Company Shareholders",
					"Registered number of a limited company which is a shareholder",
					"Description of Shareholder"
					);

				AddOwners(customer,
					oResult,
					"Limited Company Ownership Details",
					"Registered Number of the Current Ultimate Parent Company",
					"Registered Name of the Current Ultimate Parent Company"
					);
			} // if

			return oResult;
		} // Create

		private void AddOwners(Customer customer, CompanyScoreModel oPossession, string sGroupName, string sCompanyNumberField, string sCompanyNameField)
		{
			if (oPossession.dataset.ContainsKey(sGroupName))
			{
				List<ParsedDataItem> aryShareholders = oPossession.dataset[sGroupName].Data;

				foreach (var oShareholder in aryShareholders)
				{
					if (oShareholder.ContainsKey(sCompanyNumberField))
					{
						var sNumber = oShareholder[sCompanyNumberField];

						if (!string.IsNullOrWhiteSpace(sNumber))
						{
							var oOwner = BuildFromParseResult(
								ExperianParserFacade.Invoke(
									sNumber,
									oShareholder[sCompanyNameField] ?? "",
									ExperianParserFacade.Target.Company,
									TypeOfBusinessReduced.Limited
								),
								customer.Id,
								customer.Company.ExperianRefNum
							);

							if (oOwner.result == CompanyScoreModel.Ok)
							{
								oPossession.AddOwner(oOwner);
							} // if owner was found
						} // if company number is not empty
					} // if owner has a company number
				} // for each owner
			} // if contains list of owners
		} // AddOwners

		private CompanyScoreModel BuildFromParseResult(ExperianParserOutput oResult, int customerId, string refNumber)
		{
			bool isLimited = true;
			DataTable dt = db.ExecuteReader(
				"GetCompanyIsLimited",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("RefNumber", refNumber));
			
			if (dt.Rows.Count == 1)
			{
				var sr = new SafeReader(dt.Rows[0]);
				isLimited = sr["IsLimited"];
			}

			if (isLimited)
			{
				switch (oResult.ParsingResult)
				{
					case ParsingResult.Ok:
						var model = new CompanyScoreModel
							{
								result = CompanyScoreModel.Ok,
								dataset = oResult.Dataset,
								company_name = oResult.CompanyName,
								company_ref_num = oResult.CompanyRefNum,
								IsLimited = true
							};

						model.DashboardModel = BuildDashboardModel(oResult, customerId, refNumber);
						return model;
					case ParsingResult.Fail:
						return new CompanyScoreModel
							{
								result = "Failed to parse Experian response.",
								DashboardModel = new ComapanyDashboardModel
									{
										Error = string.Format("{0} {1}", "Failed to parse Experian response.", oResult.ErrorMsg)
									}
							};

					case ParsingResult.NotFound:
						return new CompanyScoreModel
							{
								result = "No data found.",
								DashboardModel = new ComapanyDashboardModel
									{
										Error = string.Format("{0} {1}", "No data found.", oResult.ErrorMsg)
									}
							};

					default:
						throw new ArgumentOutOfRangeException();
				} // switch
			}

			// Fill non limited data
			var nonLimitedModel = new CompanyScoreModel
			{
				result = CompanyScoreModel.Ok
			};

			DataTable nonLimitedDataTable = db.ExecuteReader(
				"GetNonLimitedDataForCompanyScore",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("RefNumber", refNumber));

			if (nonLimitedDataTable.Rows.Count == 1)
			{
				var nonLimitedSafeReader = new SafeReader(nonLimitedDataTable.Rows[0]);

				nonLimitedModel.company_name = nonLimitedSafeReader["BusinessName"].ToNullString();
				nonLimitedModel.company_ref_num = refNumber;
				nonLimitedModel.Address1 = nonLimitedSafeReader["Address1"].ToNullString();
				nonLimitedModel.Address1 = nonLimitedSafeReader["Address1"].ToNullString();
				nonLimitedModel.Address2 = nonLimitedSafeReader["Address2"].ToNullString();
				nonLimitedModel.Address3 = nonLimitedSafeReader["Address3"].ToNullString();
				nonLimitedModel.Address4 = nonLimitedSafeReader["Address4"].ToNullString();
				nonLimitedModel.Address5 = nonLimitedSafeReader["Address5"].ToNullString();
				nonLimitedModel.Postcode = nonLimitedSafeReader["Postcode"].ToNullString();
				nonLimitedModel.TelephoneNumber = nonLimitedSafeReader["TelephoneNumber"].ToNullString();
				nonLimitedModel.PrincipalActivities = nonLimitedSafeReader["PrincipalActivities"].ToNullString();
				nonLimitedModel.EarliestKnownDate = nonLimitedSafeReader["EarliestKnownDate"];
				nonLimitedModel.DateOwnershipCommenced = nonLimitedSafeReader["DateOwnershipCommenced"];
				nonLimitedModel.IncorporationDate = nonLimitedSafeReader["IncorporationDate"];
				nonLimitedModel.DateOwnershipCeased = nonLimitedSafeReader["DateOwnershipCeased"];
				nonLimitedModel.LastUpdateDate = nonLimitedSafeReader["LastUpdateDate"];
				nonLimitedModel.BankruptcyCountDuringOwnership = nonLimitedSafeReader["BankruptcyCountDuringOwnership"];
				nonLimitedModel.AgeOfMostRecentBankruptcyDuringOwnershipMonths = nonLimitedSafeReader["AgeOfMostRecentBankruptcyDuringOwnershipMonths"];
				nonLimitedModel.AssociatedBankruptcyCountDuringOwnership = nonLimitedSafeReader["AssociatedBankruptcyCountDuringOwnership"];
				nonLimitedModel.AgeOfMostRecentAssociatedBankruptcyDuringOwnershipMonths = nonLimitedSafeReader["AgeOfMostRecentAssociatedBankruptcyDuringOwnershipMonths"];
				nonLimitedModel.AgeOfMostRecentJudgmentDuringOwnershipMonths = nonLimitedSafeReader["AgeOfMostRecentJudgmentDuringOwnershipMonths"];
				nonLimitedModel.TotalJudgmentCountLast12Months = nonLimitedSafeReader["TotalJudgmentCountLast12Months"];
				nonLimitedModel.TotalJudgmentValueLast12Months = nonLimitedSafeReader["TotalJudgmentValueLast12Months"];
				nonLimitedModel.TotalJudgmentCountLast13To24Months = nonLimitedSafeReader["TotalJudgmentCountLast13To24Months"];
				nonLimitedModel.TotalJudgmentValueLast13To24Months = nonLimitedSafeReader["TotalJudgmentValueLast13To24Months"];
				nonLimitedModel.ValueOfMostRecentAssociatedJudgmentDuringOwnership = nonLimitedSafeReader["ValueOfMostRecentAssociatedJudgmentDuringOwnership"];
				nonLimitedModel.TotalAssociatedJudgmentCountLast12Months = nonLimitedSafeReader["TotalAssociatedJudgmentCountLast12Months"];
				nonLimitedModel.TotalAssociatedJudgmentValueLast12Months = nonLimitedSafeReader["TotalAssociatedJudgmentValueLast12Months"];
				nonLimitedModel.TotalAssociatedJudgmentCountLast13To24Months = nonLimitedSafeReader["TotalAssociatedJudgmentCountLast13To24Months"];
				nonLimitedModel.TotalAssociatedJudgmentValueLast13To24Months = nonLimitedSafeReader["TotalAssociatedJudgmentValueLast13To24Months"];
				nonLimitedModel.TotalJudgmentCountLast24Months = nonLimitedSafeReader["TotalJudgmentCountLast24Months"];
				nonLimitedModel.TotalAssociatedJudgmentCountLast24Months = nonLimitedSafeReader["TotalAssociatedJudgmentCountLast24Months"];
				nonLimitedModel.TotalJudgmentCountValue24Months = nonLimitedSafeReader["TotalJudgmentCountValue24Months"];
				nonLimitedModel.TotalAssociatedJudgmentValueLast24Months = nonLimitedSafeReader["TotalAssociatedJudgmentValueLast24Months"];
				nonLimitedModel.SupplierName = nonLimitedSafeReader["SupplierName"].ToNullString();
				nonLimitedModel.FraudCategory = nonLimitedSafeReader["FraudCategory"].ToNullString();
				nonLimitedModel.FraudCategoryDesc = nonLimitedSafeReader["FraudCategoryDesc"].ToNullString();
				nonLimitedModel.NumberOfAccountsPlacedForCollection = nonLimitedSafeReader["NumberOfAccountsPlacedForCollection"];
				nonLimitedModel.ValueOfAccountsPlacedForCollection = nonLimitedSafeReader["ValueOfAccountsPlacedForCollection"];
				nonLimitedModel.NumberOfAccountsPlacedForCollectionLast2Years = nonLimitedSafeReader["NumberOfAccountsPlacedForCollectionLast2Years"];
				nonLimitedModel.AverageDaysBeyondTermsFor0To100 = nonLimitedSafeReader["AverageDaysBeyondTermsFor0To100"];
				nonLimitedModel.AverageDaysBeyondTermsFor101To1000 = nonLimitedSafeReader["AverageDaysBeyondTermsFor101To1000"];
				nonLimitedModel.AverageDaysBeyondTermsFor1001To10000 = nonLimitedSafeReader["AverageDaysBeyondTermsFor1001To10000"];
				nonLimitedModel.AverageDaysBeyondTermsForOver10000 = nonLimitedSafeReader["AverageDaysBeyondTermsForOver10000"];
				nonLimitedModel.AverageDaysBeyondTermsForLast3MonthsOfDataReturned = nonLimitedSafeReader["AverageDaysBeyondTermsForLast3MonthsOfDataReturned"];
				nonLimitedModel.AverageDaysBeyondTermsForLast6MonthsOfDataReturned = nonLimitedSafeReader["AverageDaysBeyondTermsForLast6MonthsOfDataReturned"];
				nonLimitedModel.AverageDaysBeyondTermsForLast12MonthsOfDataReturned = nonLimitedSafeReader["AverageDaysBeyondTermsForLast12MonthsOfDataReturned"];
				nonLimitedModel.CurrentAverageDebt = nonLimitedSafeReader["CurrentAverageDebt"];
				nonLimitedModel.AverageDebtLast3Months = nonLimitedSafeReader["AverageDebtLast3Months"];
				nonLimitedModel.AverageDebtLast12Months = nonLimitedSafeReader["AverageDebtLast12Months"];
				nonLimitedModel.TelephoneNumberDN36 = nonLimitedSafeReader["TelephoneNumberDN36"].ToNullString();
				nonLimitedModel.RiskScore = nonLimitedSafeReader["RiskScore"];
				nonLimitedModel.SearchType = nonLimitedSafeReader["SearchType"].ToNullString();
				nonLimitedModel.SearchTypeDesc = nonLimitedSafeReader["SearchTypeDesc"].ToNullString();
				nonLimitedModel.CommercialDelphiScore = nonLimitedSafeReader["CommercialDelphiScore"];
				nonLimitedModel.CreditRating = nonLimitedSafeReader["CreditRating"].ToNullString();
				nonLimitedModel.CreditLimit = nonLimitedSafeReader["CreditLimit"].ToNullString();
				nonLimitedModel.ProbabilityOfDefaultScore = nonLimitedSafeReader["ProbabilityOfDefaultScore"];
				nonLimitedModel.StabilityOdds = nonLimitedSafeReader["StabilityOdds"].ToNullString();
				nonLimitedModel.RiskBand = nonLimitedSafeReader["RiskBand"].ToNullString();
				nonLimitedModel.NumberOfProprietorsSearched = nonLimitedSafeReader["NumberOfProprietorsSearched"];
				nonLimitedModel.NumberOfProprietorsFound = nonLimitedSafeReader["NumberOfProprietorsFound"];
				nonLimitedModel.Errors = nonLimitedSafeReader["Errors"].ToNullString();
			}

			return nonLimitedModel;
		}

		public ComapanyDashboardModel BuildDashboardModel(ExperianParserOutput oResult, int customerId, string refNumber)
		{
			switch (oResult.TypeOfBusinessReduced)
			{
				case TypeOfBusinessReduced.Limited:
					return BuildLimitedDashboardModel(oResult);
				case TypeOfBusinessReduced.NonLimited:
				case TypeOfBusinessReduced.Personal:
					return BuildNonLimitedDashboardModel(customerId, refNumber);
			}

			return null;
		}

		private ComapanyDashboardModel BuildNonLimitedDashboardModel(int customerId, string refNumber)
		{
			var model = new ComapanyDashboardModel { FinDataHistories = new List<FinDataModel>(), LastFinData = new FinDataModel() };
			model.IsLimited = false;
			model.CompanyRefNum = refNumber;
			
			
			DataTable dt = db.ExecuteReader(
				"GetNonLimitedCompanyDashboardDetails",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("RefNumber", refNumber));

			if (dt.Rows.Count == 1)
			{
				var sr = new SafeReader(dt.Rows[0]);

				model.CompanyName = sr["BusinessName"];
				model.Score = sr["RiskScore"];
				model.ScoreColor = CreditBureauModelBuilder.GetScorePositionAndColor(model.Score, 100, 0).Color;
				model.CcjMonths = sr["AgeOfMostRecentJudgmentDuringOwnershipMonths"];
				model.Ccjs = sr["TotalJudgmentCountLast24Months"];
				model.Ccjs += sr["TotalAssociatedJudgmentCountLast24Months"];

				DataTable scoreHistoryDataTable = db.ExecuteReader(
					"GetNonLimitedCompanyScoreHistory",
					CommandSpecies.StoredProcedure,
					new QueryParameter("CustomerId", customerId),
					new QueryParameter("RefNumber", refNumber));

				foreach (DataRow row in scoreHistoryDataTable.Rows)
				{
					var scoreHistorySafeReader = new SafeReader(row);
					try
					{
						model.NonLimScoreHistories.Add(new NonLimScoreHistory
						{
							Score = scoreHistorySafeReader["RiskScore"],
							ScoreDate = scoreHistorySafeReader["Date"]
						});
					}
					catch (Exception ex)
					{
						Log.Warn("failed to parse non limited score history", ex);
					}
				}
			}

			return model;
		}

		private ComapanyDashboardModel BuildLimitedDashboardModel(ExperianParserOutput oResult)
		{
			var model = new ComapanyDashboardModel { FinDataHistories = new List<FinDataModel>(), LastFinData = new FinDataModel() };
			model.IsLimited = true;
			var scoreStr = oResult.GetValue("Limited Company Commerical Delphi Score", "Commercial Delphi Score");
			model.CompanyName = oResult.GetValue("Limited Company Identification", "Company Name");
			model.CompanyRefNum = oResult.GetValue("Limited Company Identification", "Registered Number");
			model.Score = string.IsNullOrEmpty(scoreStr) ? 0 : int.Parse(scoreStr);
			model.ScoreColor = CreditBureauModelBuilder.GetScorePositionAndColor(model.Score, 100, 0).Color;
			var ccjAge = oResult.GetValue("Limited Company CCJ Summary", "Age of Most Recent CCJ/Decree (Months)");
			model.CcjMonths = string.IsNullOrEmpty(ccjAge) ? 0 : int.Parse(ccjAge);
			var numOfCCjsYear = oResult.GetValue("Limited Company CCJ Summary", "Number of CCJs During Last 12 Months");
			var numOfCCjs2Years = oResult.GetValue("Limited Company CCJ Summary", "Number of CCJs Between 13 And 24 Months Ago");
			model.Ccjs = (string.IsNullOrEmpty(numOfCCjsYear) ? 0 : int.Parse(numOfCCjsYear)) +
						 (string.IsNullOrEmpty(numOfCCjs2Years) ? 0 : int.Parse(numOfCCjs2Years));
			var worstStatusAll = "0";
			//Calc and add Cais Balance
			if (oResult.Dataset.ContainsKey("Limited Company Installment CAIS Details"))
			{
				if (oResult.Dataset["Limited Company Installment CAIS Details"].Data.Any())
				{
					model.CaisBalance = 0;
					foreach (var cais in oResult.Dataset["Limited Company Installment CAIS Details"].Data)
					{
						var state = GetValue(cais, "Account State");
						var balance = GetDecimalValueFromDataItem(cais, "Current Balance");

						//Sum all accounts balance that are not settled
						if (!string.IsNullOrEmpty(state) && state[0] != 'S')
						{
							model.CaisBalance += balance;
							model.CaisAccounts++;
						}

						if (!string.IsNullOrEmpty(state) && state[0] == 'D')
						{
							model.DefaultAccounts++;
							model.DefaultAmount += GetDecimalValueFromDataItem(cais, "Default Balance");
						}
						else
						{
							var status = GetValue(cais, "Account status (Last 12 Account Statuses");
							var worstStatus = CreditBureauModelBuilder.GetWorstStatus(Regex.Split(status, string.Empty));
							if (worstStatus != "0")
							{
								model.LateAccounts++;
								worstStatusAll = CreditBureauModelBuilder.GetWorstStatus(worstStatusAll, worstStatus);
							}

						}
					}
				}
			}
			string date;
			model.LateStatus = CreditBureauModelBuilder.GetAccountStatusString(worstStatusAll, out date);

			//Calc and add tangible equity and adjusted profit
			const string sKey = "Limited Company Financial Details IFRS & UK GAAP";
			ParsedData oParsedData = oResult.Dataset.ContainsKey(sKey) ? oResult.Dataset[sKey] : null;
			if ((oParsedData != null) && (oParsedData.Data != null) && (oParsedData.Data.Count > 0))
			{
				for (var i = 0; i < oParsedData.Data.Count - 1; i++)
				{
					ParsedDataItem parsedDataItem = oParsedData.Data[i];

					decimal totalShareFund = GetDecimalValueFromDataItem(parsedDataItem, "TotalShareFund");
					decimal inTngblAssets = GetDecimalValueFromDataItem(parsedDataItem, "InTngblAssets");
					decimal debtorsDirLoans = GetDecimalValueFromDataItem(parsedDataItem, "DebtorsDirLoans");
					decimal credDirLoans = GetDecimalValueFromDataItem(parsedDataItem, "CredDirLoans");
					decimal onClDirLoans = GetDecimalValueFromDataItem(parsedDataItem, "OnClDirLoans");

					decimal tangibleEquity = totalShareFund - inTngblAssets - debtorsDirLoans + credDirLoans + onClDirLoans;

					if (oParsedData.Data.Count > 1)
					{
						ParsedDataItem parsedDataItemPrev = oParsedData.Data[i + 1];

						decimal retainedEarnings = GetDecimalValueFromDataItem(parsedDataItem, "RetainedEarnings");
						decimal retainedEarningsPrev = GetDecimalValueFromDataItem(parsedDataItemPrev, "RetainedEarnings");
						decimal fixedAssetsPrev = GetDecimalValueFromDataItem(parsedDataItemPrev, "TngblAssets");

						decimal adjustedProfit = retainedEarnings - retainedEarningsPrev + fixedAssetsPrev / 5;

						var fin = new FinDataModel { TangibleEquity = tangibleEquity, AdjustedProfit = adjustedProfit };
						model.FinDataHistories.Add(fin);
						if (i == 0)
						{
							model.LastFinData = fin;
						}
					} // if
				}
			} // if
			return model;
		}
		
		private string GetValue(ParsedDataItem dataItem, string key)
		{
			if (dataItem.Values.ContainsKey(key))
			{
				return dataItem.Values[key];
			}
			return null;
		}

		private decimal GetDecimalValueFromDataItem(ParsedDataItem parsedDataItem, string requiredValueName)
		{
			if (!parsedDataItem.Values.ContainsKey(requiredValueName))
				return 0;

			string sValue = parsedDataItem.Values[requiredValueName];

			decimal result;

			if (Transformation.ParseMoney(sValue, out result))
				return result;

			if (decimal.TryParse(sValue, out result))
				return result;

			return 0;
		} // GetDecimalValueFromDataItem

		// BuildFromParseResult
	} // class CompanyScoreModelBuilder
} // namespace