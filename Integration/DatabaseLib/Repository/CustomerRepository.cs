using System;
using ApplicationMng.Repository;
using EZBob.DatabaseLib.Exceptions;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database.Repository
{
	using Ezbob.Database;
	using Ezbob.Logger;

	public interface ICustomerRepository : IRepository<Customer>
	{
		Customer Get(int clientId);
		Customer TryGet(int clientId);
		bool RefNumberExists(string refnumber);
		Customer GetCustomerByRefNum(string refnumber);
		Customer GetChecked(int id);
		Customer GetAndInitialize(int id);
		Customer TryGetByEmail(string sEmail);
	}

	public class CustomerRepository : NHibernateRepositoryBase<Customer>, ICustomerRepository
	{
		public CustomerRepository(ISession session)
			: base(session)
		{
		}

		public Customer Get(int clientId)
		{
			var m_oRetryer = new SqlRetryer(nSleepBeforeRetryMilliseconds: 500);
			var customer = m_oRetryer.Retry(() => _session.Get<Customer>(clientId));
			if (customer == null)
			{
				throw new InvalidCustomerException(clientId);
			}
			return customer;
		}

		public Customer ReallyTryGet(int clientId)
		{
			try {
				return Get(clientId);
			}
			catch (Exception e) {
				var oLog = new SafeILog(this);
				oLog.Warn(e, "Could not retrieve customer by id {0}.", clientId);
				return null;
			}
		}

		public Customer TryGet(int clientId)
		{
			return Get(clientId);
		}

		public bool RefNumberExists(string refnumber)
		{
			return _session.QueryOver<Customer>().Where(c => c.RefNumber == refnumber).RowCount() > 0;
		}

		public Customer GetCustomerByRefNum(string refnumber)
		{
			var customer = _session.QueryOver<Customer>().Where(c => c.RefNumber == refnumber).SingleOrDefault<Customer>();
			if (customer == null) throw new InvalidCustomerException(string.Format("Customer ref. #{0} was not found", refnumber));
			return customer;
		}

		public Customer TryGetByEmail(string sEmail)
		{
			return _session.QueryOver<Customer>().Where(c => c.Name == sEmail).SingleOrDefault<Customer>();
		} // TryGetByEmail

		public Customer GetChecked(int id)
		{
			var customer = _session.Get<Customer>(id);
			if (customer == null) throw new InvalidCustomerException(id);
			return customer;
		}

		public Customer GetAndInitialize(int id)
		{
			var customer = _session
								.QueryOver<Customer>()
								.Where(c => c.Id == id)
								.Fetch(x => x.Loans).Eager
								.SingleOrDefault<Customer>();
			return customer;
		}
	}
}