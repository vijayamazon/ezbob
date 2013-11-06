﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonLib
{
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class DbHelper
	{
		private static readonly LegacyLog Log;
		
		public void GetCustomers()
		{
			
		}

		/// <summary>
		/// Retrieve all shops and paypal
		/// </summary>
		/// <param name="customerId">Customer Id</param>
		/// <returns></returns>
		public List<MarketPlace> GetCustomerMarketPlaces(int customerId)
		{
			
			var conn = new SqlConnection(Log);
			var dt = conn.ExecuteReader("AV_GetCustomerMarketPlaces", new QueryParameter("@CustomerId", customerId));
			
			var mps = new List<MarketPlace>();
			foreach (DataRow row in dt.Rows)
			{
				AddMpToList(mps,row);
			}

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
			var conn = new SqlConnection(Log);
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
					Value = double.Parse(row["Value"].ToString()),
					Function = row["FunctionName"].ToString(),
					TimePeriod = (TimePeriodEnum)(int.Parse(row["TimePeriod"].ToString())),
				});
		}

		public int GetExperianScore(int customerId)
		{
			var conn = new SqlConnection(Log);
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
			var conn = new SqlConnection(Log);
			return bool.Parse(conn.ExecuteScalar<string>("AV_WasLoanApproved", new QueryParameter("@CustomerId", customerId)));
		}

		public bool HasDefaultAccounts(int customerId, int minDefBalance, int months)
		{
			var conn = new SqlConnection(Log);
			return bool.Parse(conn.ExecuteScalar<string>("AV_HasDefaultAccounts", new QueryParameter("@CustomerId", customerId), new QueryParameter("@MinDefBalance", minDefBalance), new QueryParameter("@Months", months)));
		}
	}
}
