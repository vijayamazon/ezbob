namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;
	using Twilio;
	using System.Collections.Generic;
	using System.Linq;

	public class SendSms : AStrategy {
		public SendSms(int userId, int underwriterId, string sMobilePhone, string content) {
			m_nUserId = userId;
			m_nUnderwriterId = underwriterId;

			m_sMobilePhone = sMobilePhone;
			m_sContent = content;

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					m_sAccountSid = sr["TwilioAccountSid"];
					m_sAuthToken = sr["TwilioAuthToken"];
					m_sFromNumber = sr["TwilioSendingNumber"];
					return ActionResult.SkipAll;
				},
				"GetTwilioConfigs",
				CommandSpecies.StoredProcedure
			);
		} // constructor

		public override string Name {
			get { return "Send SMS"; }
		} // Name

		public override void Execute() {
			//Debug mode
			if (m_sMobilePhone == "01111111111") {
				Result = true;
				Log.Debug("Send SMS Debug mode");
				return;
			}

			if (string.IsNullOrEmpty(m_sContent)) {
				Log.Info("Empty content message is not sent");
				return;
			}


			var twilio = new TwilioRestClient(m_sAccountSid, m_sAuthToken);
			_sendMobilePhone = string.Format("{0}{1}", UkMobilePrefix, m_sMobilePhone.Substring(1));
			_dateSent = DateTime.UtcNow;
			Log.Info("Sending sms to customer:{2}, number {0}, content:{1}", _sendMobilePhone, m_sContent, m_nUserId);
			twilio.SendMessage(m_sFromNumber, _sendMobilePhone, m_sContent, SaveSms);
			Result = true;
		}

		private void SaveSms(Message smsResponse) {
			try {
				var message = EzbobSmsMessage.FromMessage(smsResponse);
				message.UserId = m_nUserId;
				message.UnderwriterId = m_nUnderwriterId;
				message.DateSent = _dateSent;

				if (message.Status == null) {
					Result = false;
					string restException = "";
					if (smsResponse.RestException != null) {
						restException = string.Format("RestException Code:{0}, Status:{3}, Message:{1}, MoreInfo:{2}",
							smsResponse.RestException.Code, smsResponse.RestException.Message, smsResponse.RestException.MoreInfo, smsResponse.RestException.Status);
					}
					Log.Warn("Failed sending SMS to number:{0}\n{1}", _sendMobilePhone, restException);
					return;
				} // if

				DB.ExecuteNonQuery("SaveSmsMessage", CommandSpecies.StoredProcedure,
								DB.CreateTableParameter<EzbobSmsMessage>("Tbl", new List<EzbobSmsMessage> { message }));

				Result = true;
				Log.Info("Sms message sent to '{0}'. Sid:'{1}'", _sendMobilePhone, message.Sid);
			} catch (Exception ex) {
				Log.Error("Failed saving twilio SMS send response to DB: {0}", ex.Message);
				Result = false;
			}
		}

		// Execute

		public bool Result { get; private set; }
		private readonly int m_nUserId;
		private readonly int m_nUnderwriterId;
		private readonly string m_sMobilePhone;
		private readonly string m_sContent;
		private const string UkMobilePrefix = "+44";
		private string m_sAccountSid;
		private string m_sAuthToken;
		private string m_sFromNumber;
		private DateTime _dateSent;
		private string _sendMobilePhone;
	} // class GenerateMobileCode
} // namespace
