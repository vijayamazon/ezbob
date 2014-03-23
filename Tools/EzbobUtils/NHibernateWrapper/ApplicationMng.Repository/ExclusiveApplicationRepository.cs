using ApplicationMng.Model;
using NHibernate;
using System;
using System.Linq;
namespace ApplicationMng.Repository
{
	public class ExclusiveApplicationRepository : NHibernateRepositoryBase<ExclusiveApplication>
	{
		public ExclusiveApplicationRepository(ISession session) : base(session)
		{
		}
		public ExclusiveApplication Add(long ownerApplicationId)
		{
			ExclusiveApplication exclusiveApplication = new ExclusiveApplication
			{
				ApplicationId = ownerApplicationId
			};
			this._session.Save(exclusiveApplication);
			this._session.Flush();
			return exclusiveApplication;
		}
		public void Delete(long ownerApplicationId)
		{
			foreach (ExclusiveApplication current in 
				from application in this.GetAll()
				where application.ApplicationId == ownerApplicationId
				select application)
			{
				this._session.Delete(current);
			}
			this._session.Flush();
		}
		public ExclusiveApplication GetCurrent()
		{
			return this.GetAll().FirstOrDefault<ExclusiveApplication>();
		}
	}
}
