using System.IO;
using System.Net;

namespace ZohoCRM
{
    public class Zoho
    {
        private string _token;

        public string GenerateToken()
        {
            string url = "https://accounts.zoho.com/apiauthtoken/nb/create?SCOPE=ZohoCRM/crmapi&EMAIL_ID=nimrodk@ezbob.com&PASSWORD=ezbob2012";
            var data = new StreamReader(WebRequest.Create(url).GetResponse().GetResponseStream()).ReadToEnd();
            _token = data;
            return _token;
        }
    }
}
