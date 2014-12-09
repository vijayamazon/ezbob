using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using EZBob.DatabaseLib.Model.Database;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using log4net;

namespace EzBob.AmazonServiceLib.ServiceCalls
{

    public class AmazonServiceAskville
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AmazonServiceAskville));
		private readonly string askvilleAmazonLogin;
		private readonly string askvilleAmazonPass;
        private const string ContactUrl = "https://www.amazon.co.uk/gp/help/contact/contact.html?assistanceType=order&rg2=on&subject=38&writeMessageButton=Continue&step=submitEntry&asin=&orderId=&recipientId={0}&noJS=false";

        private const string SendUrl = "https://www.amazon.co.uk/gp/help/contact/contact.html?qsfp_sellerID={0}&qsfp_marketplaceID={1}";

		public AmazonServiceAskville(string askvilleAmazonLogin, string askvilleAmazonPass)
		{
			this.askvilleAmazonLogin = askvilleAmazonLogin;
			this.askvilleAmazonPass = askvilleAmazonPass;
		}

        public AskvilleSendStatus AskQuestion(string sellerId, string marketplaceId, int subject, string message)
        {
            try
            {
                Log.InfoFormat("Sending askville message to sellerId = {0}, marketplaceId={1}, subject={2}, message={3}", sellerId, marketplaceId, subject, message);
                var contactUrl = string.Format(ContactUrl, sellerId);
                var sendUrl = string.Format(SendUrl, sellerId, marketplaceId);
                var urlParams = new[]
                {
                    "_encoding=UTF8",
                    "openid.assoc_handle=gbflex",
                    "openid.return_to=" +GetEncoded(contactUrl),
                    "openid.mode=checkid_setup",
                    "openid.ns="+GetEncoded("http://specs.openid.net/auth/2.0"),
                    "openid.claimed_id="+GetEncoded("http://specs.openid.net/auth/2.0/identifier_select"),
                    "openid.pape.max_auth_age=900",
                    "openid.ns.pape="+GetEncoded("http://specs.openid.net/extensions/pape/1.0"),
                    "openid.identity="+GetEncoded("http://specs.openid.net/auth/2.0/identifier_select")
                };

                var response = MakeRequest("https://www.amazon.co.uk/ap/signin", "GET", null, urlParams, null, null);

                var html = new HtmlDocument();
                html.Load(response.GetResponseStream());
                var action = html.DocumentNode.QuerySelector("form[name=\"signIn\"]").Attributes["action"].Value;
                var reqParams = CollectHiddens(html, "signIn");
                if(reqParams == null)
                {
                    Log.Error("Sending askville message failed, login protocol has been changed or invalid user name/password");
                    return AskvilleSendStatus.InvalidAmazonLogin;
                }
                reqParams.Add("email=" + GetEncoded(askvilleAmazonLogin));
                reqParams.Add("password=" + GetEncoded(askvilleAmazonPass));
                reqParams.Add("create=0");
                reqParams.Add("x=152");
                reqParams.Add("y=5");
                var loginResponse = MakeRequest(action, "POST", "application/x-www-form-urlencoded", null, reqParams.ToArray(), response.Cookies);
                response = MakeRequest(contactUrl, "GET", null, null, null, loginResponse.Cookies);

                html.Load(response.GetResponseStream());
                reqParams = CollectHiddens(html, "writeMessageForm");
                if (reqParams == null)
                {
                    Log.Error("Sending askville message failed, invalid sellerId or marketplaceId. Possible sending protocol has been changed.");
                    return AskvilleSendStatus.InvalidSellerOrMarketplace;
                }
                reqParams.Add("commMgrComments=" + GetEncoded(message));
                reqParams.Add("sendEmailButton=" + GetEncoded("Send e-mail"));
                response = MakeRequest(sendUrl, "POST", "application/x-www-form-urlencoded", null, reqParams.ToArray(), loginResponse.Cookies);

                html.Load(response.GetResponseStream());

                if (html.DocumentNode.InnerHtml.ToLowerInvariant().Contains("message sent"))
                {
                    Log.Info("Sending askville message completed successfully");
                    return AskvilleSendStatus.Success;
                }

                Log.Error("Sending askville message failed, because web server response doesn`t contain expected success message");
                return AskvilleSendStatus.Exception;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return AskvilleSendStatus.Exception;
            }
        }

        //-----------------------------------------------------------------------------------
        private string GetEncoded(string input)
        {
            return HttpUtility.UrlEncode(input);
        }

        //-----------------------------------------------------------------------------------
        private HttpWebResponse MakeRequest(string url, string method, string contentType, string[] urlParams, string[] postParams, CookieCollection cookies)
        {
            if (urlParams != null) url += "?" + string.Join("&", urlParams);
            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = method;
            request.AllowAutoRedirect = false;
            if (!string.IsNullOrEmpty(contentType)) request.ContentType = contentType;

            request.CookieContainer = new CookieContainer();
            if (cookies != null)
            {
                foreach (Cookie cookie in cookies)
                {
                    cookie.Version = 0;
                    request.CookieContainer.Add(cookie);
                }
            }

            if (postParams != null)
            {
                var postStr = string.Join("&", postParams);
                var stOut = new StreamWriter(request.GetRequestStream(), Encoding.ASCII);
                stOut.Write(postStr);
                stOut.Close();
            }
            return request.GetResponse() as HttpWebResponse;
        }

        //-----------------------------------------------------------------------------------
        private List<string> CollectHiddens(HtmlDocument doc, string formName)
        {
            var node = doc.DocumentNode.QuerySelector(string.Format("form[name=\"{0}\"]", formName)) ?? doc.DocumentNode.QuerySelector(string.Format("form[id=\"{0}\"]", formName));
            return CollectHiddens(node);
        }

        //-----------------------------------------------------------------------------------
        private List<string> CollectHiddens(HtmlNode node)
        {
            return node == null ? null : (from hidden in node.ParentNode.QuerySelectorAll("input[type=\"hidden\"]") let name = hidden.Attributes["name"].Value let val = GetEncoded(hidden.Attributes["value"].Value) select name + "=" + val).ToList();
        }
    }
}
