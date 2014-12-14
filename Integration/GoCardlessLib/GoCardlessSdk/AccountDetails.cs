using System;

namespace GoCardlessSdk
{
    public class AccountDetails
    {
        private string _appId;
        public string AppId
        {
            get
            {
                string appId = _appId ?? ConfigManager.CurrentValues.Instance.GoCardlessAppId;
                if (appId == null)
                {
                    throw new ArgumentException("Please supply your appId");
                }
                return appId;
            }
            set { _appId = value; }
        }

        private string _appSecret;
        public string AppSecret
        {
            get
            {
                string appSecret = _appSecret ?? ConfigManager.CurrentValues.Instance.GoCardlessAppSecret;
                if (appSecret == null)
                {
                    throw new ArgumentException("Please supply your appSecret");
                } 
                return appSecret;
            }
            set { _appSecret = value; }
        }

        // can be null
        private string _token;
        public string Token
        {
            get
            {
                string accessToken = _token ?? ConfigManager.CurrentValues.Instance.GoCardlessAccessToken;
                if (accessToken == null)
                {
                    throw new ArgumentException("Please supply your access token. You can also find this in the developer panel");
                }
                return accessToken;
            }
            set { _token = value; }
        }
    }
}