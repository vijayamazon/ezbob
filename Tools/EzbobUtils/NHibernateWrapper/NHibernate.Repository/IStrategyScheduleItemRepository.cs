using ApplicationMng.Repository;
using NHibernateWrapper.StrategySchedule;
using System;
namespace NHibernateWrapper.NHibernate.Repository
{
	public interface IStrategyScheduleItemRepository : IRepository<StrategyScheduleItem>, System.IDisposable
	{
		void DeleteById(long itemId);
		StrategyScheduleItem[] GetScheduleItems();
		void UpdateNextRun(long itemId, System.DateTime nextRun);
		void UpdateItem(long itemId, string name, ScheduleItemType type, string mask, System.DateTime nextRun, ExecutionType executionType);
		long InsertItem(string name, int strategyId, ScheduleItemType type, string mask, System.DateTime nextRun, int? creatorUserId, ExecutionType executionType);
		long InsertItem(StrategyScheduleItem item);
	}
}
