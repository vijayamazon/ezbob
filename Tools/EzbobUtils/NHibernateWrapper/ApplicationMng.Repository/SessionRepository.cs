using ApplicationMng.Model;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
namespace ApplicationMng.Repository
{
	public class SessionRepository : NHibernateRepositoryBase<SecuritySession>
	{
		public SessionRepository(ISession session) : base(session)
		{
		}
		public void DeleteById(string sid)
		{
			this._session.Delete(this._session.Load<SecuritySession>(sid));
		}
		public SecuritySession CreateNewSession(int userId, int secAppId)
		{
			SecuritySession securitySession = new SecuritySession
			{
				CreationDate = System.DateTime.Now,
				SecApp = this._session.Load<SecurityApplication>(secAppId),
				User = this._session.Load<User>(userId),
				State = 1
			};
			this.EnsureTransaction(() =>
			{
				this._session.CreateQuery("update ApplicationMng.Model.SecuritySession s set s.State = 0 where s.User.Id = :userId and s.SecApp.Id = :secAppId and s.State = 1").SetInt32("userId", userId).SetInt32("secAppId", secAppId).ExecuteUpdate();
				this._session.Save(securitySession);
			});
			return securitySession;
		}
		public void DisableOldSessions(System.TimeSpan period)
		{
			System.DateTime maxTime = System.DateTime.Now.Subtract(period);
			this.EnsureTransaction(() =>
			{
				this._session.CreateQuery("update ApplicationMng.Model.SecuritySession s set s.State = 0 where s.LastAccessTime < :maxTime and s.State = 1").SetDateTime("maxTime", maxTime).ExecuteUpdate();
			});
		}
		public System.Linq.IQueryable<SecuritySession> GetExpiredSessions(System.TimeSpan period)
		{
			System.DateTime maxTime = System.DateTime.Now.Subtract(period);
			return 
				from session in this.GetAll()
				where session.State == 1 && session.LastAccessTime < maxTime
				select session;
		}
		public System.Collections.Generic.IList<SecuritySession> GetActiveSessionsHyperPage(int page, int itemsPerPage, string hqlQuery)
		{
			IQuery query = this._session.CreateQuery("from ApplicationMng.Model.SecuritySession" + hqlQuery).SetFirstResult(page * itemsPerPage);
			if (itemsPerPage > 0)
			{
				query.SetMaxResults(itemsPerPage);
			}
			return query.List<SecuritySession>();
		}
		public long GetActiveSessionsCount(string query)
		{
			return this._session.CreateQuery("select count(s.id) from ApplicationMng.Model.SecuritySession s" + query).UniqueResult<long>();
		}
		public System.Linq.IQueryable<SecuritySession> GetActiveSessionsByUserId(int userId)
		{
			return 
				from s in this.GetAll()
				where s.User.Id == userId && s.State == 1
				select s;
		}
	}
}
