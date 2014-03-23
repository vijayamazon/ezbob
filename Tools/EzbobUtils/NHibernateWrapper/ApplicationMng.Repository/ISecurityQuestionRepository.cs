using NHibernateWrapper.NHibernate.Model;
using System;
using System.Collections.Generic;
namespace ApplicationMng.Repository
{
	public interface ISecurityQuestionRepository : IRepository<SecurityQuestion>, System.IDisposable
	{
		System.Collections.Generic.IList<SecurityQuestion> GetQuestions();
	}
}
