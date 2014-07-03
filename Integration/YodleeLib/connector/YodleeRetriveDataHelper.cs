namespace YodleeLib.connector
{
	using System;
	using EzBob.CommonLib;
	using Ezbob.Utils;
	using Ezbob.Utils.Security;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.DatabaseWrapper.FunctionValues;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EZBob.DatabaseLib.Model.Database;
	using System.Collections.Generic;
	using Ezbob.Utils.Serialization;

	public class YodleeRetriveDataHelper : MarketplaceRetrieveDataHelperBase<YodleeDatabaseFunctionType>
	{
		public YodleeRetriveDataHelper(DatabaseDataHelper helper,
									   DatabaseMarketplaceBase<YodleeDatabaseFunctionType> marketplace)
			: base(helper, marketplace)
		{
		}

		protected override ElapsedTimeInfo RetrieveAndAggregate(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
												   MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			var securityInfo = (YodleeSecurityInfo)RetrieveCustomerSecurityInfo(databaseCustomerMarketPlace.Id);

			return UpdateClientOrdersInfo(databaseCustomerMarketPlace, securityInfo, ActionAccessType.Full, historyRecord);
		}

		private ElapsedTimeInfo UpdateClientOrdersInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, YodleeSecurityInfo securityInfo, ActionAccessType actionAccessType, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{

			//retrieve data from Yodlee API
			Dictionary<BankData, List<BankTransactionData>> ordersList = YodleeConnector.GetOrders(securityInfo.Name, Encrypted.Decrypt(securityInfo.Password), securityInfo.ItemId);

			var elapsedTimeInfo = new ElapsedTimeInfo();

			if (ordersList != null)
			{
				var newOrders = new YodleeOrderDictionary { Data = ordersList };

				//save orders data
				ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
																				databaseCustomerMarketPlace.Id,
																			   ElapsedDataMemberType.StoreDataToDatabase,
																			   () => Helper.StoreYodleeOrdersData(
																				   databaseCustomerMarketPlace,
																				   newOrders,
																				   historyRecord)
					);
			}
			// retrieve orders
			var allOrders = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => Helper.GetAllYodleeOrdersData(DateTime.UtcNow, databaseCustomerMarketPlace, isFirstTime: true));

			//calculate transactions aggregated data
			var transactionsAggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.AggregateData,
									() => CreateTransactionsAggregationInfo(allOrders, Helper.CurrencyConverter));
			// store transactions aggregated data
			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
							databaseCustomerMarketPlace.Id,
							ElapsedDataMemberType.StoreAggregatedData,
							() => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, transactionsAggregatedData, historyRecord));

			//calculate accounts aggregated data
			var accountsAggregatedData = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.AggregateData,
									() => CreateAccountsAggregationInfo(allOrders, Helper.CurrencyConverter));
			// store accounts aggregated data
			ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
							databaseCustomerMarketPlace.Id,
							ElapsedDataMemberType.StoreAggregatedData,
							() => Helper.StoreToDatabaseAggregatedData(databaseCustomerMarketPlace, accountsAggregatedData, historyRecord));

			return elapsedTimeInfo;
		}

		public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId)
		{
			return Serialized.Deserialize<YodleeSecurityInfo>(
				GetDatabaseCustomerMarketPlace(customerMarketPlaceId).SecurityData);

		}

		protected override void AddAnalysisValues(IDatabaseCustomerMarketPlace marketPlace, AnalysisDataInfo data)
		{

		}

		private IEnumerable<IWriteDataInfo<YodleeDatabaseFunctionType>> CreateTransactionsAggregationInfo(YodleeOrderDictionary orders, ICurrencyConvertor currencyConverter)
		{
			var aggregateTransactionsArray = new[]
				{
					YodleeDatabaseFunctionType.TotalExpense,
					YodleeDatabaseFunctionType.TotalIncome,
					YodleeDatabaseFunctionType.NumberOfTransactions,
					YodleeDatabaseFunctionType.TotalIncomeAnnualized
				};

			var updated = DateTime.UtcNow;//todo:use time from server
			var timePeriodData = DataAggregatorHelper.GetOrdersForPeriods(YodleeTransactionList.Create(updated, orders), (submittedDate, o) => new YodleeTransactionList(submittedDate, o));

			var transactionsFactory = new YodleeTransactionsAggregatorFactory();

			return DataAggregatorHelper.AggregateData(transactionsFactory, timePeriodData, aggregateTransactionsArray, updated, currencyConverter);
		}

		private IEnumerable<IWriteDataInfo<YodleeDatabaseFunctionType>> CreateAccountsAggregationInfo(YodleeOrderDictionary orders, ICurrencyConvertor currencyConverter)
		{
			var aggregateAccountsArray = new[]
				{
					YodleeDatabaseFunctionType.CurrentBalance,
					YodleeDatabaseFunctionType.AvailableBalance,
				};

			var updated = DateTime.UtcNow;//todo:use time from server
			var timePeriodData = DataAggregatorHelper.GetOrdersForPeriods(YodleeAccountList.Create(updated, orders), (submittedDate, o) => new YodleeAccountList(submittedDate, o));

			var accountsFactory = new YodleeAccountsAggregatorFactory();
			return DataAggregatorHelper.AggregateData(accountsFactory, timePeriodData, aggregateAccountsArray, updated, currencyConverter);
		}
	}
}