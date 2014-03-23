using NHibernate;
using NHibernateWrapper.NHibernate.Model;
using System;
using System.Collections.Generic;
namespace ApplicationMng.Repository
{
	public class SecurityQuestionRepository : NHibernateRepositoryBase<SecurityQuestion>, ISecurityQuestionRepository, IRepository<SecurityQuestion>, System.IDisposable
	{
		public SecurityQuestionRepository(ISession session) : base(session)
		{
		}
		public System.Collections.Generic.IList<SecurityQuestion> GetQuestions()
		{
			return (
				from q in this._session.QueryOver<SecurityQuestion>()
				where q.Name != null
				select q).Cacheable().CacheRegion("Longest").List();
		}
	}
}
