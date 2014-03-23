using ApplicationMng.Model;
using ApplicationMng.Repository;
using NHibernate;
using NHibernate.Linq;
using NHibernateWrapper.NHibernate.Model;
using System;
using System.Linq;
namespace NHibernateWrapper.NHibernate.Repository
{
	public class SingleRunApplicationRepository : NHibernateRepositoryBase<SingleRunApplication>
	{
		public SingleRunApplicationRepository(ISession session) : base(session)
		{
		}
		public SingleRunApplication Add(long ownerApplicationId)
		{
			ApplicationExecutionTypeItem executionTypeItem = this._session.Query<ApplicationExecutionTypeItem>().SingleOrDefault((ApplicationExecutionTypeItem et) => et.ApplicationId == ownerApplicationId);
			SingleRunApplication singleRunApplication = new SingleRunApplication
			{
				ExecutionTypeItem = executionTypeItem
			};
			this._session.Save(singleRunApplication);
			this._session.Flush();
			return singleRunApplication;
		}
		public void Delete(long ownerApplicationId)
		{
			foreach (SingleRunApplication current in this.SingleRunApplications(ownerApplicationId))
			{
				this._session.Delete(current);
			}
			this._session.Flush();
		}
		public SingleRunApplication Get(long ownerApplicationId)
		{
			return this.SingleRunApplications(ownerApplicationId).FirstOrDefault<SingleRunApplication>();
		}
		private System.Linq.IQueryable<SingleRunApplication> SingleRunApplications(long ownerApplicationId)
		{
			return 
				from application in this.GetAll()
				where application.ExecutionTypeItem.ApplicationId == ownerApplicationId
				select application;
		}
	}
}
