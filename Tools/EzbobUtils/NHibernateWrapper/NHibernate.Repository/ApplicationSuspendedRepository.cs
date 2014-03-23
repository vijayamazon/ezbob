using ApplicationMng.Repository;
using NHibernate;
using NHibernateWrapper.NHibernate.Model;
using System;
namespace NHibernateWrapper.NHibernate.Repository
{
	public class ApplicationSuspendedRepository : NHibernateRepositoryBase<ApplicationSuspended>
	{
		public ApplicationSuspendedRepository(ISession session) : base(session)
		{
		}
	}
}
