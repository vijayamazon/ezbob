namespace EzService {
	using EzBob.Backend.Strategies;

	partial class EzServiceImplementation {
		public WizardConfigsActionResult GetWizardConfigs() {
			GetWizardConfigs strategyInstance;

			var result = ExecuteSync(out strategyInstance, null, null);

			return new WizardConfigsActionResult {
				MetaData = result,
				IsSmsValidationActive = strategyInstance.IsSmsValidationActive,
				NumberOfMobileCodeAttempts = strategyInstance.NumberOfMobileCodeAttempts
			};
		} // GetWizardConfigs
	} // class EzServiceImplementation
} // namespace EzService
