using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationMng.Repository;
using Iesi.Collections.Generic;
using NHibernate;
using NHibernate.Linq;

namespace EZBob.DatabaseLib.Model.Database {
	public class MP_ChannelGrabberOrder {
		public MP_ChannelGrabberOrder() {
			OrderItems = new HashedSet<MP_ChannelGrabberOrderItem>();
		} // constructor

		public virtual int Id { get; set; }
		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual DateTime Created { get; set; }

		public virtual Iesi.Collections.Generic.ISet<MP_ChannelGrabberOrderItem> OrderItems { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }
	} // class MP_ChannelGrabberOrder

	public interface IMP_ChannelGrabberOrderRepository : IRepository<MP_ChannelGrabberOrder> {}

	public class MP_ChannelGrabberOrderRepository : NHibernateRepositoryBase<MP_ChannelGrabberOrder>, IMP_ChannelGrabberOrderRepository {
		public MP_ChannelGrabberOrderRepository(ISession session) : base(session) {} // constructor

		public List<MP_ChannelGrabberOrderItem> GetOrdersItemsByMakretplaceId(int marketplaceId) {
			return _session
				.Query<MP_ChannelGrabberOrderItem>()
				.Where(oi => oi.Order.CustomerMarketPlace.Id == marketplaceId)
				.ToList();
		} // GetOrdersItemsByMarketplaceId
	} // MP_ChannelGrabberOrderRepository
} // namespace
