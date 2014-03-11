namespace EzService {
	using System.Linq;
	using ActionResults;
	using EzBob.Backend.Strategies;
	using Ezbob.Database;

	partial class EzServiceImplementation {
		public DataTableActionResult GetSpResultTable(string spName, params QueryParameter[] parameters)
		{
			GetSpResultTable strategyInstance;
			var result = ExecuteSync(out strategyInstance, null, null, spName, parameters);

			return new DataTableActionResult
			{
				MetaData = result,
				DataTable = strategyInstance.Result
			};
		} // GetSpResultTable

		public BasicInterestRateActionResult GetBasicInterestRate()
		{
			GetBasicInterestRates strategyInstance;
			var result = ExecuteSync(out strategyInstance, null, null);

			var basicInterestRates = strategyInstance.BasicInterestRates.Select(current => new SingleBasicInterestRate {Id = current.Id, FromScore = current.FromScore, ToScore = current.ToScore, LoanInterestBase = current.LoanInterestBase}).ToList();

			return new BasicInterestRateActionResult
			{
				MetaData = result,
				BasicInterestRates = basicInterestRates
			};
		} // GetBasicInterestRate
	} // class EzServiceImplementation
} // namespace EzService
