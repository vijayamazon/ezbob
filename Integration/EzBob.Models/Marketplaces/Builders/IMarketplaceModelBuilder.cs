namespace EzBob.Models.Marketplaces.Builders {
	using System;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.Model.Database;

	public interface IMarketplaceModelBuilder {
		PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history);
		string GetUrl(MP_CustomerMarketPlace mp, IMarketPlaceSecurityInfo securityInfo);
		MarketPlaceModel Create(MP_CustomerMarketPlace mp, DateTime? history);
		void UpdateOriginationDate(MP_CustomerMarketPlace mp);
	}
}