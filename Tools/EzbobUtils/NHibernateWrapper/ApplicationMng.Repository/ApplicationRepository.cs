using ApplicationMng.Model;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Linq;
namespace ApplicationMng.Repository
{
	public class ApplicationRepository : NHibernateRepositoryBase<Application>, IApplicationRepository, IRepository<Application>, System.IDisposable
	{
		public ApplicationRepository(ISession session) : base(session)
		{
		}
		[System.Obsolete]
		public System.Linq.IQueryable<Application> GetAllApplications()
		{
			return this._session.Query<Application>();
		}
		public System.Linq.IQueryable<Application> GetApplicationsForUser(int userId)
		{
			return 
				from a in this._session.Query<Application>()
				where (int)a.State == 0 && a.Creator.Id == userId && a.ExecutionState != null
				select a;
		}
		[System.Obsolete]
		public Application GetApplicationById(long id)
		{
			return this._session.Get<Application>(id);
		}
		public bool StratagyIsRunning(int userId, string name)
		{
			return this.GetAll().Any((Application app) => app.Creator.Id == userId && app.Strategy.DisplayName == name && ((int)app.State == 0 || (int)app.State == 1 || (int)app.State == 4));
		}
	}
}
