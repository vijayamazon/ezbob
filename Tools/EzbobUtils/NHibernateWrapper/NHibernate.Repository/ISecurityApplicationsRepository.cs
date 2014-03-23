using ApplicationMng.Model;
using ApplicationMng.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
namespace NHibernateWrapper.NHibernate.Repository
{
	public interface ISecurityApplicationsRepository : IRepository<ApplicationMng.Model.SecurityApplication>, System.IDisposable
	{
		System.Linq.IQueryable<ApplicationMng.Model.SecurityApplication> GetWebApplications();
		System.Collections.Generic.IEnumerable<ApplicationMng.Model.SecurityApplication> GetWebApplicationsWithMenuItems();
	}
}
