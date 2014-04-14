using NHibernate;
using NHibernateWrapper.Web;
using System;
using System.Linq;
namespace ApplicationMng.Model.Filters
{
	public abstract class FilterBase
	{
		public virtual int Id
		{
			get;
			set;
		}
		public virtual string Name
		{
			get;
			set;
		}
		public virtual void ApplyFilter(ICriteria applications, IWorkplaceContext context)
		{
			throw new System.NotImplementedException();
		}
	}
}
