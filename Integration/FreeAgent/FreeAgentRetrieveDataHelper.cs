namespace FreeAgent {
	using System;
	using System.Collections.Generic;
	using EzBob.CommonLib;
	using EZBob.DatabaseLib.Model.Marketplaces.FreeAgent;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Serialization;

	public class FreeAgentRetrieveDataHelper : MarketplaceRetrieveDataHelperBase {
		public FreeAgentRetrieveDataHelper(
			DatabaseDataHelper helper,
			DatabaseMarketplaceBaseBase marketplace
		) : base(helper, marketplace) {
			expenseCategories = Helper.GetExpenseCategories();
		} // constructor

		public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId) {
			return null;
		} // RetrieveCustomerSecurityInfo

		protected override ElapsedTimeInfo RetrieveAndAggregate(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			log.Info(
				"Starting to update FreeAgent marketplace id: {0} name: '{1}'...",
				databaseCustomerMarketPlace.Id,
				databaseCustomerMarketPlace.DisplayName
			);

			FreeAgentSecurityInfo freeAgentSecurityInfo = Serialized.Deserialize<FreeAgentSecurityInfo>(
				databaseCustomerMarketPlace.SecurityData
			);

			string accessToken = freeAgentSecurityInfo.AccessToken;

			if (DateTime.UtcNow > freeAgentSecurityInfo.ValidUntil) {
				log.Info(
					"FreeAgent marketplace id: {0} name: '{1}': starting to refresh access token...",
					databaseCustomerMarketPlace.Id,
					databaseCustomerMarketPlace.DisplayName
				);

				var tokenContainer = FreeAgentConnector.RefreshToken(freeAgentSecurityInfo.RefreshToken);

				if (tokenContainer == null) {
					string err = string.Format(
						"FreeAgent marketplace id: {0} name: '{1}': failed to refresh access token.",
						databaseCustomerMarketPlace.Id,
						databaseCustomerMarketPlace.DisplayName
					);

					log.Warn(err);

					throw new Exception(err);
				} // if

				log.Info(
					"FreeAgent marketplace id: {0} name: '{1}': received new access token, will save it to DB.",
					databaseCustomerMarketPlace.Id,
					databaseCustomerMarketPlace.DisplayName
				);

				var securityData = new FreeAgentSecurityInfo {
					ApprovalToken = freeAgentSecurityInfo.ApprovalToken,
					AccessToken = tokenContainer.access_token,
					ExpiresIn = tokenContainer.expires_in,
					TokenType = tokenContainer.token_type,
					RefreshToken = freeAgentSecurityInfo.RefreshToken,
					MarketplaceId = freeAgentSecurityInfo.MarketplaceId,
					Name = freeAgentSecurityInfo.Name,
					ValidUntil = DateTime.UtcNow.AddSeconds(tokenContainer.expires_in - 60)
				};

				var serializedSecurityData = new Serialized(securityData);

				Helper.SaveOrUpdateCustomerMarketplace(
					databaseCustomerMarketPlace.DisplayName,
					new FreeAgentDatabaseMarketPlace(),
					serializedSecurityData,
					databaseCustomerMarketPlace.Customer
				);

				log.Info(
					"FreeAgent marketplace id: {0} name: '{1}': new access token was saved to DB.",
					databaseCustomerMarketPlace.Id,
					databaseCustomerMarketPlace.DisplayName
				);
			} // if

			int invoicesDeltaPeriod =
				Helper.GetFreeAgentInvoiceDeltaPeriod(databaseCustomerMarketPlace);

			log.Info(
				"FreeAgent marketplace id: {0} name: '{1}': getting invoices with delta period = '{2}'...",
				databaseCustomerMarketPlace.Id,
				databaseCustomerMarketPlace.DisplayName,
				invoicesDeltaPeriod
			);

			var freeAgentInvoices = FreeAgentConnector.GetInvoices(accessToken, invoicesDeltaPeriod);

			log.Info(
				"FreeAgent marketplace id: {0} name: '{1}': getting expenses...",
				databaseCustomerMarketPlace.Id,
				databaseCustomerMarketPlace.DisplayName
			);

			var freeAgentExpenses = FreeAgentConnector.GetExpenses(
				accessToken,
				Helper.GetFreeAgentExpenseDeltaPeriod(databaseCustomerMarketPlace)
			);

			log.Info(
				"FreeAgent marketplace id: {0} name: '{1}': filling expenses category...",
				databaseCustomerMarketPlace.Id,
				databaseCustomerMarketPlace.DisplayName
			);

			FillExpensesCategory(freeAgentExpenses, accessToken);

			log.Info(
				"FreeAgent marketplace id: {0} name: '{1}': getting company...",
				databaseCustomerMarketPlace.Id,
				databaseCustomerMarketPlace.DisplayName
			);

			FreeAgentCompany freeAgentCompany = FreeAgentConnector.GetCompany(accessToken);

			log.Info(
				"FreeAgent marketplace id: {0} name: '{1}': getting users...",
				databaseCustomerMarketPlace.Id,
				databaseCustomerMarketPlace.DisplayName
			);

			List<FreeAgentUsers> freeAgentUsers = FreeAgentConnector.GetUsers(accessToken);

			var elapsedTimeInfo = new ElapsedTimeInfo();

			log.Info(
				"FreeAgent marketplace id: {0} name: '{1}': saving request, {2} invoices & {3} expenses in DB...",
				databaseCustomerMarketPlace.Id,
				databaseCustomerMarketPlace.DisplayName,
				freeAgentInvoices.Count,
				freeAgentExpenses.Count
			);

			var mpRequest = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				databaseCustomerMarketPlace.Id,
				ElapsedDataMemberType.StoreDataToDatabase,
				() => Helper.StoreFreeAgentRequestAndInvoicesAndExpensesData(
					databaseCustomerMarketPlace,
					freeAgentInvoices,
					freeAgentExpenses,
					historyRecord
				)
			);

			StoreCompanyData(mpRequest, freeAgentCompany, elapsedTimeInfo, databaseCustomerMarketPlace.Id);

			StoreUsersData(mpRequest, freeAgentUsers, elapsedTimeInfo, databaseCustomerMarketPlace.Id);

			log.Info(
				"Finished to update FreeAgent marketplace id: {0} name: '{1}'.",
				databaseCustomerMarketPlace.Id,
				databaseCustomerMarketPlace.DisplayName
			);

			return elapsedTimeInfo;
		} // RetrieveAndAggregate

		private void FillExpensesCategory(IEnumerable<FreeAgentExpense> freeAgentExpenses, string accessToken) {
			foreach (var expense in freeAgentExpenses) {
				if (expenseCategories.ContainsKey(expense.category))
					expense.categoryItem = expenseCategories[expense.category];
				else {
					log.Info("Getting expenses category: {0}", expense.category);

					expense.categoryItem = FreeAgentConnector.GetExpenseCategory(accessToken, expense.category);

					expense.categoryItem.Id = Helper.AddExpenseCategory(expense.categoryItem);

					if (!expenseCategories.ContainsKey(expense.categoryItem.url))
						expenseCategories.Add(expense.categoryItem.url, expense.categoryItem);
				} // if
			} // for
		} // FillExpensesCategory

		private void StoreCompanyData(
			MP_FreeAgentRequest mpRequest,
			FreeAgentCompany freeAgentCompany,
			ElapsedTimeInfo elapsedTimeInfo,
			int mpId
		) {
			if (mpRequest == null)
				return;

			log.Info("Saving company to DB...");

			var mpFreeAgentCompany = new MP_FreeAgentCompany {
				Request = mpRequest,
				url = freeAgentCompany.url,
				name = freeAgentCompany.name,
				subdomain = freeAgentCompany.subdomain,
				type = freeAgentCompany.type,
				currency = freeAgentCompany.currency,
				mileage_units = freeAgentCompany.mileage_units,
				company_start_date = freeAgentCompany.company_start_date,
				freeagent_start_date = freeAgentCompany.freeagent_start_date,
				first_accounting_year_end = freeAgentCompany.first_accounting_year_end,
				company_registration_number = freeAgentCompany.company_registration_number,
				sales_tax_registration_status = freeAgentCompany.sales_tax_registration_status,
				sales_tax_registration_number = freeAgentCompany.sales_tax_registration_number,
			};

			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				mpId,
				ElapsedDataMemberType.StoreDataToDatabase,
				() => Helper.StoreFreeAgentCompanyData(mpFreeAgentCompany)
			);
		} // StoreCompanyData

		private void StoreUsersData(
			MP_FreeAgentRequest mpRequest,
			List<FreeAgentUsers> freeAgentUsers,
			ElapsedTimeInfo elapsedTimeInfo,
			int mpId
		) {
			if (mpRequest == null)
				return;

			log.Info("Saving {0} user(s) in DB...", freeAgentUsers.Count);

			var mpFreeAgentUsersList = new List<MP_FreeAgentUsers>();

			foreach (FreeAgentUsers user in freeAgentUsers) {
				var mpFreeAgentUsers = new MP_FreeAgentUsers {
					Request = mpRequest,
					url = user.url,
					first_name = user.first_name,
					last_name = user.last_name,
					email = user.email,
					role = user.role,
					permission_level = user.permission_level,
					opening_mileage = user.opening_mileage,
					updated_at = user.updated_at,
					created_at = user.created_at
				};

				mpFreeAgentUsersList.Add(mpFreeAgentUsers);
			} // for

			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				mpId,
				ElapsedDataMemberType.StoreDataToDatabase,
				() => Helper.StoreFreeAgentUsersData(mpFreeAgentUsersList)
			);
		} // StoreUsersData

		private static readonly ASafeLog log = new SafeILog(typeof(FreeAgentRetrieveDataHelper));
		private readonly Dictionary<string, FreeAgentExpenseCategory> expenseCategories;
	} // class FreeAgentRetrieveDataHelper
} // namespace