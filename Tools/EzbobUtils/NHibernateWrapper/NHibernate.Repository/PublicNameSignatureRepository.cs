using ApplicationMng.Repository;
using NHibernate;
using NHibernateWrapper.NHibernate.Model;
using System;
namespace NHibernateWrapper.NHibernate.Repository
{
	public class PublicNameSignatureRepository : NHibernateRepositoryBase<PublicNameSignature>
	{
		public PublicNameSignatureRepository(ISession session) : base(session)
		{
		}
	}
}
