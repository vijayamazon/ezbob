namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;
	using Twilio;

	public class GenerateMobileCode : AStrategy {
		public override string Name {
			get { return "Generate mobile code"; }
		}

		public bool IsError { get; private set; }

		public GenerateMobileCode(string sMobilePhone) {
			m_sMobilePhone = sMobilePhone;

			m_sAccountSid = ConfigManager.CurrentValues.Instance.TwilioAccountSid;
			m_sAuthToken = ConfigManager.CurrentValues.Instance.TwilioAuthToken;
			m_sFromNumber = ConfigManager.CurrentValues.Instance.TwilioSendingNumber;
			m_nMaxPerDay = ConfigManager.CurrentValues.Instance.MaxPerDay;
			m_nMaxPerNumber = ConfigManager.CurrentValues.Instance.MaxPerNumber;
			m_sSkipCodeGenerationNumber = ConfigManager.CurrentValues.Instance.SkipCodeGenerationNumber;

		} // constructor

		// Name
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
			var smsResponse = twilio.SendMessage(m_sFromNumber, _sendMobilePhone, content);
			SaveSms(smsResponse);
		} // GenerateCodeAndSend

		private void SaveSms(Message twilioResponse) {
			EzbobSmsMessage message = EzbobSmsMessage.FromMessage(twilioResponse);
			message.UserId = null;
			message.UnderwriterId = 1; //system id
			message.DateSent = _dateSent;

			try {
				DB.ExecuteNonQuery("SaveSmsMessage", CommandSpecies.StoredProcedure, DB.CreateTableParameter<EzbobSmsMessage>("Tbl", new List<EzbobSmsMessage> {
					message
				}));
			} catch (Exception ex) {
				Log.Error(ex, "Failed saving twilio SMS send response to DB: {0}", message);
			}

			if (message.Status == null) {
				IsError = true;
				Log.Warn("Failed sending SMS to number:{0}", _sendMobilePhone);
				return;
			} // if

			Log.Info("Sms message sent to '{0}'. Sid:'{1}'", _sendMobilePhone, message.Sid);
		} // SaveSms

		private const string UkMobilePrefix = "+44";
		private readonly string m_sMobilePhone;
		private readonly string m_sAccountSid;
		private readonly string m_sAuthToken;
		private readonly string m_sFromNumber;
		private readonly int m_nMaxPerDay;
		private readonly int m_nMaxPerNumber;
		private readonly string m_sSkipCodeGenerationNumber;
		private string _sendMobilePhone;
		private DateTime _dateSent;
	} // class GenerateMobileCode
} // namespace
