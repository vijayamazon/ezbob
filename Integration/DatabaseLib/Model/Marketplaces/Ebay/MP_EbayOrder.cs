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
	public class MP_EbayOrder
	{
		public MP_EbayOrder()
		{
			OrderItems = new HashedSet<MP_EbayOrderItem>();
		}

		public virtual int Id { get; set; }
		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual DateTime Created { get; set; }

		public virtual Iesi.Collections.Generic.ISet<MP_EbayOrderItem> OrderItems { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }
	}

    public interface IMP_EbayOrderRepository : IRepository<MP_EbayOrder>
    {

    }

    public class MP_EbayOrderRepository : NHibernateRepositoryBase<MP_EbayOrder>, IMP_EbayOrderRepository
    {
        public MP_EbayOrderRepository(ISession session) : base(session)
        {
        }

        public List<MP_EbayOrderItem> GetOrdersItemsByMakretplaceId(int marketplaceId)
        {
            return Session
                .Query<MP_EbayOrderItem>()
                .Where(oi => oi.Order.CustomerMarketPlace.Id == marketplaceId)
                .Fetch(oi => oi.ShippingAddress)
                .FetchMany(oi => oi.Transactions)
                .FetchMany(oi => oi.ExternalTransactions)
                .ToList();
        }

    }

	public class MP_EbayTransactionsRepository : NHibernateRepositoryBase<MP_EbayTransaction>, IMP_EbayTransactionsRepository
	{
		public MP_EbayTransactionsRepository(ISession session) 
			: base(session)
		{
		}

		public List<MP_EbayTransaction> GetAllItemsWithItemsID( string itemID )
		{
			return Session
				.Query<MP_EbayTransaction>()				
				.Where( t => t.ItemID == itemID && t.OrderItemDetail == null ).ToList();
		}
	}

	public interface IMP_EbayTransactionsRepository : IRepository<MP_EbayTransaction>
	{
	}
}
