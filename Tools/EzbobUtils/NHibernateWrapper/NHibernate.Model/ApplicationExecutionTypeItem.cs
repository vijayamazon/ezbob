using NHibernateWrapper.StrategySchedule;
using System;
namespace NHibernateWrapper.NHibernate.Model
{
	public class ApplicationExecutionTypeItem
	{
		public virtual long? Id
		{
			get;
			set;
		}
		public virtual long ItemId
		{
			get;
			set;
		}
		public virtual ExecutionType ExecutionType
		{
			get;
			set;
		}
		public virtual long ApplicationId
		{
			get;
			set;
		}
	}
}
