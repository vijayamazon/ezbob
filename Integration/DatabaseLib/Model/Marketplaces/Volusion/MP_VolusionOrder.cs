using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApplicationMng.Repository;
using Iesi.Collections.Generic;
using NHibernate;
using NHibernate.Linq;

namespace EZBob.DatabaseLib.Model.Database {
	public class MP_VolusionOrder {
		public MP_VolusionOrder() {
			OrderItems = new HashedSet<MP_VolusionOrderItem>();
		} // constructor

		public virtual int Id { get; set; }
		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual DateTime Created { get; set; }

		public virtual Iesi.Collections.Generic.ISet<MP_VolusionOrderItem> OrderItems { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }
	} // class MP_VolusionOrder

	public interface IMP_VolusionOrderRepository : IRepository<MP_VolusionOrder> {}

	public class MP_VolusionOrderRepository : NHibernateRepositoryBase<MP_VolusionOrder>, IMP_VolusionOrderRepository {
		public MP_VolusionOrderRepository(ISession session) : base(session) {} // constructor

		public List<MP_VolusionOrderItem> GetOrdersItemsByMakretplaceId(int marketplaceId) {
			return _session
				.Query<MP_VolusionOrderItem>()
				.Where(oi => oi.Order.CustomerMarketPlace.Id == marketplaceId)
				.ToList();
		} // GetOrdersItemsByMarketplaceId
	} // MP_VolusionOrderRepository
} // namespace
