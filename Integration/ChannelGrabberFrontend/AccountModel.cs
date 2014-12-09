namespace Integration.ChannelGrabberFrontend {
	using System;
	using EZBob.DatabaseLib.Common;
	using EZBob.DatabaseLib.DatabaseWrapper;
	using EZBob.DatabaseLib.Model.Database;
	using Integration.ChannelGrabberConfig;

	using Ezbob.Utils.Security;
	using Ezbob.Utils.Serialization;

	/// <summary>
	/// This class is used to communicate with UI using json.
	/// </summary>
	public class AccountModel : IMarketPlaceSecurityInfo {

		public static AccountModel ToModel(MP_CustomerMarketPlace account) {
			try {
				var m = Serialized.Deserialize<AccountModel>(Encrypted.Decrypt(account.SecurityData));
				m.id = account.Id;
				return m;
			}
			catch (Exception e) {
				throw new ApiException(string.Format("Failed to de-serialise security data for marketplace {0} ({1})", account.DisplayName, account.Id), e);
			}
		} // ToModel

		public static AccountModel ToModel(IDatabaseCustomerMarketPlace account) {
			try {
				var m = Serialized.Deserialize<AccountModel>(Encrypted.Decrypt(account.SecurityData));
				m.id = account.Id;
				return m;
			}
			catch (Exception e) {
				throw new ApiException(string.Format("Failed to de-serialise security data for marketplace {0} ({1})", account.DisplayName, account.Id), e);
			}
		} // ToModel

		public int id { get; set; }
		public string name { get; set; }
		public string url { get; set; }
		public string login { get; set; }
		public string password { get; set; }
		public int limitDays { get; set; }
		public string auxLogin { get; set; }
		public string auxPassword { get; set; }
		public int realmId { get; set; }
		public string accountTypeName { get; set; }

		public string displayName { get; set; }

		public AccountData Fill() {
			var oData = new AccountData(Configuration.Instance.GetVendorInfo(accountTypeName)) {
				Login = login,
				Password = password,
				Name = name,
				URL = url,
				LimitDays = limitDays,
				AuxLogin = auxLogin,
				AuxPassword = auxPassword,
				RealmID = realmId
			};

			return oData;
		} // FillIn

	} // class AccountModel
} //aadd namespace Integration.ChannelGrabberFrontend
