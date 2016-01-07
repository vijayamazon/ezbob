namespace EzBobService.Mobile {
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using Common.Logging;
    using EzBob3dPartiesApi.Twilio;
    using EzBobCommon;
    using EzBobModels.MobilePhone;
    using EzBobPersistence.MobilePhone;
    using EzBobService.ThirdParties;
    using EzBobService.ThirdParties.Twilio;

    /// <summary>
    /// Holds methods responsible for 
    /// </summary>
    internal class MobilePhone {

        [Injected]
        public ILog Log { get; set; }

        [Injected]
        public TwilioSmsAsyncSendReceive TwilioSmsSender { get; set; }

        [Injected]
        public IMobilePhoneQueries MobilePhoneQueries { get; set; }

        [Injected]
        public MobilePhoneConfig Config { get; set; }

        [Injected]
        public ThirdPartyServiceConfig ThirdPartyService { get; set; }

        /// <summary>
        /// Authorizes the mobile phone number.
        /// </summary>
        /// <param name="mobilePhoneNumber">The mobile phone number.</param>
        /// <param name="countryName">Name of the country.</param>
        /// <returns></returns>
        public InfoAccumulator AuthorizeMobilePhoneNumber(string mobilePhoneNumber, CountryName countryName = CountryName.UK) {
            InfoAccumulator errors = new InfoAccumulator();

            //validate debug mode
            if (IsInDebugMode(mobilePhoneNumber)) {
                return errors;
            }

            CountInfo countInfo = MobilePhoneQueries.GetCurrentMobileCodeCountInfo(mobilePhoneNumber);

            //check whether can continue or not
            if (!CanGenerateCodeAndSendSms(countInfo, mobilePhoneNumber, errors)) {
                return errors;
            }

            //generate code
            string code = GenerateAuthorizationCode();

            //save phone number and generated code
            //we do not care whether this operation is succeeded or not (only put logs) and continue
            SaveGeneratedCodeAndPhoneNumber(mobilePhoneNumber, code, errors);
            

            string message = string.Format("Your authentication code is: {0}", code);

            //send sms and save it in DB
            //we do not care whether this operation is succeeded or not and continue
            SendSmsAndSave(mobilePhoneNumber, message, countryName, errors);

            return errors;
        }

        /// <summary>
        /// Sends the SMS to user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="underwriterId">The underwriter identifier.</param>
        /// <param name="mobilePhoneNumber">The mobile phone number.</param>
        /// <param name="message">The message.</param>
        /// <param name="country">The country.</param>
        /// <returns></returns>
        public InfoAccumulator SendSmsToUser(int? userId, int underwriterId, string mobilePhoneNumber, string message, CountryName country)
        {
            InfoAccumulator errors = new InfoAccumulator();

            if (IsInDebugMode(mobilePhoneNumber))
            {
                return errors;
            }

            Log.InfoFormat("Sending sms to customer:{2}, number {0}, content:{1}", mobilePhoneNumber, message, userId);
            SendSmsAndSave(mobilePhoneNumber, message, country, errors);

            return errors;
        }

        /// <summary>
        /// Validates the mobile phone number.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public InfoAccumulator ValidateMobilePhoneNumber(string phoneNumber, string code)
        {
            InfoAccumulator errors = new InfoAccumulator();

            if (string.IsNullOrEmpty(phoneNumber))
            {
                String errorMsg = "got empty phone number";
                Log.Error(errorMsg);
                errors.AddError(errorMsg);
                return errors;
            }

            if (string.IsNullOrEmpty(code))
            {
                String errorMsg = "got empty code";
                Log.Error(errorMsg);
                errors.AddError(errorMsg);
                return errors;
            }

            if (!MobilePhoneQueries.ValidateMobilePhoneNumber(phoneNumber, code))
            {
                String errorMsg = String.Format("failed to validate phone number: {0} and code: {1}", phoneNumber, code);
                Log.Warn(errorMsg);
                errors.AddInfo(errorMsg);
            }

            return errors;
        } 

        /// <summary>
        /// Sends the SMS and if sent saves it in DB.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="message">The message.</param>
        /// <param name="country">The country.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        private async Task<bool> SendSmsAndSave(string phoneNumber, string message, CountryName country, InfoAccumulator errors) {

            //validate phone number
            if (string.IsNullOrEmpty(phoneNumber)) {
                String errorMessage = "attempt to send sms to an empty phone number";
                Log.Error(errorMessage);
                errors.AddError(errorMessage);
                return false;
            }

            //validate debug mode
            if (IsInDebugMode(phoneNumber)) {
                return true;
            }

            string toPhoneNumber = NormalizePhoneNumber(phoneNumber, country);

            //validate message
            if (string.IsNullOrEmpty(message)) {
                errors.AddInfo("not sending empty message");
                Log.Warn("attempt to send empty message to phone number: " + toPhoneNumber + " . SMS was not send");
                return false;
            }

            Log.InfoFormat("Sending sms to phone number: {0}, message: {1}", toPhoneNumber, message);

            //send sms
            TwilioSendSmsCommandResponse twilioResponse = await TwilioSmsSender.SendAsync(ThirdPartyService.Address, new TwilioSendSmsCommand {
                Message = message,
                PhoneNumber = toPhoneNumber
            });

            if (twilioResponse.Sms == null) {
                return false;
            }

            //save sms in DB
            if (!MobilePhoneQueries.SaveTwilioSms(twilioResponse.Sms)) {
                string errorMsg = String.Format("Failed to save Twilio SMS response to DB: {0}", message);
                Log.Error(errorMsg);
                errors.AddError(errorMsg);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Normalizes the phone number according to specified <see cref="T:EzBobService.MobilePhone.Country"/> code.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="country">The country.</param>
        /// <returns></returns>
        private string NormalizePhoneNumber(string phoneNumber, Country country)
        {
            return string.Format("{0}{1}", country.Code, phoneNumber.Substring(1));
        }

        /// <summary>
        /// We skip authorization in debug mode
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <returns></returns>
        //TODO: may be we can implement debug mode in some other way
        private bool IsInDebugMode(string phoneNumber)
        {
            return Config.SkipCodeGenerationNumber.Equals(phoneNumber, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Generates the authorization code.
        /// </summary>
        /// <returns></returns>
        private string GenerateAuthorizationCode()
        {
            var random = new Random();
            return (100000 + random.Next(899999)).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Saves the generated code and phone number.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <param name="code">The code.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        private bool SaveGeneratedCodeAndPhoneNumber(string phoneNumber, string code, InfoAccumulator errors)
        {
            if (!MobilePhoneQueries.StoreMobilePhoneAndCode(phoneNumber, code))
            {
                errors.AddError("could not save mobile phone and code");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether we can generate code and send SMS by examining count information.
        /// </summary>
        /// <param name="countInfo">The count information.</param>
        /// <param name="mobilePhoneNumber">The mobile phone number.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        private bool CanGenerateCodeAndSendSms(CountInfo countInfo, string mobilePhoneNumber, InfoAccumulator errors)
        {
            if (Config.MaxPerDay <= countInfo.SentToday)
            {
                string errorMsg = string.Format("Reached max number of daily SMS messages ({0}). SMS not sent", Config.MaxPerDay);
                errors.AddError(errorMsg);
                Log.Error(errorMsg);
                return false;
            }

            if (Config.MaxPerNumber <= countInfo.SentToNumber)
            {
                string errorMsg = string.Format("Reached max number of SMS messages ({0}) to number:{1}. SMS not sent", Config.MaxPerNumber, mobilePhoneNumber);
                errors.AddError(errorMsg);
                Log.Error(errorMsg);
                return false;
            }

            return true;
        } 
    }
}
