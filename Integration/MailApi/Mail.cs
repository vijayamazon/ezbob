namespace MailApi {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Text.RegularExpressions;
	using ConfigManager;
	using Ezbob.Logger;
	using Ezbob.Utils.Extensions;
	using Model;
	using Newtonsoft.Json;
	using RestSharp;
	using RestSharp.Deserializers;

	public class Mail {
		public static string EncodeAttachment(string plainText) {
			return Mail.EncodeAttachment(System.Text.Encoding.UTF8.GetBytes(plainText ?? string.Empty));
		} // EncodeAttachment

		public static string EncodeAttachment(byte[] plainTextBytes) {
			return System.Convert.ToBase64String(plainTextBytes ?? new byte[0]);
		} // EncodeAttachment

		public Mail(string key = null) {
			this.key = key ?? CurrentValues.Instance.MandrillKey;
		} // constructor

		/// <summary>
		/// With template
		/// </summary>
		public string Send(
			Dictionary<string, string> parameters,
			string to,
			string templateName,
			string subject = "",
			string cc = "",
			List<attachment> attachments = null
		) {
			var toList = PrepareRecipients(to);

			var message = new EmailModel {
				key = this.key,
				template_name = templateName,
				message = new EmailMessageModel {
					to = toList,
					subject = subject,
					bcc_address = cc,
					attachments = attachments,
					track_clicks = true,
					track_opens = true
				}
			};

			foreach (var var in parameters)
				message.AddGlobalVariable(var.Key, var.Value);

			return Send(message, SendTemplatePath);
		} // Send

		/// <summary>
		/// Without template
		/// </summary>
		public string Send(
			string to,
			string messageText,
			string messageHtml,
			string fromEmail,
			string fromName,
			string subject = "",
			string cc = "",
			List<attachment> attachments = null
		) {
			if (string.IsNullOrEmpty(to)) {
				log.Warn("Receiver email not provided for {0}", subject);
				return "No receiver email provided";
			} // if

			var toList = PrepareRecipients(to);

			var message = new EmailModel {
				key = this.key,
				message = new EmailMessageModel {
					to = toList,
					subject = subject,
					bcc_address = cc,
					attachments = attachments,
					track_clicks = true,
					track_opens = true,
					html = messageHtml,
					text = messageText,
					from_email = fromEmail,
					from_name = fromName
				}
			};

			return Send(message, SendTemplatelessPath);
		} // Send

		public string GetRenderedTemplate(Dictionary<string, string> parameters, string templateName) {
			var templateModel = new RenderTemplateModel {
				key = key,
				template_name = templateName,
				template_content = new template_content[] { },
				merge_vars = Utils.AddMergeVars(parameters)
			};

			var response = SendRequest(RenderTemplatePath, templateModel);

			if (response == null)
				return null;

			// Response is { "html": "response text" }
			var retVal = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);

			return retVal["html"];
		} // GetRenderedTemplate

		private static IEnumerable<EmailAddressModel> PrepareRecipients(string to) {
			var spaces = new Regex(@"\s+", RegexOptions.Compiled);

			return spaces.Replace(to, string.Empty).Split(';').Select(x => new EmailAddressModel { email = x });
		} // PrepareRecipients

		private string SendRequest(string path, object model) {
			try {
				log.Debug("ServicePointManager.SecurityProtocol = {0}", ServicePointManager.SecurityProtocol);

				log.Debug("Starting SendRequest. Path: {0} Model: {1}", path, model.ToLogStr());

				var client = new RestClient(BaseSecureUrl);
				client.AddHandler("application/json", new JsonDeserializer());

				var request = new RestRequest(path, Method.POST) { RequestFormat = DataFormat.Json };

				log.Debug("Created RestRequest object");

				request.AddBody(model);

				log.Debug("Added model to RestRequest's body");

				IRestResponse response = client.Post(request);

				log.Debug(
					"Posted RestRequest: Mandrill service call; response status code: {1}; response length: {0}.",
					response.Content.Length,
					response.StatusCode
				);

				if (response.StatusCode == HttpStatusCode.InternalServerError) {
					log.Debug("InternalServerError status code in RestRequest's response");

					var error = JsonConvert.DeserializeObject<ErrorResponseModel>(response.Content);

					throw new MandrillException(
						error,
						string.Format("InternalServerError. Post failed {0}; response: {1}", path, response.Content)
					);
				} // if

				if (response.StatusCode != HttpStatusCode.OK) {
					log.Warn(
						response.ErrorException,
						"Other than ok status code in RestRequest's response: {0}.",
						response.StatusCode
					);
					throw response.ErrorException;
				} // if

				return response.Content;
			} catch (Exception e) {
				log.Error(
					e,
					"Error occurred during SendRequest; " +
					"exception should have been thrown from SendRequest" +
					"but was blocked to avoid interfering with business logic."
				);
				return null;
			} // try
		} // SendRequest

		private string Send(EmailModel email, string path) {
			var response = SendRequest(path, email);

			if (response == null)
				return null;

			var responseDeserialized = JsonConvert.DeserializeObject<List<EmailResultModel>>(response);
			var status = responseDeserialized[0].status.ToLower();

			if (status != "sent" && status != "queued") {
				log.Warn("status: {0}, {1}", status, responseDeserialized[0]);
				return "status not 'sent' or 'queued'";
			} // if

			return "OK";
		} // Send

		private static readonly ASafeLog log = new SafeILog(typeof(Mail));
		private readonly string key;

		// API paths
		private const string BaseSecureUrl = "https://mandrillapp.com/api/1.0/";
		private const string SendTemplatePath = "/messages/send-template.json";
		private const string SendTemplatelessPath = "/messages/send.json";
		private const string RenderTemplatePath = "/templates/render.json";
	} // class Mail
} // namespace
