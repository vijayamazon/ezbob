using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.Yodlee.RequestResponse
{
    /// <summary>
    /// see https://developer.yodlee.com/Aggregation_API/Aggregation_Services_Guide/Aggregation_REST_API_Reference/register3
    /// </summary>
    internal class YRegisterUserRequest : YRequestBase {
        private static readonly string yUserName = "userCredentials.loginName";
        private static readonly string yPassword = "userCredentials.password";

        private static readonly string yObjectInstanceType = "userCredentials.objectInstanceType";
        private static readonly string yObjectInstanceTypeValue = "com.yodlee.ext.login.PasswordCredentials";

        private static readonly string yEmailAddress = "userProfile.emailAddress";

        public YRegisterUserRequest SetCobrandSessionToken(string coBrandToken)
        {
            Insert(CobSessionToken, coBrandToken);
            return this;
        }

        public YRegisterUserRequest SetUsername(string userName) {
            Insert(yUserName, userName);
            return this;
        }

        public YRegisterUserRequest SetPassword(string password) {
            Insert(yPassword, password);
            //required by yodlee api
            Insert(yObjectInstanceType, yObjectInstanceTypeValue);
            return this;
        }

        public YRegisterUserRequest SetEmailAddress(string emailAddress) {
            Insert(yEmailAddress, emailAddress);
            return this;
        }
    }
}
