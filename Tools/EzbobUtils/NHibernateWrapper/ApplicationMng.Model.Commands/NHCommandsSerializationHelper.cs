using ApplicationMng.Repository;
using System;
using System.Linq;
namespace ApplicationMng.Model.Commands
{
	public class NHCommandsSerializationHelper : ISerializationHelper
	{
		private readonly IRepository<SecurityApplication> _appRepo;
		private readonly IRepository<User> _userRepo;
		private readonly IRepository<AppStatus> _statusDescriptionRepo;
		public NHCommandsSerializationHelper(IRepository<SecurityApplication> appRepo, IRepository<User> userRepo, IRepository<AppStatus> statusDescriptionRepo)
		{
			this._statusDescriptionRepo = statusDescriptionRepo;
			this._userRepo = userRepo;
			this._appRepo = appRepo;
		}
		public SecurityApplication GetSecApp(int id)
		{
			SecurityApplication result;
			if (id < 0)
			{
				result = null;
			}
			else
			{
				result = this._appRepo.Get(id);
			}
			return result;
		}
		public User GetUser(int id)
		{
			User result;
			if (id < 0)
			{
				result = null;
			}
			else
			{
				result = this._userRepo.Get(id);
			}
			return result;
		}
		public AppStatus GetStatus(int id)
		{
			AppStatus result;
			if (id < 0)
			{
				result = null;
			}
			else
			{
				result = this._statusDescriptionRepo.Get(id);
			}
			return result;
		}
	}
}
