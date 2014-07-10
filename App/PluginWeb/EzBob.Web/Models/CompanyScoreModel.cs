namespace EzBob.Web.Models
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text.RegularExpressions;
	using Areas.Underwriter.Models;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Experian;
	using Ezbob.ExperianParser;
	using EZBob.DatabaseLib;
	using StructureMap;
	using log4net;

	public class CompanyScoreModel
	{
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
		private readonly ExperianNonLimitedResultsRepository experianNonLimitedResultsRepository;

		public CompanyScoreModelBuilder()
		{
			experianNonLimitedResultsRepository = ObjectFactory.GetInstance<ExperianNonLimitedResultsRepository>();
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
			switch (oResult.ParsingResult)
			{
				case ParsingResult.Ok:
					var model = new CompanyScoreModel
					{
						result = CompanyScoreModel.Ok,
						dataset = oResult.Dataset,
						company_name = oResult.CompanyName,
						company_ref_num = oResult.CompanyRefNum
					};

					model.DashboardModel = BuildDashboardModel(oResult, customerId, refNumber);
					return model;
				case ParsingResult.Fail:
					return new CompanyScoreModel { result = "Failed to parse Experian response.", DashboardModel = new ComapanyDashboardModel
						{
							Error = string.Format("{0} {1}", "Failed to parse Experian response.", oResult.ErrorMsg)
						}};

				case ParsingResult.NotFound:
					return new CompanyScoreModel { result = "No data found.", DashboardModel = new ComapanyDashboardModel
						{
							Error = string.Format("{0} {1}", "No data found.", oResult.ErrorMsg)
						}};

				default:
					throw new ArgumentOutOfRangeException();
			} // switch
		}

		public ComapanyDashboardModel BuildDashboardModel(ExperianParserOutput oResult, int customerId, string refNumber)
		{
			switch (oResult.TypeOfBusinessReduced)
			{
				case TypeOfBusinessReduced.Limited:
					return BuildLimitedDashboardModel(oResult);
				case TypeOfBusinessReduced.NonLimited:
				case TypeOfBusinessReduced.Personal:
					return BuildNonLimitedDashboardModel(oResult, customerId, refNumber);
			}

			return null;
		}

		private ComapanyDashboardModel BuildNonLimitedDashboardModel(ExperianParserOutput oResult, int customerId, string refNumber)
		{
			var model = new ComapanyDashboardModel { FinDataHistories = new List<FinDataModel>(), LastFinData = new FinDataModel() };
			model.IsLimited = false;
			model.CompanyRefNum = refNumber;
			
			ExperianNonLimitedResults experianNonLimitedResult = experianNonLimitedResultsRepository.GetAll().FirstOrDefault(res => res.CustomerId == customerId && res.RefNumber == refNumber);
			if (experianNonLimitedResult != null)
			{
				model.CompanyName = experianNonLimitedResult.BusinessName;
				model.Score = experianNonLimitedResult.Score;
				model.ScoreColor = CreditBureauModelBuilder.GetScorePositionAndColor(model.Score, 100, 0).Color;
				model.CcjMonths = experianNonLimitedResult.AgeOfMostRecentCcj;
				model.Ccjs = experianNonLimitedResult.NumOfCcjsInLast12Months + experianNonLimitedResult.NumOfCcjsIn13To24Months;

				foreach (ExperianNonLimitedResultsScoreHistory scoreHistory in experianNonLimitedResult.HistoryScores)
				{
					try
					{
						model.NonLimScoreHistories.Add(new NonLimScoreHistory
						{
							Score = scoreHistory.RiskScore,
							ScoreDate = scoreHistory.Date
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
							//todo default amount
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