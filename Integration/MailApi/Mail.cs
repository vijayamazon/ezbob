namespace MailApi {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Text.RegularExpressions;
	using System.Threading;
	using ConfigManager;
	using Model;
	using Newtonsoft.Json;
	using RestSharp;
	using RestSharp.Deserializers;
	using log4net;

	public class Mail {
		private RestClient _client;
		private static readonly ILog Log = LogManager.GetLogger(typeof(Mail));
		private readonly string key;

		//Api pathes
		private const string BaseSecureUrl = "https://mandrillapp.com/api/1.0/";
		private const string SendTemplatePath = "/messages/send-template.json";
		private const string RenderTemplatePath = "/templates/render.json";

		public Mail(string key = null) {
			this.key = key ?? CurrentValues.Instance.MandrillKey;

			InitRestClient();
		}

		private void InitRestClient()
		{
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

		private string SendRequest(string path, object model)
		{
			int counter = 1;
			Exception ex = null;
			while (counter < 6)
			{
				try
				{
					Log.InfoFormat("Starting SendRequest. Attempt number:{0} Path: {1} Model: {2}", counter, path, model);

					if (counter > 1)
					{
						Log.Info("Re-initializing the rest client from SendRequest");
						InitRestClient();
					}

					var request = new RestRequest(path, Method.POST) {RequestFormat = DataFormat.Json};
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
						throw new MandrillException(error,
						                            string.Format("InternalServerError. Post failed {0}; response: {1}", path,
						                                          response.Content));
					}

					if (response.StatusCode != HttpStatusCode.OK)
					{
						Log.InfoFormat("Other than ok status code in RestRequest's response :{0}", response.StatusCode);
						throw response.ErrorException;
					}

					if (counter > 1)
					{
						Log.InfoFormat("Success in attempt #{0} - retry works", counter);
					}

					return response.Content;
				}
				catch (Exception e)
				{
					Log.ErrorFormat("Error occur during SendRequest: {0}", e);
					ex = e;
					Thread.Sleep(1000);
				}
				counter++;
			}

			Log.ErrorFormat("Exception should have been throen from SendRequest but was blocked to avoid interfering with business logic: {0}", ex);
			return null;
			//throw ex ?? new Exception("Throwing empty exception from SendRequest");
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