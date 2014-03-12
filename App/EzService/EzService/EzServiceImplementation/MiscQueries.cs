namespace EzService {
	using System;
	using System.Collections.Generic;
	using ActionResults;
	using EzBob.Backend.Strategies;
	using EzBob.Web.Areas.Underwriter.Models;
	using Ezbob.Database;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	partial class EzServiceImplementation {
		public SerializedDataTableActionResult GetSpResultTable(string spName, params string[] parameters) {
			GetSpResultTable strategyInstance;
			var result = ExecuteSync(out strategyInstance, null, null, spName, parameters);

			string serializedDataTable = JsonConvert.SerializeObject(strategyInstance.Result, new DataTableConverter());
			return new SerializedDataTableActionResult {
				MetaData = result,
				SerializedDataTable = serializedDataTable
			};
		} // GetSpResultTable

		public BoolActionResult SaveBasicInterestRate(List<BasicInterestRate> basicInterestRates)
		{
			bool isError = false;
			try
			{
				DB.ExecuteNonQuery(
				"BasicInterestRate_Refill",
				CommandSpecies.StoredProcedure,
				DB.CreateTableParameter<BasicInterestRate>("@TheList", basicInterestRates, objbir =>
				{
					var bir = (BasicInterestRate)objbir;
					return new object[] { bir.FromScore, bir.ToScore, bir.LoanInterestBase, };
				})
			);
			}
			catch (Exception e)
			{
				Log.Error("Exception occurred during execution of BasicInterestRate_Refill. The exception:{0}", e);
				isError = true;
			}

			return new BoolActionResult
			{
				Value = isError
			};
		}
	} // class EzServiceImplementation
} // namespace EzService
