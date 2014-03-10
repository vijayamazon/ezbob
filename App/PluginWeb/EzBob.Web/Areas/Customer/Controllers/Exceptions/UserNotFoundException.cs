using System;

namespace EzBob.Web.Areas.Customer.Controllers.Exceptions
{
	[Serializable]
	public class UserNotFoundException : ApplicationException
	{
		public UserNotFoundException(string userName)
			: base(string.Format("UserNotFound {0}", userName))
		{
		}
	}
}