namespace EzBob.Models.Marketplaces.Builders
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Web.Areas.Customer.Models;
	using NHibernate.Linq;
	using Yodlee;
	using YodleeLib.connector;
	using EZBob.DatabaseLib.Model.Marketplaces.Yodlee;
	using NHibernate;
	using Scorto.NHibernate.Repository;

	public class YodleeMarketplaceModelBuilder : MarketplaceModelBuilder
	{
		private readonly MP_YodleeTransactionCategoriesRepository _mpYodleeTransactionCategoriesRepository;
		private readonly CustomerMarketPlaceRepository customerMarketPlaceRepository;
		private readonly CurrencyConvertor _currencyConvertor;
		private YodleeModel _yodleeModel;
		public YodleeMarketplaceModelBuilder(ISession session = null)
			: base(session)
		{
			_mpYodleeTransactionCategoriesRepository = new MP_YodleeTransactionCategoriesRepository(_session);
			customerMarketPlaceRepository = new CustomerMarketPlaceRepository(_session);
			_currencyConvertor = new CurrencyConvertor(new CurrencyRateRepository(_session));
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
				try
				{
					_yodleeModel = BuildYodlee(mp, history);
				}
				catch (Exception ex)
				{
					Log.WarnFormat("Build Yodlee Model Failed {0}", ex);
					
				}
				
			}
			model.Yodlee = _yodleeModel;
		}

		public YodleeModel BuildYodlee(int mpId)
		{
			MP_CustomerMarketPlace mp = customerMarketPlaceRepository.Get(mpId);
			return BuildYodlee(mp, null);
		}

		public YodleeModel BuildYodlee(MP_CustomerMarketPlace mp, DateTime? history)
		{
			YodleeOrderDictionary yodleeData = null;
			if (mp.Marketplace.InternalId == new YodleeServiceInfo().InternalId)
			{
				var ddh = new DatabaseDataHelper(_session);
				yodleeData = ddh.GetAllYodleeOrdersData(history.HasValue ? history.Value : DateTime.UtcNow, mp);
			}

			if (yodleeData == null) return null;

			var model = new YodleeModel();
			model.RuleModel = new YodleeRuleModel(_session);
			model.BankStatementDataModel = new BankStatementDataModel();
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
														 ezbobGroup = transaction.siteCategory,
														 ezbobSubGroup = transaction.siteCategoryType,
														 ezbobGroupPriority = transaction.customCategoryId.HasValue ? transaction.customCategoryId.Value : 0
													 };
					transactions.Add(yodleeTransactionModel);
				}
				yodleeBankModel.transactions = transactions.OrderByDescending(t => t.transactionDate);
				banks.Add(yodleeBankModel);
			}
			model.banks = banks;
			YodleeSearchWordsModel yodleeSearchWordsModel;
			YodleeRunningBalanceModel yodleeRunningBalanceModel;
			model.CashFlowReportModel = CreateYodleeCashFlowModel(model, mp.Customer, out yodleeSearchWordsModel, out yodleeRunningBalanceModel);
			model.SearchWordsModel = yodleeSearchWordsModel;
			model.RunningBalanceModel = yodleeRunningBalanceModel;
			return model;
		}

		private YodleeCashFlowReportModel CreateYodleeCashFlowModel(YodleeModel model, Customer customer, out YodleeSearchWordsModel yodleeSearchWordsModel, out YodleeRunningBalanceModel yodleeRunningBalanceModel)
		{
			var yodleeCashFlowReportModelBuilder = new YodleeCashFlowReportModelBuilder(_session);
			var yodleeSearchWordsModelBuilder = new YodleeSearchWordsModelBuilder(_session, customer);
			var yodleeRunningBalanceModelBuilder = new YodleeRunningBalanceModelBuilder();
			foreach (var bank in model.banks)
			{
				if (bank.overdraftProtection.HasValue && bank.transactions.Any())
				{
					yodleeRunningBalanceModelBuilder.SetBankFrame(bank.overdraftProtection.Value);
				}

				if (bank.asOfDate.HasValue)
				{
					yodleeCashFlowReportModelBuilder.SetAsOfDate(bank.asOfDate.Value);
					yodleeRunningBalanceModelBuilder.SetAsOfDate(bank.asOfDate.Value);
				}
				yodleeRunningBalanceModelBuilder.SetAccountCurrentBalance(bank.accountNumber, bank.currentBalance.HasValue? bank.currentBalance.Value : 0);
				foreach (var transaction in bank.transactions)
				{
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
			return yodleeCashFlowReportModelBuilder.GetModel();
		}

		public override DateTime? GetSeniority(MP_CustomerMarketPlace mp)
		{
			var s = _session.Query<MP_YodleeOrderItemBankTransaction>().Where(t => t.YodleeOrderItem.Order.CustomerMarketPlace.Id == mp.Id)
				.Where(t => t.postDate.HasValue || t.transactionDate.HasValue)
				.Select(oi => oi.postDate ?? oi.transactionDate);
			return !s.Any() ? (DateTime?)null : s.Min();
		}

		public override DateTime? GetLastTransaction(MP_CustomerMarketPlace mp)
		{
			var s = _session.Query<MP_YodleeOrderItemBankTransaction>().Where(t => t.YodleeOrderItem.Order.CustomerMarketPlace.Id == mp.Id)
				.Where(t => t.postDate.HasValue || t.transactionDate.HasValue);

			if (s.Count() != 0)
			{
				return s.Max(oi => oi.postDate ?? oi.transactionDate);
			}

			return null;
		}
	}
}