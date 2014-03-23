using NHibernate;
using NHibernateWrapper.Web;
using System;
using System.Linq;
namespace ApplicationMng.Model.Filters
{
	internal class AllAppsFilter : FilterBase
	{
		public override System.Linq.IQueryable<Application> ApplyFilter(System.Linq.IQueryable<Application> applications, IWorkplaceContext context)
		{
			return applications;
		}
		public override void ApplyFilter(ICriteria applications, IWorkplaceContext context)
		{
		}
	}
}
