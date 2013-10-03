using System;
using System.Linq;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Areas.Customer.Models;
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
    }
}