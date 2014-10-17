namespace MailApi {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Text.RegularExpressions;
	using ConfigManager;
	using Model;
	using Newtonsoft.Json;
	using RestSharp;
	using RestSharp.Deserializers;
	using log4net;

	public class Mail {
		private readonly RestClient _client;
		private static readonly ILog Log = LogManager.GetLogger(typeof(Mail));
		private string key;

		//Api pathes
		private const string BaseSecureUrl = "https://mandrillapp.com/api/1.0/";
		private const string SendTemplatePath = "/messages/send-template.json";
		private const string RenderTemplatePath = "/templates/render.json";

		public Mail(string key = null) {
			this.key = key ?? CurrentValues.Instance.MandrillKey;

			_client = new RestClient(BaseSecureUrl);
			_client.AddHandler("application/json", new JsonDeserializer());
		}

		private EmailModel PrepareEmail(string templateName, string to, Dictionary<string, string> variables, string subject, string cc = "", List<attachment> attachments = null) {
			var toList = PrepareRecipients(to);

			var message = new EmailModel {
				key = key,
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

			foreach (var var in variables) {
				message.AddGlobalVariable(var.Key, var.Value);
			}

			return message;
		}

		private static IEnumerable<EmailAddressModel> PrepareRecipients(string to) {
			var spaces = new Regex(@"\s+", RegexOptions.Compiled);
			return spaces.Replace(to, string.Empty).Split(';').Select(x => new EmailAddressModel { email = x });
		}

		private string SendRequest(string path, object model) {
			try
			{
				Log.InfoFormat("Starting SendRequest. Path: {0} Model: {1}", path, model);
				var request = new RestRequest(path, Method.POST) { RequestFormat = DataFormat.Json };
				Log.InfoFormat("Created RestRequest object");
				request.AddBody(model);
				Log.InfoFormat("Added model to RestRequest's body");
				var response = _client.Post(request);
				Log.InfoFormat("Posted RestRequest");
				Log.InfoFormat("Mandrill service call.\n Response length: \n {0}", response.Content.Length);

				if (response.StatusCode == HttpStatusCode.InternalServerError)
				{
					Log.InfoFormat("InternalServerError status code in RestRequest's response");
					var error = JsonConvert.DeserializeObject<ErrorResponseModel>(response.Content);
					throw new MandrillException(error, string.Format("InternalServerError. Post failed {0}; response: {1}", path, response.Content));
				}

				if (response.StatusCode != HttpStatusCode.OK)
				{
					Log.InfoFormat("Other than ok status code in RestRequest's response :{0}", response.StatusCode);
					throw response.ErrorException;
				}

				return response.Content;
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Error occur during SendRequest: {0}", e);
				throw;
			}
		}

		private string Send(EmailModel email, string path) {
			var response = SendRequest(path, email);
			if (response == null) {
				return null;
			}

			var responseDeserialized = JsonConvert.DeserializeObject<List<EmailResultModel>>(response);
			var status = responseDeserialized[0].status.ToLower();
			if (status != "sent" && status != "queued") {
				Log.WarnFormat("status: {0}, {1}", status, responseDeserialized[0]);
				return "status not 'sent' or 'queued'";
			}
			return "OK";
		}

		private string RenderTemplate(Dictionary<string, string> parameters, string templateName) {
			var templateModel = new RenderTemplateModel {
				key = key,
				template_name = templateName,
				template_content = new template_content[] { },
				merge_vars = Utils.AddMergeVars(parameters)
			};
			var response = SendRequest(RenderTemplatePath, templateModel);
			if (response == null) {
				return null;
			}
			var retVal = JsonConvert.DeserializeObject<Dictionary<string, string>>(response); //response is { "html": "response text" }
			return retVal["html"];
		}

		public string Send(Dictionary<string, string> parameters, string to, string templateName, string subject = "", string cc = "", List<attachment> attachments = null) {
			var message = PrepareEmail(templateName, to, parameters, subject, cc, attachments);
			return Send(message, SendTemplatePath);
		}

		public string GetRenderedTemplate(Dictionary<string, string> parameters, string templateName) {
			return RenderTemplate(parameters, templateName);
		}
	}
}