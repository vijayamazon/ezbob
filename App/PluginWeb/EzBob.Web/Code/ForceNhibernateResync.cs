namespace EzBob.Web.Code {
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository.Turnover;
	using NHibernate;
	using StructureMap;

	internal static class ForceNhibernateResync {
		public static void ForCustomer(int customerID) {
			ISession session = ObjectFactory.GetInstance<ISession>();

			Customer customer = ObjectFactory.GetInstance<CustomerRepository>().ReallyTryGet(customerID);

			if (customer == null)
				return;

			session.Evict(customer);

			MarketplaceTurnoverRepository mpTurnoverRep = ObjectFactory.GetInstance<MarketplaceTurnoverRepository>();
			foreach (MarketplaceTurnover mpt in mpTurnoverRep.GetByCustomerId(customerID))
				if (mpt != null)
					session.Evict(mpt);
		} // ForCustomer
	} // class ForceNhibernateResync
} // namespace
