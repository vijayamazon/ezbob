namespace EzBob.PayPal
{
	using ConfigManager;
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
	using StructureMap;

	public class PayPalRetriveDataHelper : MarketplaceRetrieveDataHelperBase<PayPalDatabaseFunctionType>
	{
		private readonly IPayPalConfig _Config;
		private static readonly ILog Log = LogManager.GetLogger(typeof(PayPalRetriveDataHelper));

		public PayPalRetriveDataHelper(DatabaseDataHelper helper, DatabaseMarketplaceBase<PayPalDatabaseFunctionType> marketplace)
			: base(helper, marketplace)
		{
			_Config = ObjectFactory.GetInstance<IPayPalConfig>();
		}

		protected override void InternalUpdateInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
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

		public void UpdateAccountInfo(Customer databaseCustomer)
		{
			base.UpdateAllDataFor(UpdateAccountInfo, databaseCustomer);
		}

		/*public void UpdateTransactionInfo( IDatabaseCustomer databaseCustomer )
		{
			base.UpdateAllDataFor( UpdateTransactionInfo, databaseCustomer );
		}

		public void UpdateTransactionInfo( IDatabaseCustomerMarketPlace databaseCustomerMarketPlace )
		{
			var securityInfo = RetrieveCustomerSecurityInfo<PayPalSecurityData>( databaseCustomerMarketPlace );
			UpdateTransactionInfo(databaseCustomerMarketPlace, securityInfo, TODO);
		}*/

		private void UpdateTransactionInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, PayPalSecurityData securityInfo, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			Helper.CustomerMarketplaceUpdateAction(CustomerMarketplaceUpdateActionType.UpdateTransactionInfo, databaseCustomerMarketPlace, historyRecord, () =>
				{
					var endDate = DateTime.UtcNow;
					var elapsedTimeInfo = new ElapsedTimeInfo();
					DateTime? startDate = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => Helper.GetLastPayPalTransactionRequest(databaseCustomerMarketPlace));
					if (!startDate.HasValue)
					{
						startDate = endDate.AddMonths(-CurrentValues.Instance.PayPalTransactionSearchMonthsBack);
					}

					var errorRetryingInfo = new ErrorRetryingInfo((bool)CurrentValues.Instance.PayPalEnableRetrying, CurrentValues.Instance.PayPalMinorTimeoutInSeconds, CurrentValues.Instance.PayPalUseLastTimeOut);

					errorRetryingInfo.Info = new ErrorRetryingItemInfo[3];
					errorRetryingInfo.Info[0] = new ErrorRetryingItemInfo(CurrentValues.Instance.PayPalIterationSettings1Index, CurrentValues.Instance.PayPalIterationSettings1CountRequestsExpectError, CurrentValues.Instance.PayPalIterationSettings1TimeOutAfterRetryingExpiredInMinutes);
					errorRetryingInfo.Info[1] = new ErrorRetryingItemInfo(CurrentValues.Instance.PayPalIterationSettings2Index, CurrentValues.Instance.PayPalIterationSettings2CountRequestsExpectError, CurrentValues.Instance.PayPalIterationSettings2TimeOutAfterRetryingExpiredInMinutes);
					errorRetryingInfo.Info[2] = new ErrorRetryingItemInfo(CurrentValues.Instance.PayPalIterationSettings3Index, CurrentValues.Instance.PayPalIterationSettings3CountRequestsExpectError, CurrentValues.Instance.PayPalIterationSettings3TimeOutAfterRetryingExpiredInMinutes);
		
					var reqInfo = new PayPalRequestInfo
						{
							SecurityInfo = securityInfo,
							StartDate = startDate.Value,
							EndDate = endDate,
							ErrorRetryingInfo = errorRetryingInfo,
							OpenTimeOutInMinutes = CurrentValues.Instance.PayPalOpenTimeoutInMinutes,
							SendTimeoutInMinutes = CurrentValues.Instance.PayPalSendTimeoutInMinutes
						};
					var newTransactionsList = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									ElapsedDataMemberType.RetrieveDataFromExternalService,
									() => PayPalServiceHelper.GetTransactionData(_Config, reqInfo));

					ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									ElapsedDataMemberType.StoreDataToDatabase,
									() => Helper.SavePayPalTransactionInfo(databaseCustomerMarketPlace, newTransactionsList, historyRecord));

					if (newTransactionsList != null)
					{
						var allTransactions = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
										ElapsedDataMemberType.RetrieveDataFromDatabase,
										() => Helper.GetAllPayPalTransactions(newTransactionsList.SubmittedDate, databaseCustomerMarketPlace));

						var aggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
										ElapsedDataMemberType.AggregateData,
										() => CreateTransactionAggregationInfo(allTransactions, Helper.CurrencyConverter));
						// Save
						ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
										ElapsedDataMemberType.StoreAggregatedData,
										() => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, aggregatedData, historyRecord));
					}
					else
					{
						Log.DebugFormat("PayPal New transactions list is null, skipping aggregation");
					}
					return new UpdateActionResultInfo
					{
						Name = UpdateActionResultType.TransactionItemsCount,
						Value = newTransactionsList == null ? null : (object)newTransactionsList.Count,
						RequestsCounter = newTransactionsList == null ? null : newTransactionsList.RequestsCounter,
						ElapsedTime = elapsedTimeInfo
					};

				}
			);
		}

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
			return PayPalServiceHelper.GetAccessToken(_Config, requestToken, verificationCode);
		}

		public PayPalPersonalData GetAccountInfo(PayPalPermissionsGranted securityInfo)
		{
			return PayPalServiceHelper.GetAccountInfo(_Config, securityInfo);
		}

		/*public MP_CustomerMarketPlace SaveOrUpdateCustomerMarketplace( string email, byte[] securityData, Customer customer )
		{
			return Helper.SaveOrUpdateCustomerMarketplace( email, MarketPlace, securityData, customer );
		}

		public ICustomerMarketPlace SaveOrUpdateCustomerMarketplace( string email, byte[] securityData, Customer customer )
		{
			return Helper.SaveOrUpdateCustomerMarketplace( email, MarketPlace, securityData, customer );
		}*/
	}
}