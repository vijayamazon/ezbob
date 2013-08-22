using ApplicationMng.Repository;
using NHibernate;

namespace EZBob.DatabaseLib.Model.Database {
	#region interface IBusinessRepository

	public interface IBusinessRepository : IRepository<Business> {} // IBusinessRepository

	#endregion interface IBusinessRepository

	#region class BusinessRepository

	public class BusinessRepository : NHibernateRepositoryBase<Business>, IBusinessRepository {
		public BusinessRepository(ISession session) : base(session) {} // constructor
	} // class BusinessRepository

	#endregion class BusinessRepository
} // namespace
