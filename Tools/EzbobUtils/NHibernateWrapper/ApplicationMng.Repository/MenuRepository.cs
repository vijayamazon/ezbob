using ApplicationMng.Model;
using NHibernate;
using NHibernate.Criterion;
using System;
using System.Linq;
namespace ApplicationMng.Repository
{
	public class MenuRepository : NHibernateRepositoryBase<MenuItem>, IMenuRepository, IRepository<MenuItem>, System.IDisposable
	{
		public MenuRepository(ISession session) : base(session)
		{
		}
		public MenuItem GetMenuItemByUrlAndSecApp(string url, SecurityApplication secApp)
		{
			MenuItem menuItem = this._session.CreateCriteria<MenuItem>("m").Add(Restrictions.Eq("m.Url", url)).Add(Restrictions.Eq("m.SecApp.Id", secApp.Id)).SetCacheable(true).SetCacheRegion("LongTerm").SetMaxResults(1).UniqueResult<MenuItem>();
			MenuItem result;
			if (menuItem != null)
			{
				result = menuItem;
			}
			else
			{
				result = secApp.MenuItems.FirstOrDefault((MenuItem mi) => mi.Path == url);
			}
			return result;
		}
		public bool Check(int id, string caption, int secAppId)
		{
			MenuItem menuItem2 = this.Get(id);
			MenuItem menuItem = (menuItem2 == null) ? null : menuItem2.Parent;
			return this.GetAll().Any((MenuItem m) => m.Id != id && m.Caption.ToUpper() == caption.ToUpper() && m.SecApp.Id == secAppId && m.Parent == menuItem);
		}
	}
}
