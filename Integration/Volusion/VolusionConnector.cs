using System;
using System.Collections.Generic;
using RestSharp;

namespace Integration.Volusion
{
	#region class VolusionConnector

	public class VolusionConnector {
		#region method Validate

		public bool Validate(string shopName, string url, string userName, string password, out string errMsg) {
			// RestClient
			throw new NotImplementedException();
		} // Validate

		#endregion method Validate

		#region method GetOrders

		public static List<VolusionOrder> GetOrders(string userName, string password) {
			throw new NotImplementedException();
		} // GetOrders

		#endregion method GetOrders
	} // class VolusionConnector

	#endregion class VolusionConnector
} // namespace