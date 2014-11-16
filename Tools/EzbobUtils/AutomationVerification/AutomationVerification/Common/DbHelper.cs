namespace AutomationCalculator.Common
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class DbHelper
	{
		private static ASafeLog _log;
		private static AConnection _db;
		public DbHelper(AConnection db, ASafeLog log)
		{
			_log = log;
			_db = db;
		}

		public DbHelper(ASafeLog log)
		{
			_log = log;
			_db = new SqlConnection(log);
		}

		public RejectionConstants GetRejectionConstants()
		{
			var sr = _db.ExecuteEnumerable("AV_RejectionConstants", CommandSpecies.StoredProcedure);
			var consts = new RejectionConstants();
			foreach (SafeReader row in sr)
			{
				switch (row["Name"].ToString())
				{
					case "LowCreditScore":
						consts.MinCreditScore = row["Value"];
						break;
					case "TotalAnnualTurnover":
						consts.MinAnnualTurnover = row["Value"];
						break;
					case "TotalThreeMonthTurnover":
						consts.MinThreeMonthTurnover = row["Value"];
						break;
					case "Reject_Defaults_CreditScore":
						consts.DefaultScoreBelow = row["Value"];
						break;
					case "Reject_Defaults_AccountsNum":
						break;
					case "Reject_Defaults_Amount":
						consts.DefaultMinAmount = row["Value"];
						break;
					case "Reject_Minimal_Seniority":
						consts.MinMarketPlaceSeniorityDays = row["Value"];
						break;
					case "AutoRejectionException_CreditScore":
						consts.NoRejectIfCreditScoreAbove = row["Value"];
						break;
					case "AutoRejectionException_AnualTurnover":
						consts.NoRejectIfTotalAnnualTurnoverAbove = row["Value"];
						break;
					case "RejectionExceptionMaxCompanyScore":
						consts.NoRejectIfCompanyCreditScoreAbove = row["Value"];
						break;
					case "RejectionExceptionMaxCompanyScoreForMpError":
						consts.AutoRejectIfErrorInAtLeastOneMPMinCompanyScore = row["Value"];
						break;
					case "RejectionExceptionMaxConsumerScoreForMpError":
						consts.AutoRejectIfErrorInAtLeastOneMPMinScore = row["Value"];
						break;
					case "RejectionCompanyScore":
						consts.MinCompanyCreditScore = row["Value"];
						break;
					case "Reject_LowOfflineAnnualRevenue":
						consts.LowOfflineAnnualRevenue = row["Value"];
						break;
					case "Reject_LowOfflineQuarterRevenue":
						consts.LowOfflineQuarterRevenue = row["Value"];
						break;
					case "Reject_LateLastMonthsNum":
						consts.LateAccountLastMonth = row["Value"];
						break;
					case "Reject_NumOfLateAccounts":
						consts.LateAccountMinNumber = row["Value"];
						break;
					case "RejectionLastValidLate":
						consts.LateAccountMinDays = row["Value"];
						break;
					case "Reject_Defaults_CompanyScore":
						consts.DefaultCompanyScoreBelow = row["Value"];
						break;
					case "Reject_Defaults_CompanyAccountsNum":
						consts.DefaultCompanyMinAccountsNum = row["Value"];
						break;
					case "Reject_Defaults_CompanyAmount":
						consts.DefaultCompanyMinAmount = row["Value"];
						break;	
					default:
						break;
				}
			}

			return consts;
		}

		public DateTime? GetCustomerBirthDate(int customerId)
		{
			return _db.ExecuteScalar<DateTime?>("AV_GetCustomerBirthDate", new QueryParameter("@CustomerId", customerId));
		}

		/// <summary>
		/// Retrieve all online shops and paypal
		/// </summary>
		/// <param name="customerId">Customer Id</param>
		/// <returns></returns>
		public List<MarketPlace> GetCustomerMarketPlaces(int customerId)
		{
			var mps = _db.Fill<MarketPlace>("AV_GetCustomerMarketPlaces", new QueryParameter("@CustomerId", customerId));
			return mps;
		}


		/// <summary>
		/// Retrieve all yodlee accounts
		/// </summary>
		/// <param name="customerId">Customer Id</param>
		/// <returns></returns>
		public List<MarketPlace> GetCustomerYodlees(int customerId)
		{
			var mps = _db.Fill<MarketPlace>("AV_GetCustomerYodlees", new QueryParameter("@CustomerId", customerId));
			return mps;
		}

		/// <summary>
		/// Retrieve all payment accounts without paypal
		/// </summary>
		/// <param name="customerId">Customer Id</param>
		/// <returns></returns>
		public List<string> GetCustomerPaymentMarketPlaces(int customerId)
		{
			var srList = _db.ExecuteEnumerable("AV_GetCustomerPaymentMarketPlaces", new QueryParameter("@CustomerId", customerId));
			var mps = (from SafeReader row in srList
					   select row[0].ToString()).ToList();
			return mps;

		}

		/// <summary>
		/// Retrieve last analysis functions values (only doubles)
		/// </summary>
		/// <param name="mpId">Marketplace id</param>
		/// <returns></returns>
		public List<AnalysisFunction> GetAnalysisFunctions(int mpId)
		{
			var srList = _db.ExecuteEnumerable("AV_GetAnalysisFunctions", new QueryParameter("@CustomerMarketPlaceId", mpId));

			var afvs = new List<AnalysisFunction>();
			foreach (SafeReader row in srList)
			{
				afvs.Add(new AnalysisFunction
				{
					Updated = row["Updated"],
					MarketPlaceName = row["MarketPlaceName"],
					Value = row["Value"],
					Function = row["FunctionName"],
					TimePeriod = (TimePeriodEnum)(int.Parse(row["TimePeriod"].ToString())),
				});
			}
			return afvs;
		}

		/// <summary>
		/// Retrieve last analysis functions values form min annualized income for ebay/amazon/paypal mp by for 1m 3m 6m 1y and 1y not annualized
		/// </summary>
		/// <returns></returns>
		public OnlineRevenues GetOnlineAnnaulizedRevenueForPeriod(int mpId)
		{
			var onlineRevenue = _db.FillFirst<OnlineRevenues>("AV_GetAnnualizedRevenueForPeriod", new QueryParameter("@CustomerMarketPlaceId", mpId));
			return onlineRevenue;
		}
		
		public int GetExperianScore(int customerId)
		{
			var sr = _db.GetFirst("AV_GetExperianScore", new QueryParameter("@CustomerId", customerId));
			if (sr.Count == 0)
			{
				return 0;
			}
			//todo retrieve defaults 
			return sr["ExperianScore"];
		}

		public bool WasApprovedForLoan(int customerId)
		{
			return bool.Parse(_db.ExecuteScalar<string>("AV_WasLoanApproved", new QueryParameter("@CustomerId", customerId)));
		}

		//todo retrieve defaults accounts num, amount, lates
		public bool HasDefaultAccounts(int customerId, int minDefBalance)
		{
			return bool.Parse(_db.ExecuteScalar<string>("AV_HasDefaultAccounts", new QueryParameter("@CustomerId", customerId), new QueryParameter("@MinDefBalance", minDefBalance)));
		}

		public IEnumerable<SafeReader> GetAutoDecisions(DateTime from, DateTime to)
		{
			return _db.ExecuteEnumerable("AV_GetAutomaticDecisions", new QueryParameter("@DateStart", from), new QueryParameter("@DateEnd", to));
		}

		public RejectionData GetRejectionData(int customerId) {
			return _db.FillFirst<RejectionData>("AV_GetRejectionData", CommandSpecies.StoredProcedure,
			                              new QueryParameter("@CustomerId", customerId));
		}
		
		public ReRejectionData GetReRejectionData(int customerId, int cashRequestId)
		{
			var sqlData = _db.GetFirst("AV_ReRejectionData",
				new QueryParameter("@CustomerId", customerId), 
				new QueryParameter("@CashRequestId", cashRequestId));

			var data = new ReRejectionData
				{
					ManualRejectDate = string.IsNullOrEmpty(sqlData["ManualRejectDate"].ToString()) ? (DateTime?)null : DateTime.Parse(sqlData["ManualRejectDate"].ToString()),
					IsNewClient = bool.Parse(sqlData["IsNewClient"].ToString()),
					NewDataSourceAdded = bool.Parse(sqlData["NewDataSourceAdded"].ToString()),
					LoanAmount = int.Parse(sqlData["LoanAmount"].ToString()),
					RepaidAmount = decimal.Parse(sqlData["RepaidAmount"].ToString()),
					AutomaticDecisionDate = string.IsNullOrEmpty(sqlData["AutomaticDecisionDate"].ToString()) ? DateTime.UtcNow : DateTime.Parse(sqlData["AutomaticDecisionDate"].ToString())
				};
			return data;
		}

		public ReApprovalData GetReApprovalData(int customerId, int cashRequestId)
		{
			var sqlData = _db.GetFirst("AV_ReApprovalData", 
				new QueryParameter("@CustomerId", customerId), 
				new QueryParameter("@CashRequestId", cashRequestId));

			var data = new ReApprovalData
			{
				ManualApproveDate = string.IsNullOrEmpty(sqlData["ManualApproveDate"].ToString()) ? (DateTime?)null : DateTime.Parse(sqlData["ManualApproveDate"].ToString()),
				IsNewClient = bool.Parse(sqlData["IsNewClient"].ToString()),
				NewDataSourceAdded = bool.Parse(sqlData["NewDataSourceAdded"].ToString()),
				OfferedAmount = string.IsNullOrEmpty(sqlData["OfferedAmount"].ToString()) ? 0 : int.Parse(sqlData["OfferedAmount"].ToString()),
				PrincipalRepaymentsSinceOffer = string.IsNullOrEmpty(sqlData["PrincipalRepaymentsSinceOffer"].ToString()) ? 0 : decimal.Parse(sqlData["PrincipalRepaymentsSinceOffer"].ToString()),
				TookAmountLastRequest = string.IsNullOrEmpty(sqlData["TookAmountLastRequest"].ToString()) ? 0 : int.Parse(sqlData["TookAmountLastRequest"].ToString()),
				TookLoanLastRequest = bool.Parse(sqlData["TookLoanLastRequest"].ToString()),
				WasLate = bool.Parse(sqlData["WasLate"].ToString()),
			};
			return data;
		}

		public decimal GetMedalRate(int customerId)
		{
			return _db.ExecuteScalar<decimal>("AV_GetMedalRate", new QueryParameter("@CustomerId", customerId));
		}

		public bool IsOffline(int customerId)
		{
			return _db.ExecuteScalar<bool>("AV_IsCustomerOffline", new QueryParameter("@CustomerId", customerId));
		}

		public int GetExperianCompanyScore(int customerId) {
			throw new NotImplementedException();
		}

		public MedalInputModelDb GetMedalInputModel(int customerId)
		{
			var model = _db.FillFirst<MedalInputModelDb>("AV_GetMedalInputParams", new QueryParameter("@CustomerId", customerId));
			return model;
		}

		public PositiveFeedbacksModelDb GetPositiveFeedbacks(int customerId)
		{
			var model = _db.FillFirst<PositiveFeedbacksModelDb>("AV_GetFeedbacks", new QueryParameter("@CustomerId", customerId));
			return model;
		}

		public MedalChooserInputModelDb GetMedalChooserData(int customerId)
		{
			var model = _db.FillFirst<MedalChooserInputModelDb>("AV_GetMedalChooserInputParams", new QueryParameter("@CustomerId", customerId));
			return model;
		}

		public List<MedalComparisonModel> GetMedalTestData() {
			var model = new List<MedalComparisonModel>();
			_db.ForEachRowSafe((sr, bRowsetStart) =>
			{
				MedalType medalType;
				Medal medal;

				if (!Enum.TryParse(sr["MedalType"].ToString(), false, out medalType)) {
					_log.Error("Failed to parse medal type {0} for {1}", (string)sr["MedalType"], (string)sr["CustomerId"]);
					medalType = MedalType.NoMedal;
				}

				if (!Enum.TryParse(sr["Medal"], false, out medal))
				{
					_log.Error("Failed to parse medal {0} for {1}", (string)sr["Medal"], (string)sr["CustomerId"]);
					medal = Medal.NoClassification;
				}
				DateTime? businessSeniority = sr["BusinessSeniority"];
				string businessSeniorityStr = businessSeniority.HasValue ? businessSeniority.Value.ToString("yyyy-MM-dd HH:mm:ss") : null;

				DateTime regDate = sr["EzbobSeniority"];
				string ezbobSeniorityStr = regDate.ToString("yyyy-MM-dd HH:mm:ss");

				model.Add(new MedalComparisonModel {
					CustomerId = sr["CustomerId"],
					MedalType = medalType,
					BusinessScore = new Weight { Value = sr["BusinessScore"], FinalWeight = sr["BusinessScoreWeight"], Score = sr["BusinessScoreScore"], Grade = (int)(decimal)sr["BusinessScoreGrade"] },
					FreeCashFlow = new Weight { Value = sr["FreeCashFlow"], FinalWeight = sr["FreeCashFlowWeight"], Score = sr["FreeCashFlowScore"], Grade = (int)(decimal)sr["FreeCashFlowGrade"] },
					AnnualTurnover = new Weight { Value = sr["AnnualTurnover"], FinalWeight = sr["AnnualTurnoverWeight"], Score = sr["AnnualTurnoverScore"], Grade = (int)(decimal)sr["AnnualTurnoverGrade"] },
					TangibleEquity = new Weight { Value = sr["TangibleEquity"], FinalWeight = sr["TangibleEquityWeight"], Score = sr["TangibleEquityScore"], Grade = (int)(decimal)sr["TangibleEquityGrade"] },
					BusinessSeniority = new Weight { Value = businessSeniorityStr, FinalWeight = sr["BusinessSeniorityWeight"], Score = sr["BusinessSeniorityScore"], Grade = (int)(decimal)sr["BusinessSeniorityGrade"] },
					ConsumerScore = new Weight { Value = sr["ConsumerScore"], FinalWeight = sr["ConsumerScoreWeight"], Score = sr["ConsumerScoreScore"], Grade = (int)(decimal)sr["ConsumerScoreGrade"] },
					NetWorth = new Weight { Value = sr["NetWorth"], FinalWeight = sr["NetWorthWeight"], Score = sr["NetWorthScore"], Grade = (int)(decimal)sr["NetWorthGrade"] },
					MaritalStatus = new Weight { Value = sr["MaritalStatus"], FinalWeight = sr["MaritalStatusWeight"], Score = sr["MaritalStatusScore"], Grade = (int)(decimal)sr["MaritalStatusGrade"] },
					NumOfLoans = new Weight { Value = sr["NumOfLoans"], FinalWeight = sr["NumOfLoansWeight"], Score = sr["NumOfLoansScore"], Grade = (int)(decimal)sr["NumOfLoansGrade"] },
					NumOfEarlyRepayments = new Weight { Value = sr["NumOfEarlyRepayments"], FinalWeight = sr["NumOfEarlyRepaymentsWeight"], Score = sr["NumOfEarlyRepaymentsScore"], Grade = (int)(decimal)sr["NumOfEarlyRepaymentsGrade"] },
					NumOfLateRepayments = new Weight { Value = sr["NumOfLateRepayments"], FinalWeight = sr["NumOfLateRepaymentsWeight"], Score = sr["NumOfLateRepaymentsScore"], Grade = (int)(decimal)sr["NumOfLateRepaymentsGrade"] },
					NumOfStores = new Weight { Value = sr["NumberOfStores"], FinalWeight = sr["NumberOfStoresWeight"], Score = sr["NumberOfStoresScore"], Grade = (int)(decimal)sr["NumberOfStoresGrade"] },
					PositiveFeedbacks = new Weight { Value = sr["PositiveFeedbacks"], FinalWeight = sr["PositiveFeedbacksWeight"], Score = sr["PositiveFeedbacksScore"], Grade = (int)(decimal)sr["PositiveFeedbacksGrade"] },
					EzbobSeniority = new Weight { Value = ezbobSeniorityStr, FinalWeight = sr["EzbobSeniorityWeight"], Score = sr["EzbobSeniorityScore"], Grade = (int)(decimal)sr["EzbobSeniorityGrade"] },
					ValueAdded = sr["ValueAdded"],
					InnerFlow = sr["InnerFlowName"],
					Error = sr["Error"],
					BankTurnover = sr["BankAnnualTurnover"],
					HmrcTurnover = sr["HmrcAnnualTurnover"],
					OnlineTurnover = sr["OnlineAnnualTurnover"],
					Medal = medal,
					TotalScore = sr["TotalScore"],
					TotalScoreNormalized = sr["TotalScoreNormalized"],
					FirstRepaymentDatePassed = sr["FirstRepaymentDatePassed"],
					AmazonPositiveFeedbacks = sr["AmazonPositiveFeedbacks"],
					CalculationTime = sr["CalculationTime"],
					EarliestHmrcLastUpdateDate = sr["EarliestHmrcLastUpdateDate"],
					EarliestYodleeLastUpdateDate = sr["EarliestYodleeLastUpdateDate"],
					EbayPositiveFeedbacks = sr["EbayPositiveFeedbacks"],
					MortageBalance = sr["MortageBalance"],
					NumOfHmrcMps = sr["NumOfHmrcMps"],
					NumberOfPaypalPositiveTransactions = sr["NumberOfPaypalPositiveTransactions"],
					OfferedLoanAmount = sr["OfferedLoanAmount"],
				});
				return ActionResult.Continue;
			}, "AV_GetMedalsForTest");

			return model;
		}

		public List<KeyValuePair<int, DateTime>> GetCustomersForMedalsCompare()
		{
			var customers = new List<KeyValuePair<int, DateTime>>();
			_db.ForEachRowSafe((sr, hren) =>
			{
				customers.Add(new KeyValuePair<int, DateTime>(sr["CustomerId"], sr["CalculationTime"]));
				return ActionResult.Continue;
			}, "SELECT CustomerId, CalculationTime FROM MedalCalculations", CommandSpecies.Text);
			return customers;
		}
		
		public YodleeRevenuesModelDb GetYodleeRevenues(int customerMarketplaceId) {
			var model = _db.FillFirst<YodleeRevenuesModelDb>("AV_GetYodleeRevenues", CommandSpecies.StoredProcedure, new QueryParameter("CustomerMarketplaceId", customerMarketplaceId));
			return model;
		}

		public List<MedalCoefficientsModelDb> GetMedalCoefficients() {
			var model = new List<MedalCoefficientsModelDb>();
			_db.ForEachRowSafe((sr, hren) =>
			{
				var medal = (Medal) Enum.Parse(typeof (Medal), sr["Medal"]);
				model.Add(new MedalCoefficientsModelDb {
					Medal = medal,
					AnnualTurnover = sr["AnnualTurnover"],
					ValueAdded = sr["ValueAdded"],
					FreeCashFlow = sr["FreeCashFlow"],
				});
				return ActionResult.Continue;
			}, "SELECT * FROM MedalCoefficients", CommandSpecies.Text); 
			return model;
		}

		public OfferSetupFeeRangeModelDb GetOfferSetupFeeRange(int amount) {
			return _db.FillFirst<OfferSetupFeeRangeModelDb>("AV_GetOfferSetupFeeRange", CommandSpecies.StoredProcedure,
			                                         new QueryParameter("Amount", amount));
		}

		public OfferInterestRateRangeModelDb GetOfferIneterestRateRange(Medal medal)
		{
			return _db.FillFirst<OfferInterestRateRangeModelDb>("AV_OfferInterestRateRange", CommandSpecies.StoredProcedure,
													 new QueryParameter("Medal", medal.ToString()));
		}

		public PricingScenarioModel GetPricingScenario(int amount, bool hasLoans)
		{
			var model = _db.FillFirst<PricingScenarioModel>("AV_PricingScenario", new QueryParameter("Amount", amount), new QueryParameter("HasLoans", hasLoans));
			return model;
		}

		#region big ugly insert
		public void StoreMedalVerification(MedalOutputModel model)
		{
			if (model.Dict != null)
			{
				if (!model.Dict.ContainsKey(Parameter.BusinessScore))
				{
					model.Dict[Parameter.BusinessScore] = new Weight();
				}

				if (!model.Dict.ContainsKey(Parameter.NumOfStores))
				{
					model.Dict[Parameter.NumOfStores] = new Weight();
				}

				if (!model.Dict.ContainsKey(Parameter.PositiveFeedbacks))
				{
					model.Dict[Parameter.PositiveFeedbacks] = new Weight();
				}

				if (!model.Dict.ContainsKey(Parameter.TangibleEquity))
				{
					model.Dict[Parameter.TangibleEquity] = new Weight();
				}

				if (!model.Dict.ContainsKey(Parameter.FreeCashFlow))
				{
					model.Dict[Parameter.FreeCashFlow] = new Weight();
				}

				_db.ExecuteNonQuery("AV_StoreNewMedal", CommandSpecies.StoredProcedure,
									 new QueryParameter("CustomerId", model.CustomerId),
									 new QueryParameter("CalculationTime", DateTime.UtcNow),
									 new QueryParameter("MedalType", model.MedalType.ToString()),
									 new QueryParameter("FirstRepaymentDatePassed", model.FirstRepaymentDatePassed),
									 new QueryParameter("BusinessScore", model.Dict[Parameter.BusinessScore].Value),
									 new QueryParameter("BusinessScoreWeight", model.Dict[Parameter.BusinessScore].FinalWeight),
									 new QueryParameter("BusinessScoreGrade", model.Dict[Parameter.BusinessScore].Grade),
									 new QueryParameter("BusinessScoreScore", model.Dict[Parameter.BusinessScore].Score),
									 new QueryParameter("FreeCashFlow", model.Dict[Parameter.FreeCashFlow].Value),
									 new QueryParameter("FreeCashFlowWeight", model.Dict[Parameter.FreeCashFlow].FinalWeight),
									 new QueryParameter("FreeCashFlowGrade", model.Dict[Parameter.FreeCashFlow].Grade),
									 new QueryParameter("FreeCashFlowScore", model.Dict[Parameter.FreeCashFlow].Score),
									 new QueryParameter("AnnualTurnover", model.Dict[Parameter.AnnualTurnover].Value),
									 new QueryParameter("AnnualTurnoverWeight", model.Dict[Parameter.AnnualTurnover].FinalWeight),
									 new QueryParameter("AnnualTurnoverGrade", model.Dict[Parameter.AnnualTurnover].Grade),
									 new QueryParameter("AnnualTurnoverScore", model.Dict[Parameter.AnnualTurnover].Score),
									 new QueryParameter("TangibleEquity", model.Dict[Parameter.TangibleEquity].Value),
									 new QueryParameter("TangibleEquityWeight", model.Dict[Parameter.TangibleEquity].FinalWeight),
									 new QueryParameter("TangibleEquityGrade", model.Dict[Parameter.TangibleEquity].Grade),
									 new QueryParameter("TangibleEquityScore", model.Dict[Parameter.TangibleEquity].Score),
									 new QueryParameter("BusinessSeniority", model.Dict[Parameter.BusinessSeniority].Value),
									 new QueryParameter("BusinessSeniorityWeight",
														model.Dict[Parameter.BusinessSeniority].FinalWeight),
									 new QueryParameter("BusinessSeniorityGrade", model.Dict[Parameter.BusinessSeniority].Grade),
									 new QueryParameter("BusinessSeniorityScore", model.Dict[Parameter.BusinessSeniority].Score),
									 new QueryParameter("ConsumerScore", model.Dict[Parameter.ConsumerScore].Value),
									 new QueryParameter("ConsumerScoreWeight", model.Dict[Parameter.ConsumerScore].FinalWeight),
									 new QueryParameter("ConsumerScoreGrade", model.Dict[Parameter.ConsumerScore].Grade),
									 new QueryParameter("ConsumerScoreScore", model.Dict[Parameter.ConsumerScore].Score),
									 new QueryParameter("NetWorth", model.Dict[Parameter.NetWorth].Value),
									 new QueryParameter("NetWorthWeight", model.Dict[Parameter.NetWorth].FinalWeight),
									 new QueryParameter("NetWorthGrade", model.Dict[Parameter.NetWorth].Grade),
									 new QueryParameter("NetWorthScore", model.Dict[Parameter.NetWorth].Score),
									 new QueryParameter("MaritalStatus", model.Dict[Parameter.MaritalStatus].Value),
									 new QueryParameter("MaritalStatusWeight", model.Dict[Parameter.MaritalStatus].FinalWeight),
									 new QueryParameter("MaritalStatusGrade", model.Dict[Parameter.MaritalStatus].Grade),
									 new QueryParameter("MaritalStatusScore", model.Dict[Parameter.MaritalStatus].Score),
									 new QueryParameter("NumberOfStores", model.Dict[Parameter.NumOfStores].Value),
									 new QueryParameter("NumberOfStoresWeight", model.Dict[Parameter.NumOfStores].FinalWeight),
									 new QueryParameter("NumberOfStoresGrade", model.Dict[Parameter.NumOfStores].Grade),
									 new QueryParameter("NumberOfStoresScore", model.Dict[Parameter.NumOfStores].Score),
									 new QueryParameter("PositiveFeedbacks", model.Dict[Parameter.PositiveFeedbacks].Value),
									 new QueryParameter("PositiveFeedbacksWeight",
														model.Dict[Parameter.PositiveFeedbacks].FinalWeight),
									 new QueryParameter("PositiveFeedbacksGrade", model.Dict[Parameter.PositiveFeedbacks].Grade),
									 new QueryParameter("PositiveFeedbacksScore", model.Dict[Parameter.PositiveFeedbacks].Score),
									 new QueryParameter("EzbobSeniority", model.Dict[Parameter.EzbobSeniority].Value),
									 new QueryParameter("EzbobSeniorityWeight", model.Dict[Parameter.EzbobSeniority].FinalWeight),
									 new QueryParameter("EzbobSeniorityGrade", model.Dict[Parameter.EzbobSeniority].Grade),
									 new QueryParameter("EzbobSeniorityScore", model.Dict[Parameter.EzbobSeniority].Score),
									 new QueryParameter("NumOfLoans", model.Dict[Parameter.NumOfOnTimeLoans].Value),
									 new QueryParameter("NumOfLoansWeight", model.Dict[Parameter.NumOfOnTimeLoans].FinalWeight),
									 new QueryParameter("NumOfLoansGrade", model.Dict[Parameter.NumOfOnTimeLoans].Grade),
									 new QueryParameter("NumOfLoansScore", model.Dict[Parameter.NumOfOnTimeLoans].Score),
									 new QueryParameter("NumOfLateRepayments", model.Dict[Parameter.NumOfLatePayments].Value),
									 new QueryParameter("NumOfLateRepaymentsWeight",
														model.Dict[Parameter.NumOfLatePayments].FinalWeight),
									 new QueryParameter("NumOfLateRepaymentsGrade", model.Dict[Parameter.NumOfLatePayments].Grade),
									 new QueryParameter("NumOfLateRepaymentsScore", model.Dict[Parameter.NumOfLatePayments].Score),
									 new QueryParameter("NumOfEarlyRepayments", model.Dict[Parameter.NumOfEarlyPayments].Value),
									 new QueryParameter("NumOfEarlyRepaymentsWeight",
														model.Dict[Parameter.NumOfEarlyPayments].FinalWeight),
									 new QueryParameter("NumOfEarlyRepaymentsGrade", model.Dict[Parameter.NumOfEarlyPayments].Grade),
									 new QueryParameter("NumOfEarlyRepaymentsScore", model.Dict[Parameter.NumOfEarlyPayments].Score),

									 new QueryParameter("ValueAdded", model.ValueAdded),
									 new QueryParameter("TotalScore", model.Score),
									 new QueryParameter("TotalScoreNormalized", model.NormalizedScore),
									 new QueryParameter("Medal", model.Medal.ToString()),
									 new QueryParameter("Error", model.Error),
									 new QueryParameter("OfferedLoanAmount", model.OfferedLoanAmount),
									 new QueryParameter("NumOfHmrcMps", model.NumOfHmrcMps),
									 new QueryParameter("AmazonPositiveFeedbacks", model.AmazonPositiveFeedbacks),
									 new QueryParameter("EbayPositiveFeedbacks", model.EbayPositiveFeedbacks),
									 new QueryParameter("NumberOfPaypalPositiveTransactions",
														model.NumberOfPaypalPositiveTransactions));
			}
			else
			{
				_db.ExecuteNonQuery("AV_StoreNewMedalError", CommandSpecies.StoredProcedure,
									 new QueryParameter("CustomerId", model.CustomerId),
									 new QueryParameter("CalculationTime", DateTime.UtcNow),
									 new QueryParameter("MedalType", model.MedalType.ToString()),
									 new QueryParameter("Medal", model.Medal.ToString()),
									 new QueryParameter("Error", model.Error),
									 new QueryParameter("NumOfHmrcMps", model.NumOfHmrcMps));
			}
		}
		#endregion
	}
}
