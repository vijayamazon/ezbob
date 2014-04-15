namespace EzBob.Web.Infrastructure
{
	using ApplicationMng.Model;

	public interface IWorkplaceContext
	{
		int SecAppId
		{
			get;
		}
		User User
		{
			get;
		}
		int UserId
		{
			get;
		}
		string SessionId
		{
			get;
			set;
		}
	}
}
