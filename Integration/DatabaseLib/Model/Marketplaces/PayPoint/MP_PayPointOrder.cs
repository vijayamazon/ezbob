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
	public class MP_PayPointOrder
	{
		public MP_PayPointOrder()
		{
			OrderItems = new HashedSet<MP_PayPointOrderItem>();
		}

		public virtual int Id { get; set; }
		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual DateTime Created { get; set; }

		public virtual Iesi.Collections.Generic.ISet<MP_PayPointOrderItem> OrderItems { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }
	}

    public interface IMP_PayPointOrderRepository : IRepository<MP_PayPointOrder>
    {

    }

    public class MP_PayPointOrderRepository : NHibernateRepositoryBase<MP_PayPointOrder>, IMP_PayPointOrderRepository
    {
        public MP_PayPointOrderRepository(ISession session) : base(session)
        {
        }

        public List<MP_PayPointOrderItem> GetOrdersItemsByMakretplaceId(int marketplaceId)
        {
            return _session
                .Query<MP_PayPointOrderItem>()
                .Where(oi => oi.Order.CustomerMarketPlace.Id == marketplaceId)
                .ToList();
        }

	    
    }
}
