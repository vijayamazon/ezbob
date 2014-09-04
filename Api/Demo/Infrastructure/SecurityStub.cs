namespace Demo.Infrastructure {
	using System.Collections.Generic;
	using Ezbob.Logger;
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

		public string Login(LoginModel oModel, string sRemoteAddress) {
			if (oModel == null)
				return null;

			SessionToken oToken = null;

			if ((oModel.UserName == Const.UserNames.Admin) && (oModel.Password == "123456"))
				oToken = new SessionToken(oModel.UserName, sRemoteAddress);

			if ((oModel.UserName == Const.UserNames.User) && (oModel.Password == "654321"))
				oToken = new SessionToken(oModel.UserName, sRemoteAddress);

			if (oToken != null) {
				ms_oLog.Debug(
					"User '{0}' from '{1}' has got a token '{2}' ({3}).",
					oModel.UserName,
					sRemoteAddress,
					oToken.Packed,
					oToken.Encoded
				);

				ms_oTokens.Add(oToken.Encoded);
				return oToken.Encoded;
			} // if

			return null;
		} // Login

		public ActiveUserInfo ValidateSessionToken(string sToken, string sRemoteAddress) {
			if (!ms_oTokens.Contains(sToken))
				return new ActiveUserInfo { TokenValidity = TokenValidity.Invalid, };

			SessionToken oToken = SessionToken.Deserialize(sToken);

			if (oToken == null)
				return new ActiveUserInfo { TokenValidity = TokenValidity.Invalid, };

			ms_oLog.Debug(
				"Token '{0}' has been decoded to '{1}'.",
				sToken,
				oToken.Packed
			);

			if (sRemoteAddress != oToken.RemoteAddress) {
				ms_oLog.Alert(
					"Token's remote address '{0}' differs from actual remote address '{1}'.",
					oToken.RemoteAddress,
					sRemoteAddress
					);

				return new ActiveUserInfo { TokenValidity = TokenValidity.Invalid, };
			} // if

			ms_oLog.Debug("Token's '{0}' remote address matches.", oToken.Packed);

			// TODO: other checks: expiration time...

			var oRes = new ActiveUserInfo {
				TokenValidity = TokenValidity.Valid,
				UserName = oToken.UserName,
			};

			ms_oLog.Debug("Token '{0}' is valid.", oToken.Packed);

			return oRes;
		} // ValidateSessionToken

		public bool IsActionEnabled(string sUserName, Action nAction) {
			return (nAction == Action.Read) || (sUserName == Const.UserNames.Admin);
		} // IsActionEnabled

		private static readonly SortedSet<string> ms_oTokens = new SortedSet<string>();

		private static readonly ASafeLog ms_oLog = new SafeILog(typeof(SecurityStub));
	} // class SecurityStub
} // namespace