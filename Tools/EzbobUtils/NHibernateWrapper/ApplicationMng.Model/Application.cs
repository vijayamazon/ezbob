using Iesi.Collections.Generic;
using NHibernateWrapper.NHibernate.Model;
using System;
namespace ApplicationMng.Model
{
	public class Application
	{
		public virtual long Id
		{
			get;
			set;
		}
		public virtual int Version
		{
			get;
			set;
		}
		public virtual User Locker
		{
			get;
			set;
		}
		public virtual ApplicationStrategyState State
		{
			get;
			set;
		}
		public virtual User Creator
		{
			get;
			set;
		}
		public virtual System.DateTime CreationDate
		{
			get;
			set;
		}
		public virtual bool IsTimeLimitExceeded
		{
			get;
			set;
		}
		public virtual long AppCounter
		{
			get;
			set;
		}
		public virtual ExecutionState ExecutionState
		{
			get;
			set;
		}
		public virtual int? ParentAppID
		{
			get;
			set;
		}
		public virtual byte[] ExecutionPathBin
		{
			get;
			set;
		}
		public Application()
		{
		}
	}
}
