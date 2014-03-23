using ApplicationMng.Model;
using ApplicationMng.Repository;
using NHibernate;
using NHibernateWrapper.StrategySchedule;
using System;
using System.Linq;
namespace NHibernateWrapper.NHibernate.Repository
{
	public class StrategyScheduleItemRepository : NHibernateRepositoryBase<StrategyScheduleItem>, IStrategyScheduleItemRepository, IRepository<StrategyScheduleItem>, System.IDisposable
	{
		public StrategyScheduleItemRepository(ISession session) : base(session)
		{
		}
		public StrategyScheduleItem[] GetScheduleItems()
		{
			this._session.SessionFactory.Evict(typeof(Strategy));
			return this.GetAll().ToArray<StrategyScheduleItem>();
		}
		public void UpdateNextRun(long itemId, System.DateTime nextRun)
		{
			StrategyScheduleItem scheduleItem = this.GetScheduleItem(itemId);
			if (scheduleItem != null)
			{
				scheduleItem.NextRun = nextRun;
				this.Update(scheduleItem);
			}
		}
		public void UpdateItem(long itemId, string name, ScheduleItemType type, string mask, System.DateTime nextRun, ExecutionType executionType)
		{
			StrategyScheduleItem scheduleItem = this.GetScheduleItem(itemId);
			if (scheduleItem != null)
			{
				scheduleItem.Name = name;
				scheduleItem.ScheduleType = type;
				scheduleItem.Mask = mask;
				scheduleItem.NextRun = nextRun;
				scheduleItem.ExecutionType = executionType;
				this.Update(scheduleItem);
			}
		}
		public long InsertItem(string name, int strategyId, ScheduleItemType type, string mask, System.DateTime nextRun, int? creatorUserId, ExecutionType executionType)
		{
			StrategyScheduleItem strategyScheduleItem = new StrategyScheduleItem
			{
				Name = name,
				Strategy = this._session.Load<Strategy>(strategyId),
				ScheduleType = type,
				Mask = mask,
				NextRun = nextRun,
				ExecutionType = executionType
			};
			if (creatorUserId.HasValue)
			{
				strategyScheduleItem.CreatorUser = this._session.Load<User>(creatorUserId);
			}
			return (long)this.Save(strategyScheduleItem);
		}
		public long InsertItem(StrategyScheduleItem item)
		{
			return (long)this.Save(item);
		}
		public void DeleteById(long itemId)
		{
			this.Delete(this._session.Load<StrategyScheduleItem>(itemId));
		}
		private StrategyScheduleItem GetScheduleItem(long itemId)
		{
			return (
				from item in this.GetAll()
				where item.Id == (long?)itemId
				select item).FirstOrDefault<StrategyScheduleItem>();
		}
	}
}
