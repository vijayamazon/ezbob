using System;

namespace Integration.ChannelGrabberSecurityInfoConverter {

	public class AccountModel {

		public enum NodeNames {
			MarketplaceId,
			Name,
			Url,
			Login,
			Password,
			LimitDays,
			AuxLogin,
			AuxPassword,
			RealmId,
			AccountTypeName,
			DisplayName,
		} // enum NodeNames

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

		public void Set(NodeNames nNodeName, string sValue) {
			switch (nNodeName) {
			case NodeNames.MarketplaceId:
				id = Convert.ToInt32(sValue);
				break;

			case NodeNames.Name:
				name = sValue;
				break;

			case NodeNames.Url:
				url = sValue;
				break;

			case NodeNames.Login:
				login = sValue;
				break;

			case NodeNames.Password:
				password = sValue;
				break;

			case NodeNames.DisplayName:
				displayName = sValue;
				break;

			case NodeNames.LimitDays:
				limitDays = Convert.ToInt32(sValue);
				break;

			case NodeNames.AuxLogin:
				auxLogin = sValue;
				break;

			case NodeNames.AuxPassword:
				auxPassword = sValue;
				break;

			case NodeNames.RealmId:
				realmId = Convert.ToInt32(sValue);
				break;

			case NodeNames.AccountTypeName:
				accountTypeName = sValue;
				break;

			default:
				throw new ArgumentOutOfRangeException("nNodeName");
			} // switch
		} // Set

		public void Validate() {
			if ((displayName ?? "").Trim() == string.Empty)
				displayName = name;
		} // Validate

	} // class AccountModel

} // namespace Integration.ChannelGrabberSecurityInfoConverter
