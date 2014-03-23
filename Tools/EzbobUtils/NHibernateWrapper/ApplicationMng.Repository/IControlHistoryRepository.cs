using ApplicationMng.Model;
using System;
using System.Linq;
namespace ApplicationMng.Repository
{
	public interface IControlHistoryRepository : IRepository<HistoryItem>, System.IDisposable
	{
		System.Linq.IQueryable<HistoryItem> GetItems(long appId, string controlName);
	}
}
