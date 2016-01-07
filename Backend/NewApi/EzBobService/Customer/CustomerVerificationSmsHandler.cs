namespace EzBobService.Customer {
    using System.Text;
    using System.Threading.Tasks;
    using EzBob3dPartiesApi.Twilio;
    using EzBobApi.Commands.Customer;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobCommon.Utils;
    using EzBobCommon.Utils.Encryption;
    using EzBobPersistence.MobilePhone;
    using EzBobService.Misc;
    using EzBobService.ThirdParties;
    using EzBobService.ThirdParties.Twilio;
    using NServiceBus;

    public class CustomerVerificationSmsHandler : HandlerBase<CustomerSendVerificationSmsCommandResponse>, IHandleMessages<CustomerSendVerificationSmsCommand>, IHandleMessages<TwilioSendSmsCommandResponse> {

        [Injected]
        public ThirdPartyServiceConfig Config { get; set; }

        [Injected]
        public IMobilePhoneQueries MobilePhoneQueries { get; set; }

        [Injected]
        public TwilioSmsAsyncSendReceive TwilioSmsAsyncSendReceive { get; set; }

        [Injected]
        public VerificationHelper VerificationHelper { get; set; }

        /// <summary>
        /// Handles the specified request.
        /// </summary>
        /// <param name="command">The request.</param>
        public async void Handle(CustomerSendVerificationSmsCommand command) {
            string verificationWord = StringUtils.GenerateRandomEnglishString();

            TwilioSendSmsCommand sendSmsCommand = CreateSendSmsCommand(command, verificationWord);
            InfoAccumulator info = new InfoAccumulator();

            try {
                var response = await TwilioSmsAsyncSendReceive.SendAsync(Config.Address, sendSmsCommand);
                if (response.Sms != null) {
                    response.Sms.UserId = int.Parse(EncryptionUtils.SafeDecrypt(command.CustomerId));
                    MobilePhoneQueries.SaveTwilioSms(response.Sms);
                } else {
                    info.AddError("error sending sms");
                }

                SendReply(info, command);
            } catch (TaskCanceledException ex) {
                Log.Error("Time out on sending sms");
                info.AddError("Time out sending sms");
                SendReply(info, command);
            }
        }

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Handle(TwilioSendSmsCommandResponse message) {}

        /// <summary>
        /// Creates the send SMS command.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="verificationWord">The verification word.</param>
        /// <returns></returns>
        private TwilioSendSmsCommand CreateSendSmsCommand(CustomerSendVerificationSmsCommand request, string verificationWord)
        {
            TwilioSendSmsCommand sendSmsCommand = new TwilioSendSmsCommand();
            
            StringBuilder msgBuilder = new StringBuilder();

            if (!string.IsNullOrEmpty(request.MessageHeader)) {
                msgBuilder.Append(request.MessageHeader);
                msgBuilder.AppendLine();
            } else {
                msgBuilder.Append("Your verification code is: ");
                msgBuilder.AppendLine();
            }

            msgBuilder.Append(verificationWord);
            
            if (!string.IsNullOrEmpty(request.MessageFooter)) {
                msgBuilder.AppendLine();
                msgBuilder.Append(request.MessageFooter);
            }

            sendSmsCommand.Message = msgBuilder.ToString();
            sendSmsCommand.PhoneNumber = request.PhoneNumber;
            return sendSmsCommand;
        }
    }
}
