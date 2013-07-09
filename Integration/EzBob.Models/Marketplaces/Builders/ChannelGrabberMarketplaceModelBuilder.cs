using EZBob.DatabaseLib.Model.Database;
using EZBob.DatabaseLib.Model.Database.Repository;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Areas.Underwriter.Models;
using Integration.ChannelGrabberConfig;

namespace EzBob.Models.Marketplaces.Builders {
	class ChannelGrabberMarketplaceModelBuilder : MarketplaceModelBuilder {
		public ChannelGrabberMarketplaceModelBuilder(CustomerMarketPlaceRepository customerMarketplaces) : base(customerMarketplaces) {
		} // constructor

		public override PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model) {
			VendorInfo vi = Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(mp.Marketplace.Name);

			if (!vi.HasExpenses)
				return null;

			var paymentAccountModel = new PaymentAccountsModel {
				TotalNetInPayments = 0,
				TotalNetOutPayments = 0,
				TransactionsNumber = 0
			};

			MP_AnalyisisFunctionValue earliestNumOfExpenses = GetEarliestValueFor(mp, FunctionType.NumOfExpenses.ToString());
			MP_AnalyisisFunctionValue earliestSumOfExpenses = GetEarliestValueFor(mp, FunctionType.TotalSumOfExpenses.ToString());

			MP_AnalyisisFunctionValue earliestNumOfInvoices = GetEarliestValueFor(mp, FunctionType.NumOfOrders.ToString());
			MP_AnalyisisFunctionValue earliestSumOfInvoices = GetEarliestValueFor(mp, FunctionType.TotalSumOfOrders.ToString());

			if ((earliestNumOfExpenses != null) && earliestNumOfExpenses.ValueInt.HasValue)
				paymentAccountModel.TransactionsNumber += earliestNumOfExpenses.ValueInt.Value;

			if ((earliestNumOfInvoices != null) && earliestNumOfInvoices.ValueInt.HasValue)
				paymentAccountModel.TransactionsNumber += earliestNumOfInvoices.ValueInt.Value;

			if (earliestSumOfInvoices != null && earliestSumOfInvoices.ValueFloat.HasValue)
				paymentAccountModel.TotalNetInPayments = earliestSumOfInvoices.ValueFloat.Value;

			if (earliestSumOfExpenses != null && earliestSumOfExpenses.ValueFloat.HasValue)
				paymentAccountModel.TotalNetOutPayments = earliestSumOfExpenses.ValueFloat.Value;

			return paymentAccountModel;
		} // GetPaymetAccountModel
	} // class ChannelGrabberMarketplaceBuilder
} // namespace
