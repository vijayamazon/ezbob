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

		public DbHelper(ASafeLog log)
		{
			_log = log;
		}

		public RejectionConstants GetRejectionConstants()
		{

			var conn = new SqlConnection(_log);
			var sr = conn.ExecuteEnumerable("AV_RejectionConstants", CommandSpecies.StoredProcedure);
			
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
			var conn = new SqlConnection(_log);
			return conn.ExecuteScalar<DateTime?>("AV_GetCustomerBirthDate", new QueryParameter("@CustomerId", customerId));
		}

		/// <summary>
		/// Retrieve all online shops and paypal
		/// </summary>
		/// <param name="customerId">Customer Id</param>
		/// <returns></returns>
		public List<MarketPlace> GetCustomerMarketPlaces(int customerId)
		{

			var conn = new SqlConnection(_log);
			var mps = conn.Fill<MarketPlace>("AV_GetCustomerMarketPlaces", new QueryParameter("@CustomerId", customerId));
			return mps;
		}


		/// <summary>
		/// Retrieve all yodlee accounts
		/// </summary>
		/// <param name="customerId">Customer Id</param>
		/// <returns></returns>
		public List<MarketPlace> GetCustomerYodlees(int customerId)
		{

			var conn = new SqlConnection(_log);
			var mps = conn.Fill<MarketPlace>("AV_GetCustomerYodlees", new QueryParameter("@CustomerId", customerId));
			return mps;

		}

		/// <summary>
		/// Retrieve all payment accounts without paypal
		/// </summary>
		/// <param name="customerId">Customer Id</param>
		/// <returns></returns>
		public List<string> GetCustomerPaymentMarketPlaces(int customerId)
		{

			var conn = new SqlConnection(_log);
			var srList = conn.ExecuteEnumerable("AV_GetCustomerPaymentMarketPlaces", new QueryParameter("@CustomerId", customerId));

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
			var conn = new SqlConnection(_log);
			var srList = conn.ExecuteEnumerable("AV_GetAnalysisFunctions", new QueryParameter("@CustomerMarketPlaceId", mpId));

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
		/// Retrieve last analysis functions values form min annaulized income for ebay/amazon/paypal mp
		/// </summary>
		/// <param name="mpId">Marketplace id</param>
		/// <returns></returns>
		public decimal GetOnlineAnnaulizedRevenue(int mpId)
		{
			var conn = new SqlConnection(_log);
			var minAnnualizedRevenue = conn.ExecuteScalar<decimal>("AV_GetMinAnnualizedRevenue", new QueryParameter("@CustomerMarketPlaceId", mpId));
			return minAnnualizedRevenue;
		}
		
		public int GetExperianScore(int customerId)
		{
			var conn = new SqlConnection(_log);
			var sr = conn.GetFirst("AV_GetExperianScore", new QueryParameter("@CustomerId", customerId));
			if (sr.Count == 0)
			{
				return 0;
			}
			//todo retrieve defaults 
			return sr["ExperianScore"];
		}

		public bool WasApprovedForLoan(int customerId)
		{
			var conn = new SqlConnection(_log);
			return bool.Parse(conn.ExecuteScalar<string>("AV_WasLoanApproved", new QueryParameter("@CustomerId", customerId)));
		}

		//todo retrieve defaults accounts num, amount, lates
		public bool HasDefaultAccounts(int customerId, int minDefBalance)
		{
			var conn = new SqlConnection(_log);
			return bool.Parse(conn.ExecuteScalar<string>("AV_HasDefaultAccounts", new QueryParameter("@CustomerId", customerId), new QueryParameter("@MinDefBalance", minDefBalance)));
		}

		public IEnumerable<SafeReader> GetAutoDecisions(DateTime from, DateTime to)
		{
			var conn = new SqlConnection(_log);
			return conn.ExecuteEnumerable("AV_GetAutomaticDecisions", new QueryParameter("@DateStart", from), new QueryParameter("@DateEnd", to));
		}

		public RejectionData GetRejectionData(int customerId) {
			var conn = new SqlConnection(_log);
			return conn.FillFirst<RejectionData>("AV_GetRejectionData", CommandSpecies.StoredProcedure,
			                              new QueryParameter("@CustomerId", customerId));
		}
		
		public ReRejectionData GetReRejectionData(int customerId, int cashRequestId)
		{
			var conn = new SqlConnection(_log);
			var sqlData = conn.GetFirst("AV_ReRejectionData",
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
			var conn = new SqlConnection(_log);
			var sqlData = conn.GetFirst("AV_ReApprovalData", 
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
			var conn = new SqlConnection(_log);
			return conn.ExecuteScalar<decimal>("AV_GetMedalRate", new QueryParameter("@CustomerId", customerId));
		}

		public bool IsOffline(int customerId)
		{
			var conn = new SqlConnection(_log);
			return conn.ExecuteScalar<bool>("AV_IsCustomerOffline", new QueryParameter("@CustomerId", customerId));
		}

		public int GetExperianCompanyScore(int customerId) {
			throw new NotImplementedException();
		}

		public MedalInputModelDb GetMedalInputModel(int customerId)
		{
			var conn = new SqlConnection(_log);
			var model = conn.FillFirst<MedalInputModelDb>("AV_GetMedalInputParams", new QueryParameter("@CustomerId", customerId));
			return model;
		}

		public PositiveFeedbacksModelDb GetPositiveFeedbacks(int customerId)
		{
			var conn = new SqlConnection(_log);
			var model = conn.FillFirst<PositiveFeedbacksModelDb>("AV_GetFeedbacks", new QueryParameter("@CustomerId", customerId));
			return model;
		}
	}
}
