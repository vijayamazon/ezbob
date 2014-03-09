namespace EzService {
	using EzBob.Backend.Strategies;
	using FraudChecker;

	partial class EzServiceImplementation {
		public ActionMetaData FirstOfMonthStatusNotifier() {
			return Execute(null, null, typeof(FirstOfMonthStatusNotifier));
		} // FirstOfMonthStatusNotifier

		public ActionMetaData FraudChecker(int customerId, FraudMode mode) {
			return Execute(customerId, null, typeof (FraudChecker), customerId, mode);
		} // FraudChecker

		public ActionMetaData LateBy14Days() {
			return Execute(null, null, typeof(LateBy14Days));
		} // LateBy14Days

		public ActionMetaData PayPointCharger() {
			return Execute(null, null, typeof(PayPointCharger));
		} // PayPointCharger

		public ActionMetaData SetLateLoanStatus() {
			return Execute(null, null, typeof(SetLateLoanStatus));
		} // SetLateLoanStatus

		public ActionMetaData UpdateMarketplace(int customerId, int marketplaceId, bool doUpdateWizardStep) {
			return Execute(customerId, null, typeof(UpdateMarketplace), customerId, marketplaceId, doUpdateWizardStep);
		} // UpdateMarketplace

		public ActionMetaData UpdateTransactionStatus() {
			return Execute(null, null, typeof(UpdateTransactionStatus));
		} // UpdateTransactionStatus

		public ActionMetaData XDaysDue() {
			return Execute(null, null, typeof(XDaysDue));
		} // XDaysDue

		public CrmLookupsActionResult CrmLoadLookups() {
			CrmLoadLookups oInstance;

			ActionMetaData oResult = ExecuteSync(out oInstance, null, null);

			return new CrmLookupsActionResult {
				MetaData = oResult,
				Actions = oInstance.Actions,
				Statuses = oInstance.Statuses,
			};
		} // CrmLoadLookups

		public ActionMetaData UpdateCurrencyRates() {
			return Execute(null, null, typeof(UpdateCurrencyRates));
		} // UpdateCurrencyRates
	} // class EzServiceImplementation
} // namespace EzService
