﻿namespace EzService.EzServiceImplementation {
	using System;
	using System.Linq;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Backend.Strategies.Postcode;
	using Ezbob.Backend.Strategies.VatReturn;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.ModelsWithDB;
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

		public ActionMetaData UpdateMarketplace(int customerId, int marketplaceId, bool doUpdateWizardStep, int userId) {
			var onfail = new Action<ActionMetaData>(
				amd =>
					this.DB.ExecuteNonQuery(
						"RecordMpUpdateFailure",
						CommandSpecies.StoredProcedure,
						new QueryParameter("MpId", marketplaceId),
						new QueryParameter("Error", "Status: " + amd.Status + " " + amd.Comment),
						new QueryParameter("Now", DateTime.UtcNow)
					)
			);

			return Execute(new ExecuteArguments(customerId, marketplaceId, doUpdateWizardStep) {
				StrategyType = typeof(UpdateMarketplace),
				CustomerID = customerId,
				UserID = userId,
				OnException = onfail,
				OnFail = onfail,
			});
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

		public ActionMetaData UpdateConfigurationVariables(int userId) {
			return Execute<UpdateConfigurationVariables>(null, userId);
		} //UpdateConfigurationVariables

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

		public ActionMetaData MarketplaceInstantUpdate(int nMarketplaceID) {
			return ExecuteSync<MarketplaceInstantUpdate>(null, null, nMarketplaceID);
		} // MarketplaceInstantUpdate

		public ActionMetaData EncryptChannelGrabberMarketplaces() {
			return Execute<EncryptChannelGrabberMarketplaces>(null, null);
		} // EncryptChannelGrabberMarketplaces

		public ActionMetaData DisplayMarketplaceSecurityData(int nCustomerID) {
			return ExecuteSync<DisplayMarketplaceSecurityData>(nCustomerID, null, nCustomerID);
		} // DisplayMarketplaceSecurityData

		public AccountsToUpdateActionResult FindAccountsToUpdate(int nCustomerID) {
			FindAccountsToUpdate oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, nCustomerID, null, nCustomerID);

			return new AccountsToUpdateActionResult {
				MetaData = oMetaData,
				AccountInfo = oInstance.Result,
			};
		} // FindAccountsToUpdate

		public ActionMetaData UpdateLinkedHmrcPassword(string sCustomerID, string sDisplayName, string sPassword, string sHash) {
			return ExecuteSync<UpdateLinkedHmrcPassword>(null, null, sCustomerID, sDisplayName, sPassword, sHash);
		} // UpdateLinkedHmrcPassword

		public StringActionResult ValidateAndUpdateLinkedHmrcPassword(string sCustomerID, string sDisplayName, string sPassword, string sHash) {
			ValidateAndUpdateLinkedHmrcPassword oInstanse;

			ActionMetaData oMetaData = ExecuteSync(out oInstanse, null, null, sCustomerID, sDisplayName, sPassword, sHash);

			return new StringActionResult {
				MetaData = oMetaData,
				Value = oInstanse.ErrorMessage,
			};
		} // ValidateAndUpdateLinkedHmrcPassword

		public MarketplacesActionResult CalculateModelsAndAffordability(int userId, int nCustomerID, DateTime? oHistory) {
			CalculateModelsAndAffordability oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, nCustomerID, userId, nCustomerID, oHistory);

			return new MarketplacesActionResult {
				MpModel = oInstance.MpModel,
				MetaData = oMetaData,
			};
		} // CalculateModelsAndAffordability

		public ActionMetaData SaveSourceRefHistory(int nUserID, string sSourceRefList, string sVisitTimeList, CampaignSourceRef campaignSourceRef) {
			return Execute<SaveSourceRefHistory>(nUserID, null, nUserID, sSourceRefList, sVisitTimeList, campaignSourceRef);
		} // SaveSourceRefHistory

		public StringStringMapActionResult LoadCustomerLeadFieldNames() {
			LoadCustomerLeadFieldNames oInstance;

			ActionMetaData oMetaData = ExecuteSync(out oInstance, null, null);

			return new StringStringMapActionResult {
				MetaData = oMetaData,
				Map = oInstance.Result,
			};
		} // LoadCustomerLeadFieldNames

		public ActionMetaData UpdateGoogleAnalytics(DateTime? oBackfillStartDate, DateTime? oBackfillEndDate) {
			return Execute<UpdateGoogleAnalytics>(null, null, oBackfillStartDate, oBackfillEndDate);
		} // UpdateGoogleAnalytics

	} // class EzServiceImplementation
} // namespace EzService
