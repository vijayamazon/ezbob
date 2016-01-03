using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Yodlee.RequestResponse
{
    using EzBob3dParties.Yodlee.Models;
    using EzBob3dParties.Yodlee.Models.Login;

    #region RESPONSE EXAMPLE
    /*
     
     * {
   "userContext":{
      "conversationCredentials":{
         "sessionToken":"06142010_2:6e2835d48bacb8e324ea6f1984b959a221b694fb746d608fbf6978fbfcac6a55a27241e687f8c7110323c190d4f852f560324f3067a9ab3a9c91685e1c7675f2"
      },
      "valid":true,
      "isPasswordExpired":false,
      "cobrandId":10000004,
      "channelId":-1,
      "locale":"en_US",
      "tncVersion":2,
      "applicationId":"17CBE222A42161A3FF450E47CF4C1A00",
      "cobrandConversationCredentials":{
         "sessionToken":"06142010_0:479415bf75ed206b94fe5430dcdc5ca639d95f40e99c3ff2102458b09966bace89cd5ce3272ae42aadf9727fe189223649d025f0d6441bffab5cb42c0005ecc8"
      },
      "preferenceInfo":{
         "currencyCode":"USD",
         "timeZone":"PST",
         "dateFormat":"MM/dd/yyyy",
         "currencyNotationType":{
         "currencyNotationType":"SYMBOL"
      },
      "numberFormat":{
         "decimalSeparator":".",
         "groupingSeparator":",",
         "groupPattern":"###,##0.##"
      }
      }
   },
   "lastLoginTime":1377062991,
   "loginCount":0,
   "passwordRecovered":false,
   "emailAddress":"johndoe@abc.com",
   "loginName":"Arch1376475200981",
   "userId":10499359
}
     
     */
    
    #endregion

    class YRegisterUserResponse : YResponseBase
    {
        public UserContext userContext { get; set; }
        public bool valid { get; set; }
        public bool isPasswordExpired { get; set; }
        public int cobrandId { get; set; }
        public int channelId { get; set; }
        public string locale { get; set; }
        public int tncVersion { get; set; }
        public string applicationId { get; set; }
        public ConversationCredentials cobrandConversationCredentials { get; set; }
        public PreferenceInfo preferenceInfo { get; set; }
        public NumberFormat numberFormat { get; set; }
        public int lastLoginTime { get; set; }
        public int loginCount { get; set; }
        public bool passwordRecovered { get; set; }
        public string emailAddress { get; set; }
        public string loginName { get; set; }
        public string userId { get; set; }
    }
}
