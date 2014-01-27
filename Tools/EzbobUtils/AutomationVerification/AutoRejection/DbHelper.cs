namespace AutomationCalculator
{
	using System;
	using System.Collections.Generic;
	using System.Data;
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
			var dt = conn.ExecuteReader("AV_RejectionConstants");
			if (dt.Rows.Count == 0)
			{
				return null;
			}
			var consts = new RejectionConstants();
			foreach (DataRow row in dt.Rows)
			{
				switch (row["Name"].ToString())
				{
					case "LowCreditScore":
						consts.MinCreditScore = int.Parse(row["Value"].ToString());
						break;
					case "TotalAnnualTurnover":
						consts.MinAnnualTurnover = int.Parse(row["Value"].ToString());
						break;
					case "TotalThreeMonthTurnover":
						consts.MinThreeMonthTurnover = int.Parse(row["Value"].ToString());
						break;
					case "Reject_Defaults_CreditScore":
						consts.DefaultScoreBelow = int.Parse(row["Value"].ToString());
						break;
					case "Reject_Defaults_AccountsNum":
						break;
					case "Reject_Defaults_Amount":
						consts.DefaultMinAmount = int.Parse(row["Value"].ToString());
						break;
					case "Reject_Defaults_MonthsNum":
						consts.DefaultMinMonths = int.Parse(row["Value"].ToString());
						break;
					case "Reject_Minimal_Seniority":
						consts.MinMarketPlaceSeniorityDays = int.Parse(row["Value"].ToString());
						break;
					case "AutoRejectionException_CreditScore":
						consts.NoRejectIfCreditScoreAbove = int.Parse(row["Value"].ToString());
						break;
					case "AutoRejectionException_AnualTurnover":
						consts.NoRejectIfTotalAnnualTurnoverAbove = int.Parse(row["Value"].ToString());
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
			var dt = conn.ExecuteReader("AV_GetCustomerBirthDate", new QueryParameter("@CustomerId", customerId));
			if (dt.Rows.Count == 0)
			{
				return null;
			}
			return DateTime.Parse(dt.Rows[0]["DateOfBirth"].ToString());
		}

		/// <summary>
		/// Retrieve all shops and paypal
		/// </summary>
		/// <param name="customerId">Customer Id</param>
		/// <returns></returns>
		public List<MarketPlace> GetCustomerMarketPlaces(int customerId)
		{

			var conn = new SqlConnection(_log);
			var dt = conn.ExecuteReader("AV_GetCustomerMarketPlaces", new QueryParameter("@CustomerId", customerId));

			var mps = new List<MarketPlace>();
			foreach (DataRow row in dt.Rows)
			{
				AddMpToList(mps, row);
			}

			dt.Dispose();
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
			var dt = conn.ExecuteReader("AV_GetCustomerPaymentMarketPlaces", new QueryParameter("@CustomerId", customerId));

			var mps = (from DataRow row in dt.Rows 
					   select row[0].ToString()).ToList();

			dt.Dispose();
			return mps;

		}

		private void AddMpToList(List<MarketPlace> mps, DataRow row)
		{
			DateTime? originationDate = null;
			string odStr = row["OriginationDate"].ToString();
			if (!string.IsNullOrEmpty(odStr))
			{
				originationDate = DateTime.Parse(odStr);
			}
			mps.Add(new MarketPlace
				{
					Id = int.Parse(row["mpId"].ToString()),
					Name = row["Name"].ToString(),
					Type = row["Type"].ToString(),
					OriginationDate = originationDate
				});
		}

		/// <summary>
		/// Retrieve last analysis functions values (only doubles)
		/// </summary>
		/// <param name="mpId">Marketplace id</param>
		/// <returns></returns>
		public List<AnalysisFunction> GetAnalysisFunctions(int mpId)
		{
			var conn = new SqlConnection(_log);
			var dt = conn.ExecuteReader("AV_GetAnalysisFunctions", new QueryParameter("@CustomerMarketPlaceId", mpId));

			var afvs = new List<AnalysisFunction>();
			foreach (DataRow row in dt.Rows)
			{
				AddAnalysisFunctionToList(afvs, row);
			}
			return afvs;
		}

		private void AddAnalysisFunctionToList(List<AnalysisFunction> afvs, DataRow row)
		{
			afvs.Add(new AnalysisFunction()
				{
					Updated = DateTime.Parse(row["Updated"].ToString()),
					MarketPlaceName = row["MarketPlaceName"].ToString(),
					Value = double.Parse(row["Value"].ToString()),
					Function = row["FunctionName"].ToString(),
					TimePeriod = (TimePeriodEnum)(int.Parse(row["TimePeriod"].ToString())),
				});
		}

		public int GetExperianScore(int customerId)
		{
			var conn = new SqlConnection(_log);
			var dt = conn.ExecuteReader("AV_GetExperianScore", new QueryParameter("@CustomerId", customerId));
			if (dt.Rows.Count == 0)
			{
				return 0;
			}
			//todo retrieve defaults 
			//var experianJson = dt.Rows[0]["JsonPacket"].ToString();
			return int.Parse(dt.Rows[0]["ExperianScore"].ToString());
		}

		public bool WasApprovedForLoan(int customerId)
		{
			var conn = new SqlConnection(_log);
			return bool.Parse(conn.ExecuteScalar<string>("AV_WasLoanApproved", new QueryParameter("@CustomerId", customerId)));
		}

		public bool HasDefaultAccounts(int customerId, int minDefBalance, int months)
		{
			var conn = new SqlConnection(_log);
			return bool.Parse(conn.ExecuteScalar<string>("AV_HasDefaultAccounts", new QueryParameter("@CustomerId", customerId), new QueryParameter("@MinDefBalance", minDefBalance), new QueryParameter("@Months", months)));
		}

		public DataTable GetAutoDecisions(DateTime from, DateTime to)
		{
			var conn = new SqlConnection(_log);
			return conn.ExecuteReader("AV_GetAutomaticDecisions", new QueryParameter("@DateStart", from), new QueryParameter("@DateEnd", to));
		}

		public ReRejectionData GetReRejectionData(int customerId, int cashRequestId)
		{
			var conn = new SqlConnection(_log);
			var sqlData = conn.ExecuteReader("AV_ReRejectionData", new QueryParameter("@CustomerId", customerId), new QueryParameter("@CashRequestId", cashRequestId));

			var data = new ReRejectionData
				{
					ManualRejectDate = string.IsNullOrEmpty(sqlData.Rows[0]["ManualRejectDate"].ToString()) ? (DateTime?)null : DateTime.Parse(sqlData.Rows[0]["ManualRejectDate"].ToString()),
					IsNewClient = bool.Parse(sqlData.Rows[0]["IsNewClient"].ToString()),
					NewDataSourceAdded = bool.Parse(sqlData.Rows[0]["NewDataSourceAdded"].ToString()),
					LoanAmount = int.Parse(sqlData.Rows[0]["LoanAmount"].ToString()),
					RepaidAmount = decimal.Parse(sqlData.Rows[0]["RepaidAmount"].ToString()),
					AutomaticDecisionDate = string.IsNullOrEmpty(sqlData.Rows[0]["AutomaticDecisionDate"].ToString()) ? DateTime.UtcNow : DateTime.Parse(sqlData.Rows[0]["AutomaticDecisionDate"].ToString())
				};
			return data;
		}

		public ReApprovalData GetReApprovalData(int customerId, int cashRequestId)
		{
			var conn = new SqlConnection(_log);
			var sqlData = conn.ExecuteReader("AV_ReApprovalData", 
				new QueryParameter("@CustomerId", customerId), 
				new QueryParameter("@CashRequestId", cashRequestId));

			var data = new ReApprovalData
			{
				ManualApproveDate = string.IsNullOrEmpty(sqlData.Rows[0]["ManualApproveDate"].ToString()) ? (DateTime?)null : DateTime.Parse(sqlData.Rows[0]["ManualApproveDate"].ToString()),
				IsNewClient = bool.Parse(sqlData.Rows[0]["IsNewClient"].ToString()),
				NewDataSourceAdded = bool.Parse(sqlData.Rows[0]["NewDataSourceAdded"].ToString()),
				OfferedAmount = string.IsNullOrEmpty(sqlData.Rows[0]["OfferedAmount"].ToString()) ? 0 : int.Parse(sqlData.Rows[0]["OfferedAmount"].ToString()),
				PrincipalRepaymentsSinceOffer = string.IsNullOrEmpty(sqlData.Rows[0]["PrincipalRepaymentsSinceOffer"].ToString()) ? 0 : decimal.Parse(sqlData.Rows[0]["PrincipalRepaymentsSinceOffer"].ToString()),
				TookAmountLastRequest = string.IsNullOrEmpty(sqlData.Rows[0]["TookAmountLastRequest"].ToString()) ? 0 : int.Parse(sqlData.Rows[0]["TookAmountLastRequest"].ToString()),
				TookLoanLastRequest = bool.Parse(sqlData.Rows[0]["TookLoanLastRequest"].ToString()),
				WasLate = bool.Parse(sqlData.Rows[0]["WasLate"].ToString()),
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
			var dt = conn.ExecuteReader("AV_IsCustomerOffline", new QueryParameter("@CustomerId", customerId));
			if (dt.Rows.Count == 0)
			{
				return false;
			}
			return bool.Parse(dt.Rows[0]["IsOffline"].ToString());
		}
	}
}
