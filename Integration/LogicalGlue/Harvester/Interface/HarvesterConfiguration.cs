namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using System;
	using System.Collections.Generic;
	using System.Text;

	public class HarvesterConfiguration {
		public string HostName { get; set; }
		public string NewCustomerRequestPath { get; set; }
		public string OldCustomerRequestPath { get; set; }
		public string AuthorizationScheme { get; set; }

		public string UserName {
			get { return this.userName; }
			set {
				this.userName = value;
				SetAccessToken();
			} // set
		} // UserName

		public string Password {
			get { return this.password; }
			set {
				this.password = value;
				SetAccessToken();
			} // set
		} // Password

		public string AccessToken { get; private set; }

		public List<string> Validate() {
			var result = new List<string>();

			if (string.IsNullOrWhiteSpace(HostName))
				result.Add("Host name not specified.");

			if (string.IsNullOrWhiteSpace(AuthorizationScheme))
				result.Add("Authorization scheme not specified.");

			if (string.IsNullOrWhiteSpace(UserName))
				result.Add("User name not specified.");

			if (string.IsNullOrWhiteSpace(Password))
				result.Add("Password not specified.");

			ValidatePath(result, "New customer request", NewCustomerRequestPath);
			ValidatePath(result, "Old customer request", OldCustomerRequestPath);

			return result.Count > 0 ? result : null;
		} // Validate

		private void ValidatePath(List<string> result, string pathName, string path) {
			if (string.IsNullOrWhiteSpace(path)) {
				result.Add(string.Format("'{0}' not specified.", pathName));
				return;
			} // if

			if (!path.StartsWith("/"))
				result.Add(string.Format("'{0}' does not start with '/'.", pathName));

			if (path.IndexOf("{0}", StringComparison.InvariantCulture) <= 0)
				result.Add(string.Format("'{0}' does not contain '{{0}}' (request id placeholder).", pathName));
		} // ValidatePath

		private void SetAccessToken() {
			string credentials = string.Format("{0}:{1}", UserName, Password);
			AccessToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
		} // SetAccessToken

		private string userName;
		private string password;
	} // class HarvesterConfiguration
} // namespace
