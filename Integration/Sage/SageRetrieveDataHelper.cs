namespace Sage {
	using EzBob.CommonLib;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.Model.Database;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Serialization;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;

	public class SageRetrieveDataHelper : MarketplaceRetrieveDataHelperBase {
		public SageRetrieveDataHelper(
			DatabaseDataHelper helper,
			DatabaseMarketplaceBaseBase marketplace
			)
			: base(helper, marketplace) {
		}

		public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId) {
			return null;
		}

		protected override ElapsedTimeInfo RetrieveAndAggregate(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			log.Info("Starting to update Sage marketplace. Id:{0} Name:{1}", databaseCustomerMarketPlace.Id, databaseCustomerMarketPlace.DisplayName);

			SageSecurityInfo sageSecurityInfo = (Serialized.Deserialize<SageSecurityInfo>(databaseCustomerMarketPlace.SecurityData));

			string accessToken = sageSecurityInfo.AccessToken;

			log.Info("Getting sales invoices...");

			SageSalesInvoicesList salesInvoices = SageConnector.GetSalesInvoices(
				accessToken,
				Helper.GetSageDeltaPeriod(databaseCustomerMarketPlace)
			);

			log.Info("Getting incomes...");

			SageIncomesList incomes = SageConnector.GetIncomes(
				accessToken,
				Helper.GetSageDeltaPeriod(databaseCustomerMarketPlace)
			);

			log.Info("Getting purchase invoices...");

			SagePurchaseInvoicesList purchaseInvoices = SageConnector.GetPurchaseInvoices(
				accessToken,
				Helper.GetSageDeltaPeriod(databaseCustomerMarketPlace)
			);

			log.Info("Getting expenditures...");

			SageExpendituresList expenditures = SageConnector.GetExpenditures(
				accessToken,
				Helper.GetSageDeltaPeriod(databaseCustomerMarketPlace)
			);

			var elapsedTimeInfo = new ElapsedTimeInfo();

			log.Info("Saving request, {0} sales invoices, {1} purchase invoices, {2} incomes, {3} expenditures in DB...", salesInvoices.Count, purchaseInvoices.Count, incomes.Count, expenditures.Count);

			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				databaseCustomerMarketPlace.Id,
				ElapsedDataMemberType.StoreDataToDatabase,
				() => Helper.StoreSageData(
					databaseCustomerMarketPlace,
					salesInvoices,
					purchaseInvoices,
					incomes,
					expenditures,
					historyRecord
				)
			);

			log.Info("Getting payment statuses...");
			var paymentStatuses = SageConnector.GetPaymentStatuses(accessToken);

			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				databaseCustomerMarketPlace.Id,
				ElapsedDataMemberType.StoreDataToDatabase,
				() => Helper.StoreSagePaymentStatuses(paymentStatuses)
			);

			return elapsedTimeInfo;
		}

		private static readonly ASafeLog log = new SafeILog(typeof(SageRetrieveDataHelper));
	}
}