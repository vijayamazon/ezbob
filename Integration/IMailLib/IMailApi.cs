namespace IMailLib {
	using System;

	public class IMailApi {
		public bool Authenticate() {
			IMailApiNS.imail_apiSoapClient client = new IMailApiNS.imail_apiSoapClient("imail_apiSoap12");
			var response = client.Authenticate(Username: "", Password: "");
			if (response != null && response["Response"] != null && bool.Parse(response["Response"].Attributes["success"].Value)) {
				return true;
			}

			if (response != null && response["Response"] != null && !bool.Parse(response["Response"].Attributes["success"].Value)) {
				Console.WriteLine("Authenticate failed with error: {0}", response["Response"].Attributes["error_message"].Value);
				return true;
			}
			return false;
		}
	}
}
