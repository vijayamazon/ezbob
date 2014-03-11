﻿namespace EzService {
	using ActionResults;
	using EzBob.Backend.Strategies;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	partial class EzServiceImplementation {
		public SerializedDataTableActionResult GetSpResultTable(string spName, params string[] parameters)
		{
			GetSpResultTable strategyInstance;
			var result = ExecuteSyncParamsAtEnd(out strategyInstance, null, null, spName, parameters);

			string serializedDataTable = JsonConvert.SerializeObject(strategyInstance.Result, new DataTableConverter());
			return new SerializedDataTableActionResult
			{
				MetaData = result,
				SerializedDataTable = serializedDataTable
			};
		} // GetSpResultTable
		
		/*TODO - remove if serialized is ok
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
		} // GetBasicInterestRate*/
	} // class EzServiceImplementation
} // namespace EzService
