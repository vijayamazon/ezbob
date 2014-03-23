using ApplicationMng.Model;
using NHibernate;
using System;
using System.Linq;
namespace ApplicationMng.Repository
{
	internal class ControlHistoryRepository : NHibernateRepositoryBase<HistoryItem>, IControlHistoryRepository, IRepository<HistoryItem>, System.IDisposable
	{
		public ControlHistoryRepository(ISession session) : base(session)
		{
		}
		public System.Linq.IQueryable<HistoryItem> GetItems(long appId, string controlName)
		{
			return 
				from x in this.GetAll()
				where x.ControlName == controlName && x.App.Id == appId
				orderby x.ChangeTime descending
				select x;
		}
	}
}
