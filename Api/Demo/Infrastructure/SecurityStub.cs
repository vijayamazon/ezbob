namespace Demo.Infrastructure {
	using System.Collections.Generic;
	using Models;

	internal enum TokenValidity {
		Valid,
		Expired,
		Invalid,
	} // enum TokenValidity

	internal enum Action {
		Create,
		Edit,
		Delete,
	} // enum Action

	internal class SecurityStub {
		public bool IsAppKeyValid(string sAppKey) {
			return sAppKey == "1234";
		} // IsAppKeyValid

		public string Login(LoginModel oModel) {
			if (oModel == null)
				return null;

			string sToken = null;

			if ((oModel.UserName == "admin") && (oModel.Password == "123456"))
				sToken = AdminToken;

			if ((oModel.UserName == "user") && (oModel.Password == "654321"))
				sToken = UserToken;

			if (sToken != null)
				ms_oTokens.Add(sToken);

			return sToken;
		} // Login

		public TokenValidity ValidateSessionToke(string sToken) {
			if (!ms_oTokens.Contains(sToken))
				return TokenValidity.Invalid;

			// TODO: other checks: remote IP, expiration time...

			return TokenValidity.Valid;
		} // ValidateSessionToken

		public bool IsActionEnabled(string sToken, Action nAction) {
			return sToken == AdminToken;
		} // IsActionEnabled

		private const string UserToken = "user-token";
		private const string AdminToken = "admin-token";

		private static readonly SortedSet<string> ms_oTokens = new SortedSet<string>();
	} // class SecurityStub
} // namespace