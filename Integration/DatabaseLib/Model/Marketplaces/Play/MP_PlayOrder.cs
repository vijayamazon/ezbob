using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApplicationMng.Repository;
using Iesi.Collections.Generic;
using NHibernate;
using NHibernate.Linq;

namespace EZBob.DatabaseLib.Model.Database {
	public class MP_PlayOrder {
		public MP_PlayOrder() {
			OrderItems = new HashedSet<MP_PlayOrderItem>();
		} // constructor

		public virtual int Id { get; set; }
		public virtual MP_CustomerMarketPlace CustomerMarketPlace { get; set; }
		public virtual DateTime Created { get; set; }

		public virtual Iesi.Collections.Generic.ISet<MP_PlayOrderItem> OrderItems { get; set; }

		public virtual MP_CustomerMarketplaceUpdatingHistory HistoryRecord { get; set; }
	} // class MP_PlayOrder

	public interface IMP_PlayOrderRepository : IRepository<MP_PlayOrder> {}

	public class MP_PlayOrderRepository : NHibernateRepositoryBase<MP_PlayOrder>, IMP_PlayOrderRepository {
		public MP_PlayOrderRepository(ISession session) : base(session) {} // constructor

		public List<MP_PlayOrderItem> GetOrdersItemsByMakretplaceId(int marketplaceId) {
			return _session
				.Query<MP_PlayOrderItem>()
				.Where(oi => oi.Order.CustomerMarketPlace.Id == marketplaceId)
				.ToList();
		} // GetOrdersItemsByMarketplaceId
	} // MP_PlayOrderRepository
} // namespace
