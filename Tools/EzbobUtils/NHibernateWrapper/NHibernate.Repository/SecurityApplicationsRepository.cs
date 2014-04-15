using ApplicationMng.Model;
using ApplicationMng.Repository;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
namespace NHibernateWrapper.NHibernate.Repository
{
	public class SecurityApplicationsRepository : NHibernateRepositoryBase<ApplicationMng.Model.SecurityApplication>, ISecurityApplicationsRepository, IRepository<ApplicationMng.Model.SecurityApplication>, System.IDisposable
	{
		public SecurityApplicationsRepository(ISession session) : base(session)
		{
		}
		public System.Linq.IQueryable<ApplicationMng.Model.SecurityApplication> GetWebApplications()
		{
			return 
				from a in this.GetAll()
				where (int)a.ApplicationType == 1
				select a;
		}
		public bool CheckName(int id, string name)
		{
			return this.GetAll().Any((ApplicationMng.Model.SecurityApplication s) => s.Id != id && s.Name.ToUpper() == name.ToUpper());
		}
	}
}
