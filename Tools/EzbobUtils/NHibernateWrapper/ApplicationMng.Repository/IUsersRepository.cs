using ApplicationMng.Model;
using System;
using System.Collections.Generic;
using System.Linq;
namespace ApplicationMng.Repository
{
	public interface IUsersRepository : IRepository<User>, System.IDisposable
	{
		User GetUserByLogin(string login);
		bool IsUserInRole(int userId, string role);
		bool CheckUserLogin(int userId, string userName);
		bool CheckUserDomainName(int userId, string domainUserName);
		void BlockUser(User user);
		void DisableSessionsAndApplications(User user);
		void UnBlockUser(User user);
		System.Linq.IQueryable<User> GetActiveUsers();
		System.Collections.Generic.IList<User> GetUsers(int page, int itemsPerPage, string query);
		long GetUsersCount(string query);
		bool HasAccesTo(int userId, int appId);
		void EvictAll();
		void Refresh(User user);
	}
}
