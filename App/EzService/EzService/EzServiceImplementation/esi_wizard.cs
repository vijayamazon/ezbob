namespace EzService.EzServiceImplementation {
	using Ezbob.Backend.Strategies.MainStrategy;
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
			if (oArgs.DoMain) {
				return Execute<MainStrategy>(
					oArgs.CustomerID,
					underwriterId,
					new MainStrategyArguments{
						UnderwriterID = underwriterId,
						CustomerID = oArgs.CustomerID,
						NewCreditLine = oArgs.NewCreditLineOption,
						AvoidAutoDecision = oArgs.AvoidAutoDecision,
						FinishWizardArgs = oArgs,
						CashRequestID = null,
						CashRequestOriginator = oArgs.CashRequestOriginator,
					}
				);
			} // if

			return Execute<FinishWizard>(oArgs.CustomerID, underwriterId, oArgs);
		} // FinishWizard
	} // class EzServiceImplementation
} // namespace EzService
