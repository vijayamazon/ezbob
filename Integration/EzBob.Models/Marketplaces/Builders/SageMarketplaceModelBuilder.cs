namespace EzBob.Models.Marketplaces.Builders
{
	using System;
	using NHibernate;
	using NHibernate.Linq;
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EZBob.DatabaseLib.Model.Marketplaces.Sage;
	using Marketplaces;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using Web.Areas.Customer.Models;

	class SageMarketplaceModelBuilder : MarketplaceModelBuilder
	{
		readonly Dictionary<string, string> _invoiceStatuses = new Dictionary<string, string>();

		public SageMarketplaceModelBuilder(MP_SagePaymentStatusRepository sagePaymentStatusRepository, ISession session)
			: base(session)
		{
			foreach (var status in sagePaymentStatusRepository.GetAll())
			{
				_invoiceStatuses.Add(status.SageId.ToString(), status.name);
			}
		}

		public override PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history)
		{
			var paymentAccountModel = new PaymentAccountsModel();
			MP_AnalyisisFunctionValue earliestNumOfSalesInvoices = GetEarliestValueFor(mp, "NumOfOrders");
			MP_AnalyisisFunctionValue earliestNumOfPurchaseInvoices = GetEarliestValueFor(mp, "NumOfPurchaseInvoices");
			MP_AnalyisisFunctionValue earliestNumOfIncomes = GetEarliestValueFor(mp, "NumOfIncomes");
			MP_AnalyisisFunctionValue earliestNumOfExpenditures = GetEarliestValueFor(mp, "NumOfExpenditures");
			MP_AnalyisisFunctionValue earliestSumOfSalesInvoices = GetEarliestValueFor(mp, "TotalSumOfOrders");
			MP_AnalyisisFunctionValue earliestSumOfPurchaseInvoices = GetEarliestValueFor(mp, "TotalSumOfPurchaseInvoices");
			MP_AnalyisisFunctionValue earliestSumOfIncomes = GetEarliestValueFor(mp, "TotalSumOfIncomes");
			MP_AnalyisisFunctionValue earliestSumOfExpenditures = GetEarliestValueFor(mp, "TotalSumOfExpenditures");
			MP_AnalyisisFunctionValue monthSumOfSalesInvoices = GetMonthValueFor(mp, "TotalSumOfOrders");
			MP_AnalyisisFunctionValue monthSumOfIncomes = GetMonthValueFor(mp, "TotalSumOfIncomes");

			paymentAccountModel.MonthInPayments = 0;
			if (monthSumOfSalesInvoices != null && monthSumOfSalesInvoices.ValueFloat.HasValue)
			{
				paymentAccountModel.MonthInPayments += monthSumOfSalesInvoices.ValueFloat.Value;
			}
			if (monthSumOfIncomes != null && monthSumOfIncomes.ValueFloat.HasValue)
			{
				paymentAccountModel.MonthInPayments += monthSumOfIncomes.ValueFloat.Value;
			}

			paymentAccountModel.TransactionsNumber = 0;
			if (earliestNumOfSalesInvoices != null && earliestNumOfSalesInvoices.ValueInt.HasValue)
			{
				paymentAccountModel.TransactionsNumber += earliestNumOfSalesInvoices.ValueInt.Value;
			}
			if (earliestNumOfPurchaseInvoices != null && earliestNumOfPurchaseInvoices.ValueInt.HasValue)
			{
				paymentAccountModel.TransactionsNumber += earliestNumOfPurchaseInvoices.ValueInt.Value;
			}
			if (earliestNumOfIncomes != null && earliestNumOfIncomes.ValueInt.HasValue)
			{
				paymentAccountModel.TransactionsNumber += earliestNumOfIncomes.ValueInt.Value;
			}
			if (earliestNumOfExpenditures != null && earliestNumOfExpenditures.ValueInt.HasValue)
			{
				paymentAccountModel.TransactionsNumber += earliestNumOfExpenditures.ValueInt.Value;
			}

			paymentAccountModel.TotalNetInPayments = 0;
			if (earliestSumOfSalesInvoices != null && earliestSumOfSalesInvoices.ValueFloat.HasValue)
			{
				paymentAccountModel.TotalNetInPayments += earliestSumOfSalesInvoices.ValueFloat.Value;
			}
			if (earliestSumOfIncomes != null && earliestSumOfIncomes.ValueFloat.HasValue)
			{
				paymentAccountModel.TotalNetInPayments += earliestSumOfIncomes.ValueFloat.Value;
			}

			paymentAccountModel.TotalNetOutPayments = 0;
			if (earliestSumOfPurchaseInvoices != null && earliestSumOfPurchaseInvoices.ValueFloat.HasValue)
			{
				paymentAccountModel.TotalNetOutPayments += earliestSumOfPurchaseInvoices.ValueFloat.Value;
			}
			if (earliestSumOfExpenditures != null && earliestSumOfExpenditures.ValueFloat.HasValue)
			{
				paymentAccountModel.TotalNetOutPayments += earliestSumOfExpenditures.ValueFloat.Value;
			}

			return paymentAccountModel;
		}

		protected override void InitializeSpecificData(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history)
		{
			model.Sage = BuildSage(mp);
		}

		private SageModel BuildSage(MP_CustomerMarketPlace mp)
		{
			var dbSalesInvoices = mp.SageRequests.SelectMany(sageRequest => sageRequest.SalesInvoices).OrderByDescending(salesInvoice => salesInvoice.Request.Id).Distinct(new SageSalesInvoiceComparer()).OrderByDescending(salesInvoice => salesInvoice.date);
			var dbPurchaseInvoices = mp.SageRequests.SelectMany(sageRequest => sageRequest.PurchaseInvoices).OrderByDescending(purchaseInvoice => purchaseInvoice.Request.Id).Distinct(new SagePurchaseInvoiceComparer()).OrderByDescending(purchaseInvoice => purchaseInvoice.date);
			var dbIncomes = mp.SageRequests.SelectMany(sageRequest => sageRequest.Incomes).OrderByDescending(income => income.Request.Id).Distinct(new SageIncomeComparer()).OrderByDescending(income => income.date);
			var dbExpenditures = mp.SageRequests.SelectMany(sageRequest => sageRequest.Expenditures).OrderByDescending(expenditure => expenditure.Request.Id).Distinct(new SageExpenditureComparer()).OrderByDescending(expenditure => expenditure.date);

			var model = new SageModel
			{
				SalesInvoices = SageSalesInvoicesConverter.GetSageSalesInvoices(dbSalesInvoices),
				PurchaseInvoices = SagePurchaseInvoicesConverter.GetSagePurchaseInvoices(dbPurchaseInvoices),
				Incomes = SageIncomesConverter.GetSageIncomes(dbIncomes),
				Expenditures = SageExpendituresConverter.GetSageExpenditures(dbExpenditures),
				InvoicesStatuses = _invoiceStatuses
			};

			return model;
		}

		public override DateTime? GetSeniority(MP_CustomerMarketPlace mp)
		{
			var salesInvoices = _session.Query<MP_SageSalesInvoice>()
				.Where(oi => oi.Request.CustomerMarketPlace.Id == mp.Id)
				.Where(oi => oi.date != null)
				.Select(oi => oi.date);
			var purchaseInvoices = _session.Query<MP_SagePurchaseInvoice>()
				.Where(oi => oi.Request.CustomerMarketPlace.Id == mp.Id)
				.Where(oi => oi.date != null)
				.Select(oi => oi.date);
			var incomes = _session.Query<MP_SageIncome>()
				.Where(oi => oi.Request.CustomerMarketPlace.Id == mp.Id)
				.Where(oi => oi.date != null)
				.Select(oi => oi.date);
			var expenditures = _session.Query<MP_SageExpenditure>()
				.Where(oi => oi.Request.CustomerMarketPlace.Id == mp.Id)
				.Where(oi => oi.date != null)
				.Select(oi => oi.date);

			DateTime? result = null;
			if (salesInvoices.Any())
			{
				result = salesInvoices.Min();
			}
			if (purchaseInvoices.Any())
			{
				DateTime? tmp = purchaseInvoices.Min();
				if (result == null || (tmp.HasValue && result.Value > tmp.Value))
				{
					result = tmp;
				}
			}
			if (incomes.Any())
			{
				DateTime? tmp = incomes.Min();
				if (result == null || (tmp.HasValue && result.Value > tmp.Value))
				{
					result = tmp;
				}
			}
			if (expenditures.Any())
			{
				DateTime? tmp = expenditures.Min();
				if (result == null || (tmp.HasValue && result.Value > tmp.Value))
				{
					result = tmp;
				}
			}

			return result;
		}

		public override DateTime? GetLastTransaction(MP_CustomerMarketPlace mp)
		{
			DateTime? salesInvoices = null;
			DateTime? purchaseInvoices = null;
			DateTime? incomes = null;
			DateTime? expenditures = null;
			var dates = new List<DateTime?>();

			var tmpSalesInvoices = _session.Query<MP_SageSalesInvoice>()
				.Where(oi => oi.Request.CustomerMarketPlace.Id == mp.Id)
				.Where(oi => oi.date != null);

			if (tmpSalesInvoices.Count() != 0)
			{
				salesInvoices = tmpSalesInvoices.Max(oi => oi.date);
			}

			var tmpPurchaseInvoices = _session.Query<MP_SagePurchaseInvoice>()
				.Where(oi => oi.Request.CustomerMarketPlace.Id == mp.Id)
				.Where(oi => oi.date != null);

			if (tmpPurchaseInvoices.Count() != 0)
			{
				purchaseInvoices = tmpPurchaseInvoices.Max(oi => oi.date);
			}

			var tmpIncomes = _session.Query<MP_SageIncome>()
				.Where(oi => oi.Request.CustomerMarketPlace.Id == mp.Id)
				.Where(oi => oi.date != null);

			if (tmpIncomes.Count() != 0)
			{
				incomes = tmpIncomes.Max(oi => oi.date);
			}

			var tmpExpenditures = _session.Query<MP_SageExpenditure>()
				.Where(oi => oi.Request.CustomerMarketPlace.Id == mp.Id)
				.Where(oi => oi.date != null);

			if (tmpExpenditures.Count() != 0)
			{
				expenditures = tmpExpenditures.Max(oi => oi.date);
			}

			dates.Add(salesInvoices);
			dates.Add(purchaseInvoices);
			dates.Add(incomes);
			dates.Add(expenditures);

			return dates.Max();
		}
	}
}
