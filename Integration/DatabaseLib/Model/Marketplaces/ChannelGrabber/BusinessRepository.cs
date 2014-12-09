using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database {

	public interface IBusinessRepository : IRepository<Business> {} // IBusinessRepository

	public class BusinessRepository : NHibernateRepositoryBase<Business>, IBusinessRepository {
		public BusinessRepository(ISession session) : base(session) {} // constructor
	} // class BusinessRepository

} // namespace
