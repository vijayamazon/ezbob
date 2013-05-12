using System.Collections.Generic;
using EZBob.DatabaseLib.Model.Database;
using log4net;
using Integration.ChannelGrabberAPI;

namespace Integration.Play {
	#region class PlayConnector

	public class PlayConnector {
		#region public

		#region method Validate

		public void Validate(
			ILog oLog,
			Customer oCustomer,
			string sShopName,
			string sUserName,
			string sPassword
		) {
			var oApi = new PlayProle(oLog, oCustomer);

			oApi.Validate(new PlayAccountData {
				name = sShopName,
				username = sUserName,
				password = sPassword
			});
		} // Validate

		#endregion method Validate

		#region method GetOrders

		public static List<ChannelGrabberOrder> GetOrders(ILog oLog, Customer oCustomer, string sShopName, string sUserName) {
			var oApi = new PlayProle(oLog, oCustomer);

			return oApi.GetOrders(new PlayAccountData {
				name = sShopName,
				username = sUserName
			});
		} // GetOrders

		#endregion method GetOrders

		#endregion public
	} // class PlayConnector

	#endregion class PlayConnector
} // namespace