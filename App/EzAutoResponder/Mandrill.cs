namespace EzAutoResponder
{
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;
	using System.Net;
	using System.Text.RegularExpressions;
	using Ezbob.Logger;
	using MailApi.Model;
	using Newtonsoft.Json;
	using RestSharp;
	using RestSharp.Deserializers;

	public class Mandrill
	{
		private readonly RestClient _client;
		private readonly ASafeLog _log;

		//Api pathes
		private const string SendTemplatePath = "/messages/send-template.json";
		private const string BaseSecureUrl = "https://mandrillapp.com/api/1.0/";

		public Mandrill(ASafeLog log)
		{
			_log = log;
			_client = new RestClient(BaseSecureUrl);
			_client.AddHandler("application/json", new JsonDeserializer());
		}

		public string Send(Dictionary<string, string> parameters, string to, string templateName, string subject = "", string cc = "")
		{
			var message = PrepareEmail(templateName, to, parameters, subject, cc);
			return Send(message, SendTemplatePath);
		}

		private EmailModel PrepareEmail(string templateName, string to, Dictionary<string, string> variables, string subject, string cc = "")
		{

			var toList = PrepareRecipients(to);
			var env = ConfigurationManager.AppSettings.Get("CurrentEnv");
			var message = new EmailModel
			{
				key = ConfigurationManager.AppSettings.Get(env),
				template_name = templateName,
				message = new EmailMessageModel
				{
					to = toList,
					subject = subject,
					bcc_address = cc
				},
			};

			foreach (var var in variables)
			{
				message.AddGlobalVariable(var.Key, var.Value);
			}

			return message;
		}

		private static IEnumerable<EmailAddressModel> PrepareRecipients(string to)
		{
			var spaces = new Regex(@"\s+", RegexOptions.Compiled);
			return spaces.Replace(to, string.Empty).Split(';').Select(x => new EmailAddressModel { email = x });
		}
		
		private string Send(EmailModel email, string path)
		{
			var response = SendRequest(path, email);
			if (response == null)
			{
				return null;
			}

			var responseDeserialized = JsonConvert.DeserializeObject<List<EmailResultModel>>(response);
			var status = responseDeserialized[0].status.ToLower();
			if (status != "sent" && status != "queued")
			{
				_log.Warn(responseDeserialized[0].ToString());
				return "status not 'sent' or 'queued'";
			}
			return "OK";
		}

		private string SendRequest(string path, object model)
		{
			var request = new RestRequest(path, Method.POST) { RequestFormat = DataFormat.Json };
			request.AddBody(model);
			var response = _client.Post(request);

			_log.Info("Mandrill service call.\n Request: \n {0} \n Response: \n {1}", request.Parameters[0], response.Content);

			if (response.StatusCode == HttpStatusCode.InternalServerError)
			{
				var error = JsonConvert.DeserializeObject<ErrorResponseModel>(response.Content);
				throw new MandrillException(error, string.Format("InternalServerError. Post failed {0}", path));
			}

			if (response.StatusCode != HttpStatusCode.OK)
			{
				throw response.ErrorException;
			}

			return response.Content;
		}
	}
}
