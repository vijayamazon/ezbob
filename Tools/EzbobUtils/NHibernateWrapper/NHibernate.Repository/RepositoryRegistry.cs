using ApplicationMng.Repository;
using NHibernate;
using StructureMap.Configuration.DSL;
namespace NHibernateWrapper.NHibernate.Repository
{
	public class RepositoryRegistry : Registry
	{
		public RepositoryRegistry()
		{
			base.For(typeof(IRepository<>)).Use(typeof(NHibernateRepositoryBase<>));
			base.For<ISessionFactory>().Use(() => NHibernateManager.SessionFactory);
			base.For<IUsersRepository>().Use<UsersRepository>();
			base.For<IRolesRepository>().Use<RolesRepository>();
			base.For<ISecurityQuestionRepository>().Use<SecurityQuestionRepository>();
		}
	}
}
