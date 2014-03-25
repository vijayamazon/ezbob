namespace EzService.EzServiceImplementation {
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

		public ActionMetaData FinishWizard(int customerId, int underwriterId) {
			return Execute(customerId, underwriterId, typeof(FinishWizard), customerId);
		} // FinishWizard
	} // class EzServiceImplementation
} // namespace EzService
