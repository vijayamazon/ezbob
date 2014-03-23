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
		public abstract System.Linq.IQueryable<Application> ApplyFilter(System.Linq.IQueryable<Application> applications, IWorkplaceContext context);
		public virtual void ApplyFilter(ICriteria applications, IWorkplaceContext context)
		{
			throw new System.NotImplementedException();
		}
	}
}
