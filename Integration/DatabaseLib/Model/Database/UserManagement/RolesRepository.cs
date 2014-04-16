namespace EZBob.DatabaseLib.Model.Database.UserManagement
{
	using ApplicationMng.Repository;
	using NHibernate;

	public interface IRolesRepository : IRepository<Role>
	{
	}

	public class RolesRepository : NHibernateRepositoryBase<Role>, IRolesRepository
	{
		public RolesRepository(ISession session) : base(session)
		{
		}
	}
}
