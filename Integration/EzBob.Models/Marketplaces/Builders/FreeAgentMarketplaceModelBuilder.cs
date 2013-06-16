namespace EzBob.Models
{
	using EZBob.DatabaseLib.Model.Marketplaces.FreeAgent;
	using NHibernate;
	using Scorto.NHibernate.Repository;
	using System.Collections.Generic;
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using Web.Areas.Customer.Models;
	using Web.Areas.Underwriter.Models;
	using YodleeLib.connector;
	using System.Linq;

	class FreeAgentMarketplaceModelBuilder : MarketplaceModelBuilder
    {
		private readonly MP_FreeAgentRequestRepository _freeAgentRequestRepository;
        private readonly ISession _session;

		public FreeAgentMarketplaceModelBuilder(MP_FreeAgentRequestRepository freeAgentRequestRepository, CustomerMarketPlaceRepository customerMarketplaces, ISession session)
            : base(customerMarketplaces)
        {
			_freeAgentRequestRepository = freeAgentRequestRepository;
            _session = session;
        }

        public override PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model)
        {
			var paymentAccountModel = new PaymentAccountsModel();
			MP_AnalyisisFunctionValue earliestNumOfExpenses = GetEarliestValueFor(mp, "NumOfExpenses");
			MP_AnalyisisFunctionValue earliestNumOfInvoices = GetEarliestValueFor(mp, "NumOfOrders");
			MP_AnalyisisFunctionValue earliestSumOfExpenses = GetEarliestValueFor(mp, "TotalSumOfExpenses");
			MP_AnalyisisFunctionValue earliestSumOfInvoices = GetEarliestValueFor(mp, "TotalSumOfOrders");

	        if (earliestNumOfExpenses != null && earliestNumOfExpenses.ValueInt.HasValue &&
	            earliestNumOfInvoices != null && earliestNumOfInvoices.ValueInt.HasValue)
	        {
		        paymentAccountModel.TransactionsNumber = earliestNumOfExpenses.ValueInt.Value + earliestNumOfInvoices.ValueInt.Value;
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

			if (earliestSumOfExpenses != null && earliestSumOfExpenses.ValueFloat.HasValue)
			{
				paymentAccountModel.TotalNetOutPayments = earliestSumOfExpenses.ValueFloat.Value;
			}
			else
			{
				paymentAccountModel.TotalNetOutPayments = 0;
			}
				
	        return paymentAccountModel;
        }
    }
}