namespace EZBob.DatabaseLib.Model.Database.Repository {
	using System;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Exceptions;
	using Ezbob.Database;
	using Ezbob.Logger;
	using NHibernate;

	public interface ICustomerRepository : IRepository<Customer> {
		Customer Get(int clientId);
		Customer GetCustomerByRefNum(string refnumber);
		Customer GetChecked(int id);
		Customer GetAndInitialize(int id);
		Customer ReallyTryGet(int id);
	} // interface ICustomerRepository

	public class CustomerRepository : NHibernateRepositoryBase<Customer>, ICustomerRepository {
		public CustomerRepository(ISession session) : base(session) { }

		public Customer Get(int clientId) {
			var m_oRetryer = new SqlRetryer(nSleepBeforeRetryMilliseconds: 500);

			// var customer = m_oRetryer.Retry(() => Session.Get<Customer>(clientId));
			var customer = m_oRetryer.Retry(
				() => Session.QueryOver<Customer>().Where(c => c.Id == clientId).SingleOrDefault<Customer>()
			);

			if (customer == null)
				throw new InvalidCustomerException(clientId);

			return customer;
		} // Get

		public Customer ReallyTryGet(int clientId) {
			try {
				return Get(clientId);
			} catch (Exception e) {
				var oLog = new SafeILog(this);
				oLog.Warn(e, "Could not retrieve customer by id {0}.", clientId);
				return null;
			} // try
		} // ReallyTryGet

		public Customer GetCustomerByRefNum(string refnumber) {
			return Session.QueryOver<Customer>().Where(c => c.RefNumber == refnumber).SingleOrDefault<Customer>();
		} // GetCustomerByRefNum

		public Customer GetChecked(int id) {
			var customer = Session.Get<Customer>(id);

			if (customer == null)
				throw new InvalidCustomerException(id);

			return customer;
		} // GetChecked

		public Customer GetAndInitialize(int id) {
			return Session
				.QueryOver<Customer>()
				.Where(c => c.Id == id)
				.Fetch(x => x.Loans)
				.Eager
				.SingleOrDefault<Customer>();
		} // GetAndInitialize
	} // class CustomerRepository
} // namespace