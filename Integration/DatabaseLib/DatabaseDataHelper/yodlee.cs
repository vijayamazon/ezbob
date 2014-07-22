namespace EZBob.DatabaseLib
{
	#region using

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;
	using EzBob.CommonLib;
	using Model.Experian;
	using Newtonsoft.Json;
	using Common;
	using DatabaseWrapper;
	using DatabaseWrapper.Order;
	using Model.Database;
	using Model.Database.Repository;
	using Model.Marketplaces.Yodlee;

	#endregion using

	public partial class DatabaseDataHelper
	{
		#region public

		#region property YodleeBanks

		private string m_sYodleeBanks;

		public string YodleeBanks
		{
			get
			{
				if (m_sYodleeBanks == null)
				{
					var banks = _yodleeBanksRepository.GetAll();

					var dict = new Dictionary<string, YodleeParentBankModel>();
					var yodleeBanksModel = new YodleeBanksModel
					{
						DropDownBanks = new List<YodleeSubBankModel>(),
					};

					foreach (var bank in banks)
					{
						if (bank.Active && bank.Image)
						{
							var sub = new YodleeSubBankModel { csId = bank.ContentServiceId, displayName = bank.Name };

							if (!dict.ContainsKey(bank.ParentBank))
								dict.Add(bank.ParentBank, new YodleeParentBankModel { parentBankName = bank.ParentBank, subBanks = new List<YodleeSubBankModel>() });

							dict[bank.ParentBank].subBanks.Add(sub);
						} // if

						if (bank.Active && !bank.Image)
							yodleeBanksModel.DropDownBanks.Add(new YodleeSubBankModel { csId = bank.ContentServiceId, displayName = bank.Name });
					} // for

					yodleeBanksModel.ImageBanks = dict.Values.ToList();

					m_sYodleeBanks = JsonConvert.SerializeObject(yodleeBanksModel);
				} // if

				return m_sYodleeBanks;
			}
		} // YodleeBanks

		#endregion property YodleeBanks

		#region method StoreYodleeOrdersData

		public void StoreYodleeOrdersData(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, YodleeOrderDictionary ordersData, MP_CustomerMarketplaceUpdatingHistory historyRecord)
		{
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			// var yodleeGroupRepository = new YodleeGroupRepository(_session).GetAll().ToList();
			// var yodleeGroupRuleMapRepository = new YodleeGroupRuleMapRepository(_session).GetAll().ToList();

			// LogData("Yodlee Orders Data", customerMarketPlace, ordersData);

			if (ordersData == null)
			{
				return;
			}

			DateTime submittedDate = DateTime.UtcNow;
			var mpOrder = new MP_YodleeOrder
			{
				CustomerMarketPlace = customerMarketPlace,
				Created = submittedDate,
				HistoryRecord = historyRecord
			};


			foreach (var item in ordersData.Data.Keys)
			{
				var avaliableBalance = CurrencyXchg(item.availableBalance, item.asOfDate);
				var currentBalance = CurrencyXchg(item.currentBalance, item.asOfDate);
				var overdraftProtection = CurrencyXchg(item.overdraftProtection, item.asOfDate);
				var mpOrderItem = new MP_YodleeOrderItem
				{
					Order = mpOrder,
					isSeidFromDataSource = item.isSeidFromDataSource,
					isSeidMod = item.isSeidMod,
					acctTypeId = item.acctTypeId,
					acctType = item.acctType,
					localizedAcctType = item.localizedAcctType,
					srcElementId = item.srcElementId,
					bankAccountId = item.bankAccountId,
					isDeleted = item.isDeleted,
					lastUpdated = item.lastUpdated,
					hasDetails = item.hasDetails,
					interestRate = item.interestRate,
					accountNumber = item.accountNumber,
					link = item.link,
					accountHolder = item.accountHolder,
					tranListToDate =
						(item.tranListToDate != null && item.tranListToDate.dateSpecified) ? item.tranListFromDate.date : null,
					tranListFromDate =
						(item.tranListFromDate != null && item.tranListFromDate.dateSpecified)
							? item.tranListFromDate.date
							: null,
					availableBalance = avaliableBalance != null ? avaliableBalance.Value : (double?)null,
					availableBalanceCurrency = avaliableBalance != null ? avaliableBalance.CurrencyCode : null,
					currentBalance = currentBalance != null ? currentBalance.Value : (double?)null,
					currentBalanceCurrency = currentBalance != null ? currentBalance.CurrencyCode : null,
					overdraftProtection = overdraftProtection != null ? overdraftProtection.Value : (double?)null,
					overdraftProtectionCurrency = overdraftProtection != null ? overdraftProtection.CurrencyCode : null,
					accountName = item.accountName,
					routingNumber = item.routingNumber,
					maturityDate =
						(item.maturityDate != null && item.maturityDate.dateSpecified) ? item.maturityDate.date : null,
					asOfDate =
						(item.asOfDate != null && item.asOfDate.dateSpecified) ? item.asOfDate.date : mpOrder.Created,
					isPaperlessStmtOn = item.isPaperlessStmtOn,
					siteAccountStatus = item.siteAccountStatus.ToString(),
					accountClassification = item.accountClassification.ToString(),
					created = item.created,
					secondaryAccountHolderName = item.secondaryAccountHolderName,
					accountOpenDate =
						(item.accountOpenDate != null && item.accountOpenDate.dateSpecified)
							? item.accountOpenDate.date
							: null,
					accountCloseDate =
						(item.accountCloseDate != null && item.accountCloseDate.dateSpecified)
							? item.accountCloseDate.date
							: null,
					itemAccountId = item.itemAccountId
				};


				foreach (var bankTransaction in ordersData.Data[item])
				{
					var date = bankTransaction.transactionDate;
					if (bankTransaction.transactionDate == null || !bankTransaction.transactionDate.date.HasValue)
					{
						date = bankTransaction.postDate;
					}
					var runningBalance = CurrencyXchg(bankTransaction.runningBalance, date);
					var calcRunningBalance = CurrencyXchg(bankTransaction.calcRunningBalance, date);
					var transactionAmount = CurrencyXchg(bankTransaction.transactionAmount, date);

					var orderBankTransaction = new MP_YodleeOrderItemBankTransaction
						{
							YodleeOrderItem = mpOrderItem,
							isSeidFromDataSource = bankTransaction.isSeidFromDataSource,
							isSeidMod = bankTransaction.isSeidMod,
							srcElementId = bankTransaction.srcElementId,
							transactionTypeId = bankTransaction.transactionTypeId,
							transactionType = bankTransaction.transactionType,
							transactionStatusId = bankTransaction.transactionStatusId,
							transactionStatus = bankTransaction.transactionStatus,
							transactionBaseTypeId = bankTransaction.transactionBaseTypeId,
							transactionBaseType = bankTransaction.transactionBaseType,
							categoryId = bankTransaction.categoryId,
							bankTransactionId = bankTransaction.bankTransactionId,
							bankAccountId = bankTransaction.bankAccountId,
							bankStatementId = bankTransaction.bankStatementId,
							isDeleted = bankTransaction.isDeleted,
							lastUpdated = bankTransaction.lastUpdated,
							hasDetails = bankTransaction.hasDetails,
							transactionId = bankTransaction.transactionId,
							transactionCategory =
								_MP_YodleeTransactionCategoriesRepository.GetYodleeTransactionCategoryByCategoryId(
									bankTransaction.transactionCategoryId),
							classUpdationSource = bankTransaction.classUpdationSource,
							lastCategorised = bankTransaction.lastCategorised,
							transactionDate =
								(bankTransaction.transactionDate != null && bankTransaction.transactionDate.dateSpecified)
									? bankTransaction.transactionDate.date
									: null,
							prevLastCategorised = bankTransaction.prevLastCategorised,
							runningBalance = runningBalance != null ? runningBalance.Value : (double?)null,
							runningBalanceCurrency = runningBalance != null ? runningBalance.CurrencyCode : null,
							categorisationSourceId = bankTransaction.categorisationSourceId,
							plainTextDescription = bankTransaction.plainTextDescription,
							calcRunningBalance = calcRunningBalance != null ? calcRunningBalance.Value : (double?)null,
							calcRunningBalanceCurrency = calcRunningBalance != null ? calcRunningBalance.CurrencyCode : null,
							category = bankTransaction.category,
							link = bankTransaction.link,
							postDate =
								(bankTransaction.postDate != null && bankTransaction.postDate.dateSpecified)
									? bankTransaction.postDate.date
									: null,
							prevTransactionCategoryId = bankTransaction.prevTransactionCategoryId,
							descriptionViewPref = bankTransaction.descriptionViewPref,
							prevCategorisationSourceId = bankTransaction.prevCategorisationSourceId,
							transactionAmount = transactionAmount != null ? transactionAmount.Value : (double?)null,
							transactionAmountCurrency = transactionAmount != null ? transactionAmount.CurrencyCode : null,
							checkNumber = bankTransaction.checkNumber,
							description = bankTransaction.description,
							categorizationKeyword = bankTransaction.categorizationKeyword,
						};
					mpOrderItem.OrderItemBankTransactions.Add(orderBankTransaction);
				}

				mpOrder.OrderItems.Add(mpOrderItem);

			}

			customerMarketPlace.YodleeOrders.Add(mpOrder);
			_CustomerMarketplaceRepository.Update(customerMarketPlace);
			_session.Flush();
		}

		#endregion method StoreYodleeOrdersData

		private AmountInfo CurrencyXchg(YMoney coin, YDate date)
		{
			try
			{
				var oDate = (date != null && date.date.HasValue) ? date.date.Value : (DateTime?)null;
				return (coin != null && coin.amount.HasValue)
						   ? _CurrencyConvertor.ConvertToBaseCurrency(coin.currencyCode, coin.amount.Value, oDate)
						   : null;
			}
			catch
			{
				return null;
			}
		} // CurrencyXchg

		#region method CalculateYodleeRunningBalance(

		public void CalculateYodleeRunningBalance(
			MP_CustomerMarketPlace mp,
			string sourceId,
			AmountInfo currentBalance,
			List<MP_YodleeOrderItemBankTransaction> transactions,
			List<string> directors
		)
		{
			if (transactions.Count < 1) return;
			List<MP_YodleeGroup> yodleeGroupRepository = new YodleeGroupRepository(_session).GetAll().ToList();
			List<MP_YodleeGroupRuleMap> yodleeGroupRuleMapRepository = new YodleeGroupRuleMapRepository(_session).GetAll().ToList();

			var currDate = new DateTime();

			int currIndex = 0;

			MP_YodleeOrderItemBankTransaction oLastTransaction = null;
			int nCurTransactionPosition = 0;

			foreach (MP_YodleeOrderItemBankTransaction oCurrentTransaction in transactions)
			{
				CategorizeTransaction(oCurrentTransaction, yodleeGroupRepository, yodleeGroupRuleMapRepository, mp, directors);

				if (!oCurrentTransaction.runningBalance.HasValue)
				{
					if (nCurTransactionPosition == 0)
					{
						oCurrentTransaction.runningBalance = currentBalance.Value;
						oCurrentTransaction.runningBalanceCurrency = currentBalance.CurrencyCode;
						currDate = (oCurrentTransaction.postDate ?? oCurrentTransaction.transactionDate).Value;
					}
					else
					{
						double amount = 0;

						if (oCurrentTransaction.transactionAmount.HasValue)
						{
							amount = _CurrencyConvertor.ConvertToBaseCurrency(
								oCurrentTransaction.transactionAmountCurrency,
								oCurrentTransaction.transactionAmount.Value,
								oCurrentTransaction.postDate ?? oCurrentTransaction.transactionDate
							).Value;
						} // if

						if (oCurrentTransaction.transactionBaseType == "credit")
							oCurrentTransaction.runningBalance = oLastTransaction.runningBalance + amount;
						else //debit
							oCurrentTransaction.runningBalance = oLastTransaction.runningBalance - amount;

						oCurrentTransaction.runningBalanceCurrency = currentBalance.CurrencyCode;

						var newDate = (oCurrentTransaction.postDate ?? oCurrentTransaction.transactionDate).Value;

						if (newDate.Date != currDate.Date)
						{
							for (int j = currIndex; j < nCurTransactionPosition; j++)
								transactions[j].runningBalance = oLastTransaction.runningBalance;

							currDate = (oCurrentTransaction.postDate ?? oCurrentTransaction.transactionDate).Value.Date;
							currIndex = nCurTransactionPosition;
						} // if new date differs from current date
					} // if first transaction
				} // if has no running balance

				oLastTransaction = oCurrentTransaction;
				nCurTransactionPosition++;
			} // for

			_session.Flush();
		}

		private List<MP_YodleeOrderItemBankTransaction> GetTransactions(IDatabaseCustomerMarketPlace dmp, string sourceId, out MP_CustomerMarketPlace mp,
									 out List<string> directors)
		{
			mp = GetCustomerMarketPlace(dmp.Id);

			if (mp == null)
			{
				directors = new List<string>();
				return new List<MP_YodleeOrderItemBankTransaction>();
			}

			directors = GetExperianDirectors(mp.Customer);

			var transactions = mp.YodleeOrders
								 .SelectMany(yo => yo.OrderItems)
								 .Where(oi => oi.srcElementId == sourceId)
								 .SelectMany(oi => oi.OrderItemBankTransactions)
								 .Where(t => t.isSeidMod.HasValue && t.isSeidMod.Value == 0)
								 .Distinct(new YodleeTransactionComparer())
								 .OrderByDescending(x => (x.postDate ?? x.transactionDate).Value)
								 .ToList();

			return transactions;
		}

		// CalculateYodleeRunningBalance

		#endregion method CalculateYodleeRunningBalance(

		#region method GetAllYodleeOrdersData

		public YodleeOrderDictionary GetAllYodleeOrdersData(DateTime history, IDatabaseCustomerMarketPlace databaseCustomerMarketPlace, bool isFirstTime, out List<string> directors)
		{
			directors = new List<string>();
			MP_CustomerMarketPlace customerMarketPlace = GetCustomerMarketPlace(databaseCustomerMarketPlace.Id);

			var orders = new YodleeOrderDictionary { Data = new Dictionary<BankData, List<BankTransactionData>>() };

			var mpYodleeOrder = customerMarketPlace.YodleeOrders.LastOrDefault(y => y.Created.Date <= history);

			#region yodlee data retrieve

			if (mpYodleeOrder != null)
			{
				var orderItems = mpYodleeOrder.OrderItems.Where(x => x.isSeidMod.HasValue && x.isSeidMod.Value == 0);

				//Not Retrieving Seid transactions as they seem to be duplicated data
				foreach (var item in orderItems)
				{
					var bankData = new BankData
					{
						acctType = item.acctType,
						srcElementId = item.srcElementId,
						bankAccountId = item.bankAccountId,
						isDeleted = item.isDeleted,
						lastUpdated = item.lastUpdated,
						accountNumber = item.accountNumber,
						accountHolder = item.accountHolder,
						availableBalance = new YMoney { amount = item.availableBalance, currencyCode = item.availableBalanceCurrency },
						currentBalance = new YMoney { amount = item.currentBalance, currencyCode = item.currentBalanceCurrency },
						overdraftProtection = new YMoney { amount = item.overdraftProtection, currencyCode = item.overdraftProtectionCurrency },
						accountName = item.accountName,
						routingNumber = item.routingNumber,
						asOfDate = new YDate { date = item.asOfDate },
						secondaryAccountHolderName = item.secondaryAccountHolderName,
					};

					MP_CustomerMarketPlace mp;
					var directorsList = new List<string>();
					List<MP_YodleeOrderItemBankTransaction> transactions = GetTransactions(databaseCustomerMarketPlace, item.srcElementId, out mp, out directorsList);
					directors = directorsList;
					if (!isFirstTime)
					{
						double currentBalance = item.currentBalance == null ? 0 : item.currentBalance.Value;
						string currentBalanceCurrency = string.IsNullOrEmpty(item.currentBalanceCurrency)
															? "GBP"
															: item.currentBalanceCurrency;

						CalculateYodleeRunningBalance(mp,
							item.srcElementId,
							_CurrencyConvertor.ConvertToBaseCurrency(currentBalanceCurrency, currentBalance, item.asOfDate), transactions, directorsList);
					}


					// Not Retrieving Seid transactions as they seem to be duplicated data
					var bankTransactionsDataList = transactions
						.Where(t =>
							(
								(t.transactionDate.HasValue && t.transactionDate.Value.Date <= history) ||
								(t.postDate.HasValue && t.postDate.Value.Date <= history)
							)
						)
						.Select(bankTransaction => new BankTransactionData
						{
							isSeidMod = bankTransaction.isSeidMod,
							srcElementId = bankTransaction.srcElementId,
							transactionStatusId = bankTransaction.transactionStatusId,
							userDescription = bankTransaction.transactionCategory.Type, //userdesc used for category type
							memo = bankTransaction.transactionCategory.Name, //memo used for category name
							transactionStatus = bankTransaction.transactionStatus,
							transactionBaseTypeId = bankTransaction.transactionBaseTypeId,
							transactionBaseType = bankTransaction.transactionBaseType,
							bankTransactionId = bankTransaction.bankTransactionId,
							isDeleted = bankTransaction.isDeleted,
							transactionDate = new YDate { date = bankTransaction.transactionDate },
							runningBalance = new YMoney
							{
								amount = bankTransaction.runningBalance,
								currencyCode = bankTransaction.runningBalanceCurrency
							},
							siteCategoryType = bankTransaction.ezbobCategory != null ? bankTransaction.ezbobCategory.SubGroup : null,
							siteCategory = bankTransaction.ezbobCategory != null ? bankTransaction.ezbobCategory.Group : null,
							customCategoryId = bankTransaction.ezbobCategory != null ? bankTransaction.ezbobCategory.Priority : (long?)null,
							postDate = new YDate { date = bankTransaction.postDate },
							transactionAmount = new YMoney
							{
								amount = bankTransaction.transactionAmount,
								currencyCode = bankTransaction.transactionAmountCurrency
							},
							checkNumber = bankTransaction.checkNumber,
							description = bankTransaction.description,
						})
						.Distinct(new YodleeOrderComparer())
						.ToList();

					orders.Data.Add(bankData, bankTransactionsDataList);
				} // for each order item
			} // if order is not null

			#endregion yodlee data retrieve

			return orders;
		} // GetAllYodleeOrdersData

		#endregion method GetAllYodleeOrdersData

		#region method HasYodleeOrders

		public bool HasYodleeOrders(IDatabaseCustomerMarketPlace databaseCustomerMarketPlace)
		{
			return !GetCustomerMarketPlace(databaseCustomerMarketPlace.Id).YodleeOrders.IsEmpty;
		} // HasYodleeOrders

		#endregion method HasYodleeOrders

		#endregion public

		#region private

		private readonly IMP_YodleeTransactionCategoriesRepository _MP_YodleeTransactionCategoriesRepository;
		private readonly YodleeBanksRepository _yodleeBanksRepository;
		#region method GetExperianDirectors

		private List<string> GetExperianDirectors(Customer customer)
		{

			var lastNames = m_oExperianDirectorRepository.GetCustomerDirectorsLastNames(customer.Id);
			var experianDirectors = new List<string>();

			foreach (var lastName in lastNames)
			{
				if (!string.IsNullOrEmpty(lastName))
					experianDirectors.Add(lastName);
			} // foreach

			var company = customer.Company;
			if (company != null)
			{
				var directorsNames = company.Directors.Select(d => d.Surname.ToLowerInvariant().Trim()).ToList();

				experianDirectors.AddRange(directorsNames);
			} // if

			return experianDirectors.Distinct().ToList();
		} // GetExperianDirectors

		#endregion method GetExperianDirectors

		#region method CategorizeTransaction

		private MP_YodleeGroup CategorizeTransaction(List<MP_YodleeGroup> yodleeGroupRepository, List<MP_YodleeGroupRuleMap> yodleeGroupRuleMapRepository, string description, string baseType, int amount, int customerId, string customerSurname, List<string> directors)
		{
			if (!string.IsNullOrEmpty(customerSurname))
				directors.Add(customerSurname);

			var category = yodleeGroupRepository.FirstOrDefault(x => x.Id == (int)YodleeGroup.RevenuesException);

			if (baseType == "debit")
				category = yodleeGroupRepository.FirstOrDefault(x => x.Id == (int)YodleeGroup.Opex);

			foreach (var mpYodleeGroup in yodleeGroupRepository)
			{
				MP_YodleeGroup group = mpYodleeGroup;

				if (group.Id == (int)YodleeGroup.Opex || group.Id == (int)YodleeGroup.RevenuesException)
					continue;

				var includeLiteral = yodleeGroupRuleMapRepository.Where(x => x.Group == group && x.Rule.Id == (int)YodleeRule.IncludeLiteralWord).Select(x => new { x.Literal, x.IsRegex }).ToList();
				var dontIncludeLiteral = yodleeGroupRuleMapRepository.Where(x => x.Group == group && x.Rule.Id == (int)YodleeRule.DontIncludeLiteralWord).Select(x => x.Literal).ToList();
				bool includeDirector = yodleeGroupRuleMapRepository.Any(x => x.Group == group && x.Rule.Id == (int)YodleeRule.IncludeDirector);
				bool dontIncludeDirector = yodleeGroupRuleMapRepository.Any(x => x.Group == group && x.Rule.Id == (int)YodleeRule.DontIncludeDirector);
				bool roundFigure = yodleeGroupRuleMapRepository.Any(x => x.Group == group && x.Rule.Id == (int)YodleeRule.TransactionRoundFigure);

				bool containsLiteral = false;

				foreach (var literal in includeLiteral)
				{
					if (literal.IsRegex)
						containsLiteral = Regex.IsMatch(description.ToLowerInvariant(), literal.Literal);
					else if (description.ToLowerInvariant().Contains(literal.Literal))
						containsLiteral = true;
				} // foreach

				bool dontContainsLiteral = true;

				foreach (var literal in dontIncludeLiteral)
					if (description.ToLowerInvariant().Contains(literal))
						dontContainsLiteral = false;

				bool containsDirector = false;

				if (includeDirector || dontIncludeDirector)
				{
					foreach (var directorsName in directors)
					{
						if (description.ToLowerInvariant().Contains(directorsName))
							containsDirector = true;
					} // foreach
				} // if

				bool isRoundFigure = false;

				if (roundFigure)
					if (amount % 100 == 0)
						isRoundFigure = true;

				if (includeLiteral.Any() && !containsLiteral)
					continue;

				if (dontIncludeLiteral.Any() && !dontContainsLiteral)
					continue;

				if ((includeDirector && !containsDirector) || (dontIncludeDirector && containsDirector))
					continue;

				if (roundFigure && !isRoundFigure)
					continue;

				if (!string.IsNullOrEmpty(group.BaseType) &&
					group.BaseType.ToLowerInvariant().Trim() != baseType.ToLowerInvariant().Trim())
				{
					continue;
				} // if

				return group;
			} // foreach

			return category;
		} // CategorizeTransaction

		#endregion method CategorizeTransaction

		private void CategorizeTransaction(
			MP_YodleeOrderItemBankTransaction oTransaction,
			List<MP_YodleeGroup> yodleeGroupRepository,
			List<MP_YodleeGroupRuleMap> yodleeGroupRuleMapRepository,
			MP_CustomerMarketPlace mp,
			List<string> directors
		)
		{
			if (oTransaction.ezbobCategory != null)
				return;

			try
			{
				oTransaction.ezbobCategory = CategorizeTransaction(
					yodleeGroupRepository,
					yodleeGroupRuleMapRepository,
					oTransaction.description,
					oTransaction.transactionBaseType,
					(int)oTransaction.transactionAmount.Value,
					mp.Customer.Id,
					mp.Customer.PersonalInfo.Surname,
					directors
				);
			}
			catch (Exception e)
			{
				_Log.Debug("Yodlee Categorize transaction failed.", e);
			} // try
		} // CategorizeTransaction

		#endregion private
	} // class DatabaseDataHelper
} // namespace EZBob.DatabaseLib
