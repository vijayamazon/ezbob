using ApplicationMng.Repository;
using NHibernate;
using NHibernateWrapper.NHibernate.Model;
using System;
namespace NHibernateWrapper.NHibernate.Repository
{
	public class CreditProductsSignatureRepository : NHibernateRepositoryBase<CreditProductSignature>
	{
		public CreditProductsSignatureRepository(ISession session) : base(session)
		{
		}
	}
}
