namespace EZBob.DatabaseLib.Model.Database.UserManagement
{
	using ApplicationMng.Repository;
	using NHibernate;
	using System.Collections.Generic;

	public interface ISecurityQuestionRepository : IRepository<SecurityQuestion>
	{
		IList<SecurityQuestion> GetQuestions();
	}

	public class SecurityQuestionRepository : NHibernateRepositoryBase<SecurityQuestion>, ISecurityQuestionRepository
	{
		public SecurityQuestionRepository(ISession session) : base(session)
		{
		}
		public IList<SecurityQuestion> GetQuestions()
		{
			return (
				from q in Session.QueryOver<SecurityQuestion>()
				where q.Name != null
				select q).Cacheable().CacheRegion("Longest").List();
		}
	}
}
