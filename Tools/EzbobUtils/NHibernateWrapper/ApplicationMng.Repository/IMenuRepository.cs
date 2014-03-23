using ApplicationMng.Model;
using System;
namespace ApplicationMng.Repository
{
	public interface IMenuRepository : IRepository<MenuItem>, System.IDisposable
	{
		MenuItem GetMenuItemByUrlAndSecApp(string url, SecurityApplication secApp);
		bool Check(int id, string caption, int secAppId);
	}
}
