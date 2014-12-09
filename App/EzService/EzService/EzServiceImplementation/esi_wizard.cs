namespace EzService.EzServiceImplementation {
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Backend.Models;

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

		public ActionMetaData FinishWizard(FinishWizardArgs oArgs, int underwriterId) {
			return Execute<FinishWizard>(oArgs.CustomerID, underwriterId, oArgs);
		} // FinishWizard
	} // class EzServiceImplementation
} // namespace EzService
