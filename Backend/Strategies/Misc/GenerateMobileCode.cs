namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Globalization;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;
	using Twilio;
	using System.Collections.Generic;

	public class GenerateMobileCode : AStrategy {

		public GenerateMobileCode(string sMobilePhone) {
			m_sMobilePhone = sMobilePhone;

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					m_sAccountSid = sr["TwilioAccountSid"];
					m_sAuthToken = sr["TwilioAuthToken"];
					m_sFromNumber = sr["TwilioSendingNumber"];
					m_nMaxPerDay = sr["MaxPerDay"];
					m_nMaxPerNumber = sr["MaxPerNumber"];
					m_sSkipCodeGenerationNumber = sr["SkipCodeGenerationNumber"];
					return ActionResult.SkipAll;
				},
				"GetTwilioConfigs",
				CommandSpecies.StoredProcedure
			);
		} // constructor

		public override string Name {
			get { return "Generate mobile code"; }
		} // Name

		public bool IsError { get; private set; }

		public override void Execute() {
			if (m_sSkipCodeGenerationNumber == m_sMobilePhone) {
				Log.Info("'Skip code generation' number {0} detected. Code won't be generated.", m_sMobilePhone);
				return;
			} // if

			int sentToday = 0;
			int sentNumber = 0;

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					sentToday = sr["SentToday"];
					sentNumber = sr["SentToNumber"];
					return ActionResult.SkipAll;
				},
				"GetCurrentMobileCodeCount",
				CommandSpecies.StoredProcedure,
				new QueryParameter("Phone", m_sMobilePhone)
			);

			if (m_nMaxPerDay <= sentToday) {
				IsError = true;
				Log.Warn("Reached max number of daily SMS messages ({0}). SMS not sent", m_nMaxPerDay);
				return;
			} // if

			if (m_nMaxPerNumber <= sentNumber) {
				IsError = true;
				Log.Warn("Reached max number of SMS messages ({0}) to number:{1}. SMS not sent", m_nMaxPerNumber, m_sMobilePhone);
				return;
			} // if

			GenerateCodeAndSend();
		} // Execute

		private void GenerateCodeAndSend() {
			var random = new Random();
			string code = (100000 + random.Next(899999)).ToString(CultureInfo.InvariantCulture);

			DB.ExecuteNonQuery(
				"StoreMobileCode",
				CommandSpecies.StoredProcedure,
				new QueryParameter("Phone", m_sMobilePhone),
				new QueryParameter("Code", code)
			);

			var twilio = new TwilioRestClient(m_sAccountSid, m_sAuthToken);

			string content = string.Format("Your authentication code is: {0}", code);

			_sendMobilePhone = string.Format("{0}{1}", UkMobilePrefix, m_sMobilePhone.Substring(1));

			_dateSent = DateTime.UtcNow;
			twilio.SendSmsMessage(m_sFromNumber, _sendMobilePhone, content, SaveSms);
		}

		private void SaveSms(SMSMessage twilioResponse) {
			EzbobSmsMessage message = EzbobSmsMessage.FromSmsMessage(twilioResponse);
			message.UserId = null;
			message.UnderwriterId = 1; //system id
			message.DateSent = _dateSent;

			try {
				DB.ExecuteNonQuery("SaveSmsMessage", CommandSpecies.StoredProcedure, DB.CreateTableParameter<EzbobSmsMessage>("Tbl", new List<EzbobSmsMessage> { message }));
			}
			catch (Exception ex) {
				Log.Error(string.Format("Failed saving twilio SMS send response to DB: {0}", message));
			}

			if (message.Status == null)
			{
				IsError = true;
				Log.Warn(string.Format("Failed sending SMS to number:{0}", _sendMobilePhone));
				return;
			} // if

			Log.Info("Sms message sent to '{0}'. Sid:'{1}'", _sendMobilePhone, message.Sid);
		}

// GenerateCodeAndSend

		private readonly string m_sMobilePhone;
		private const string UkMobilePrefix = "+44";

		private string m_sAccountSid;
		private string m_sAuthToken;
		private string m_sFromNumber;
		private int m_nMaxPerDay;
		private int m_nMaxPerNumber;
		private string m_sSkipCodeGenerationNumber;
		private string _sendMobilePhone;
		private DateTime _dateSent;

	} // class GenerateMobileCode
} // namespace
