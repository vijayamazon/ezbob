using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace GmailAPI {
    public class GmailOps {
        private const string ApplicationName = "Gmail API Quickstart";

        private static readonly string[] Scopes = new string[]
        {
            GmailService.Scope.GmailReadonly
        };

        public string ExtactLinkFromMessage(string mailTitle, string contactEmail, string enviorment) {
            UserCredential credential;
            using (FileStream stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read)) {
                string credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials");
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets, GmailOps.Scopes, "test@ezbob.com", CancellationToken.None, new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }
            GmailService service = new GmailService(new BaseClientService.Initializer {
                HttpClientInitializer = credential,
                ApplicationName = "Gmail API Quickstart"
            });
            List<Message> messageList = GmailOps.ListMessages(service, "me", "subject: " + mailTitle);
            foreach (Message message in messageList) {
                Message fullMessage = GmailOps.GetMessage(service, "me", message.Id);
                MessagePartHeader messageInfo = fullMessage.Payload.Headers.FirstOrDefault((MessagePartHeader x) => x.Name == "Delivered-To");
                if (messageInfo != null && string.Equals(messageInfo.Value, contactEmail)) {
                    string bodyData = fullMessage.Payload.Body.Data.Replace("-", "+").Replace("_", "/");
                    byte[] data64Base = Convert.FromBase64String(bodyData);
                    string htmlCode = Encoding.UTF8.GetString(data64Base);
                    Regex linkParser = new Regex("http(s)?://([\\w-]+\\.)+[\\w-]+(/[\\w- ./?%&=]*)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    foreach (object link in linkParser.Matches(htmlCode)) {
                        if (link.ToString().Contains(enviorment)) {
                            return link.ToString();
                        }
                    }
                }
            }
            return null;
        }

        public bool CheckIncomingMessages(string mailTitle, string contactEmail) {
            UserCredential credential;
            using (FileStream stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read)) {
                string credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials");
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets, GmailOps.Scopes, "test@ezbob.com", CancellationToken.None, new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }
            GmailService service = new GmailService(new BaseClientService.Initializer {
                HttpClientInitializer = credential,
                ApplicationName = "Gmail API Quickstart"
            });
            List<Message> messageList = GmailOps.ListMessages(service, "me", "subject: " + mailTitle);

            foreach (Message message in messageList) {
                Message fullMessage = GmailOps.GetMessage(service, "me", message.Id);
                MessagePartHeader messageInfo = fullMessage.Payload.Headers.FirstOrDefault((MessagePartHeader x) => x.Name == "Delivered-To");
                if (messageInfo != null && string.Equals(messageInfo.Value, contactEmail)) {
                    return true;
                }
            }
            return false;
        }

        public static List<Message> ListMessages(GmailService service, string userId, string query) {
            List<Message> result = new List<Message>();
            UsersResource.MessagesResource.ListRequest request = service.Users.Messages.List(userId);
            request.Q = query;
            do {
                try {
                    ListMessagesResponse response = request.Execute();
                    result.AddRange(response.Messages);
                    request.PageToken = response.NextPageToken;
                } catch (Exception e) {
                    Console.WriteLine("An error occurred: " + e.Message);
                }
            }
            while (!string.IsNullOrEmpty(request.PageToken));
            return result;
        }

        public static Message GetMessage(GmailService service, string userId, string messageId) {
            Message result;
            try {
                result = service.Users.Messages.Get(userId, messageId).Execute();
                return result;
            } catch (Exception e) {
                Console.WriteLine("An error occurred: " + e.Message);
            }

            return null;
        }
    }
}
