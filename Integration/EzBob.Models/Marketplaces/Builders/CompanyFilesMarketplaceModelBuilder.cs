using System;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;
using NHibernate.Linq;

namespace EzBob.Models.Marketplaces.Builders
{
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
	}
}