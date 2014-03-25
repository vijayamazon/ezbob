namespace EZBob.DatabaseLib.Model.Database.Repository {
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using NHibernate;
	using NHibernate.Linq;

	public class PostcodeServiceLogRepository : NHibernateRepositoryBase<PostcodeServiceLog> {
		public PostcodeServiceLogRepository(ISession session) : base(session) {
		} // constructor

		public IEnumerable<PostcodeServiceLog> GetByCustomer(Customer customer) {
			return GetAll().Where(x => (x.Customer != null) && (x.Customer.Id == customer.Id)).ToFuture();
		} // GetByCustomer
	} // PostcodeServiceLogRepository
} // namespace
