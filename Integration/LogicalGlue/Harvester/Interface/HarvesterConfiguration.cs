namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	using System;
	using System.Collections.Generic;

	public class HarvesterConfiguration {
		public string HostName { get; set; }
		public string NewCustomerRequestPath { get; set; }
		public string OldCustomerRequestPath { get; set; }

		public List<string> Validate() {
			var result = new List<string>();

			if (string.IsNullOrWhiteSpace(HostName))
				result.Add("Host name not specified.");

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
	} // class HarvesterConfiguration
} // namespace
