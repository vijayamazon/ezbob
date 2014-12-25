namespace YodleeLib.connector {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using BankTransactionsParser;
	using Ezbob.Database;
	using Ezbob.Utils;
	using Ezbob.Utils.Security;
	using Ezbob.Utils.Serialization;
	using EzBob.CommonLib;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EZBob.DatabaseLib.Model.Database;

	public class YodleeRetriveDataHelper : MarketplaceRetrieveDataHelperBase {
		public YodleeRetriveDataHelper(DatabaseDataHelper helper,
			DatabaseMarketplaceBaseBase marketplace)
			: base(helper, marketplace) { }

		public override IMarketPlaceSecurityInfo RetrieveCustomerSecurityInfo(int customerMarketPlaceId) {
			return Serialized.Deserialize<YodleeSecurityInfo>(GetDatabaseCustomerMarketPlace(customerMarketPlaceId)
				.SecurityData);
		} // RetrieveCustomerSecurityInfo

		protected override ElapsedTimeInfo RetrieveAndAggregate(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace,
			MP_CustomerMarketplaceUpdatingHistory historyRecord) {
			var securityInfo = (YodleeSecurityInfo)RetrieveCustomerSecurityInfo(databaseCustomerMarketPlace.Id);

			return UpdateClientOrdersInfo(databaseCustomerMarketPlace, securityInfo, historyRecord);
		} // RetrieveAndAggregate

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
						amount = (double?)parsedData.Balance,
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
				isDeleted = 0
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
					transactionStatusId = (int)datatypes.TransactionStatus.Posted,
					transactionBaseTypeId = trn.IsCredit ? (int)datatypes.TransactionBaseType.Credit : (int)datatypes.TransactionBaseType.Debit,
					bankTransactionId = lastTransactionId + (++i)
				});
			}

			data.Add(bankData, transactionsData);

			return data;
		} // ConvertData

		private ElapsedTimeInfo UpdateClientOrdersInfo(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, YodleeSecurityInfo securityInfo, MP_CustomerMarketplaceUpdatingHistory historyRecord) {
			Dictionary<BankData, List<BankTransactionData>> ordersList;

			if (databaseCustomerMarketPlace.DisplayName == "ParsedBank") {
				//retrieve data from file
				var fileInfo = Helper.GetFileInfo((int)securityInfo.ItemId);
				var lastTransactionId = Helper.GetLastTransactionId();
				if (fileInfo == null)
					throw new Exception("file not found");

				var parser = new TransactionsParser();
				var parsedData = parser.ParseFile(fileInfo.FilePath);

				if (parsedData == null)
					throw new Exception(string.Format("failed to parse the file {0}", fileInfo.FileName));
				if (!string.IsNullOrEmpty(parsedData.Error))
					throw new Exception(string.Format("failed to parse the file {0} \n {1}", fileInfo.FileName, parsedData.Error));

				ordersList = ConvertData(parsedData, fileInfo.FileName, securityInfo.ItemId, lastTransactionId);
			} else {
				//retrieve data from Yodlee API
				ordersList = YodleeConnector.GetOrders(securityInfo.Name, Encrypted.Decrypt(securityInfo.Password),
					securityInfo.ItemId);
			}

			var elapsedTimeInfo = new ElapsedTimeInfo();

			if (ordersList != null) {
				var newOrders = new YodleeOrderDictionary {
					Data = ordersList
				};

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

			DbConnectionGenerator.Get()
				.ExecuteNonQuery(
					"UpdateMpTotalsYodlee",
					CommandSpecies.StoredProcedure,
					new QueryParameter("HistoryID", historyRecord.Id)
				);

			return elapsedTimeInfo;
		} // UpdateClientsOrderInfo
	} // class YodleeRetrieveDAtaHelper
} // namespace
