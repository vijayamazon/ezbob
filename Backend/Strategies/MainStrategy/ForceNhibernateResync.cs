namespace Ezbob.Backend.Strategies.MainStrategy {
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository.Turnover;
	using NHibernate;
	using StructureMap;

	internal static class ForceNhibernateResync {
		public static void Do(int customerID) {
			ISession session = ObjectFactory.GetInstance<ISession>();

			Customer customer = ObjectFactory.GetInstance<CustomerRepository>().ReallyTryGet(customerID);

			if (customer == null)
				return;

			foreach (MP_CustomerMarketPlace mp in customer.CustomerMarketPlaces) {
				if (mp == null)
					continue;

				foreach (MP_CustomerMarketplaceUpdatingHistory h in mp.UpdatingHistory)
					if (h != null)
						session.Evict(h);

				session.Evict(mp);
			} // for each customer marketplace

			session.Evict(customer);

			MarketplaceTurnoverRepository mpTurnoverRep = ObjectFactory.GetInstance<MarketplaceTurnoverRepository>();
			foreach (MarketplaceTurnover mpt in mpTurnoverRep.GetByCustomerId(customerID))
				if (mpt != null)
					session.Evict(mpt);
		} // Do
	} // class ForceNhibernateResync
} // namespace
