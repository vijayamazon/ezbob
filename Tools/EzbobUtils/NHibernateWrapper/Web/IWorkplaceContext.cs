using ApplicationMng.Model;
using System;
namespace NHibernateWrapper.Web
{
	public interface IWorkplaceContext
	{
		SecurityApplication SecApp
		{
			get;
		}
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
