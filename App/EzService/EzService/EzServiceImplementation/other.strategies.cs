﻿namespace EzService.EzServiceImplementation {
	using System;
	using System.Linq;
	using EzBob.Backend.Strategies.Misc;
	using EzBob.Backend.Strategies.Postcode;
	using EzBob.Backend.Strategies.VatReturn;
	using Ezbob.Backend.Models;
	using Ezbob.Database;

	partial class EzServiceImplementation {
		public ActionMetaData FirstOfMonthStatusNotifier() {
			return Execute<FirstOfMonthStatusNotifier>(null, null);
		} // FirstOfMonthStatusNotifier

		public ActionMetaData FraudChecker(int customerId, FraudMode mode) {
			return Execute<FraudChecker>(customerId, null, customerId, mode);
		} // FraudChecker

		public ActionMetaData LateBy14Days() {
			return Execute<LateBy14Days>(null, null);
		} // LateBy14Days

		public ActionMetaData PayPointCharger() {
			return Execute<PayPointCharger>(null, null);
		} // PayPointCharger

		public ActionMetaData SetLateLoanStatus() {
			return Execute<SetLateLoanStatus>(null, null);
		} // SetLateLoanStatus

		public ActionMetaData UpdateMarketplace(int customerId, int marketplaceId, bool doUpdateWizardStep) {
			return Execute<UpdateMarketplace>(
				customerId,
				null,
				null,
				amd =>
					DB.ExecuteNonQuery(
						"RecordMpUpdateFailure",
						CommandSpecies.StoredProcedure,
						new QueryParameter("MpId", marketplaceId),
						new QueryParameter("Error", "Status: " + amd.Status + " " + amd.Comment),
						new QueryParameter("Now", DateTime.UtcNow)
					),
				customerId,
				marketplaceId,
				doUpdateWizardStep
			);
		} // UpdateMarketplace

		public ActionMetaData UpdateTransactionStatus() {
			return Execute<UpdateTransactionStatus>(null, null);
		} // UpdateTransactionStatus

		public ActionMetaData XDaysDue() {
			return Execute<XDaysDue>(null, null);
		} // XDaysDue

		public CrmLookupsActionResult CrmLoadLookups() {
			CrmLoadLookups oInstance;

			ActionMetaData oResult = ExecuteSync(out oInstance, null, null);

			return new CrmLookupsActionResult {
				MetaData = oResult,
				Actions = oInstance.Actions.ToArray(),
				Statuses = oInstance.Statuses.ToArray(),
				Ranks = oInstance.Ranks.ToArray(),
			};
		} // CrmLoadLookups

		public ActionMetaData UpdateCurrencyRates() {
			return Execute<UpdateCurrencyRates>(null, null);
		} // UpdateCurrencyRates

		public ActionMetaData UpdateConfigurationVariables() {
			return Execute<UpdateConfigurationVariables>(null, null);
		} //UpdateConfigurationVariables

		#region method PostcodeSaveLog

		public ActionMetaData PostcodeSaveLog(
			string sRequestType,
			string sUrl,
			string sStatus,
			string sResponseData,
			string sErrorMessage,
			int nUserID
		) {
			return Execute<PostcodeSaveLog>(
				null,
				nUserID,
				sRequestType,
				sUrl,
				sStatus,
				sResponseData,
				sErrorMessage,
				nUserID
			);
		} 
		// PostcodeSaveLog

		#endregion method PostcodeSaveLog

		#region method MarketplaceInstantUpdate

		public ActionMetaData MarketplaceInstantUpdate(int nMarketplaceID) {
			return ExecuteSync<MarketplaceInstantUpdate>(null, null, nMarketplaceID);
		} // MarketplaceInstantUpdate

		#endregion method MarketplaceInstantUpdate

		#region method EncryptChannelGrabberMarketplaces

		public ActionMetaData EncryptChannelGrabberMarketplaces() {
			return Execute<EncryptChannelGrabberMarketplaces>(null, null);
		} // EncryptChannelGrabberMarketplaces

		#endregion method EncryptChannelGrabberMarketplaces

		#region method DisplayMarketplaceSecurityData

		public ActionMetaData DisplayMarketplaceSecurityData(int nCustomerID) {
			return ExecuteSync<DisplayMarketplaceSecurityData>(nCustomerID, null, nCustomerID);
		} // DisplayMarketplaceSecurityData

		#endregion method DisplayMarketplaceSecurityData

		#region method FindAccountsToUpdate

		public AccountsToUpdateActionResult FindAccountsToUpdate(int nCustomerID) {
			FindAccountsToUpdate oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, nCustomerID, null, nCustomerID);

			return new AccountsToUpdateActionResult {
				MetaData = oMetaData,
				AccountInfo = oInstance.Result,
			};
		} // FindAccountsToUpdate

		#endregion method FindAccountsToUpdate

		#region method UpdateLinkedHmrcPassword

		public ActionMetaData UpdateLinkedHmrcPassword(string sCustomerID, string sDisplayName, string sPassword, string sHash) {
			return ExecuteSync<UpdateLinkedHmrcPassword>(null, null, sCustomerID, sDisplayName, sPassword, sHash);
		} // UpdateLinkedHmrcPassword

		#endregion method UpdateLinkedHmrcPassword

		#region method ValidateAndUpdateLinkedHmrcPassword

		public StringActionResult ValidateAndUpdateLinkedHmrcPassword(string sCustomerID, string sDisplayName, string sPassword, string sHash) {
			ValidateAndUpdateLinkedHmrcPassword oInstanse;

			ActionMetaData oMetaData = ExecuteSync(out oInstanse, null, null, sCustomerID, sDisplayName, sPassword, sHash);

			return new StringActionResult {
				MetaData = oMetaData,
				Value = oInstanse.ErrorMessage,
			};
		} // ValidateAndUpdateLinkedHmrcPassword

		#endregion method UpdateLinkedHmrcPassword

		#region method CalculateModelsAndAffordability

		public MarketplacesActionResult CalculateModelsAndAffordability(int nCustomerID, DateTime? oHistory) {
			CalculateModelsAndAffordability oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, nCustomerID, null, nCustomerID, oHistory);

			return new MarketplacesActionResult {
				Models = oInstance.Models,
				Affordability = oInstance.Affordability.ToArray(),
				MetaData = oMetaData,
			};
		} // CalculateModelsAndAffordability

		#endregion method CalculateModelsAndAffordability

		#region method SaveSourceRefHistory

		public ActionMetaData SaveSourceRefHistory(int nUserID, string sSourceRefList, string sVisitTimeList) {
			return Execute<SaveSourceRefHistory>(nUserID, null, nUserID, sSourceRefList, sVisitTimeList);
		} // SaveSourceRefHistory

		#endregion method SaveSourceRefHistory
	} // class EzServiceImplementation
} // namespace EzService
