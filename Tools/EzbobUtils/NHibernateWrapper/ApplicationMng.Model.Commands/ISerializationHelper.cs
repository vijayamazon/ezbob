using System;
namespace ApplicationMng.Model.Commands
{
	public interface ISerializationHelper
	{
		Strategy GetStrategy(string name);
		PublicName GetPublicName(int id);
		SecurityApplication GetSecApp(int id);
		User GetUser(int id);
		AppStatus GetStatus(int id);
	}
}
