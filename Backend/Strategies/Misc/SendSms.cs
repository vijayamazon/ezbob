namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;
	using Twilio;
	using System.Collections.Generic;

	public class SendSms : AStrategy {
		public SendSms(int? userId, int underwriterId, string sMobilePhone, string content, bool? phoneOriginIsrael = false) {
			this.m_nUserId = userId;
			this.m_nUnderwriterId = underwriterId;

			this.m_sMobilePhone = sMobilePhone;
			this.m_sContent = content;

			this.m_sAccountSid = ConfigManager.CurrentValues.Instance.TwilioAccountSid;
			this.m_sAuthToken = ConfigManager.CurrentValues.Instance.TwilioAuthToken;
			this.m_sFromNumber = ConfigManager.CurrentValues.Instance.TwilioSendingNumber;

			this.fromNumberUK = ConfigManager.CurrentValues.Instance.TwilioSendingNumber;
			this.fromNumberIsrael = ConfigManager.CurrentValues.Instance.TwilioSendingNumberIsrael;

			this.phoneOriginIsrael = phoneOriginIsrael ?? false;

		} // constructor

		public override string Name {
			get { return "Send SMS"; }
		} // Name

		public override void Execute() {
			//Debug mode
			if (this.m_sMobilePhone == "01111111111") {
				Result = true;
				Log.Info("Send SMS Debug mode: customer id: {0} content: {1}", this.userId, this.content);
				return;
			}

			if (string.IsNullOrEmpty(this.m_sContent)) {
				Log.Info("Empty content message is not sent");
				return;
			}


			var twilio = new TwilioRestClient(this.m_sAccountSid, this.m_sAuthToken);

			var mobilePrefix = this.phoneOriginIsrael ? ISrMobilePrefix : UkMobilePrefix;
			this._sendMobilePhone = string.Format("{0}{1}", mobilePrefix, this.m_sMobilePhone.Substring(1));

			this.m_sFromNumber = this.phoneOriginIsrael ? this.fromNumberIsrael : this.fromNumberUK;
			this._dateSent = DateTime.UtcNow;
			Log.Info("Sending sms to customer:{2}, number {0}, content:{1}", this._sendMobilePhone, this.m_sContent, this.m_nUserId);
			var smsResponse = twilio.SendMessage(this.m_sFromNumber, this._sendMobilePhone, this.m_sContent);
			
			SaveSms(smsResponse);
		}

		private void SaveSms(Message smsResponse) {
			try {
				var message = EzbobSmsMessage.FromMessage(smsResponse);
				message.UserId = this.m_nUserId;
				message.UnderwriterId = this.m_nUnderwriterId;
				message.DateSent = this._dateSent;

				if (smsResponse.RestException != null) {
					string restException = string.Format("RestException Code:{0}, Status:{3}, Message:{1}, MoreInfo:{2}",
						smsResponse.RestException.Code, smsResponse.RestException.Message, smsResponse.RestException.MoreInfo, smsResponse.RestException.Status);
					Log.Warn("Failed sending SMS to number:{0}\n{1}", this._sendMobilePhone, restException);
					Result = false;
					return;
				}
				
				if(smsResponse.ErrorCode.HasValue || !string.IsNullOrEmpty(smsResponse.ErrorMessage)){
					Log.Warn("Failed sending SMS to number:{0}\n{1} {2}", this._sendMobilePhone, smsResponse.ErrorCode, smsResponse.ErrorMessage);
					message.Status = smsResponse.ErrorCode + " " + smsResponse.ErrorMessage;
				}
				
				DB.ExecuteNonQuery("SaveSmsMessage", CommandSpecies.StoredProcedure,
								DB.CreateTableParameter<EzbobSmsMessage>("Tbl", new List<EzbobSmsMessage> { message }));

				Result = true;
				Log.Info("Sms message sent to '{0}'. Sid:'{1}'", this._sendMobilePhone, message.Sid);
			} catch (Exception ex) {
				Log.Error("Failed saving twilio SMS send response to DB: {0}", ex.Message);
				Result = false;
			}
		}

		public bool Result { get; private set; }
		private readonly int? m_nUserId;
		private readonly int underwriterId;
		private readonly string mobilePhone;
		private readonly string content;
		private const string UkMobilePrefix = "+44";
		private const string ISrMobilePrefix = "+972";
		private readonly string m_sAccountSid;
		private readonly string m_sAuthToken;
		private DateTime dateSent;
		private string sendMobilePhone;
		private readonly string fromNumberUK;
		private readonly string fromNumberIsrael;
		private readonly bool phoneOriginIsrael;
	} // class GenerateMobileCode
} // namespace
