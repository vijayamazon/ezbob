namespace EzBob.Models.Marketplaces.Builders
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
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
		private readonly MP_YodleeTransactionCategoriesRepository _mpYodleeTransactionCategoriesRepository;
		private readonly CurrencyConvertor _currencyConvertor;
		private YodleeModel _yodleeModel;
		public YodleeMarketplaceModelBuilder(ISession session)
			: base(session)
		{
			_mpYodleeTransactionCategoriesRepository = new MP_YodleeTransactionCategoriesRepository(_session);
			_currencyConvertor = new CurrencyConvertor(new CurrencyRateRepository(session));
		}

		public override PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history)
		{
			var av =  GetAnalysisFunctionValues(mp, history);

			var tnop = 0.0;
			var tnip = 0.0;
			var mip = 0.0;
			var tn = 0;
			if (av != null)
			{
				var tnipN = GetClosestToYear(av.Where(x => x.ParameterName == "Total Income"));
				var tnopN = GetClosestToYear(av.Where(x => x.ParameterName == "Total Expense"));
				var mipN = GetMonth(av.Where(x => x.ParameterName == "Total Income"));
				var tnN = GetClosestToYear(av.Where(x => x.ParameterName == "Number Of Transactions"));

				if (mipN != null) mip = Math.Abs(Convert.ToDouble(mipN.Value, CultureInfo.InvariantCulture));
				if (tnipN != null) tnip = Math.Abs(Convert.ToDouble(tnipN.Value, CultureInfo.InvariantCulture));
				if (tnopN != null) tnop = Math.Abs(Convert.ToDouble(tnopN.Value, CultureInfo.InvariantCulture));
				if (tnN != null) tn = Math.Abs(Convert.ToInt32(tnN.Value, CultureInfo.InvariantCulture));
			}
			var status = mp.GetUpdatingStatus(history);
			
			var yodleeModel = new PaymentAccountsModel
			{
				displayName = mp.DisplayName,
				TotalNetInPayments = tnip,
				MonthInPayments = mip,
				TotalNetOutPayments = tnop,
				TransactionsNumber = tn,
				id = mp.Id,
				Status = status,
			};
			return yodleeModel;
		}

		protected override void InitializeSpecificData(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history)
		{
			if (_yodleeModel == null)
			{
				_yodleeModel = BuildYodlee(mp, history);
			}
			model.Yodlee = _yodleeModel;
		}

		private YodleeModel BuildYodlee(MP_CustomerMarketPlace mp, DateTime? history)
		{
			YodleeOrderDictionary yodleeData = null;
			if (mp.Marketplace.InternalId == new YodleeServiceInfo().InternalId)
			{
				var ddh = new DatabaseDataHelper(_session);
				yodleeData = ddh.GetAllYodleeOrdersData(history.HasValue ? history.Value : DateTime.UtcNow, mp);
			}

			if (yodleeData == null) return null;

			var model = new YodleeModel();
			var banks = new List<YodleeBankModel>();
			foreach (var bank in yodleeData.Data.Keys)
			{
				var yodleeBankModel = new YodleeBankModel
					{
						isDeleted = bank.isDeleted != 0,
						accountNumber = bank.accountNumber,
						accountHolder = bank.accountHolder,
						accountType = bank.acctType,
						availableBalance = bank.availableBalance.amount.HasValue
							                   ? _currencyConvertor.ConvertToBaseCurrency(
								                   bank.availableBalance.currencyCode,
								                   bank.availableBalance.amount.Value,
								                   bank.asOfDate.date).Value
							                   : (double?) null,
						currentBalance = bank.currentBalance.amount.HasValue
							                 ? _currencyConvertor.ConvertToBaseCurrency(
								                 bank.currentBalance.currencyCode,
								                 bank.currentBalance.amount.Value,
								                 bank.asOfDate.date).Value
							                 : (double?) null,
						accountName = bank.accountName,
						routingNumber = bank.routingNumber,
						asOfDate = bank.asOfDate.date,
						overdraftProtection = bank.overdraftProtection.amount.HasValue
							                      ? _currencyConvertor.ConvertToBaseCurrency(
								                      bank.overdraftProtection.currencyCode,
								                      bank.overdraftProtection.amount.Value,
								                      bank.asOfDate.date).Value
							                      : (double?) null,
					};
				var transactions = new List<YodleeTransactionModel>();
				
				foreach (var transaction in yodleeData.Data[bank])
				{
					var yodleeTransactionModel = new YodleeTransactionModel
													 {
														 transactionBaseType = transaction.transactionBaseType,
														 transactionDate = (transaction.postDate.date ?? transaction.transactionDate.date).Value,
														 categoryName = _mpYodleeTransactionCategoriesRepository.GetYodleeTransactionCategoryByCategoryId(transaction.transactionCategoryId).Name,
														 categoryType = _mpYodleeTransactionCategoriesRepository.GetYodleeTransactionCategoryByCategoryId(transaction.transactionCategoryId).Type,
														 transactionAmount = transaction.transactionAmount.amount.HasValue ? _currencyConvertor.ConvertToBaseCurrency(
															 transaction.transactionAmount.currencyCode,
															 transaction.transactionAmount.amount.Value,
															 transaction.postDate.date ?? transaction.transactionDate.date).Value : (double?)null,
														 description = transaction.description,
														 runningBalance = transaction.runningBalance.amount.HasValue ? _currencyConvertor.ConvertToBaseCurrency(
															 transaction.runningBalance.currencyCode,
															 transaction.runningBalance.amount.Value,
															 transaction.postDate.date ?? transaction.transactionDate.date).Value : (double?)null,
														 transactionStatus = transaction.transactionStatus,
														 bankTransactionId = transaction.bankTransactionId,
													 };
					transactions.Add(yodleeTransactionModel);
				}
				yodleeBankModel.transactions = transactions.OrderByDescending(t => t.transactionDate);
				banks.Add(yodleeBankModel);
			}
			model.banks = banks;
			YodleeSearchWordsModel yodleeSearchWordsModel;
			YodleeRunningBalanceModel yodleeRunningBalanceModel;
			model.CashFlowReportModel = CreateYodleeCashFlowModel(model, mp.Customer.PersonalInfo.Surname, out yodleeSearchWordsModel, out yodleeRunningBalanceModel);
			model.SearchWordsModel = yodleeSearchWordsModel;
			model.RunningBalanceModel = yodleeRunningBalanceModel;
			return model;
		}

		private YodleeCashFlowReportModel CreateYodleeCashFlowModel(YodleeModel model, string customerSurName, out YodleeSearchWordsModel yodleeSearchWordsModel, out YodleeRunningBalanceModel yodleeRunningBalanceModel)
		{
			var yodleeCashFlowReportModel = new YodleeCashFlowReportModel(_session);
			yodleeSearchWordsModel = new YodleeSearchWordsModel(_session, customerSurName);
			yodleeRunningBalanceModel = new YodleeRunningBalanceModel();
			foreach (var bank in model.banks)
			{
				if (bank.overdraftProtection.HasValue)
				{
					yodleeRunningBalanceModel.BankFrame -= bank.overdraftProtection.Value;
				}else if (bank.availableBalance.HasValue && bank.currentBalance.HasValue)
				{
					yodleeRunningBalanceModel.BankFrame += (bank.currentBalance.Value - bank.availableBalance.Value);
				}

				if (bank.asOfDate.HasValue)
				{
					yodleeCashFlowReportModel.AsOfDate = bank.asOfDate.Value;
					yodleeRunningBalanceModel.AsOfDate = bank.asOfDate.Value;
				}
				yodleeRunningBalanceModel.AccountCurrentBalanceDict[bank.accountNumber] = bank.currentBalance.HasValue? bank.currentBalance.Value : 0;
				foreach (var transaction in bank.transactions)
				{
					yodleeCashFlowReportModel.Add(transaction);
					yodleeSearchWordsModel.Add(transaction);
					yodleeRunningBalanceModel.Add(transaction, bank.accountNumber);
				}
			}

			yodleeCashFlowReportModel.AddMissingAndSort();
			yodleeSearchWordsModel.AddMissing();
			yodleeRunningBalanceModel.CalculateMergedRunningBalace();
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