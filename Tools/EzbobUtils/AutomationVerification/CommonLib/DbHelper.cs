using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib
{
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;

	class DbHelper
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
			mps.Add(new MarketPlace
				{
					Id = int.Parse(row["mpId"].ToString()),
					Name = row["Name"].ToString(),
					Type = row["Type"].ToString()
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
	}
}
