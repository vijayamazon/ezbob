namespace EZBob.DatabaseLib.Model.Database.UserManagement {
	using System;
	using ApplicationMng.Repository;
	using NHibernate;
	using NHibernate.Linq;
	using System.Linq;
	using Ezbob.Logger;

	public interface IUsersRepository : IRepository<User> {
		User GetUserByLogin(string login, int? originID);
		User GetUserByLogin(string login, CustomerOriginEnum originID);
		int ExternalUserCount(string login);
	} // interface IUsersRepository

	public class UsersRepository : NHibernateRepositoryBase<User>, IUsersRepository {
		public UsersRepository(ISession session) : base(session) { }

		public User GetUserByLogin(string login, CustomerOriginEnum originID) {
			return GetUserByLogin(login, (int)originID);
		} // GetUserByLogin

		public User GetUserByLogin(string login, int? originID) {
			User user = null;

			try {
				EnsureTransaction(() => {
					user = GetAll()
						.Where(u => u.Name == login && u.OriginID == originID && u.IsDeleted == 0)
						.Cacheable<User>()
						.CacheRegion("Longest")
						.SingleOrDefault();
				});
			} catch (Exception ex) {
				Log.Error(
					ex,
					"Failed to retrieve user by login {0} and origin {1}.",
					login,
					originID.HasValue ? originID.Value.ToString() : "-- null --"
				);
			} // try
			
			return user;
		} // GetUserByLogin

		public int ExternalUserCount(string login) {
			return GetAll().Count(u => u.Name == login && u.OriginID != null);
		} // ExternalUserCount

		protected static ASafeLog Log = new SafeILog(typeof(UsersRepository));
	} // class UsersRepository
} // namespace
