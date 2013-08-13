using System;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Areas.Customer.Models;
using EzBob.Web.Areas.Underwriter.Models;
using Integration.ChannelGrabberConfig;
using NHibernate;
using NHibernate.Linq;

namespace EzBob.Models.Marketplaces.Builders {
	class ChannelGrabberMarketplaceModelBuilder : MarketplaceModelBuilder {
        public ChannelGrabberMarketplaceModelBuilder(ISession session)
            : base(session)
        {
		} // constructor

		public override PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model) {
			VendorInfo vi = Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(mp.Marketplace.Name);

			if (!vi.HasExpenses)
				return null;

			var paymentAccountModel = new PaymentAccountsModel {
				TotalNetInPayments = 0,
				TotalNetOutPayments = 0,
				TransactionsNumber = 0,
				MonthInPayments = 0
			};

			MP_AnalyisisFunctionValue earliestNumOfExpenses = GetEarliestValueFor(mp, FunctionType.NumOfExpenses.ToString());
			MP_AnalyisisFunctionValue earliestSumOfExpenses = GetEarliestValueFor(mp, FunctionType.TotalSumOfExpenses.ToString());

			MP_AnalyisisFunctionValue earliestNumOfInvoices = GetEarliestValueFor(mp, FunctionType.NumOfOrders.ToString());
			MP_AnalyisisFunctionValue earliestSumOfInvoices = GetEarliestValueFor(mp, FunctionType.TotalSumOfOrders.ToString());

			MP_AnalyisisFunctionValue monthSumOfInvoices = GetMonthValueFor(mp, FunctionType.TotalSumOfOrders.ToString());

			if ((earliestNumOfExpenses != null) && earliestNumOfExpenses.ValueInt.HasValue)
				paymentAccountModel.TransactionsNumber += earliestNumOfExpenses.ValueInt.Value;

			if ((earliestNumOfInvoices != null) && earliestNumOfInvoices.ValueInt.HasValue)
				paymentAccountModel.TransactionsNumber += earliestNumOfInvoices.ValueInt.Value;

			if (earliestSumOfInvoices != null && earliestSumOfInvoices.ValueFloat.HasValue)
				paymentAccountModel.TotalNetInPayments = earliestSumOfInvoices.ValueFloat.Value;

			if (earliestSumOfExpenses != null && earliestSumOfExpenses.ValueFloat.HasValue)
				paymentAccountModel.TotalNetOutPayments = earliestSumOfExpenses.ValueFloat.Value;

			if (monthSumOfInvoices != null && monthSumOfInvoices.ValueFloat.HasValue)
				paymentAccountModel.MonthInPayments = monthSumOfInvoices.ValueFloat.Value;

			return paymentAccountModel;
		} // GetPaymetAccountModel

	    public override DateTime? GetSeniority(MP_CustomerMarketPlace mp)
	    {
	        if (null == Integration.ChannelGrabberConfig.Configuration.Instance.GetVendorInfo(mp.Marketplace.Name))
	        {
	            return null;
	        }
	        
            var s = _session.Query<MP_ChannelGrabberOrderItem>()
	            .Where(oi => oi.Order.CustomerMarketPlace.Id == mp.Id)
	            .Where(oi => oi.PaymentDate != null)
	            .Select(oi => oi.PaymentDate);
	        return !s.Any() ? (DateTime?) null : s.Min();
	    }
	} // class ChannelGrabberMarketplaceBuilder
} // namespace
