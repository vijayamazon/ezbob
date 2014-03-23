using ApplicationMng.Model;
using ApplicationMng.Repository;
using NHibernate;
using System;
namespace NHibernateWrapper.NHibernate.Repository
{
	public class CreditProductsRepository : NHibernateRepositoryBase<CreditProduct>
	{
		public CreditProductsRepository(ISession session) : base(session)
		{
		}
	}
}
