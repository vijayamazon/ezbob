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
	using System.Globalization;
	using EZBob.DatabaseLib.Model.Marketplaces.Yodlee;
	using NHibernate;
	using Scorto.NHibernate.Repository;

	class YodleeMarketplaceModelBuilder : MarketplaceModelBuilder
	{
		private readonly MP_YodleeOrderRepository _yodleeOrderRepository;

		public YodleeMarketplaceModelBuilder(MP_YodleeOrderRepository yodleeOrderRepository, ISession session)
			: base(session)
		{
			_yodleeOrderRepository = yodleeOrderRepository;
		}

		public override PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model)
		{
			return CreateYodleeAccountModelModel(mp);
		}

		public PaymentAccountsModel CreateYodleeAccountModelModel(MP_CustomerMarketPlace m)
		{
			var values = RetrieveDataHelper.GetAnalysisValuesByCustomerMarketPlace(m.Id);
			var analisysFunction = values;
			var av = values.Data.FirstOrDefault(x => x.Key == analisysFunction.Data.Max(y => y.Key)).Value;

			var tnop = 0.0;
			var tnip = 0.0;
			var mip = 0.0;
			if (av != null)
			{
				var tnipN = GetClosestToYear(av.Where(x => x.ParameterName == "Total Income"));
				var tnopN = GetClosestToYear(av.Where(x => x.ParameterName == "Total Expense"));
				var mipN = GetMonth(av.Where(x => x.ParameterName == "Total Income"));

				if (mipN != null) mip = Math.Abs(Convert.ToDouble(mipN.Value, CultureInfo.InvariantCulture));
				if (tnipN != null) tnip = Math.Abs(Convert.ToDouble(tnipN.Value, CultureInfo.InvariantCulture));
				if (tnopN != null) tnop = Math.Abs(Convert.ToDouble(tnopN.Value, CultureInfo.InvariantCulture));

			}

			var tc = m.YodleeOrders.SelectMany(o => o.OrderItems).Sum(i => i.OrderItemBankTransactions.Count());

			var status = m.GetUpdatingStatus();

			var yodleeModel = new PaymentAccountsModel
			{
				displayName = m.DisplayName,
				TotalNetInPayments = tnip,
				MonthInPayments = mip,
				TotalNetOutPayments = tnop,
				TransactionsNumber = tc,
				id = m.Id,
				Status = status,
			};
			return yodleeModel;
		}

		protected override void InitializeSpecificData(MP_CustomerMarketPlace mp, MarketPlaceModel model)
		{
			model.Yodlee = BuildYodlee(mp);
		}

		private YodleeModel BuildYodlee(MP_CustomerMarketPlace mp)
		{
			List<MP_YodleeOrderItem> yodleeData = null;
			if (mp.Marketplace.InternalId == new YodleeServiceInfo().InternalId)
			{
				yodleeData = _yodleeOrderRepository.GetOrdersItemsByMakretplaceId(mp.Id);
			}

			if (yodleeData == null) return null;

			var model = new YodleeModel();
			var banks = new List<YodleeBankModel>();
			var currencyConvertor = new CurrencyConvertor(new CurrencyRateRepository(_session));
			foreach (var bank in yodleeData)
			{
				var yodleeBankModel = new YodleeBankModel
										  {
											  customName = bank.customName,
											  customDescription = bank.customDescription,
											  isDeleted = bank.isDeleted.ToString(),
											  accountNumber = bank.accountNumber,
											  accountHolder = bank.accountHolder,
											  availableBalance = bank.availableBalance.HasValue ? currencyConvertor.ConvertToBaseCurrency(
												  bank.availableBalanceCurrency,
												  bank.availableBalance.Value,
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
				foreach (var transaction in bank.OrderItemBankTransactions)
				{
					var yodleeTransactionModel = new YodleeTransactionModel
													 {
														 transactionBaseType = transaction.transactionBaseType,
														 transactionDate = (transaction.postDate ?? transaction.transactionDate).ToString(),
														 categoryName = transaction.transactionCategory.Name,
														 categoryType = transaction.transactionCategory.Type,
														 transactionAmount = transaction.transactionAmount.HasValue ? currencyConvertor.ConvertToBaseCurrency(
															 transaction.transactionAmountCurrency,
															 transaction.transactionAmount.Value,
															 transaction.postDate ?? transaction.transactionDate).Value.ToString() : null,
														 description = transaction.description,
													 };
					transactions.Add(yodleeTransactionModel);
				}
				yodleeBankModel.transactions = transactions.OrderByDescending(t => DateTime.Parse(t.transactionDate));
				banks.Add(yodleeBankModel);
			}
			model.banks = banks;
			YodleeSearchWordsModel yodleeSearchWordsModel;
			model.CashFlowReportModel = CreateYodleeCashFlowModel(yodleeData, out yodleeSearchWordsModel);
			model.SearchWordsModel = yodleeSearchWordsModel;
			return model;
		}

		private YodleeCashFlowReportModel CreateYodleeCashFlowModel(IEnumerable<MP_YodleeOrderItem> yodleeData, out YodleeSearchWordsModel yodleeSearchWordsModel)
		{
			var yodleeCashFlowReportModel = new YodleeCashFlowReportModel(_session);
			yodleeSearchWordsModel = new YodleeSearchWordsModel(_session);
			foreach (var bank in yodleeData)
			{
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
			var s = _session.Query<MP_YodleeOrderItem>()
				.Where(oi => oi.Order.CustomerMarketPlace.Id == mp.Id)
				.Where(oi => oi.accountOpenDate != null)
				.Select(oi => oi.accountOpenDate);
			return !s.Any() ? (DateTime?)null : s.Min();
		}
	}
}