using NHibernateWrapper.NHibernate.Model;
using System;
namespace ApplicationMng.Model
{
	public class SingleRunApplication
	{
		public virtual long? Id
		{
			get;
			set;
		}
		public virtual ApplicationExecutionTypeItem ExecutionTypeItem
		{
			get;
			set;
		}
	}
}
