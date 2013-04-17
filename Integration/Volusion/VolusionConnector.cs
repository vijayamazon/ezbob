using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.Model.Database;
using log4net;
using Integration.ChannelGrabberAPI;

namespace Integration.Volusion
{
	#region class VolusionConnector

	public class VolusionConnector {
		#region method Validate

		public void Validate(
			ILog log,
			Customer customer,
			string sShopName,
			string sUrl,
			string sUserName,
			string sPassword
		) {
			var oApi = new VolusionProle(log, customer);

			oApi.Validate(new VolusionAccountData {
				name     = sShopName,
				endpoint = sUrl,
				username = sUserName,
				password = sPassword
			});
		} // Validate

		#endregion method Validate

		#region method GetOrders

		public static List<VolusionOrder> GetOrders(ILog log, Customer customer, string url, string userName, string password) {
			var oApi = new VolusionProle(log, customer);
			// return oApi.GetOrders(url, userName);
			throw new NotImplementedException();
		} // GetOrders

		#endregion method GetOrders
	} // class VolusionConnector

	#endregion class VolusionConnector
} // namespace