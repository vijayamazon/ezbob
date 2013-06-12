using System;
using System.Collections.Generic;
using EZBob.DatabaseLib;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.DatabaseWrapper.AccountInfo;
using EZBob.DatabaseLib.DatabaseWrapper.FunctionValues;
using EZBob.DatabaseLib.DatabaseWrapper.Transactions;
using EZBob.DatabaseLib.Model.Database;
using EzBob.CommonLib;
using EzBob.CommonLib.TimePeriodLogic.DependencyChain;
using EzBob.CommonLib.TimePeriodLogic.DependencyChain.Factories;
using EzBob.PayPalDbLib;
using EzBob.PayPalDbLib.Models;
using EzBob.PayPalServiceLib;
using EzBob.PayPalServiceLib.Common;
using StructureMap;

namespace EzBob.PayPal
{
	public class PayPalRetriveDataHelper : MarketplaceRetrieveDataHelperBase<PayPalDatabaseFunctionType>
	{
		private readonly IPayPalConfig _Config;
		private readonly IPayPalMarketplaceSettings _Settings;

		public PayPalRetriveDataHelper(DatabaseDataHelper helper, DatabaseMarketplaceBase<PayPalDatabaseFunctionType> marketplace)
			: base(helper, marketplace)
		{
			_Config = ObjectFactory.GetInstance<IPayPalConfig>();
			_Settings = ObjectFactory.GetInstance<IPayPalMarketplaceSettings>();
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
			PayPalPersonalData personalData = GetAccountInfo(securityInfo.RermissionsGranted);
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
						startDate = endDate.AddMonths(-_Settings.MonthsBack);
					}

					int maxMonthsPerRequest = _Settings.MaxMonthsPerRequest;
					var errorRetryingInfo = _Settings.ErrorRetryingInfo;

					var reqInfo = new PayPalRequestInfo
						{
							SecurityInfo = securityInfo,
							StartDate = startDate.Value,
							EndDate = endDate,
							MaxMonthsPerRequest = maxMonthsPerRequest,
							ErrorRetryingInfo = errorRetryingInfo,
							OpenTimeOutInMinutes = _Settings.OpenTimeOutInMinutes,
							SendTimeoutInMinutes = _Settings.SendTimeoutInMinutes
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
					PayPalDatabaseFunctionType.TotalNetOutPayments,
					PayPalDatabaseFunctionType.TotalNetRevenues,
					PayPalDatabaseFunctionType.TotalNetExpenses,
					PayPalDatabaseFunctionType.NumOfTotalTransactions,
					PayPalDatabaseFunctionType.RevenuesForTransactions,
					PayPalDatabaseFunctionType.NetNumOfRefundsAndReturns,
					PayPalDatabaseFunctionType.TransferAndWireOut,
					PayPalDatabaseFunctionType.TransferAndWireIn,
				};

			var updated = data.SubmittedDate;

			var nodesCreationFactory = TimePeriodNodesCreationTreeFactoryFactory.CreateHardCodeTimeBoundaryCalculationStrategy();
			var timeChain = TimePeriodChainContructor.CreateDataChain(new TimePeriodNodeWithDataFactory<PayPalTransactionItem>(), data, nodesCreationFactory);

			var timePeriodData = TimePeriodChainContructor.ExtractDataWithCorrectTimePeriod(timeChain, updated);

			if (timeChain.HasNoData)
			{
				return null;
			}

			var factory = new PayPalTransactionAgregatorFactory();

			return DataAggregatorHelper.AggregateData(factory, timePeriodData, aggregateFunctionArray, updated, currencyConverter);

		}

		public PayPalRermissionsGranted GetAccessToken(string requestToken, string verificationCode)
		{
			return PayPalServiceHelper.GetAccessToken(_Config, requestToken, verificationCode);
		}

		public PayPalPersonalData GetAccountInfo(PayPalRermissionsGranted securityInfo)
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