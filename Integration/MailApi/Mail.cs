using System.Collections.Generic;
using System.Net;
using EzBob.Configuration;
using MailApi.Model;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Deserializers;
using log4net;

namespace MailApi
{
    public class Mail: IMail
    {
        private readonly RestClient _client;
        private static readonly ILog Log = LogManager.GetLogger(typeof(Mail));
        private readonly IMandrillConfig _config;

        public Mail(IMandrillConfig config = null)
        {
            _config = config ?? ConfigurationRootBob.GetConfiguration().MandrillConfig;
            _client = new RestClient(_config.BaseSecureUrl);
            _client.AddHandler("application/json", new JsonDeserializer());
        }

        private EmailModel PrepareEmail(string templateName, string to,  Dictionary<string,string> variables, string subject)
        {
            var message = new EmailModel
            {
                key = _config.Key,
                template_name = templateName,
                message = new EmailMessageModel
                {
                    to = new[] { new EmailAddressModel { email = to } },
                    subject = subject
                }
            };

            foreach (var var in variables)
            {
                message.AddGlobalVariable(var.Key, var.Value);
            }

            return message;
        }

        private string Send(EmailModel email)
        {
            if (!_config.Enable)
            {
                return string.Empty;
            }
            var request = new RestRequest(_config.SendTemplatePath, Method.POST) { RequestFormat = DataFormat.Json };
            request.AddBody(email);
            var response = _client.Post(request);

            Log.InfoFormat("Sending mail. Request: \n {0} \n Response: \n {1}", request.Parameters[0], response.Content);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                var error = JsonConvert.DeserializeObject<ErrorResponseModel>(response.Content);
                Log.Error(new MandrillException(error, string.Format("Post failed {0}", _config.SendTemplatePath)));
                return "InternalServerError";
            }
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log.Error(response.ErrorException);
                return "HttpStatusCode not OK";
            }
            var responseDeserialized = JsonConvert.DeserializeObject<List<EmailResultModel>>(response.Content);
            if (responseDeserialized[0].status.ToLower() != "sent")
            {
                Log.Warn(responseDeserialized[0].ToString());
                return "status not 'sent'";
            }
            Log.Info(responseDeserialized[0].ToString());
            return "OK";
        }
        //----------------------------------------------------------------------------------
        public string ForUnitTest(string emailTo, Dictionary<string, string> vars)
        {
            var message = PrepareEmail(_config.FinishWizardTemplateName, emailTo, vars, "Finish Application");
            return Send(message);
        }

        public void SendMessageFinishWizard(string emailTo, string fullName)
        {
            var vars = new Dictionary<string, string>
                {
                    {"CUSTOMER_NAME", fullName}, 
                };
            var message = PrepareEmail(_config.FinishWizardTemplateName, emailTo, vars, "Finish Application");
            Send(message);
        }
    }
}