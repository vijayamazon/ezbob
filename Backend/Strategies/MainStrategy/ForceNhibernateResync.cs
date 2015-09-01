namespace Ezbob.Backend.Strategies.MainStrategy {
	using System;
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

			try {
				session.Evict(customer);
			} catch (Exception) {
				// Silently ignore
			} // try

			MarketplaceTurnoverRepository mpTurnoverRep = ObjectFactory.GetInstance<MarketplaceTurnoverRepository>();

			foreach (MarketplaceTurnover mpt in mpTurnoverRep.GetByCustomerId(customerID)) {
				if (mpt != null) {
					try {
						session.Evict(mpt);
					} catch (Exception) {
						// Silently ignore
					} // try
				} // if
			} // for
		} // ForCustomer
	} // class ForceNhibernateResync
} // namespace
