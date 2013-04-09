using System;
using System.Collections.Generic;

namespace Integration.Volusion
{
	#region class VolusionConnector

	public class VolusionConnector {
		#region method Validate

		public bool Validate(string userName, string password, out string errMsg) {
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