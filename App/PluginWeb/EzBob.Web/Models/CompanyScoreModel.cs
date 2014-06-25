namespace EzBob.Web.Models
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Areas.Underwriter.Models;
	using CommonLib;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.ExperianParser;
	using EZBob.DatabaseLib;

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
		public int DefaultAmount { get; set; }
		public int LateAccounts { get; set; }
		public int LateAmount { get; set; }
		public List<FinDataModel> FinDataHistories { get; set; }

		
	}

	public class FinDataModel
	{
		public decimal TangibleEquity { get; set; }
		public decimal AdjustedProfit { get; set; }
	}

	// CompanyScoreModel

	public class CompanyScoreModelBuilder
	{
		public CompanyScoreModel Create(Customer customer)
		{
			ExperianParserOutput oOutput = customer.ParseExperian(ExperianParserFacade.Target.Company);

			CompanyScoreModel oResult = BuildFromParseResult(oOutput);

			if (oResult.result != CompanyScoreModel.Ok)
				return oResult;

			if (oOutput.TypeOfBusinessReduced == TypeOfBusinessReduced.Limited)
			{
				AddOwners(
					oResult,
					"Limited Company Shareholders",
					"Registered number of a limited company which is a shareholder",
					"Description of Shareholder"
					);

				AddOwners(
					oResult,
					"Limited Company Ownership Details",
					"Registered Number of the Current Ultimate Parent Company",
					"Registered Name of the Current Ultimate Parent Company"
					);
			} // if

			return oResult;
		} // Create

		private void AddOwners(CompanyScoreModel oPossession, string sGroupName, string sCompanyNumberField, string sCompanyNameField)
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
								)
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

		private CompanyScoreModel BuildFromParseResult(ExperianParserOutput oResult)
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

					model.DashboardModel = BuildDashboardModel(oResult);
					return model;
				case ParsingResult.Fail:
					return new CompanyScoreModel { result = "Failed to parse Experian response." };

				case ParsingResult.NotFound:
					return new CompanyScoreModel { result = "No data found." };

				default:
					throw new ArgumentOutOfRangeException();
			} // switch
		}

		private ComapanyDashboardModel BuildDashboardModel(ExperianParserOutput oResult)
		{
			var model = new ComapanyDashboardModel{ FinDataHistories = new List<FinDataModel>()};
			
			if (oResult.TypeOfBusinessReduced == TypeOfBusinessReduced.Limited)
			{
				var scoreStr = GetValue(oResult, "Limited Company Commerical Delphi Score", "Commercial Delphi Score");
				model.CompanyName = GetValue(oResult, "Limited Company Identification", "Company Name");
				model.CompanyRefNum = GetValue(oResult, "Limited Company Identification", "Registered Number");
				model.Score = string.IsNullOrEmpty(scoreStr) ? 0 : int.Parse(scoreStr);
				model.ScoreColor = CreditBureauModelBuilder.GetScorePositionAndColor(model.Score, 100, 0).Color;

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
								//todo default amount
							}

							//todo late accounts
						}
					}
				}


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
							ParsedDataItem parsedDataItemPrev = oParsedData.Data[i+1];

							decimal retainedEarnings = GetDecimalValueFromDataItem(parsedDataItem, "RetainedEarnings");
							decimal retainedEarningsPrev = GetDecimalValueFromDataItem(parsedDataItemPrev, "RetainedEarnings");
							decimal fixedAssetsPrev = GetDecimalValueFromDataItem(parsedDataItemPrev, "TngblAssets");

							decimal adjustedProfit = retainedEarnings - retainedEarningsPrev + fixedAssetsPrev/5;

							var fin = new FinDataModel {TangibleEquity = tangibleEquity, AdjustedProfit = adjustedProfit};
							model.FinDataHistories.Add(fin);
							if (i == 0)
							{
								model.LastFinData = fin;
							}
						} // if
					}
				} // if
			}

			return model;
		}

		private string GetValue(ExperianParserOutput oResult, string section, string key)
		{
			if (oResult.Dataset.ContainsKey(section))
			{
				if (oResult.Dataset[section].Data.Any())
				{
					if (oResult.Dataset[section].Data[0].Values.ContainsKey(key))
					{
						return oResult.Dataset[section].Data[0].Values[key];
					}
				}
			}
			return null;
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