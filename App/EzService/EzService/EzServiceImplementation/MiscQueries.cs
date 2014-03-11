namespace EzService {
	using System.Collections.Generic;
	using System.Linq;
	using ActionResults;
	using EzBob.Backend.Strategies;

	partial class EzServiceImplementation {
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
