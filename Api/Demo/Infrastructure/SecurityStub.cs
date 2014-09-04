namespace Demo.Infrastructure {
	using System.Collections.Generic;
	using Models;

	internal enum TokenValidity {
		Valid,
		Expired,
		Invalid,
	} // enum TokenValidity

	internal enum Action {
		Read,
		Create,
		Edit,
		Delete,
	} // enum Action

	internal struct ActiveUserInfo {
		public string UserName;
		public TokenValidity TokenValidity;
	} // struct ActiveUserInfo

	internal class SecurityStub {
		public bool IsAppKeyValid(string sAppKey) {
			return sAppKey == "1234";
		} // IsAppKeyValid

		public string Login(LoginModel oModel) {
			if (oModel == null)
				return null;

			string sToken = null;

			if ((oModel.UserName == Const.UserNames.Admin) && (oModel.Password == "123456"))
				sToken = Const.AdminToken;

			if ((oModel.UserName == Const.UserNames.User) && (oModel.Password == "654321"))
				sToken = Const.UserToken;

			if (sToken != null)
				ms_oTokens.Add(sToken);

			return sToken;
		} // Login

		public ActiveUserInfo ValidateSessionToken(string sToken) {
			if (!ms_oTokens.Contains(sToken))
				return new ActiveUserInfo { TokenValidity = TokenValidity.Invalid, };

			// TODO: other checks: remote IP, expiration time...

			var oRes = new ActiveUserInfo {
				TokenValidity = TokenValidity.Valid,
			};

			if (sToken == Const.AdminToken)
				oRes.UserName = Const.UserNames.Admin;
			else if (sToken == Const.UserToken)
				oRes.UserName = Const.UserNames.User;

			return oRes;
		} // ValidateSessionToken

		public bool IsActionEnabled(string sUserName, Action nAction) {
			return (nAction == Action.Read) || (sUserName == Const.UserNames.Admin);
		} // IsActionEnabled

		private static readonly SortedSet<string> ms_oTokens = new SortedSet<string>();
	} // class SecurityStub
} // namespace