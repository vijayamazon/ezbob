using System;


namespace com.paypal.sdk.core
{
    public class OAuthException :Exception
    {
        #region Priavte Members
        /// <summary>
        /// Short message
        /// </summary>
        private string OauthExpMessage;
        /// <summary>
        /// Long message
        /// </summary>
        private string OauthExpLongMessage;

        #endregion

        #region Constructors

        public OAuthException(string OauthExceptionMessage, Exception exception)
        {
            this.OauthExpMessage = OauthExceptionMessage;
            this.OauthExpLongMessage = exception.Message;
        }
        public OAuthException(string OauthExceptionMessage)
        {
            this.OauthExpMessage = OauthExceptionMessage;
        }

        #endregion

        #region Public Properties

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

        #endregion
       
    }
}
