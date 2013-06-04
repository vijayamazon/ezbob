namespace PayPoint
{
	using System;
	using System.IO;
	using log4net;

	public class PayPointConnector
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(PayPointConnector));

		/// <summary>
		/// checks if the mid, vpn and remote passwords are correct
		/// </summary>
		/// <param name="mid">
		/// This is your PayPoint.net Gateway account name (usually six letters and two numbers).
		/// You can see this ID in the top right corner while logged into the Extranet.
		/// </param>
		/// <param name="vpnPassword">
		/// Your VPN password can be set from within the Extranet.
		/// https://www.paypoint.net/secnet/app (Click on "Account" then "Remote Passwords" 
		/// and select VPN from the drop down list).
		/// </param>
		/// <param name="remotePassword">
		/// Your Remote password can be set from within the Extranet. 
		/// https://www.paypoint.net/secnet/app (Click on "Account" then "Remote Passwords" 
		/// and select Remote from the drop down list). 
		/// </param>
		/// <param name="errMsg">Contains error message or empty</param>
		/// <returns>true if credentials correct, false otherwise</returns>
		public static bool Validate(string mid, string vpnPassword, string remotePassword, out string errMsg)
		{
			var secVpnService = new SECVPNService();
			string res = secVpnService.getReport(mid, vpnPassword, remotePassword, "XML-Report", "Batch", "1", "GBP", string.Empty, false, false);

			bool isValid = (!string.IsNullOrEmpty(res) && !res.Contains("Remote Password Invalid"));
			errMsg = !isValid ? string.Format("Failure: {0}", string.IsNullOrEmpty(res) ? "Invalid Credentials" : res) : string.Empty;
			return isValid;
		}

		public static PayPointDataSet.TransactionDataTable GetOrders(string condition, string mid, string vpnPassword, string remotePassword)
		{
			var secVpnService = new SECVPNService { Timeout = 3000000 };// 50 minutes - overrides default of 100000
			var payPointDataSet = new PayPointDataSet();
			try
			{
				string reportXmlString = secVpnService.getReport(mid, vpnPassword, remotePassword, "XML-Report", "Date", condition, "GBP", string.Empty, false, true);
				using (Stream xmlStream = reportXmlString.ToStream())
				{
					payPointDataSet.ReadXml(xmlStream);
				}
			}
			catch (Exception e)
			{
				log.ErrorFormat("Error getting PayPoint orders. Marketplace will appear empty. Try to recheck later. Exception:{0}", e);
			}

			return payPointDataSet.Transaction;
		}
	}
}