﻿using System.Collections.Generic;
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

        public Mail()
        {
            _config = ConfigurationRootBob.GetConfiguration().MandrillConfig;
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

        private void Send(EmailModel email)
        {
            var request = new RestRequest(_config.SendTemplatePath, Method.POST) { RequestFormat = DataFormat.Json };
            request.AddBody(email);
            var response = _client.Post(request);

            Log.InfoFormat("Sending mail. Request: \n {0} \n Response: \n {1}", request.Parameters[0], response.Content);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                var error = JsonConvert.DeserializeObject<ErrorResponseModel>(response.Content);
                Log.Error(new MandrillException(error, string.Format("Post failed {0}", _config.SendTemplatePath)));
                return;
            }
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log.Error(response.ErrorException);
                return;
            }
            var responseDeserialized = JsonConvert.DeserializeObject<List<EmailResultModel>>(response.Content);
            if (responseDeserialized[0].status.ToLower() != "sent")
            {
                Log.Warn(responseDeserialized[0].ToString());
                return;
            }
            Log.Info(responseDeserialized[0].ToString());
        }
        //----------------------------------------------------------------------------------

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