namespace EZBob.DatabaseLib.Model.Marketplaces.FreeAgent
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using Database;
	using Iesi.Collections.Generic;
	using NHibernate;
	using NHibernate.Linq;

	public class MP_FreeAgentOrder
	{
		public MP_FreeAgentOrder()
		{
			OrderItems = new HashedSet<MP_FreeAgentOrderItem>();
		}

		public virtual int Id { get; set; }
		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual DateTime Created { get; set; }

		public virtual Iesi.Collections.Generic.ISet<MP_FreeAgentOrderItem> OrderItems { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }
	}

	public interface IMP_FreeAgentOrderRepository : IRepository<MP_FreeAgentOrder>
    {

    }

    public class MP_FreeAgentOrderRepository : NHibernateRepositoryBase<MP_FreeAgentOrder>, IMP_FreeAgentOrderRepository
    {
		public MP_FreeAgentOrderRepository(ISession session)
			: base(session)
        {
        }

		public List<MP_FreeAgentOrderItem> GetOrdersItemsByMakretplaceId(int marketplaceId)
        {
            return _session
				.Query<MP_FreeAgentOrderItem>()
                .Where(oi => oi.Order.CustomerMarketPlace.Id == marketplaceId)
                .ToList();
        }

	    
    }
}
