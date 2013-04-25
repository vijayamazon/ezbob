using System.Collections.Generic;
using EZBob.DatabaseLib.Model.Database;
using log4net;
using Integration.ChannelGrabberAPI;

namespace Integration.Volusion
{
	#region class VolusionConnector

	public class VolusionConnector {
		#region public

		#region method Validate

		public void Validate(
			ILog oLog,
			Customer oCustomer,
			string sShopName,
			string sUrl,
			string sUserName,
			string sPassword
		) {
			var oApi = new VolusionProle(oLog, oCustomer);

			oApi.Validate(new VolusionAccountData {
				name     = sShopName,
				endpoint = sUrl,
				username = sUserName,
				password = sPassword
			});
		} // Validate

		#endregion method Validate

		#region method GetOrders

		public static List<ChannelGrabberOrder> GetOrders(ILog oLog, Customer oCustomer, string sUrl, string sUserName) {
			var oApi = new VolusionProle(oLog, oCustomer);

			return oApi.GetOrders(new VolusionAccountData {
				endpoint = sUrl,
				username = sUserName
			});
		} // GetOrders

		#endregion method GetOrders

		#endregion public
	} // class VolusionConnector

	#endregion class VolusionConnector
} // namespace