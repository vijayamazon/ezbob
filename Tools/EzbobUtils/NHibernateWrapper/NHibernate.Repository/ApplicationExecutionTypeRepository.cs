using ApplicationMng.Model;
using ApplicationMng.Repository;
using NHibernate;
using NHibernateWrapper.NHibernate.Model;
using NHibernateWrapper.StrategySchedule;
using System;
using System.Collections.Generic;
using System.Linq;
namespace NHibernateWrapper.NHibernate.Repository
{
	public class ApplicationExecutionTypeRepository : NHibernateRepositoryBase<ApplicationExecutionTypeItem>
	{
		private static readonly object LockObjet = new object();
		public ApplicationExecutionTypeRepository(ISession session) : base(session)
		{
		}
		public bool IsActiveApplicationLinkedWithExecutingType(long itemId)
		{
			return this.GetExecutingTypeLinkedWithActiveApplication(itemId) != null;
		}
		public ApplicationExecutionTypeItem GetExecutingTypeLinkedWithActiveApplication(long itemId)
		{
			ApplicationExecutionTypeItem result;
			lock (ApplicationExecutionTypeRepository.LockObjet)
			{
				result = this._session.CreateQuery("from NHibernateWrapper.NHibernate.Model.ApplicationExecutionTypeItem ati\r\n                      where ati.Id = :id \r\n                        and ati.ApplicationId  in \r\n                            (select app.Id \r\n                             from ApplicationMng.Model.Application app \r\n                             where app.State = :needProcessByNode or app.State = :needProcessBySE )").SetInt64("id", itemId).SetEnum("needProcessBySE", ApplicationStrategyState.NeedProcessBySE).SetEnum("needProcessByNode", ApplicationStrategyState.NeedProcessByNode).UniqueResult<ApplicationExecutionTypeItem>();
			}
			return result;
		}
		public void Add(ExecutionType executionType, long itemId, long applicationId)
		{
			if (executionType != (ExecutionType)0)
			{
				lock (ApplicationExecutionTypeRepository.LockObjet)
				{
					if (!this.GetAll().Any((ApplicationExecutionTypeItem item) => item.ApplicationId == applicationId))
					{
						this.Save(new ApplicationExecutionTypeItem
						{
							ApplicationId = applicationId,
							ItemId = itemId,
							ExecutionType = executionType
						});
						this._session.Flush();
					}
				}
			}
		}
		public ApplicationExecutionTypeItem Get(long applicationId, ExecutionType executionType)
		{
			ApplicationExecutionTypeItem result;
			lock (ApplicationExecutionTypeRepository.LockObjet)
			{
				result = this.GetAll(applicationId, executionType).FirstOrDefault<ApplicationExecutionTypeItem>();
			}
			return result;
		}
		public System.Collections.Generic.List<ApplicationExecutionTypeItem> GetAll(long applicationId, ExecutionType executionType)
		{
			System.Collections.Generic.List<ApplicationExecutionTypeItem> result;
			lock (ApplicationExecutionTypeRepository.LockObjet)
			{
				result = (
					from typeItem in (
						from item in this.GetAll()
						where item.ApplicationId == applicationId
						select item).ToList<ApplicationExecutionTypeItem>()
					where (typeItem.ExecutionType & executionType) == executionType
					select typeItem).ToList<ApplicationExecutionTypeItem>();
			}
			return result;
		}
	}
}
