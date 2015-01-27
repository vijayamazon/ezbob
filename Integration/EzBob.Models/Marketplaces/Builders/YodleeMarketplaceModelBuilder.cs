namespace EzBob.Models.Marketplaces.Builders {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Ezbob.Backend.Models;
	using Ezbob.Models;
	using NHibernate.Linq;
	using Yodlee;
	using YodleeLib.connector;
	using EZBob.DatabaseLib.Model.Marketplaces.Yodlee;
	using NHibernate;
	using EZBob.DatabaseLib.Repository;

	public class YodleeMarketplaceModelBuilder : MarketplaceModelBuilder {
		public YodleeMarketplaceModelBuilder(ISession session = null)
			: base(session) {
			customerMarketPlaceRepository = new CustomerMarketPlaceRepository(_session);
			_currencyConvertor = new CurrencyConvertor(new CurrencyRateRepository(_session));
		}

		public YodleeModel BuildYodlee(int mpId) {
			MP_CustomerMarketPlace mp = customerMarketPlaceRepository.Get(mpId);
			return BuildYodlee(mp, null);
		}

		public YodleeModel BuildYodlee(MP_CustomerMarketPlace mp, DateTime? history) {
			YodleeOrderDictionary yodleeData = null;
			var directors = new List<string>();
			_timeElapsed = new List<System.Tuple<string, double>>();
			Stopwatch sw = Stopwatch.StartNew();
			if (mp.Marketplace.InternalId == new YodleeServiceInfo().InternalId) {
				var ddh = new DatabaseDataHelper(_session);
				yodleeData = ddh.GetAllYodleeOrdersData(history.HasValue ? history.Value : DateTime.UtcNow, mp, false, out directors);
			} // if
			sw.Stop();
			_timeElapsed.Add(new System.Tuple<string, double>("GetAllYodleeOrdersData", sw.Elapsed.TotalMilliseconds));
			if (yodleeData == null) {
				Log.Debug("Yodlee model building complete for marketplace {0}: no data.", mp.Stringify());
				return null;
			} // if

			var model = new YodleeModel();
			sw.Restart();
			var ruleModelBuilder = new YodleeRuleModelBuilder(_session);

			model.RuleModel = ruleModelBuilder.Build();
			sw.Stop();
			_timeElapsed.Add(new System.Tuple<string, double>("YodleeRuleModel", sw.Elapsed.TotalMilliseconds));
			model.BankStatementDataModel = new BankStatementDataModel();

			var banks = new List<YodleeBankModel>();

			Log.Debug("Yodlee model is being built for marketplace {0}...", mp.Stringify());
			sw.Restart();
			foreach (var bank in yodleeData.Data.Keys) {
				Log.Debug("Yodlee model is being built for marketplace {0}, bank {1}...", mp.Stringify(), bank.customName);

				double? availableBalance = CurrencyXchg(bank.availableBalance, bank.asOfDate.date);
				double? currentBalance = CurrencyXchg(bank.currentBalance, bank.asOfDate.date);
				double? overdraftProtection = CurrencyXchg(bank.overdraftProtection, bank.asOfDate.date);

				var yodleeBankModel = new YodleeBankModel {
					isDeleted = bank.isDeleted != 0,
					accountNumber = bank.accountNumber,
					accountHolder = bank.accountHolder,
					accountType = bank.acctType,
					availableBalance = availableBalance,
					currentBalance = currentBalance,
					accountName = bank.accountName,
					routingNumber = bank.routingNumber,
					asOfDate = bank.asOfDate.date,
					overdraftProtection = overdraftProtection,
				};

				var transactions = new List<YodleeTransactionModel>();

				Log.Debug("Yodlee model is being built for marketplace {0}, bank {1}, going over transactions...", mp.Stringify(), bank.customName);

				foreach (var transaction in yodleeData.Data[bank]) {
					DateTime? oDate = transaction.postDate.date ?? transaction.transactionDate.date;
					double? transactionAmount = CurrencyXchg(transaction.transactionAmount, oDate);
					double? runningBalance = CurrencyXchg(transaction.runningBalance, oDate);

					var yodleeTransactionModel = new YodleeTransactionModel {
						transactionBaseType = transaction.transactionBaseType,
						transactionType = transaction.transactionType,
						transactionDate = (transaction.postDate.date ?? transaction.transactionDate.date).Value,
						categoryName = transaction.memo,
						categoryType = transaction.userDescription,
						transactionAmount = transactionAmount,
						description = transaction.description,
						runningBalance = runningBalance,
						transactionStatus = transaction.transactionStatus,
						bankTransactionId = transaction.bankTransactionId,
						ezbobGroup = transaction.siteCategory,
						ezbobSubGroup = transaction.siteCategoryType,
						ezbobGroupPriority = transaction.customCategoryId.HasValue ? transaction.customCategoryId.Value : 0
					};

					transactions.Add(yodleeTransactionModel);
				} // for each transaction

				Log.Debug("Yodlee model is being built for marketplace {0}, bank {1}, done going over transactions.", mp.Stringify(), bank.customName);

				yodleeBankModel.transactions = transactions.OrderByDescending(t => t.transactionDate).ToList();

				banks.Add(yodleeBankModel);

				Log.Debug("Yodlee model is being built for marketplace {0}, done with bank {1}.", mp.Stringify(), bank.customName);
			} // for each bank

			model.banks = banks;
			sw.Stop();
			_timeElapsed.Add(new System.Tuple<string, double>("YodleeTransactionsModel", sw.Elapsed.TotalMilliseconds));

			Log.Debug("Yodlee model is being built for marketplace {0}, done with banks.", mp.Stringify());

			YodleeSearchWordsModel yodleeSearchWordsModel;

			YodleeRunningBalanceModel yodleeRunningBalanceModel;
			sw.Restart();
			model.CashFlowReportModel = CreateYodleeCashFlowModel(model, mp.Id, mp.Customer, directors, out yodleeSearchWordsModel, out yodleeRunningBalanceModel);
			model.SearchWordsModel = yodleeSearchWordsModel;
			model.RunningBalanceModel = yodleeRunningBalanceModel;
			sw.Stop();
			_timeElapsed.Add(new System.Tuple<string, double>("YodleeCashFlowModel", sw.Elapsed.TotalMilliseconds));

			Log.Debug("Yodlee model is ready for marketplace {0}.", mp.Stringify());
			LogElapsedTimes(mp.Id, mp.Customer.Id);
			return model;
		}

		public override DateTime? GetLastTransaction(MP_CustomerMarketPlace mp) {
			var s = _session.Query<MP_YodleeOrderItemBankTransaction>()
				.Where(t => t.YodleeOrderItem.Order.CustomerMarketPlace == mp)
				.Where(t => t.postDate.HasValue || t.transactionDate.HasValue)
				.Select(oi => oi.postDate ?? oi.transactionDate);

			return !s.Any() ? null : s.Max();
		}

		public override PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history, List<IAnalysisDataParameterInfo> av) {
			var status = mp.GetUpdatingStatus(history);

			var yodleeModel = new YodleePaymentAccountsModel(mp, history);
			
			yodleeModel.Load(av);

			return yodleeModel;
		}

		public override DateTime? GetSeniority(MP_CustomerMarketPlace mp) {
			var s = _session.Query<MP_YodleeOrderItemBankTransaction>()
				.Where(t => t.YodleeOrderItem.Order.CustomerMarketPlace == mp)
				.Where(t => t.postDate.HasValue || t.transactionDate.HasValue)
				.Select(oi => oi.postDate ?? oi.transactionDate);
			return !s.Any() ? null : s.Min();
		}

		protected override void InitializeSpecificData(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history) {
			if (_yodleeModel == null) {
				try {
					_yodleeModel = BuildYodlee(mp, history);
				} catch (Exception ex) {
					Log.Warn(ex, "Build Yodlee Model Failed.");
				}

			}
			model.Yodlee = _yodleeModel;
		}

		private YodleeCashFlowReportModel CreateYodleeCashFlowModel(YodleeModel model, int mpId, Customer customer, IEnumerable<string> directors, out YodleeSearchWordsModel yodleeSearchWordsModel, out YodleeRunningBalanceModel yodleeRunningBalanceModel) {
			var yodleeCashFlowReportModelBuilder = new YodleeCashFlowReportModelBuilder(_session);
			var yodleeSearchWordsModelBuilder = new YodleeSearchWordsModelBuilder(_session, customer, directors);
			var yodleeRunningBalanceModelBuilder = new YodleeRunningBalanceModelBuilder();
			foreach (var bank in model.banks) {
				if (bank.overdraftProtection.HasValue && bank.transactions.Any()) {
					yodleeRunningBalanceModelBuilder.SetBankFrame(bank.overdraftProtection.Value);
				}

				if (bank.asOfDate.HasValue) {
					yodleeCashFlowReportModelBuilder.SetAsOfDate(bank.asOfDate.Value);
					yodleeRunningBalanceModelBuilder.SetAsOfDate(bank.asOfDate.Value);
				}
				yodleeRunningBalanceModelBuilder.SetAccountCurrentBalance(bank.accountNumber, bank.currentBalance.HasValue ? bank.currentBalance.Value : 0);
				foreach (var transaction in bank.transactions) {
					yodleeCashFlowReportModelBuilder.Add(transaction);
					yodleeSearchWordsModelBuilder.Add(transaction);
					yodleeRunningBalanceModelBuilder.Add(transaction, bank.accountNumber);
				}
			}

			yodleeCashFlowReportModelBuilder.AddMissingAndSort();
			yodleeSearchWordsModelBuilder.AddMissing();

			yodleeSearchWordsModel = yodleeSearchWordsModelBuilder.GetModel();
			yodleeRunningBalanceModelBuilder.CalculateMergedRunningBalace();

			yodleeRunningBalanceModel = yodleeRunningBalanceModelBuilder.GetModel();
			model.BankStatementDataModel = yodleeCashFlowReportModelBuilder.GetBankStatementDataModel();
			if (model.banks.Any() && model.banks.SelectMany(x => x.transactions).Any()) {
				model.BankStatementDataModel.DateFrom = model.banks.SelectMany(x => x.transactions).Min(x => x.transactionDate);
				model.BankStatementDataModel.DateTo = model.banks.Max(x => x.asOfDate);
				model.BankStatementAnnualizedModel =
					yodleeCashFlowReportModelBuilder.GetAnualizedBankStatementDataModel(model.BankStatementDataModel);

				Log.Debug("Yodlee data for customer {0} mp {1}: \nDate from:{2} Date to:{3} num of transactions: {4} Revenues:{5} Annualized Revenues: {6}",
					customer.Id, mpId, model.BankStatementDataModel.DateFrom,
					model.BankStatementDataModel.DateTo,
					model.banks.SelectMany(x => x.transactions).Count(),
					model.BankStatementDataModel.Revenues,
					model.BankStatementAnnualizedModel.Revenues);
			} else {
				model.BankStatementAnnualizedModel = new BankStatementDataModel();
				Log.Debug("Yodlee data for customer {0} mp {1}: no data retrieved", customer.Id, mpId);
			}
			return yodleeCashFlowReportModelBuilder.GetModel();
		}

		private double? CurrencyXchg(YMoney coin, DateTime? oDate) {
			return coin.amount.HasValue
				? _currencyConvertor.ConvertToBaseCurrency(coin.currencyCode, coin.amount.Value, oDate).Value
				: (double?)null;
		}

		private void LogElapsedTimes(int mpId, int customerId) {
			var sb = new StringBuilder();
			sb.AppendFormat("YodleeMarketPlaceModelBuilder elapsed times customer {0} mp {1}\n", customerId, mpId);
			foreach (var time in _timeElapsed) {
				sb.AppendFormat("{0}: {1}ms \n", time.Item1, time.Item2);
			}
			Log.Debug(sb);
		}

		private readonly CurrencyConvertor _currencyConvertor;
		private readonly CustomerMarketPlaceRepository customerMarketPlaceRepository;
		private List<System.Tuple<string, double>> _timeElapsed;
		private YodleeModel _yodleeModel;
	}
}