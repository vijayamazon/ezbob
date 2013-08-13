namespace EzBob.Models.Marketplaces.Builders
{
	using System;
	using System.Globalization;
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
	using Web.Areas.Underwriter.Models;

	class SageMarketplaceModelBuilder : MarketplaceModelBuilder
    {
		readonly Dictionary<int, string> _invoiceStatuses = new Dictionary<int, string>();

		public SageMarketplaceModelBuilder(MP_SagePaymentStatusRepository sagePaymentStatusRepository, ISession session)
            : base(session)
        {
			foreach (var status in sagePaymentStatusRepository.GetAll())
			{
				_invoiceStatuses.Add(status.SageId, status.name);
			}
        }

        public override PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model)
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

        protected override void InitializeSpecificData(MP_CustomerMarketPlace mp, MarketPlaceModel model)
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
                DateTime tmp = purchaseInvoices.Min();
                if (result == null || result > tmp)
                {
                    result = tmp;
                }
            }
            if (incomes.Any())
            {
                DateTime tmp = incomes.Min();
                if (result == null || result > tmp)
                {
                    result = tmp;
                }
            }
            if (expenditures.Any())
            {
                DateTime tmp = expenditures.Min();
                if (result == null || result > tmp)
                {
                    result = tmp;
                }
            }

            return result;
	    }
    }
}