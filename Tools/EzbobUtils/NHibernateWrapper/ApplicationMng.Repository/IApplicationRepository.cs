using ApplicationMng.Model;
using System;
using System.Linq;
namespace ApplicationMng.Repository
{
	public interface IApplicationRepository : IRepository<Application>, System.IDisposable
	{
		System.Linq.IQueryable<Application> GetAllApplications();
		System.Linq.IQueryable<Application> GetApplicationsForUser(int userId);
		Application GetApplicationById(long id);
		void EvictAll();
		bool StratagyIsRunning(int userId, string name);
	}
}
