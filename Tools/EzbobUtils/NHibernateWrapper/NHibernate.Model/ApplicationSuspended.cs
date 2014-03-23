using System;
namespace NHibernateWrapper.NHibernate.Model
{
	public class ApplicationSuspended
	{
		public virtual long ApplicationId
		{
			get;
			set;
		}
		public virtual string ExecutionState
		{
			get;
			set;
		}
		public virtual string Postfix
		{
			get;
			set;
		}
		public virtual string Target
		{
			get;
			set;
		}
		public virtual string Label
		{
			get;
			set;
		}
		public virtual byte[] Message
		{
			get;
			set;
		}
		public virtual long? AppSpecific
		{
			get;
			set;
		}
		public virtual System.DateTime Date
		{
			get;
			set;
		}
		public virtual int? ExecutionType
		{
			get;
			set;
		}
		public virtual long ExecutionPathCurrentItemId
		{
			get;
			set;
		}
	}
}
