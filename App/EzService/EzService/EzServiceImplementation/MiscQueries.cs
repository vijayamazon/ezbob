namespace EzService {
	using System.Collections.Generic;
	using ActionResults;
	using EzBob.Backend.Strategies;
	using EzBob.Web.Areas.Underwriter.Models;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	partial class EzServiceImplementation {
		public SerializedDataTableActionResult GetSpResultTable(string spName, params string[] parameters)
		{
			GetSpResultTable strategyInstance;
			var result = ExecuteSync(true, out strategyInstance, null, null, spName, parameters);

			string serializedDataTable = JsonConvert.SerializeObject(strategyInstance.Result, new DataTableConverter());
			return new SerializedDataTableActionResult
			{
				MetaData = result,
				SerializedDataTable = serializedDataTable
			};
		} // GetSpResultTable

		public BoolActionResult SaveBasicInterestRate(List<BasicInterestRate> basicInterestRates)
		{
			string statement = "DELETE FROM BasicInterestRate\n";
			foreach (BasicInterestRate basicInterestRate in basicInterestRates)
			{
				statement +=
					string.Format("INSERT INTO BasicInterestRate (FromScore, ToScore, LoanInterestBase) VALUES ({0}, {1}, {2})\n",
					              basicInterestRate.FromScore, basicInterestRate.ToScore, basicInterestRate.LoanInterestBase);
			}

			ExecuteQuery strategyInstance;
			ActionMetaData result = ExecuteSync(false, out strategyInstance, null, null, statement);

			return new BoolActionResult
				{
					MetaData = result,
					Value = strategyInstance.IsError
				};
		}
	} // class EzServiceImplementation
} // namespace EzService
