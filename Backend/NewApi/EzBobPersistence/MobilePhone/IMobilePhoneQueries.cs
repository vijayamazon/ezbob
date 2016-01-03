namespace EzBobPersistence.MobilePhone {
    using EzBobModels.MobilePhone;
    using EzBobModels.ThirdParties.Twilio;

    public interface IMobilePhoneQueries {
        /// <summary>
        /// Validates the mobile phone.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        bool ValidateMobilePhoneNumber(string phoneNumber, string code);

        /// <summary>
        /// Gets the current mobile code count information.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <returns></returns>
        CountInfo GetCurrentMobileCodeCountInfo(string phoneNumber);

        /// <summary>
        /// Stores the mobile phone and code.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        bool StoreMobilePhoneAndCode(string phoneNumber, string code);

        /// <summary>
        /// Saves the Twilio SMS.
        /// </summary>
        /// <param name="sms">The SMS.</param>
        /// <returns></returns>
        bool SaveTwilioSms(TwilioSms sms);
    }
}

