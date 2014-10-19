namespace MailApi.Model {
	using System.Collections.Generic;
	using System.Linq;
	using RestSharp;

	public class EmailMessageModel {
		public EmailMessageModel() {
			merge = true;
		} // constructor

		// ReSharper disable InconsistentNaming

		public string html { get; set; }
		public string text { get; set; }
		public string subject { get; set; }
		public string from_email { get; set; }
		public string from_name { get; set; }
		public IEnumerable<EmailAddressModel> to { get; set; }
		public JsonObject headers { get; set; }
		public bool track_opens { get; set; }
		public bool track_clicks { get; set; }
		public bool auto_text { get; set; }
		public bool url_strip_qs { get; set; }
		public bool preserve_recipients { get; set; }
		public string bcc_address { get; set; }
		public bool merge { get; set; }
		public bool important { get; set; }
		public List<merge_var> merge_vars { get; set; }
		public List<merge_var> global_merge_vars { get; set; }
		public IEnumerable<string> tags { get; set; }
		public IEnumerable<string> google_analytics_domains { get; set; }
		public string google_analytics_campaign { get; set; }
		public JsonObject metadata { get; set; }
		public List<attachment> attachments { get; set; }
		public IEnumerable<image> images { get; set; }
		public string raw_message { get; set; }

		// ReSharper enable InconsistentNaming

		public override string ToString() {
			List<string> oTo = to == null
				? new List<string>()
				: to.Select(t => t.ToString()).ToList();

			string sTo = oTo.Count < 1
				? "no recipients"
				: string.Format("[{0}] {{ {1} }}", oTo.Count, string.Join(", ", oTo));

			return string.Format(
				"From: {0} <{1}>, To: {3}, Bcc: '{4}', Subject: '{2}'", from_name, from_email, subject, sTo, bcc_address ?? "-- null --"
			);
		} // ToString
	} // class EmailMessageModel
} // namespace
