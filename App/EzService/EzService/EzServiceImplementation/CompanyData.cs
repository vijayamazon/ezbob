namespace EzService.EzServiceImplementation {
	using EzBob.Backend.Strategies.Misc;
	using Ezbob.Backend.Models;

	partial class EzServiceImplementation {
		public CompanyDataForCompanyScoreActionResult GetCompanyDataForCompanyScore(int underwriterId, string refNumber)
		{
			GetCompanyDataForCompanyScore strategyInstance;
			var result = ExecuteSync(out strategyInstance, null, underwriterId, refNumber);

			return new CompanyDataForCompanyScoreActionResult
			{
				MetaData = result,
				Data = strategyInstance.Data
			};
		}

		public CompanyDataForCreditBureauActionResult GetCompanyDataForCreditBureau(int underwriterId, string refNumber)
		{
			GetCompanyDataForCreditBureau strategyInstance;

			var result = ExecuteSync(out strategyInstance, null, underwriterId, refNumber);

			return new CompanyDataForCreditBureauActionResult {
				MetaData = result,
				Result = new CompanyDataForCreditBureau {
					LastUpdate = strategyInstance.LastUpdate,
					Score = strategyInstance.Score,
					Errors = strategyInstance.Errors
				}
			};
		}
	} // class EzServiceImplementation
} // namespace EzService
