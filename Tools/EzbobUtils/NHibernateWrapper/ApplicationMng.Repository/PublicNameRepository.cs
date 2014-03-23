using ApplicationMng.Model;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
namespace ApplicationMng.Repository
{
	public class PublicNameRepository : NHibernateRepositoryBase<PublicName>, IPublicNameRepository, IRepository<PublicName>, System.IDisposable
	{
		public PublicNameRepository(ISession session) : base(session)
		{
		}
		public System.Linq.IQueryable<PublicName> GetAllPublicNames()
		{
			return this._session.Query<PublicName>();
		}
		public System.Linq.IQueryable<PublicName> GetActivePublicNames()
		{
			return 
				from pn in this.GetAllPublicNames()
				where (pn.IsStopped == null || pn.IsStopped == (int?)0) && (pn.IsDeleted == null || pn.IsDeleted == (int?)0)
				select pn;
		}
		public System.Collections.Generic.IList<PublicName> GetActivePublicNamesList()
		{
			return this._session.CreateCriteria<PublicName>().Add(Restrictions.Or(Restrictions.IsNull("IsStopped"), Restrictions.Eq("IsStopped", 0))).Add(Restrictions.Or(Restrictions.IsNull("IsDeleted"), Restrictions.Eq("IsDeleted", 0))).SetCacheable(true).SetCacheRegion("VeryShort").List<PublicName>();
		}
	}
}
