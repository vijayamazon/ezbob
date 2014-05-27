namespace EzService.EzServiceImplementation {
	using EzBob.Backend.Strategies;
	using EzBob.Backend.Strategies.Postcode;
	using Ezbob.Backend.Models;

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


		public ActionMetaData UpdateConfigurationVariables()
		{
			return Execute(null, null, typeof (UpdateConfigurationVariables));
		}//UpdateConfigurationVariables

		#region method PostcodeSaveLog

		public ActionMetaData PostcodeSaveLog(
			string sRequestType,
			string sUrl,
			string sStatus,
			string sResponseData,
			string sErrorMessage,
			int nUserID
		) {
			return Execute<PostcodeSaveLog>(null, nUserID,
				sRequestType,
				sUrl,
				sStatus,
				sResponseData,
				sErrorMessage,
				nUserID
			);
		} // PostcodeSaveLog

		#endregion method PostcodeSaveLog

		#region method CalculateVatReturnSummary

		public ActionMetaData CalculateVatReturnSummary(int nCustomerMarketplaceID) {
			return Execute<CalculateVatReturnSummary>(null, null, nCustomerMarketplaceID);
		} // CalculateVatReturnSummary

		#endregion method CalculateVatReturnSummary

		#region method MarketplaceInstantUpdate

		public ActionMetaData MarketplaceInstantUpdate(int nMarketplaceID) {
			return ExecuteSync<MarketplaceInstantUpdate>(null, null, nMarketplaceID);
		} // MarketplaceInstantUpdate

		#endregion method MarketplaceInstantUpdate

		#region method LoadVatReturnSummary

		public VatReturnSummaryActionResult LoadVatReturnSummary(int nCustomerID, int nMarketplaceID) {
			LoadVatReturnSummary oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null, nCustomerID, nMarketplaceID);

			return new VatReturnSummaryActionResult {
				MetaData = oMetaData,
				Summary = oInstance.Summary,
			};
		} // LoadVatReturnSummary

		#endregion method LoadVatReturnSummary

		#region method AndRecalculateVatReturnSummaryForAll

		public ActionMetaData AndRecalculateVatReturnSummaryForAll() {
			return Execute<AndRecalculateVatReturnSummaryForAll>(null, null);
		} // AndRecalculateVatReturnSummaryForAll

		#endregion method AndRecalculateVatReturnSummaryForAll

		#region method EncryptChannelGrabberMarketplaces

		public ActionMetaData EncryptChannelGrabberMarketplaces() {
			return Execute<EncryptChannelGrabberMarketplaces>(null, null);
		} // EncryptChannelGrabberMarketplaces

		#endregion method EncryptChannelGrabberMarketplaces
	} // class EzServiceImplementation
} // namespace EzService
