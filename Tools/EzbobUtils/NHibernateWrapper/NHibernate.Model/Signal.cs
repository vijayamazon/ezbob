using ApplicationMng.Model;
using System;
namespace NHibernateWrapper.NHibernate.Model
{
	public class Signal
	{
		public virtual long Id
		{
			get;
			set;
		}
		public virtual long Priority
		{
			get;
			set;
		}
		public virtual long OwnerApplicationId
		{
			get;
			set;
		}
		public virtual Application App
		{
			get;
			set;
		}
		public virtual byte[] Message
		{
			get;
			set;
		}
	}
}
