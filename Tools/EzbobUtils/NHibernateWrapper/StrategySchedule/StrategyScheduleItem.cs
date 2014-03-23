using ApplicationMng.Model;
using Iesi.Collections.Generic;
using NHibernateWrapper.NHibernate.Model;
using System;
namespace NHibernateWrapper.StrategySchedule
{
	public class StrategyScheduleItem
	{
		public virtual long? Id
		{
			get;
			set;
		}
		public virtual string Name
		{
			get;
			set;
		}
		public virtual ScheduleItemType ScheduleType
		{
			get;
			set;
		}
		public virtual ExecutionType ExecutionType
		{
			get;
			set;
		}
		public virtual string Mask
		{
			get;
			set;
		}
		public virtual System.DateTime NextRun
		{
			get;
			set;
		}
		public virtual User CreatorUser
		{
			get;
			set;
		}
		public virtual Strategy Strategy
		{
			get;
			set;
		}
		public virtual bool IsPaused
		{
			get;
			set;
		}
		public virtual ISet<StrategyScheduleParam> StrategyScheduledInputs
		{
			get;
			set;
		}
		public StrategyScheduleItem()
		{
			this.StrategyScheduledInputs = new HashedSet<StrategyScheduleParam>();
		}
	}
}
