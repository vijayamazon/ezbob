namespace Ezbob.Backend.Strategies.MainStrategy.Helpers {
	using System;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
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
		} // ForCustomer
	} // class ForceNhibernateResync
} // namespace
