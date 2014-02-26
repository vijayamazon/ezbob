using System;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using NHibernate;
using NHibernate.Linq;

namespace EzBob.Models.Marketplaces.Builders
{
	class EkmMarketplaceModelBuilder : MarketplaceModelBuilder
	{
		public EkmMarketplaceModelBuilder(ISession session)
			: base(session)
		{
		}

		public override DateTime? GetSeniority(MP_CustomerMarketPlace mp)
		{
			var s = _session.Query<MP_EkmOrderItem>()
				.Where(oi => oi.Order.CustomerMarketPlace.Id == mp.Id)
				.Where(oi => oi.OrderDate != null)
				.Select(oi => oi.OrderDate);
			return s.Any() ? s.Min() : (DateTime?)null;
		}

		public override DateTime? GetLastTransaction(MP_CustomerMarketPlace mp)
		{
			var s = _session.Query<MP_EkmOrderItem>().Where(oi => oi.Order.CustomerMarketPlace.Id == mp.Id).Where(oi => oi.OrderDate != null);

			if (s.Count() != 0)
			{
				return s.Max(oi => oi.OrderDate);
			}

			return null;
		}
	}
}