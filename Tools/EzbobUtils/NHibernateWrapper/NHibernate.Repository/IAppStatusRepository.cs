using ApplicationMng.Model;
using ApplicationMng.Repository;
using System;
using System.Collections.Generic;
namespace NHibernateWrapper.NHibernate.Repository
{
	public interface IAppStatusRepository : IRepository<AppStatus>, System.IDisposable
	{
		void DeleteById(int id);
		AppStatus GetByName(string name);
		bool CheckName(int id, string name);
		System.Collections.Generic.ICollection<AppStatus> GetByNames(string[] statuses);
	}
}
