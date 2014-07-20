namespace EzService.EzServiceImplementation {
	using EzBob.Backend.Strategies.Misc;

	partial class EzServiceImplementation {
		public CompanyDataForCompanyScoreActionResult GetCompanyDataForCompanyScore(int underwriterId, int customerId, string refNumber)
		{
			GetCompanyDataForCompanyScore strategyInstance;
			var result = ExecuteSync(out strategyInstance, null, underwriterId, customerId, refNumber);

			return new CompanyDataForCompanyScoreActionResult
			{
				MetaData = result,
				Data = strategyInstance.Data
			};
		}

		public CompanyDataForCreditBureauActionResult GetCompanyDataForCreditBureau(int underwriterId, int customerId, string refNumber)
		{
			GetCompanyDataForCreditBureau strategyInstance;
			var result = ExecuteSync(out strategyInstance, null, underwriterId, customerId, refNumber);

			return new CompanyDataForCreditBureauActionResult
			{
				MetaData = result,
				LastUpdate = strategyInstance.LastUpdate,
				Score = strategyInstance.Score,
				Errors = strategyInstance.Errors
			};
		}
	} // class EzServiceImplementation
} // namespace EzService
