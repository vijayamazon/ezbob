using System;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;
using NHibernate.Linq;

namespace EzBob.Models.Marketplaces.Builders
{
	using System.Globalization;
	using Web.Areas.Customer.Models;

	class CompanyFilesMarketplaceModelBuilder : MarketplaceModelBuilder
	{
		public CompanyFilesMarketplaceModelBuilder(ISession session)
			: base(session)
		{
		}

		public override DateTime? GetSeniority(MP_CustomerMarketPlace mp)
		{
			return null;
		}

		public override DateTime? GetLastTransaction(MP_CustomerMarketPlace mp)
		{
			return null;
		}

		public override PaymentAccountsModel GetPaymentAccountModel(MP_CustomerMarketPlace mp, MarketPlaceModel model, DateTime? history)
		{
			var companyFiles = new PaymentAccountsModel
			{
				displayName = mp.DisplayName,
				TotalNetInPayments = 0,
				MonthInPayments = 0,
				TotalNetOutPayments = 0,
				TransactionsNumber = 0,
				id = mp.Id,
				Status = ""  ,
			};
			return companyFiles;
		}
	}
}