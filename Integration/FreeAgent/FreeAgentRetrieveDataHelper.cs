namespace FreeAgent {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using System.Xml.Serialization;
	using EzBob.CommonLib;
	using EZBob.DatabaseLib.Model.Marketplaces.FreeAgent;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Security;
	using Ezbob.Utils.Serialization;

	public class FreeAgentRetrieveDataHelper : MarketplaceRetrieveDataHelperBase {
		public FreeAgentRetrieveDataHelper(
			DatabaseDataHelper helper,
			DatabaseMarketplaceBaseBase marketplace
		) : base(helper, marketplace) {
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

			FreeAgentSecurityInfo freeAgentSecurityInfo;

			try {
				log.Debug(
					"Trying to deserialize (without decrypting) security info for marketplace {0} name: '{1}'...",
					databaseCustomerMarketPlace.Id,
					databaseCustomerMarketPlace.DisplayName
				);

				freeAgentSecurityInfo = Serialized.Deserialize<FreeAgentSecurityInfo>(
					databaseCustomerMarketPlace.SecurityData
				);

				log.Debug(
					"Successfully deserialized (without decrypting) security info for marketplace {0} name: '{1}'.",
					databaseCustomerMarketPlace.Id,
					databaseCustomerMarketPlace.DisplayName
				);
			} catch (Exception e) {
				log.Warn(
					e,
					"Failed to deserialize (without decrypting) security info for marketplace {0} name: '{1}'.",
					databaseCustomerMarketPlace.Id,
					databaseCustomerMarketPlace.DisplayName
				);

				freeAgentSecurityInfo = null;
			} // try

			if (freeAgentSecurityInfo == null) {
				try {
					log.Debug(
						"Trying to decrypt (without deserializing) security info for marketplace {0} name: '{1}'...",
						databaseCustomerMarketPlace.Id,
						databaseCustomerMarketPlace.DisplayName
					);

					string decrypted = Encrypted.Decrypt(databaseCustomerMarketPlace.SecurityData);

					log.Debug("Decrypted text: {0}", decrypted);

					MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(
						decrypted
					));

					freeAgentSecurityInfo = (FreeAgentSecurityInfo)new XmlSerializer(typeof(FreeAgentSecurityInfo))
						.Deserialize(memStream);

					log.Debug(
						"Successfully decrypted (without deserializing) security info for marketplace {0} name: '{1}'.",
						databaseCustomerMarketPlace.Id,
						databaseCustomerMarketPlace.DisplayName
					);
				} catch (Exception e) {
					log.Warn(
						e,
						"Failed to decrypt (without deserializing) security info for marketplace {0} name: '{1}'.",
						databaseCustomerMarketPlace.Id,
						databaseCustomerMarketPlace.DisplayName
					);

					freeAgentSecurityInfo = null;
				} // try
			} // if

			if (freeAgentSecurityInfo == null) {
				try {
					log.Debug(
						"Trying to deserialize and decrypt security info for marketplace {0} name: '{1}'...",
						databaseCustomerMarketPlace.Id,
						databaseCustomerMarketPlace.DisplayName
					);

					freeAgentSecurityInfo = Serialized.Deserialize<FreeAgentSecurityInfo>(Encrypted.Decrypt(
						databaseCustomerMarketPlace.SecurityData
					));

					log.Debug(
						"Successfully deserialized and decrypted security info for marketplace {0} name: '{1}'.",
						databaseCustomerMarketPlace.Id,
						databaseCustomerMarketPlace.DisplayName
					);
				} catch (Exception e) {
					log.Warn(
						e,
						"Failed to deserialize and decrypt security info for marketplace {0} name: '{1}'.",
						databaseCustomerMarketPlace.Id,
						databaseCustomerMarketPlace.DisplayName
					);

					freeAgentSecurityInfo = null;
				} // try
			} // if

			if (freeAgentSecurityInfo == null) {
				throw new Exception(string.Format(
					"FreeAgent marketplace id: {0} name: '{1}': failed to decrypt/deserialize security info.",
					databaseCustomerMarketPlace.Id,
					databaseCustomerMarketPlace.DisplayName
				));
			} // if

			var connector = new FreeAgentConnector();

			string accessToken = freeAgentSecurityInfo.AccessToken;

			if (DateTime.UtcNow > freeAgentSecurityInfo.ValidUntil) {
				log.Info(
					"FreeAgent marketplace id: {0} name: '{1}': starting to refresh access token...",
					databaseCustomerMarketPlace.Id,
					databaseCustomerMarketPlace.DisplayName
				);

				var tokenContainer = connector.RefreshToken(freeAgentSecurityInfo.RefreshToken);

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

			var freeAgentInvoices = connector.GetInvoices(accessToken, invoicesDeltaPeriod);

			log.Info(
				"FreeAgent marketplace id: {0} name: '{1}': getting expenses...",
				databaseCustomerMarketPlace.Id,
				databaseCustomerMarketPlace.DisplayName
			);

			var freeAgentExpenses = connector.GetExpenses(
				accessToken,
				Helper.GetFreeAgentExpenseDeltaPeriod(databaseCustomerMarketPlace)
			);

			log.Info(
				"FreeAgent marketplace id: {0} name: '{1}': filling expenses category...",
				databaseCustomerMarketPlace.Id,
				databaseCustomerMarketPlace.DisplayName
			);

			foreach (var expense in freeAgentExpenses) {
				expense.categoryItem = connector.GetExpenseCategory(accessToken, expense.category);
				expense.categoryItem.Id = Helper.AddFreeAgentExpenseCategory(expense.categoryItem);
			} // for each

			log.Info(
				"FreeAgent marketplace id: {0} name: '{1}': getting company...",
				databaseCustomerMarketPlace.Id,
				databaseCustomerMarketPlace.DisplayName
			);

			FreeAgentCompany freeAgentCompany = connector.GetCompany(accessToken);

			log.Info(
				"FreeAgent marketplace id: {0} name: '{1}': getting users...",
				databaseCustomerMarketPlace.Id,
				databaseCustomerMarketPlace.DisplayName
			);

			List<FreeAgentUsers> freeAgentUsers = connector.GetUsers(accessToken);

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
				() => {
					Helper.StoreFreeAgentCompanyData(mpFreeAgentCompany);
					Helper.UpdateMarketplaceDisplayName(mpRequest.CustomerMarketPlace, freeAgentCompany.name);
				}
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
	} // class FreeAgentRetrieveDataHelper
} // namespace