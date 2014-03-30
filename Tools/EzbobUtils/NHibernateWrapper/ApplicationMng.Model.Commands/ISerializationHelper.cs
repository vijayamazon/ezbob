using System;
namespace ApplicationMng.Model.Commands
{
	public interface ISerializationHelper
	{
		SecurityApplication GetSecApp(int id);
		User GetUser(int id);
		AppStatus GetStatus(int id);
	}
}
