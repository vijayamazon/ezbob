using ApplicationMng.Model;
using ApplicationMng.Repository;
using NHibernate;
using System;
namespace NHibernateWrapper.NHibernate.Repository
{
	public class PublicNameStrategyRepository : NHibernateRepositoryBase<PublicNameStrategy>
	{
		public PublicNameStrategyRepository(ISession session) : base(session)
		{
		}
	}
}
