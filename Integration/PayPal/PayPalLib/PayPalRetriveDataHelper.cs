namespace EzBob.PayPal {
	using System;
	using System.Threading;
	using ConfigManager;
	using Ezbob.Database;
	using Ezbob.Utils;
	using EzBob.CommonLib;
	using EzBob.PayPalDbLib.Models;
	using EzBob.PayPalServiceLib;
	using EzBob.PayPalServiceLib.Common;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.DatabaseWrapper.AccountInfo;
	using EZBob.DatabaseLib.DatabaseWrapper.Transactions;
	using EZBob.DatabaseLib.Model.Database;
	using log4net;

	public class PayPalRetriveDataHelper : MarketplaceRetrieveDataHelperBase {
		public PayPalRetriveDataHelper(DatabaseDataHelper helper, DatabaseMarketplaceBaseBase marketplace)
			: base(helper, marketplace) { }

		public PayPalPermissionsGranted GetAccessToken(string requestToken, string verificationCode) {
			return PayPalServiceHelper.GetAccessToken(requestToken, verificationCode);
		}

		public PayPalPersonalData GetAccountInfo(PayPalPermissionsGranted securityInfo) {
			return PayPalServiceHelper.GetAccountInfo(securityInfo);
		}

		public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId) {
			return RetrieveCustomerSecurityInfo<PayPalSecurityData>(GetDatabaseCustomerMarketPlace(customerMarketPlaceId));
		}

		public override void Update(int nCustomerMarketplaceID) {
			UpdateCustomerMarketplaceFirst(nCustomerMarketplaceID);
		}

		public void UpdateAccountInfo(int umi) {
			var databaseCustomerMarketPlace = Helper.GetDatabaseCustomerMarketPlace(Marketplace, umi);
			UpdateAccountInfo(databaseCustomerMarketPlace);
		}

		protected override void InternalUpdateInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, MP_CustomerMarketplaceUpdatingHistory historyRecord) {
			if (!databaseCustomerMarketPlace.Created.HasValue || (databaseCustomerMarketPlace.Created.Value.Date == DateTime.Today)) {
				Log.DebugFormat("Paypal added today, waiting for {0} milliseconds", CurrentValues.Instance.PayPalFirstTimeWait.Value);
				Thread.Sleep((int)CurrentValues.Instance.PayPalFirstTimeWait);
			} // if

			var securityInfo = RetrieveCustomerSecurityInfo<PayPalSecurityData>(databaseCustomerMarketPlace);

			Helper.CustomerMarketplaceUpdateAction(
				CustomerMarketplaceUpdateActionType.UpdateTransactionInfo,
				databaseCustomerMarketPlace,
				historyRecord,
				() => UpdateTransactionInfo(databaseCustomerMarketPlace, securityInfo, historyRecord)
				);
		}

		protected override ElapsedTimeInfo RetrieveAndAggregate(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
			) {
			// This method is not implemented here because elapsed time is got from over source.
			throw new NotImplementedException();
		}

		private void UpdateAccountInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace) {
			var securityInfo = RetrieveCustomerSecurityInfo<PayPalSecurityData>(databaseCustomerMarketPlace);
			PayPalPersonalData personalData = GetAccountInfo(securityInfo.PermissionsGranted);
			Helper.SaveOrUpdateAcctountInfo(databaseCustomerMarketPlace, personalData);
		}

		private UpdateActionResultInfo UpdateTransactionInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, PayPalSecurityData securityInfo, MP_CustomerMarketplaceUpdatingHistory historyRecord) {
			var endDate = DateTime.UtcNow;
			var elapsedTimeInfo = new ElapsedTimeInfo();

			DateTime? startDate = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				databaseCustomerMarketPlace.Id,
				ElapsedDataMemberType.RetrieveDataFromDatabase,
				() => Helper.GetLastPayPalTransactionDate(databaseCustomerMarketPlace)
				);

			if (!startDate.HasValue)
				startDate = endDate.AddMonths(-CurrentValues.Instance.PayPalTransactionSearchMonthsBack);

			var errorRetryingInfo = new ErrorRetryingInfo((bool)CurrentValues.Instance.PayPalEnableRetrying, CurrentValues.Instance.PayPalMinorTimeoutInSeconds, CurrentValues.Instance.PayPalUseLastTimeOut);

			errorRetryingInfo.Info = new ErrorRetryingItemInfo[3];
			errorRetryingInfo.Info[0] = new ErrorRetryingItemInfo(CurrentValues.Instance.PayPalIterationSettings1Index, CurrentValues.Instance.PayPalIterationSettings1CountRequestsExpectError, CurrentValues.Instance.PayPalIterationSettings1TimeOutAfterRetryingExpiredInMinutes);
			errorRetryingInfo.Info[1] = new ErrorRetryingItemInfo(CurrentValues.Instance.PayPalIterationSettings2Index, CurrentValues.Instance.PayPalIterationSettings2CountRequestsExpectError, CurrentValues.Instance.PayPalIterationSettings2TimeOutAfterRetryingExpiredInMinutes);
			errorRetryingInfo.Info[2] = new ErrorRetryingItemInfo(CurrentValues.Instance.PayPalIterationSettings3Index, CurrentValues.Instance.PayPalIterationSettings3CountRequestsExpectError, CurrentValues.Instance.PayPalIterationSettings3TimeOutAfterRetryingExpiredInMinutes);

			var reqInfo = new PayPalRequestInfo {
				SecurityInfo = securityInfo,
				StartDate = startDate.Value,
				EndDate = endDate,
				ErrorRetryingInfo = errorRetryingInfo,
				OpenTimeOutInMinutes = CurrentValues.Instance.PayPalOpenTimeoutInMinutes,
				SendTimeoutInMinutes = CurrentValues.Instance.PayPalSendTimeoutInMinutes,
				DaysPerRequest = CurrentValues.Instance.PayPalDaysPerRequest
			};

			MP_PayPalTransaction mpTransaction = null;
			var trnList = new PayPalTransactionsList(DateTime.UtcNow);
			trnList.RequestsCounter = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
				elapsedTimeInfo,
				databaseCustomerMarketPlace.Id,
				ElapsedDataMemberType.RetrieveDataFromExternalService,
				() => PayPalServiceHelper.GetTransactionData(reqInfo, data => {
					mpTransaction = Helper.SavePayPalTransactionInfo(databaseCustomerMarketPlace, data, historyRecord, mpTransaction);
					return trnList.TryAddNewData(data);
				})
				);

			DbConnectionGenerator.Get()
				.ExecuteNonQuery(
					"UpdateMpTotalsPayPal",
					CommandSpecies.StoredProcedure,
					new QueryParameter("HistoryID", historyRecord.Id)
				);

			return new UpdateActionResultInfo {
				Name = UpdateActionResultType.TransactionItemsCount,
				Value = trnList.Count,
				RequestsCounter = trnList.RequestsCounter,
				ElapsedTime = elapsedTimeInfo
			};
		}

		private static readonly ILog Log = LogManager.GetLogger(typeof(PayPalRetriveDataHelper));
	} // class PayPalRetriveDataHelper
} // namespace
