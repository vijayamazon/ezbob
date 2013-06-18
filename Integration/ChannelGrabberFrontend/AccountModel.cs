using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.Model.Database;
using EzBob.CommonLib;
using Integration.ChannelGrabberConfig;

namespace Integration.ChannelGrabberFrontend {
	/// <summary>
	/// This class is used to communicate with UI using json.
	/// </summary>
	public class AccountModel {
		#region static

		#region method ToModel

		public static AccountModel ToModel(MP_CustomerMarketPlace account) {
			return Create(account.Id, SerializeDataHelper.DeserializeType<SecurityInfo>(account.SecurityData));
		} // ToModel

		public static AccountModel ToModel(IDatabaseCustomerMarketPlace account) {
			return Create(account.Id, SerializeDataHelper.DeserializeType<SecurityInfo>(account.SecurityData));
		} // ToModel

		#endregion method ToModel

		#region method Create

		private static AccountModel Create(int nAccountID, SecurityInfo si) {
			return new AccountModel {
				id = nAccountID,
				login = si.AccountData.Login,
				password = si.AccountData.Password,
				name = si.AccountData.Name,
				url = si.AccountData.URL,
				limitDays = si.AccountData.LimitDays,
				auxLogin = si.AccountData.AuxLogin,
				auxPassword = si.AccountData.AuxPassword,
				realmId = si.AccountData.RealmID,
				accountTypeName = si.AccountData.AccountTypeName(),
				displayName = si.AccountData.Name,
			};
		} // Create

		#endregion method Create

		#endregion static

		#region properties

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

		#endregion properties

		#region method Fill

		public void Fill(AccountData oData) {
			oData.Login = login;
			oData.Password = password;
			oData.Name = name;
			oData.URL = url;
			oData.LimitDays = limitDays;
			oData.AuxLogin = auxLogin;
			oData.AuxPassword = auxPassword;
			oData.RealmID = realmId;
		} // Fill

		#endregion method Fill
	} // class AccountModel
} //aadd namespace Integration.ChannelGrabberFrontend
