namespace EzBob.Web.Infrastructure.Email {
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.UserManagement;
	using Ezbob.Backend.Models;
	using StructureMap;

	public static class EmailConfirmationState {
		public static string Get(Customer oCustomer) {
			if (oCustomer == null)
				return EmailConfirmationRequestState.Unknown.ToString();

			IUsersRepository oUsers = ObjectFactory.GetInstance<IUsersRepository>();

			var oUser = oUsers.GetAll().FirstOrDefault(x => x.Id == oCustomer.Id);

			if (oUser == null)
				return EmailConfirmationRequestState.Unknown.ToString();

			if (!oUser.EmailStateID.HasValue)
				return EmailConfirmationRequestState.Unknown.ToString();

			if (oUser.EmailStateID.Value < 0)
				return EmailConfirmationRequestState.Unknown.ToString();

			if (oUser.EmailStateID.Value >= (int)EmailConfirmationRequestState._MAX_)
				return EmailConfirmationRequestState.Unknown.ToString();

			return ((EmailConfirmationRequestState)oUser.EmailStateID.Value).ToString();
		} // Get
	} // class EmailConfirmationState
} // namespace