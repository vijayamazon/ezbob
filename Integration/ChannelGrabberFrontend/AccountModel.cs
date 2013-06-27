using System;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper;
using EZBob.DatabaseLib.Model.Database;
using EzBob.CommonLib;
using Integration.ChannelGrabberAPI;
using Integration.ChannelGrabberConfig;

namespace Integration.ChannelGrabberFrontend {
	/// <summary>
	/// This class is used to communicate with UI using json.
	/// </summary>
	public class AccountModel : IMarketPlaceSecurityInfo {
		#region static

		#region method ToModel

		public static AccountModel ToModel(MP_CustomerMarketPlace account) {
			try {
				var m = SerializeDataHelper.DeserializeType<AccountModel>(account.SecurityData);
				m.id = account.Id;
				return m;
			}
			catch (Exception e) {
				throw new ApiException(string.Format("Failed to deserialise security data for marketplace {0} ({1})", account.DisplayName, account.Id), e);
			}
		} // ToModel

		public static AccountModel ToModel(IDatabaseCustomerMarketPlace account) {
			try {
				var m = SerializeDataHelper.DeserializeType<AccountModel>(account.SecurityData);
				m.id = account.Id;
				return m;
			}
			catch (Exception e) {
				throw new ApiException(string.Format("Failed to deserialise security data for marketplace {0} ({1})", account.DisplayName, account.Id), e);
			}
		} // ToModel

		#endregion method ToModel

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

		public AccountData Fill() {
			var oData = new AccountData(Configuration.Instance.GetVendorInfo(accountTypeName));
			oData.Login = login;
			oData.Password = password;
			oData.Name = name;
			oData.URL = url;
			oData.LimitDays = limitDays;
			oData.AuxLogin = auxLogin;
			oData.AuxPassword = auxPassword;
			oData.RealmID = realmId;
			return oData;
		} // FillIn

		#endregion method Fill
	} // class AccountModel
} //aadd namespace Integration.ChannelGrabberFrontend
