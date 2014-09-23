namespace EzBob.PayPal
{
	using System.Threading;
	using ConfigManager;
	using Ezbob.Utils;
	using log4net;
	using System;
	using System.Collections.Generic;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.DatabaseWrapper.AccountInfo;
	using EZBob.DatabaseLib.DatabaseWrapper.FunctionValues;
	using EZBob.DatabaseLib.DatabaseWrapper.Transactions;
	using EZBob.DatabaseLib.Model.Database;
	using CommonLib;
	using PayPalDbLib;
	using PayPalDbLib.Models;
	using PayPalServiceLib;
	using PayPalServiceLib.Common;

	public class PayPalRetriveDataHelper : MarketplaceRetrieveDataHelperBase<PayPalDatabaseFunctionType>
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(PayPalRetriveDataHelper));

		public PayPalRetriveDataHelper(DatabaseDataHelper helper, DatabaseMarketplaceBase<PayPalDatabaseFunctionType> marketplace)
			: base(helper, marketplace)
		{
		}

		public override void Update(int nCustomerMarketplaceID) {
			UpdateCustomerMarketplaceFirst(nCustomerMarketplaceID);
		} // Update

		protected override ElapsedTimeInfo RetrieveAndAggregate(
			IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord
		) {
			// This method is not implemented here because elapsed time is got from over source.
			throw new NotImplementedException();
		}

		protected override void InternalUpdateInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			if (!databaseCustomerMarketPlace.Created.HasValue || (databaseCustomerMarketPlace.Created.Value.Date == DateTime.Today)) {
				Log.DebugFormat("Paypal added today, waiting for {0} milliseconds", CurrentValues.Instance.PayPalFirstTimeWait.Value);
				Thread.Sleep((int)CurrentValues.Instance.PayPalFirstTimeWait);
			}

			var securityInfo = RetrieveCustomerSecurityInfo<PayPalSecurityData>(databaseCustomerMarketPlace);
			UpdateTransactionInfo(databaseCustomerMarketPlace, securityInfo, historyRecord);
		}

		protected override void AddAnalysisValues(IDatabaseCustomerMarketPlace marketPlace, AnalysisDataInfo data)
		{
		}

		public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId)
		{
			return RetrieveCustomerSecurityInfo<PayPalSecurityData>(GetDatabaseCustomerMarketPlace(customerMarketPlaceId));
		}

		public void UpdateAccountInfo(int umi)
		{
			var databaseCustomerMarketPlace = Helper.GetDatabaseCustomerMarketPlace(Marketplace, umi);
			UpdateAccountInfo(databaseCustomerMarketPlace);
		}

		private void UpdateAccountInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			var securityInfo = RetrieveCustomerSecurityInfo<PayPalSecurityData>(databaseCustomerMarketPlace);
			PayPalPersonalData personalData = GetAccountInfo(securityInfo.PermissionsGranted);
			SaveOrUpdateAcctountInfo(databaseCustomerMarketPlace, personalData);
		}

		private void SaveOrUpdateAcctountInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, PayPalPersonalData personalData)
		{
			Helper.SaveOrUpdateAcctountInfo(databaseCustomerMarketPlace, personalData);
		}

		private void UpdateTransactionInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, PayPalSecurityData securityInfo, MP_CustomerMarketplaceUpdatingHistory historyRecord) {
			Helper.CustomerMarketplaceUpdateAction(
				CustomerMarketplaceUpdateActionType.UpdateTransactionInfo,
				databaseCustomerMarketPlace,
				historyRecord,
				() => {
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
						SendTimeoutInMinutes = CurrentValues.Instance.PayPalSendTimeoutInMinutes
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

					if (trnList.Count > 0) {
						var allTransactions = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
							elapsedTimeInfo,
							databaseCustomerMarketPlace.Id,
							ElapsedDataMemberType.RetrieveDataFromDatabase,
							() => Helper.GetAllPayPalTransactions(trnList.SubmittedDate, databaseCustomerMarketPlace)
						);

						var aggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
							elapsedTimeInfo,
							databaseCustomerMarketPlace.Id,
							ElapsedDataMemberType.AggregateData,
							() => CreateTransactionAggregationInfo(allTransactions, Helper.CurrencyConverter)
						);

						// Save
						ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(
							elapsedTimeInfo,
							databaseCustomerMarketPlace.Id,
							ElapsedDataMemberType.StoreAggregatedData,
							() => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, aggregatedData, historyRecord)
						);
					}
					else
						Log.DebugFormat("PayPal New transactions list is null, skipping aggregation");

					return new UpdateActionResultInfo {
						Name = UpdateActionResultType.TransactionItemsCount,
						Value = trnList.Count,
						RequestsCounter = trnList.RequestsCounter,
						ElapsedTime = elapsedTimeInfo
					};
				}
			);
		} // UpdateTransactionInfo

		private IEnumerable<IWriteDataInfo<PayPalDatabaseFunctionType>> CreateTransactionAggregationInfo(PayPalTransactionsList data, ICurrencyConvertor currencyConverter)
		{
			var aggregateFunctionArray = new[]
				{
					PayPalDatabaseFunctionType.TransactionsNumber,
					PayPalDatabaseFunctionType.TotalNetInPayments,
					PayPalDatabaseFunctionType.TotalNetInPaymentsAnnualized,
					PayPalDatabaseFunctionType.TotalNetOutPayments,
					PayPalDatabaseFunctionType.TotalNetRevenues,
					PayPalDatabaseFunctionType.TotalNetExpenses,
					PayPalDatabaseFunctionType.NumOfTotalTransactions,
					PayPalDatabaseFunctionType.RevenuesForTransactions,
					PayPalDatabaseFunctionType.NetNumOfRefundsAndReturns,
					PayPalDatabaseFunctionType.TransferAndWireOut,
					PayPalDatabaseFunctionType.TransferAndWireIn,
					
					PayPalDatabaseFunctionType.GrossIncome,
					PayPalDatabaseFunctionType.GrossProfitMargin,
					PayPalDatabaseFunctionType.RevenuePerTrasnaction,
					PayPalDatabaseFunctionType.NetSumOfRefundsAndReturns,
					PayPalDatabaseFunctionType.RatioNetSumOfRefundsAndReturnsToNetRevenues,
					PayPalDatabaseFunctionType.NetTransfersAmount,
					PayPalDatabaseFunctionType.OutstandingBalance,
					PayPalDatabaseFunctionType.NumTransfersOut,
					PayPalDatabaseFunctionType.AmountPerTransferOut,
					PayPalDatabaseFunctionType.NumTransfersIn,
					PayPalDatabaseFunctionType.AmountPerTransferIn,
				};

			var updated = data.SubmittedDate;
			var timePeriodData = DataAggregatorHelper.GetOrdersForPeriods(data, (submittedDate, o) => new PayPalTransactionsList(submittedDate, o));

			var factory = new PayPalTransactionAgregatorFactory();

			return DataAggregatorHelper.AggregateData(factory, timePeriodData, aggregateFunctionArray, updated, currencyConverter);

		}

		public PayPalPermissionsGranted GetAccessToken(string requestToken, string verificationCode)
		{
			return PayPalServiceHelper.GetAccessToken(requestToken, verificationCode);
		}

		public PayPalPersonalData GetAccountInfo(PayPalPermissionsGranted securityInfo)
		{
			return PayPalServiceHelper.GetAccountInfo(securityInfo);
		}
	}
}