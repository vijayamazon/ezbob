namespace EZBob.DatabaseLib.Model.Database.UserManagement
{
	using System;
	using ApplicationMng.Repository;
	using NHibernate;

	public class SessionRepository : NHibernateRepositoryBase<SecuritySession>
	{
		public SessionRepository(ISession session) : base(session)
		{
		}

		public void DeleteById(string sid)
		{
			_session.Delete(_session.Load<SecuritySession>(sid));
		}

		public SecuritySession CreateNewSession(int userId, int secAppId)
		{
			var securitySession = new SecuritySession
			{
				CreationDate = DateTime.UtcNow,
				SecApp = _session.Load<SecurityApplication>(secAppId),
				User = _session.Load<User>(userId),
				State = 1
			};

			EnsureTransaction(() =>
			{
				_session.CreateQuery("update EZBob.DatabaseLib.Model.Database.UserManagement.SecuritySession s set s.State = 0 where s.User.Id = :userId and s.SecApp.Id = :secAppId and s.State = 1").SetInt32("userId", userId).SetInt32("secAppId", secAppId).ExecuteUpdate();
				_session.Save(securitySession);
			});

			return securitySession;
		}
	}
}
