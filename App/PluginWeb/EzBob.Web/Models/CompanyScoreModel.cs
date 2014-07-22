namespace EzBob.Web.Models {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using System.Text.RegularExpressions;
	using Areas.Underwriter.Models;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.ExperianParser;
	using EZBob.DatabaseLib;
	using Ezbob.Logger;
	using Infrastructure;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using StructureMap;
	using log4net;

	public class CompanyScoreModel {
		public CompanyData Data { get; set; }

		public const string Ok = "ok";

		public string result { get; set; }

		public Dictionary<string, ParsedData> dataset { get; set; }

		public string company_name { get; set; }

		public string company_ref_num { get; set; }

		public ComapanyDashboardModel DashboardModel { get; set; }

		public CompanyScoreModel[] Owners { get { return ReferenceEquals(m_oOwners, null) ? null : m_oOwners.ToArray(); } }

		public void AddOwner(CompanyScoreModel oOwner) {
			if (ReferenceEquals(m_oSavedOwners, null)) {
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
	} // class CompanyScoreModel

	public class ComapanyDashboardModel {
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
	} // class ComapanyDashboardModel

	public class NonLimScoreHistory {
		public int Score { get; set; }
		public DateTime ScoreDate { get; set; }
	} // class NonLimScoreHistory

	public class FinDataModel {
		public decimal TangibleEquity { get; set; }
		public decimal AdjustedProfit { get; set; }
	} // class FinDataModel

	public class CompanyScoreModelBuilder {
		private readonly ServiceClient serviceClient = new ServiceClient();
		private readonly IWorkplaceContext context = ObjectFactory.GetInstance<IWorkplaceContext>();
		private static readonly ILog Log = LogManager.GetLogger(typeof(CompanyScoreModelBuilder));
		private readonly AConnection db;
		private readonly SafeILog log;

		public CompanyScoreModelBuilder() {
			log = new SafeILog(LogManager.GetLogger(typeof(CompanyScoreModelBuilder)));
			var env = new Ezbob.Context.Environment(log);
			db = new SqlConnection(env, log);
		} // constructor

		public CompanyScoreModel Create(Customer customer) {
			bool bHasCompany = false;
			TypeOfBusinessReduced nBusinessType = TypeOfBusinessReduced.Personal;

			if (customer != null) {
				if (customer.Company != null) {
					nBusinessType = customer.Company.TypeOfBusiness.Reduce();
					bHasCompany = true;
				} // if
			} // if

			if (!bHasCompany) {
				return new CompanyScoreModel {
					result = "No data found.",
					DashboardModel = new ComapanyDashboardModel { Error = "No data found.", },
				};
			} // if

			if (nBusinessType != TypeOfBusinessReduced.Limited) {
				CompanyDataForCompanyScoreActionResult ar = serviceClient.Instance.GetCompanyDataForCompanyScore(
					context.UserId,
					customer.Id,
					customer.Company.ExperianRefNum
				);

				return new CompanyScoreModel {
					result = CompanyScoreModel.Ok,
					company_name = ar.Data.BusinessName,
					company_ref_num = customer.Company.ExperianRefNum,
					Data = ar.Data,
				};
			} // if

			CompanyScoreModel oResult = BuildFromParseResult(
				customer.ParseExperian(ExperianParserFacade.Target.Company)
			);

			if (oResult.result != CompanyScoreModel.Ok)
				return oResult;

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

			return oResult;
		} // Create

		private void AddOwners(CompanyScoreModel oPossession, string sGroupName, string sCompanyNumberField, string sCompanyNameField) {
			if (oPossession.dataset == null) // TODO: looks like this is affected by ExperianLtd
				return;

			if (oPossession.dataset.ContainsKey(sGroupName)) {
				List<ParsedDataItem> aryShareholders = oPossession.dataset[sGroupName].Data;

				foreach (var oShareholder in aryShareholders) {
					if (oShareholder.ContainsKey(sCompanyNumberField)) {
						var sNumber = oShareholder[sCompanyNumberField];

						if (!string.IsNullOrWhiteSpace(sNumber)) {
							var oOwner = BuildFromParseResult(
								ExperianParserFacade.Invoke(
									sNumber,
									oShareholder[sCompanyNameField] ?? "",
									ExperianParserFacade.Target.Company,
									TypeOfBusinessReduced.Limited
								)
							);

							if (oOwner.result == CompanyScoreModel.Ok)
								oPossession.AddOwner(oOwner);
						} // if company number is not empty
					} // if owner has a company number
				} // for each owner
			} // if contains list of owners
		} // AddOwners

		private CompanyScoreModel BuildFromParseResult(ExperianParserOutput oResult) {
			switch (oResult.ParsingResult) {
			case ParsingResult.Ok:
				return new CompanyScoreModel {
					result = CompanyScoreModel.Ok,
					dataset = oResult.Dataset,
					company_name = oResult.CompanyName,
					company_ref_num = oResult.CompanyRefNum,
					Data = null,
					DashboardModel = BuildLimitedDashboardModel(oResult),
				};

			case ParsingResult.Fail:
				return new CompanyScoreModel {
					result = "Failed to parse Experian response.",
					DashboardModel = new ComapanyDashboardModel {
						Error = string.Format("{0} {1}", "Failed to parse Experian response.", oResult.ErrorMsg)
					}
				};

			case ParsingResult.NotFound:
				return new CompanyScoreModel {
					result = "No data found.",
					DashboardModel = new ComapanyDashboardModel {
						Error = string.Format("{0} {1}", "No data found.", oResult.ErrorMsg)
					}
				};

			default:
				throw new ArgumentOutOfRangeException();
			} // switch
		} // BuildFromParseResult

		private ComapanyDashboardModel BuildNonLimitedDashboardModel(int customerId, string refNumber) {
			var model = new ComapanyDashboardModel { FinDataHistories = new List<FinDataModel>(), LastFinData = new FinDataModel() };
			model.IsLimited = false;
			model.CompanyRefNum = refNumber;


			DataTable dt = db.ExecuteReader(
				"GetNonLimitedCompanyDashboardDetails",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerId", customerId),
				new QueryParameter("RefNumber", refNumber));

			if (dt.Rows.Count == 1) {
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

				foreach (DataRow row in scoreHistoryDataTable.Rows) {
					var scoreHistorySafeReader = new SafeReader(row);
					try {
						model.NonLimScoreHistories.Add(new NonLimScoreHistory {
							Score = scoreHistorySafeReader["RiskScore"],
							ScoreDate = scoreHistorySafeReader["Date"]
						});
					}
					catch (Exception ex) {
						Log.Warn("failed to parse non limited score history", ex);
					}
				}
			}

			return model;
		} // BuildNonLimitedDashboardModel

		public ComapanyDashboardModel BuildLimitedDashboardModel(ExperianParserOutput oResult) {
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
			if (oResult.Dataset.ContainsKey("Limited Company Installment CAIS Details")) {
				if (oResult.Dataset["Limited Company Installment CAIS Details"].Data.Any()) {
					model.CaisBalance = 0;
					foreach (var cais in oResult.Dataset["Limited Company Installment CAIS Details"].Data) {
						var state = GetValue(cais, "Account State");
						var balance = GetDecimalValueFromDataItem(cais, "Current Balance");

						//Sum all accounts balance that are not settled
						if (!string.IsNullOrEmpty(state) && state[0] != 'S') {
							model.CaisBalance += balance;
							model.CaisAccounts++;
						} // if

						if (!string.IsNullOrEmpty(state) && state[0] == 'D') {
							model.DefaultAccounts++;
							model.DefaultAmount += GetDecimalValueFromDataItem(cais, "Default Balance");
						}
						else {
							var status = GetValue(cais, "Account status (Last 12 Account Statuses");
							var worstStatus = CreditBureauModelBuilder.GetWorstStatus(Regex.Split(status, string.Empty));
							if (worstStatus != "0") {
								model.LateAccounts++;
								worstStatusAll = CreditBureauModelBuilder.GetWorstStatus(worstStatusAll, worstStatus);
							}

						} // if
					} // for each
				} // if
			} // if
			string date;
			model.LateStatus = CreditBureauModelBuilder.GetAccountStatusString(worstStatusAll, out date);

			//Calc and add tangible equity and adjusted profit
			const string sKey = "Limited Company Financial Details IFRS & UK GAAP";
			ParsedData oParsedData = oResult.Dataset.ContainsKey(sKey) ? oResult.Dataset[sKey] : null;
			if ((oParsedData != null) && (oParsedData.Data != null) && (oParsedData.Data.Count > 0)) {
				for (var i = 0; i < oParsedData.Data.Count - 1; i++) {
					ParsedDataItem parsedDataItem = oParsedData.Data[i];

					decimal totalShareFund = GetDecimalValueFromDataItem(parsedDataItem, "TotalShareFund");
					decimal inTngblAssets = GetDecimalValueFromDataItem(parsedDataItem, "InTngblAssets");
					decimal debtorsDirLoans = GetDecimalValueFromDataItem(parsedDataItem, "DebtorsDirLoans");
					decimal credDirLoans = GetDecimalValueFromDataItem(parsedDataItem, "CredDirLoans");
					decimal onClDirLoans = GetDecimalValueFromDataItem(parsedDataItem, "OnClDirLoans");

					decimal tangibleEquity = totalShareFund - inTngblAssets - debtorsDirLoans + credDirLoans + onClDirLoans;

					if (oParsedData.Data.Count > 1) {
						ParsedDataItem parsedDataItemPrev = oParsedData.Data[i + 1];

						decimal retainedEarnings = GetDecimalValueFromDataItem(parsedDataItem, "RetainedEarnings");
						decimal retainedEarningsPrev = GetDecimalValueFromDataItem(parsedDataItemPrev, "RetainedEarnings");
						decimal fixedAssetsPrev = GetDecimalValueFromDataItem(parsedDataItemPrev, "TngblAssets");

						decimal adjustedProfit = retainedEarnings - retainedEarningsPrev + fixedAssetsPrev / 5;

						var fin = new FinDataModel { TangibleEquity = tangibleEquity, AdjustedProfit = adjustedProfit };
						model.FinDataHistories.Add(fin);
						if (i == 0) {
							model.LastFinData = fin;
						}
					} // if
				}
			} // if

			return model;
		} // BuildLimitedDashboardModel

		private string GetValue(ParsedDataItem dataItem, string key) {
			return dataItem.Values.ContainsKey(key) ? dataItem.Values[key] : null;
		} // GetValue

		private decimal GetDecimalValueFromDataItem(ParsedDataItem parsedDataItem, string requiredValueName) {
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
	} // class CompanyScoreModelBuilder
} // namespace