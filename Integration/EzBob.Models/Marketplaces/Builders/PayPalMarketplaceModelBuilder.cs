using System;
using System.Linq;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Areas.Customer.Models;
using NHibernate;
using NHibernate.Linq;

namespace EzBob.Models.Marketplaces.Builders
{
	public class PayPalMarketplaceModelBuilder : MarketplaceModelBuilder
	{
		public PayPalMarketplaceModelBuilder(ISession session)
			: base(session)
		{
		}

		public override PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history)
		{
			return model.PayPal.GeneralInfo;
		}

		public override string GetUrl(MP_CustomerMarketPlace mp, IMarketPlaceSecurityInfo securityInfo)
		{
			var url = base.GetUrl(mp, securityInfo);
			if (mp.EbayUserData == null) return url;
			var mpEbayUserData = mp.EbayUserData.LastOrDefault();
			if (mpEbayUserData == null || mpEbayUserData.SellerInfo == null) return url;
			if (string.IsNullOrEmpty(mpEbayUserData.SellerInfo.SellerInfoStoreURL)) return url;
			return mpEbayUserData.SellerInfo.SellerInfoStoreURL;
		}

		public override DateTime? GetSeniority(MP_CustomerMarketPlace mp)
		{
			var payPalTransactions =
				_session.Query<MP_PayPalTransaction>()
					   .Where(t => t.CustomerMarketPlace.Id == mp.Id)
					   .SelectMany(x => x.TransactionItems);

			var transactionsMinDate = payPalTransactions.Any() ? payPalTransactions.Min(f => f.Created) : (DateTime?)null;

			return transactionsMinDate;
		}

		public override DateTime? GetLastTransaction(MP_CustomerMarketPlace mp)
		{
			return _session.Query<MP_PayPalTransaction>()
					   .Where(t => t.CustomerMarketPlace.Id == mp.Id)
					   .SelectMany(x => x.TransactionItems).Max(x => x.Created);
		}

		protected override void InitializeSpecificData(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history)
		{
			model.PayPal = PayPalModelBuilder.CreatePayPal(mp, history);
		}
	}
}