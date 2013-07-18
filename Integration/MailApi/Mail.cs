using System;
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

        //Api pathes
        private const string BaseSecureUrl = "https://mandrillapp.com/api/1.0/";
        private const string SendTemplatePath = "/messages/send-template.json";
        private const string RenderTemplatePath = "/templates/render.json";

        public Mail(IMandrillConfig config = null)
        {
            _config = config ?? ConfigurationRootBob.GetConfiguration().MandrillConfig;
            _client = new RestClient(BaseSecureUrl);
            _client.AddHandler("application/json", new JsonDeserializer());
        }

        private EmailModel PrepareEmail(string templateName, string to,  Dictionary<string,string> variables, string subject, string cc="")
        {
            var message = new EmailModel
            {
                key = _config.Key,
                template_name = templateName,
                message = new EmailMessageModel
                {
                    to = new[] { new EmailAddressModel { email = to } },
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

        private string SendRequest(string path, object model)
        {
            if (!_config.Enable)
            {
                const string retVal = "Mandrrill is disabled";
                Log.Warn(retVal);
                return retVal;
            }
            var request = new RestRequest(path, Method.POST) { RequestFormat = DataFormat.Json };
            request.AddBody(model);
            var response = _client.Post(request);

            Log.InfoFormat("Mandrill service call.\n Request: \n {0} \n Response: \n {1}", request.Parameters[0], response.Content);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                var error = JsonConvert.DeserializeObject<ErrorResponseModel>(response.Content);
                throw new MandrillException(error, string.Format("InternalServerError. Post failed {0}", SendTemplatePath));
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw response.ErrorException;
            }

            return response.Content;
        }

        private string Send(EmailModel email)
        {
            var response = SendRequest(SendTemplatePath, email);
            var responseDeserialized = JsonConvert.DeserializeObject<List<EmailResultModel>>(response);
            if (responseDeserialized[0].status.ToLower() != "sent")
            {
                Log.Warn(responseDeserialized[0].ToString());
                return "status not 'sent'";
            }
            return "OK";
        }

        private string RenderTemplate(Dictionary<string, string> parameters, string templateName)
        {
            var templateModel = new RenderTemplateModel
                {
                    key = _config.Key,
                    template_name = templateName,
                    template_content = new template_content[] {},
                    merge_vars = Utils.AddMergeVars(parameters)
                };
            var response = SendRequest(RenderTemplatePath, templateModel);
            var retVal = JsonConvert.DeserializeObject<Dictionary<string, string>>(response); //response is { "html": "response text" }
            return retVal["html"];
        }
        //----------------------------------------------------------------------------------
        public string Send(Dictionary<string, string> parameters, string to, string templateName, string subject = "", string cc = "")
        {
            var message = PrepareEmail(templateName, to, parameters, subject, cc);
            return Send(message);
        }

        public string GetRenderedTemplate(Dictionary<string, string> parameters, string templateName)
        {
            return RenderTemplate(parameters, templateName);
        }
    }
}