namespace EzBob.Models.Marketplaces.Builders
{
	using System.Collections.Generic;
	using System.Linq;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using EZBob.DatabaseLib.Model.Marketplaces.Sage;
	using Marketplaces;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Sage;
	using Scorto.NHibernate.Repository;
	using Web.Areas.Customer.Models;
	using Web.Areas.Underwriter.Models;

	class SageMarketplaceModelBuilder : MarketplaceModelBuilder
    {
		private readonly ICurrencyConvertor currencyConverter;

		public SageMarketplaceModelBuilder(CustomerMarketPlaceRepository customerMarketplaces, CurrencyRateRepository currencyRateRepository)
            : base(customerMarketplaces)
        {
			currencyConverter = new CurrencyConvertor(currencyRateRepository);
        }

        public override PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model)
        {
			var paymentAccountModel = new PaymentAccountsModel();
			MP_AnalyisisFunctionValue earliestNumOfInvoices = GetEarliestValueFor(mp, "NumOfOrders");
			MP_AnalyisisFunctionValue earliestSumOfInvoices = GetEarliestValueFor(mp, "TotalSumOfOrders");

	        if (earliestNumOfInvoices != null && earliestNumOfInvoices.ValueInt.HasValue)
	        {
		        paymentAccountModel.TransactionsNumber = earliestNumOfInvoices.ValueInt.Value;
	        }
	        else
	        {
		        paymentAccountModel.TransactionsNumber = 0;
			}

			if (earliestSumOfInvoices != null && earliestSumOfInvoices.ValueFloat.HasValue)
			{
				paymentAccountModel.TotalNetInPayments = earliestSumOfInvoices.ValueFloat.Value;
			}
			else
			{
				paymentAccountModel.TotalNetInPayments = 0;
			}

			paymentAccountModel.TotalNetOutPayments = 0;
				
	        return paymentAccountModel;
        }

        protected override void InitializeSpecificData(MP_CustomerMarketPlace mp, MarketPlaceModel model)
        {
            model.Sage = BuildSage(mp);
        }

		private SageModel BuildSage(MP_CustomerMarketPlace mp)
		{
			var dbSalesInvoices = mp.SageRequests.SelectMany(sageRequest => sageRequest.SalesInvoices).OrderByDescending(salesInvoice => salesInvoice.Request.Id).Distinct(new SageInvoiceComparer()).OrderByDescending(salesInvoice => salesInvoice.date);
			var dbIncomes = mp.SageRequests.SelectMany(sageRequest => sageRequest.Incomes).OrderByDescending(income => income.Request.Id).Distinct(new SageIncomeComparer()).OrderByDescending(income => income.date);

			var model = new SageModel
			{
				SalesInvoices = SageSalesInvoicesConverter.GetSageSalesInvoices(dbSalesInvoices),
				Incomes = SageSalesInvoicesConverter.GetSageIncomes(dbIncomes)
			};

			return model;
		}
    }
}