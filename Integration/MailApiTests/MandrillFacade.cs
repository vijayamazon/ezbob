namespace MailApiTests {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Text.RegularExpressions;
	using Ezbob.Logger;
	using MailApi.Model;
	using Newtonsoft.Json;
	using RestSharp;

	class MandrillFacade {

		public MandrillFacade(ASafeLog oLog) {
			m_oLog = oLog ?? new SafeLog();

			m_oClient = new RestClient(BaseSecureUrl);
		} // constructor

		public string SendEmail(
			string to,
			string templateName,
			Dictionary<string, string> parameters,
			string subject = "",
			string cc = "",
			List<attachment> attachments = null
		) {
			return ProcessRequest(
				SendTemplatePath,
				PrepareEmail(templateName, to, parameters, subject, cc, attachments)
			);
		} // SendEmail

		private string ProcessRequest(string sPath, EmailModel oModel) {
			try {
				m_oLog.Debug("SendRequest({0}, {1})...", sPath, oModel);

				var request = new RestRequest(sPath, Method.POST) { RequestFormat = DataFormat.Json };

				m_oLog.Info("Created RestRequest object");
				request.AddBody(oModel);

				m_oLog.Info("Added model to RestRequest's body");

				IRestResponse response = m_oClient.Execute(request);

				m_oLog.Info("Mandrill service call complete, response length: {0}", response.Content.Length);

				if (response.ResponseStatus != ResponseStatus.Completed) {
					m_oLog.Warn(
						"A network transport error encountered, response status is {0}, content is '{1}'.",
						response.ResponseStatus,
						response.Content
					);

					throw new MandrillException(string.Format("A network transport error while executing {0}.", sPath));
				} // if

				if (response.StatusCode == HttpStatusCode.InternalServerError) {
					m_oLog.Warn("InternalServerError (HTTP 500) status code in RestRequest's response while executing {0}.", sPath);

					throw new MandrillException(
						JsonConvert.DeserializeObject<ErrorResponseModel>(response.Content),
						string.Format("InternalServerError. Post failed {0}; response: {1}", sPath, response.Content)
					);
				} // if

				if (response.StatusCode != HttpStatusCode.OK) {
					m_oLog.Warn("{0} status code in RestRequest's response while executing {1}.", response.StatusCode, sPath);
					throw response.ErrorException;
				} // if

				m_oLog.Debug("SendRequest({0}, {1}) complete.", sPath, oModel);

				return response.Content;
			}
			catch (Exception e) {
				m_oLog.Alert(e, "Error occurred during SendRequest({0}, {1}).", sPath, oModel);
				return null;
			} // try
		} // ProcessRequest

		private EmailModel PrepareEmail(string templateName, string to, Dictionary<string, string> variables, string subject, string cc = "", List<attachment> attachments = null) {
			var toList = PrepareRecipients(to);

			var message = new EmailModel {
				key = MandrillKey,
				template_name = templateName,
				message = new EmailMessageModel {
					to = toList,
					subject = subject,
					bcc_address = cc,
					attachments = attachments,
					track_clicks = true,
					track_opens = true,
				},
			};

			foreach (var var in variables)
				message.AddGlobalVariable(var.Key, var.Value);

			return message;
		} // PrepareEmail

		private static IEnumerable<EmailAddressModel> PrepareRecipients(string to) {
			var spaces = new Regex(@"\s+", RegexOptions.Compiled);
			return spaces.Replace(to, string.Empty).Split(';').Select(x => new EmailAddressModel { email = x });
		} // PrepareRecipients

		private RestClient m_oClient;

		private const string BaseSecureUrl = "https://mandrillapp.com/api/1.0/";
		private const string SendTemplatePath = "/messages/send-template.json";
		private const string RenderTemplatePath = "/templates/render.json";

		private const string MandrillKey = "nNAb_KZhxEqLCyzEGOWvlg";

		private readonly ASafeLog m_oLog;

	} // class MandrillFacade
} // namespace
