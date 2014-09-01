namespace YodleeLib.connector
{
	using System;
	using System.Globalization;
	using BankTransactionsParser;
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
			: base(helper, marketplace) {
		}

		protected override ElapsedTimeInfo RetrieveAndAggregate(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
												   MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			var securityInfo = (YodleeSecurityInfo)RetrieveCustomerSecurityInfo(databaseCustomerMarketPlace.Id);

			return UpdateClientOrdersInfo(databaseCustomerMarketPlace, securityInfo, ActionAccessType.Full, historyRecord);
		}

		private ElapsedTimeInfo UpdateClientOrdersInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, YodleeSecurityInfo securityInfo, ActionAccessType actionAccessType, MP_CustomerMarketplaceUpdatingHistory historyRecord) {
			Dictionary<BankData, List<BankTransactionData>> ordersList;

			if (databaseCustomerMarketPlace.DisplayName == "ParsedBank") {
				//retrieve data from file
				var fileInfo = Helper.GetFileInfo((int)securityInfo.ItemId);
				var lastTransactionId = Helper.GetLastTransactionId();
				if(fileInfo == null) throw new Exception("file not found");
				
				var parser = new TransactionsParser();
				var parsedData = parser.ParseFile(fileInfo.FilePath);
				
				if(parsedData == null) throw new Exception(string.Format("failed to parse the file {0}", fileInfo.FileName));

				ordersList = ConvertData(parsedData, fileInfo.FileName, securityInfo.ItemId, lastTransactionId);
			}
			else {
				//retrieve data from Yodlee API
				ordersList = YodleeConnector.GetOrders(securityInfo.Name, Encrypted.Decrypt(securityInfo.Password),
				                                       securityInfo.ItemId);
			}

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
			List<string> directors;
			var allOrders = ElapsedTimeHelper.CalculateAndStoreElapsedTimeForCallInSeconds(elapsedTimeInfo,
									databaseCustomerMarketPlace.Id,
									ElapsedDataMemberType.RetrieveDataFromDatabase,
									() => Helper.GetAllYodleeOrdersData(DateTime.UtcNow, databaseCustomerMarketPlace, true, out directors));

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

		private Dictionary<BankData, List<BankTransactionData>> ConvertData(ParsedBankAccount parsedData, string fileName, long fileId, int lastTransactionId) {
			var data = new Dictionary<BankData, List<BankTransactionData>>();
			var bankData = new BankData {
				accountName = fileName,
				accountNumber = fileId.ToString(CultureInfo.InvariantCulture),
				asOfDate = new YDate {
					date = parsedData.DateTo,
					dateSpecified = true
				},
				currentBalance =
					new YMoney() {
						amount = (double?) parsedData.Balance,
						currencyCode = "GBP",
						amountSpecified = parsedData.Balance.HasValue
					},
				tranListFromDate = new YDate {
					date = parsedData.DateFrom,
					dateSpecified = true
				},
				tranListToDate = new YDate {
					date = parsedData.DateTo,
					dateSpecified = true
				},
				isSeidMod = 0,
			};

			var transactionsData = new List<BankTransactionData>(parsedData.NumOfTransactions);
			int i = 0;
			foreach (var trn in parsedData.Transactions) {
				transactionsData.Add(new BankTransactionData {
					transactionAmount = new YMoney {
						amount = (double?)trn.Amount,
						amountSpecified = true,
						currencyCode = "GBP"
					},
					transactionDate = new YDate {
						date = trn.Date,
						dateSpecified = true
					},
					description = trn.Description,
					runningBalance = new YMoney {
						amount = (double?)trn.Balance,
						amountSpecified = trn.Balance.HasValue,
						currencyCode = "GBP"
					},
					isSeidMod = 0,
					transactionType = trn.IsCredit ? "credit" : "debit",
					transactionBaseType = trn.IsCredit ? "credit" : "debit",
					bankTransactionId = lastTransactionId + (++i)
				});

			}
			
			data.Add(bankData, transactionsData);

			return data;
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

			var updated = DateTime.UtcNow;
			var timePeriodData = DataAggregatorHelper.GetOrdersForPeriods(YodleeAccountList.Create(updated, orders), (submittedDate, o) => new YodleeAccountList(submittedDate, o));

			var accountsFactory = new YodleeAccountsAggregatorFactory();
			return DataAggregatorHelper.AggregateData(accountsFactory, timePeriodData, aggregateAccountsArray, updated, currencyConverter);
		}
	}
}