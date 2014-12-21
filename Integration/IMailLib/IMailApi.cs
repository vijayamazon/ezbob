namespace IMailLib {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using System.Xml;
	using IMailLib.IMailApiNS;
	using RestSharp;

	public class IMailApi {
		private readonly IMailApiNS.imail_apiSoapClient client;
		public IMailApi() {
			client = new IMailApiNS.imail_apiSoapClient("imail_apiSoap");
		}

		public bool Authenticate() {

			var response = client.Authenticate(Username: "Emma123456", Password: "Ezbob123");
			if (response != null && response["Response"] != null && bool.Parse(response["Response"].Attributes["success"].Value)) {
				return true;
			}

			if (response != null && response["Response"] != null && !bool.Parse(response["Response"].Attributes["success"].Value)) {
				Console.WriteLine("Authenticate failed with error: {0}", response["Response"].Attributes["error_message"].Value);
				return false;
			}
			return false;
		}

		public List<AttachmentModel> ListAttachment() {
			var attachmentModels = new List<AttachmentModel>();
			if (Authenticate()) {
				var response = client.ListAttachments();
				if (response != null && response["Response"] != null && bool.Parse(response["Response"].Attributes["success"].Value)) {
					var attachments = response["Response"].SelectNodes("Attachment");
					if (attachments != null) {
						foreach (XmlNode attachment in attachments) {
							attachmentModels.Add(new AttachmentModel {
								Name = attachment.Attributes["name"].Value,
								Date = DateTime.ParseExact(attachment.Attributes["date"].Value, "yyyyMMdd HH:mm:ss", CultureInfo.InvariantCulture),
								Size = long.Parse(attachment.Attributes["size"].Value)
							});
						}
					}
				}

				if (response != null && response["Response"] != null && !bool.Parse(response["Response"].Attributes["success"].Value)) {
					Console.WriteLine("ListAttachment failed with error: {0}", response["Response"].Attributes["error_message"].Value);

				}
			}
			return attachmentModels;
		}

		public string GetReturns() {
			if (Authenticate()) {
				//RestClient restClient = new RestClient("https://www.imail.co.uk/webservice/imail_api.asmx/webservice/imail_api.asmx/GetReturns");
				//var request = new RestRequest(Method.POST);
				//request.AddParameter(new Parameter { Name = "startDate", Type = ParameterType.QueryString, Value = DateTime.Today.AddYears(-1).ToString("yyyy-MM-dd")});
				//request.AddParameter(new Parameter { Name = "endDate", Type = ParameterType.QueryString, Value = DateTime.Today.ToString("yyyy-MM-dd")});
				//request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
				//request.AddHeader("Content-Length", "0");
				//var response = restClient.Post(request);
				//return response.Content;

				var response = client.GetReturns(DateTime.Today.AddYears(-1).ToString("yyyy-MM-dd"), DateTime.Today.ToString("yyyy-MM-dd"));
				if (response != null && response["Response"] != null && bool.Parse(response["Response"].Attributes["success"].Value)) {
					return response["Response"].Attributes["CSVdata"].Value;
				}
			}

			return null;
		}
	}
}
