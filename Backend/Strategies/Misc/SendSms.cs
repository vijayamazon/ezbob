namespace EzBob.Backend.Strategies.Misc
{
	using System;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Twilio;
	using System.Collections.Generic;

	public class SendSms : AStrategy {
		public SendSms(int userId, int underwriterId, string sMobilePhone, string content, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog) {
			m_nUserId = userId;
			m_nUnderwriterId = underwriterId;

			m_sMobilePhone = sMobilePhone;
			m_sContent = content;

			DB.ForEachRowSafe(
				(sr, bRowsetStart) =>
				{
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

			var twilio = new TwilioRestClient(m_sAccountSid, m_sAuthToken);
			_sendMobilePhone = string.Format("{0}{1}", UkMobilePrefix, m_sMobilePhone.Substring(1));
			_dateSent = DateTime.UtcNow;
			twilio.SendSmsMessage(m_sFromNumber, _sendMobilePhone, m_sContent, SaveSms);
			Result = true;
		}

		private void SaveSms(SMSMessage smsResponse) {
			var message = EzbobSmsMessage.FromSmsMessage(smsResponse);
			message.UserId = m_nUserId;
			message.UnderwriterId = m_nUnderwriterId;
			message.DateSent = _dateSent;

			try {
				DB.ExecuteNonQuery("SaveSmsMessage", CommandSpecies.StoredProcedure,
							DB.CreateTableParameter<EzbobSmsMessage>("Tbl", new List<EzbobSmsMessage> { message }));
			}
			catch (Exception ex) {
				Log.Error("Failed saving twilio SMS send response to DB: {0}", ex.Message);
			}

			if (message.Status == null)
			{
				Result = false;
				Log.Warn("Failed sending SMS to number:{0}", _sendMobilePhone);
				return;
			} // if

			Result = true;
			Log.Info("Sms message sent to '{0}'. Sid:'{1}'", _sendMobilePhone, message.Sid);

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
