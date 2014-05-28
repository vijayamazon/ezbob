﻿namespace EzService.EzServiceImplementation {
	using EzBob.Backend.Strategies;
	using EzBob.Backend.Strategies.Misc;
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
			return Execute(oArgs.CustomerID, underwriterId, typeof(FinishWizard), oArgs);
		} // FinishWizard
	} // class EzServiceImplementation
} // namespace EzService
