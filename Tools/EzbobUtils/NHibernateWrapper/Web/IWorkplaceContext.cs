using ApplicationMng.Model;
using System;
namespace NHibernateWrapper.Web
{
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
