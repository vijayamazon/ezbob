using ApplicationMng.Model;
using ApplicationMng.Repository;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
namespace NHibernateWrapper.NHibernate.Repository
{
	internal class AppStatusRepository : NHibernateRepositoryBase<AppStatus>, IAppStatusRepository, IRepository<AppStatus>, System.IDisposable
	{
		public AppStatusRepository(ISession session) : base(session)
		{
		}
		public void DeleteById(int id)
		{
			this.EnsureTransaction(() => this._session.Delete(this._session.Load<AppStatus>(id)));
		}
		public AppStatus GetByName(string name)
		{
			return this._session.CreateCriteria<AppStatus>().Add(Restrictions.Eq("Name", name)).UniqueResult<AppStatus>();
		}
		public bool CheckName(int id, string name)
		{
			return this.GetAll().Any((AppStatus s) => s.Id != id && s.Name.ToUpper() == name.ToUpper());
		}
		public System.Collections.Generic.ICollection<AppStatus> GetByNames(string[] statuses)
		{
			System.Collections.Generic.ICollection<AppStatus> result;
			if (statuses == null || statuses.Length == 0)
			{
				result = new System.Collections.Generic.List<AppStatus>();
			}
			else
			{
				result = (
					from s in this.GetAll()
					where statuses.Contains(s.Name)
					select s).ToArray<AppStatus>();
			}
			return result;
		}
	}
}
