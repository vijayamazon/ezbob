using System;

namespace com.paypal.sdk.core
{
    public class OAuthException :Exception
    {

        /// <summary>
        /// Short message
        /// </summary>
        private string OauthExpMessage;
        /// <summary>
        /// Long message
        /// </summary>
        private string OauthExpLongMessage;

        public OAuthException(string OauthExceptionMessage, Exception exception)
        {
            this.OauthExpMessage = OauthExceptionMessage;
            this.OauthExpLongMessage = exception.Message;
        }
        public OAuthException(string OauthExceptionMessage)
        {
            this.OauthExpMessage = OauthExceptionMessage;
        }

        /// <summary>
        /// Short message.
        /// </summary>
        public string OauthExceptionMessage
        {
            get
            {
                return OauthExpMessage;
            }
            set
            {
                OauthExpMessage = value;
            }
        }

        /// <summary>
        /// Long message
        /// </summary>
        public string OauthExceptionLongMessage
        {
            get
            {
                return OauthExpLongMessage;
            }
            set
            {
                OauthExpLongMessage = value;
            }
        }

    }
}
