namespace EzBob.Models.Marketplaces.Builders
{
	using System.Linq;
	using EZBob.DatabaseLib.DatabaseWrapper.Order;
	using Marketplaces;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
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
			MP_AnalyisisFunctionValue earliestNumOfSalesInvoices = GetEarliestValueFor(mp, "NumOfOrders");
			MP_AnalyisisFunctionValue earliestNumOfIncomes = GetEarliestValueFor(mp, "NumOfIncomes");
			MP_AnalyisisFunctionValue earliestNumOfPurchaseInvoices = GetEarliestValueFor(mp, "NumOfPurchaseInvoices");
			MP_AnalyisisFunctionValue earliestSumOfSalesInvoices = GetEarliestValueFor(mp, "TotalSumOfOrders");
			MP_AnalyisisFunctionValue earliestSumOfIncomes = GetEarliestValueFor(mp, "TotalSumOfIncomes");
			MP_AnalyisisFunctionValue earliestSumOfPurchaseInvoices = GetEarliestValueFor(mp, "TotalSumOfPurchaseInvoices");

			paymentAccountModel.TransactionsNumber = 0;
			if (earliestNumOfSalesInvoices != null && earliestNumOfSalesInvoices.ValueInt.HasValue)
			{
				paymentAccountModel.TransactionsNumber += earliestNumOfSalesInvoices.ValueInt.Value;
			}
			if (earliestNumOfIncomes != null && earliestNumOfIncomes.ValueInt.HasValue)
			{
				paymentAccountModel.TransactionsNumber += earliestNumOfIncomes.ValueInt.Value;
			}
			if (earliestNumOfPurchaseInvoices != null && earliestNumOfPurchaseInvoices.ValueInt.HasValue)
			{
				paymentAccountModel.TransactionsNumber += earliestNumOfPurchaseInvoices.ValueInt.Value;
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
			var dbPurchaseInvoices = mp.SageRequests.SelectMany(sageRequest => sageRequest.PurchaseInvoices).OrderByDescending(purchaseInvoice => purchaseInvoice.Request.Id).Distinct(new SagePurchaseInvoiceComparer()).OrderByDescending(purchaseInvoice => purchaseInvoice.date);
			
			var model = new SageModel
			{
				SalesInvoices = SageSalesInvoicesConverter.GetSageSalesInvoices(dbSalesInvoices),
				Incomes = SageIncomesConverter.GetSageIncomes(dbIncomes),
				PurchaseInvoices = SagePurchaseInvoicesConverter.GetSagePurchaseInvoices(dbPurchaseInvoices)
			};

			return model;
		}
    }
}