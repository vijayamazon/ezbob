using ApplicationMng.Model;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
namespace ApplicationMng.Repository
{
	public class AttachDocTypeRepository : NHibernateRepositoryBase<AttachDocType>, IAttachDocTypeRepository, IRepository<AttachDocType>, System.IDisposable
	{
		public AttachDocTypeRepository(ISession session) : base(session)
		{
		}
		public System.Collections.Generic.IEnumerable<string> GetTypes(string groupName)
		{
			ICriteria criteria = this._session.CreateCriteria<AttachDocType>("doc");
			if (!string.IsNullOrEmpty(groupName))
			{
				criteria.Add(Restrictions.Eq("doc.Group", groupName));
			}
			criteria.SetCacheable(true);
			System.Collections.Generic.IList<AttachDocType> source = criteria.List<AttachDocType>();
			return 
				from a in source
				select a.Name;
		}
	}
}
