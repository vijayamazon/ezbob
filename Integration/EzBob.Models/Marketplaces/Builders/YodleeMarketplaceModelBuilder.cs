namespace EzBob.Models.Marketplaces.Builders
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using Web.Areas.Customer.Models;
	using NHibernate.Linq;
	using Yodlee;
	using YodleeLib.connector;
	using EZBob.DatabaseLib.Model.Marketplaces.Yodlee;
	using NHibernate;
	using Scorto.NHibernate.Repository;

	class YodleeMarketplaceModelBuilder : MarketplaceModelBuilder
	{
		private readonly MP_YodleeOrderRepository _yodleeOrderRepository;
		private readonly CurrencyConvertor _currencyConvertor;
		private YodleeModel _yodleeModel;
		public YodleeMarketplaceModelBuilder(MP_YodleeOrderRepository yodleeOrderRepository, ISession session)
			: base(session)
		{
			_yodleeOrderRepository = yodleeOrderRepository;
			_currencyConvertor = new CurrencyConvertor(new CurrencyRateRepository(session));
		}

		public override PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history = null)
		{
			if (_yodleeModel == null)
			{
				_yodleeModel = BuildYodlee(mp, history);
			}

			var status = mp.GetUpdatingStatus(history);

			var yodleeModel = new PaymentAccountsModel
			{
				displayName = mp.DisplayName,
				TotalNetInPayments = _yodleeModel.CashFlowReportModel.YodleeCashFlowReportModelDict["2Total Income"][999999],
				MonthInPayments = _yodleeModel.CashFlowReportModel.MonthInPayments,
				TotalNetOutPayments = _yodleeModel.CashFlowReportModel.YodleeCashFlowReportModelDict["7Total Expenses"][999999],
				TransactionsNumber = _yodleeModel.CashFlowReportModel.YodleeCashFlowReportModelDict["3Num Of Transactions"][999999] +
									 _yodleeModel.CashFlowReportModel.YodleeCashFlowReportModelDict["8Num Of Transactions"][999999],
				id = mp.Id,
				Status = status,
			};
			return yodleeModel;
		}

		protected override void InitializeSpecificData(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history)
		{
			if (_yodleeModel == null)
			{
				_yodleeModel = BuildYodlee(mp);
			}
			model.Yodlee = _yodleeModel;
		}

		private YodleeModel BuildYodlee(MP_CustomerMarketPlace mp, DateTime? history = null)
		{
			List<MP_YodleeOrderItem> yodleeData = null;
			if (mp.Marketplace.InternalId == new YodleeServiceInfo().InternalId)
			{
				yodleeData = _yodleeOrderRepository.GetOrdersItemsByMakretplaceId(mp.Id);
			}

			if (yodleeData == null) return null;

			var model = new YodleeModel();
			var banks = new List<YodleeBankModel>();
			var dataBaseDataHelper = new DatabaseDataHelper(_session);
			foreach (var bank in yodleeData)
			{
				var yodleeBankModel = new YodleeBankModel
										  {
											  customName = bank.customName,
											  customDescription = bank.customDescription,
											  isDeleted = bank.isDeleted.ToString(),
											  accountNumber = bank.accountNumber,
											  accountHolder = bank.accountHolder,
											  availableBalance = bank.availableBalance.HasValue ? _currencyConvertor.ConvertToBaseCurrency(
												  bank.availableBalanceCurrency,
												  bank.availableBalance.Value,
												  bank.asOfDate).Value.ToString() : null,
											  currentBalance = bank.currentBalance.HasValue ? _currencyConvertor.ConvertToBaseCurrency(
											  bank.currentBalanceCurrency,
											  bank.currentBalance.Value,
											  bank.asOfDate).Value.ToString() : null,
											  term = bank.term,
											  accountName = bank.accountName,
											  routingNumber = bank.routingNumber,
											  accountNicknameAtSrcSite = bank.accountNicknameAtSrcSite,
											  secondaryAccountHolderName = bank.secondaryAccountHolderName,
											  accountOpenDate = bank.accountOpenDate.ToString(),
											  taxesWithheldYtd = bank.taxesWithheldYtd.ToString(),
										  };
				var transactions = new List<YodleeTransactionModel>();
				if (bank.currentBalance.HasValue)
				{
					dataBaseDataHelper.CalculateYodleeRunningBalance(bank.OrderItemBankTransactions,
																	 _currencyConvertor.ConvertToBaseCurrency(
																		 bank.currentBalanceCurrency, bank.currentBalance.Value,
																		 bank.asOfDate));
				}
				foreach (var transaction in bank.OrderItemBankTransactions)
				{
					var yodleeTransactionModel = new YodleeTransactionModel
													 {
														 transactionBaseType = transaction.transactionBaseType,
														 transactionDate = (transaction.postDate ?? transaction.transactionDate).ToString(),
														 categoryName = transaction.transactionCategory.Name,
														 categoryType = transaction.transactionCategory.Type,
														 transactionAmount = transaction.transactionAmount.HasValue ? _currencyConvertor.ConvertToBaseCurrency(
															 transaction.transactionAmountCurrency,
															 transaction.transactionAmount.Value,
															 transaction.postDate ?? transaction.transactionDate).Value.ToString() : null,
														 description = transaction.description,
														 runningBalance = transaction.runningBalance.HasValue ? _currencyConvertor.ConvertToBaseCurrency(
															 transaction.runningBalanceCurrency,
															 transaction.runningBalance.Value,
															 transaction.postDate ?? transaction.transactionDate).Value.ToString() : null,
													 };
					transactions.Add(yodleeTransactionModel);
				}
				yodleeBankModel.transactions = transactions.OrderByDescending(t => DateTime.Parse(t.transactionDate));
				banks.Add(yodleeBankModel);
			}
			model.banks = banks;
			YodleeSearchWordsModel yodleeSearchWordsModel;
			model.CashFlowReportModel = CreateYodleeCashFlowModel(yodleeData, mp.Customer.PersonalInfo.Surname, out yodleeSearchWordsModel);
			model.SearchWordsModel = yodleeSearchWordsModel;
			return model;
		}

		private YodleeCashFlowReportModel CreateYodleeCashFlowModel(IEnumerable<MP_YodleeOrderItem> yodleeData, string customerSurName, out YodleeSearchWordsModel yodleeSearchWordsModel)
		{
			var yodleeCashFlowReportModel = new YodleeCashFlowReportModel(_session);
			yodleeSearchWordsModel = new YodleeSearchWordsModel(_session, customerSurName);
			var databaseDataHelper = new DatabaseDataHelper(_session);
			foreach (var bank in yodleeData)
			{
				if (bank.currentBalance.HasValue)
				{
					databaseDataHelper.CalculateYodleeRunningBalance(bank.OrderItemBankTransactions,
																	 _currencyConvertor.ConvertToBaseCurrency(
																		 bank.currentBalanceCurrency, bank.currentBalance.Value,
																		 bank.asOfDate));

					
				}

				if (bank.overdraftProtection.HasValue)
				{
					yodleeCashFlowReportModel.BankFrame -= _currencyConvertor.ConvertToBaseCurrency(
						bank.overdraftProtectionCurrency, bank.overdraftProtection.Value,
						bank.asOfDate).Value;
				}else if (bank.availableBalance.HasValue && bank.currentBalance.HasValue)
				{
					yodleeCashFlowReportModel.BankFrame -= _currencyConvertor.ConvertToBaseCurrency(
						bank.availableBalanceCurrency, (bank.currentBalance.Value - bank.availableBalance.Value),
						bank.asOfDate).Value;
				}
				yodleeCashFlowReportModel.AsOfDate = bank.asOfDate.HasValue ? bank.asOfDate.Value : new DateTime(1900,1,1);

				foreach (var transaction in bank.OrderItemBankTransactions)
				{
					yodleeCashFlowReportModel.Add(transaction);
					yodleeSearchWordsModel.Add(transaction);
				}
			}

			yodleeCashFlowReportModel.AddMissingAndSort();
			yodleeSearchWordsModel.AddMissing();

			return yodleeCashFlowReportModel;
		}

		public override DateTime? GetSeniority(MP_CustomerMarketPlace mp)
		{
			var s = _session.Query<MP_YodleeOrderItemBankTransaction>().Where(t => t.YodleeOrderItem.Order.CustomerMarketPlace.Id == mp.Id)
				.Where(t => t.postDate.HasValue || t.transactionDate.HasValue)
				.Select(oi => oi.postDate ?? oi.transactionDate);
			return !s.Any() ? (DateTime?)null : s.Min();
		}
	}
}