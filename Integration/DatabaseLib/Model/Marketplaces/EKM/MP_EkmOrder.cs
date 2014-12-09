using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApplicationMng.Repository;
using Iesi.Collections.Generic;
using NHibernate;
using NHibernate.Linq;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EkmOrder
	{
		public MP_EkmOrder()
		{
			OrderItems = new HashedSet<MP_EkmOrderItem>();
		}

		public virtual int Id { get; set; }
		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual DateTime Created { get; set; }

		public virtual Iesi.Collections.Generic.ISet<MP_EkmOrderItem> OrderItems { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }
	}

    public interface IMP_EkmOrderRepository : IRepository<MP_EkmOrder>
    {

    }

    public class MP_EkmOrderRepository : NHibernateRepositoryBase<MP_EkmOrder>, IMP_EkmOrderRepository
    {
        public MP_EkmOrderRepository(ISession session) : base(session)
        {
        }

        public List<MP_EkmOrderItem> GetOrdersItemsByMakretplaceId(int marketplaceId)
        {
            return _session
                .Query<MP_EkmOrderItem>()
                .Where(oi => oi.Order.CustomerMarketPlace.Id == marketplaceId)
                .ToList();
        }

    }
}
