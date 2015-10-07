namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;
	using Twilio;
	using System.Collections.Generic;

	public class SendSms : AStrategy {
		public SendSms(int? userId, int underwriterId, string sMobilePhone, string content, bool? phoneOriginIsrael = false) {
			this.userId = userId;
			this.underwriterId = underwriterId;

			this.mobilePhone = sMobilePhone;
			this.content = content;

			this.accountSid = ConfigManager.CurrentValues.Instance.TwilioAccountSid;
			this.authToken = ConfigManager.CurrentValues.Instance.TwilioAuthToken;
			
			this.fromNumberUK = ConfigManager.CurrentValues.Instance.TwilioSendingNumber;
			this.fromNumberIsrael = ConfigManager.CurrentValues.Instance.TwilioSendingNumberIsrael;

			this.phoneOriginIsrael = phoneOriginIsrael ?? false;

		} // constructor

		public override string Name {
			get { return "Send SMS"; }
		} // Name

		public override void Execute() {
			//Debug mode
			if (this.mobilePhone == "01111111111") {
				Result = true;
				Log.Info("Send SMS Debug mode: customer id: {0} content: {1}", this.userId, this.content);
				return;
			}

			if (string.IsNullOrEmpty(this.content) || string.IsNullOrEmpty(this.mobilePhone) || this.mobilePhone.Length < 1) {
				Log.Info("Empty content / wrong phone number, message is not sent");
				return;
			}


			var twilio = new TwilioRestClient(this.accountSid, this.authToken);

			var mobilePrefix = this.phoneOriginIsrael ? IsraelMobilePrefix : UkMobilePrefix;
			this.sendMobilePhone = string.Format("{0}{1}", mobilePrefix, this.mobilePhone.Substring(1));

			var fromNumber = this.phoneOriginIsrael ? this.fromNumberIsrael : this.fromNumberUK;
			this.dateSent = DateTime.UtcNow;
			Log.Info("Sending sms to customer:{2}, number {0}, content:{1}", this.sendMobilePhone, this.content, this.userId);
			var smsResponse = twilio.SendMessage(fromNumber, this.sendMobilePhone, this.content);
			
			SaveSms(smsResponse);
		}

		private void SaveSms(Message smsResponse) {
			try {
				var message = EzbobSmsMessage.FromMessage(smsResponse);
				message.UserId = this.userId;
				message.UnderwriterId = this.underwriterId;
				message.DateSent = this.dateSent;

				if (smsResponse.RestException != null) {
					string restException = string.Format("RestException Code:{0}, Status:{3}, Message:{1}, MoreInfo:{2}",
						smsResponse.RestException.Code, smsResponse.RestException.Message, smsResponse.RestException.MoreInfo, smsResponse.RestException.Status);
					Log.Warn("Failed sending SMS to number:{0}\n{1}", this.sendMobilePhone, restException);
					Result = false;
					return;
				}
				
				if(smsResponse.ErrorCode.HasValue || !string.IsNullOrEmpty(smsResponse.ErrorMessage)){
					Log.Warn("Failed sending SMS to number:{0}\n{1} {2}", this.sendMobilePhone, smsResponse.ErrorCode, smsResponse.ErrorMessage);
					message.Status = smsResponse.ErrorCode + " " + smsResponse.ErrorMessage;
				}
				
				DB.ExecuteNonQuery("SaveSmsMessage", CommandSpecies.StoredProcedure,
								DB.CreateTableParameter<EzbobSmsMessage>("Tbl", new List<EzbobSmsMessage> { message }));

				Result = true;
				Log.Info("Sms message sent to '{0}'. Sid:'{1}'", this.sendMobilePhone, message.Sid);
			} catch (Exception ex) {
				Log.Error("Failed saving twilio SMS send response to DB: {0}", ex.Message);
				Result = false;
			}
		}

		public bool Result { get; private set; }
		private readonly int? userId;
		private readonly int underwriterId;
		private readonly string mobilePhone;
		private readonly string content;
		private const string UkMobilePrefix = "+44";
		private const string IsraelMobilePrefix = "+972";
		private readonly string accountSid;
		private readonly string authToken;
		private DateTime dateSent;
		private string sendMobilePhone;
		private readonly string fromNumberUK;
		private readonly string fromNumberIsrael;
		private readonly bool phoneOriginIsrael;
	} // class GenerateMobileCode
} // namespace
