using ApplicationMng.Repository;
using System;
using System.Linq;
namespace ApplicationMng.Model.Commands
{
	public class NHCommandsSerializationHelper : ISerializationHelper
	{
		private readonly IRepository<Strategy> _strategyRepo;
		private readonly IRepository<PublicName> _publicNameRepo;
		private readonly IRepository<SecurityApplication> _appRepo;
		private readonly IRepository<User> _userRepo;
		private readonly IRepository<AppStatus> _statusDescriptionRepo;
		public NHCommandsSerializationHelper(IRepository<Strategy> strategyRepo, IRepository<PublicName> publicNameRepo, IRepository<SecurityApplication> appRepo, IRepository<User> userRepo, IRepository<AppStatus> statusDescriptionRepo)
		{
			this._strategyRepo = strategyRepo;
			this._statusDescriptionRepo = statusDescriptionRepo;
			this._userRepo = userRepo;
			this._appRepo = appRepo;
			this._publicNameRepo = publicNameRepo;
		}
		public Strategy GetStrategy(string name)
		{
			Strategy result;
			if (string.IsNullOrEmpty(name))
			{
				result = null;
			}
			else
			{
				result = this._strategyRepo.GetAll().Single((Strategy s) => s.DisplayName == name && s.IsDeleted == (int?)0);
			}
			return result;
		}
		public PublicName GetPublicName(int id)
		{
			PublicName result;
			if (id < 0)
			{
				result = null;
			}
			else
			{
				result = this._publicNameRepo.Get(id);
			}
			return result;
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
