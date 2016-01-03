namespace EzBob3dParties.Twilio
{
    using System.Threading.Tasks;
    using EzBobModels.ThirdParties.Twilio;
    using global::Twilio;

    public interface ITwilio {
        /// <summary>
        /// Sends the SMS.
        /// </summary>
        /// <param name="toPhoneNumber">To phone number.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        Task<Message> SendSms(string toPhoneNumber, string message);
    }
}