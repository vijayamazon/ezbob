namespace EzBobAcceptanceTests.Customer.Requests {
    using EzBobAcceptanceTests.Infra.Utils;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents body of customer sign-up REST request
    /// </summary>
    public class SignupRequest : JObject {

        public SignupRequest WithRandomEmailAddress() {
            return WithEmailAddress(Generator.GetRandomEmailAddress());
        }

        public SignupRequest WithEmailAddress(string emailAddress) {
            this["EmailAddress"] = emailAddress;
            return this;
        }

        public SignupRequest WithRandomPassword() {
            return WithPassword(Generator.GetRandomPassword());
        }

        public SignupRequest WithPassword(string password) {
            this["Password"] = password;
            return this;
        }

        public SignupRequest WithPhoneNumber(string phoneNumber) {
            this["PhoneNumber"] = phoneNumber;
            return this;
        }

        public SignupRequest WithVerificationCode(string verificationCode) {
            this["PhoneNumberValidationCode"] = verificationCode;
            return this;
        }

        public SignupRequest WithSecurityQuestionId(int id) {
            this["SequrityQuestionId"] = id;
            return this;
        }

        public SignupRequest WithSecurityQuestionAnswer(string answer) {
            this["SecurityQuestionAnswer"] = answer;
            return this;
        }
    }
}
