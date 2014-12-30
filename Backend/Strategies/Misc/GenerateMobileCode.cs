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
			this.m_sMobilePhone = sMobilePhone;

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					this.m_sAccountSid = sr["TwilioAccountSid"];
					this.m_sAuthToken = sr["TwilioAuthToken"];
					this.m_sFromNumber = sr["TwilioSendingNumber"];
					this.m_nMaxPerDay = sr["MaxPerDay"];
					this.m_nMaxPerNumber = sr["MaxPerNumber"];
					this.m_sSkipCodeGenerationNumber = sr["SkipCodeGenerationNumber"];
					return ActionResult.SkipAll;
				},
				"GetTwilioConfigs",
				CommandSpecies.StoredProcedure
				);
		} // constructor

		// Name
		public override void Execute() {
			if (this.m_sSkipCodeGenerationNumber == this.m_sMobilePhone) {
				Log.Info("'Skip code generation' number {0} detected. Code won't be generated.", this.m_sMobilePhone);
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
				new QueryParameter("Phone", this.m_sMobilePhone)
				);

			if (this.m_nMaxPerDay <= sentToday) {
				IsError = true;
				Log.Warn("Reached max number of daily SMS messages ({0}). SMS not sent", this.m_nMaxPerDay);
				return;
			} // if

			if (this.m_nMaxPerNumber <= sentNumber) {
				IsError = true;
				Log.Warn("Reached max number of SMS messages ({0}) to number:{1}. SMS not sent", this.m_nMaxPerNumber, this.m_sMobilePhone);
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
				new QueryParameter("Phone", this.m_sMobilePhone),
				new QueryParameter("Code", code)
				);

			var twilio = new TwilioRestClient(this.m_sAccountSid, this.m_sAuthToken);

			string content = string.Format("Your authentication code is: {0}", code);

			this._sendMobilePhone = string.Format("{0}{1}", UkMobilePrefix, this.m_sMobilePhone.Substring(1));

			this._dateSent = DateTime.UtcNow;
			twilio.SendSmsMessage(this.m_sFromNumber, this._sendMobilePhone, content, SaveSms);
		} // GenerateCodeAndSend

		private void SaveSms(SMSMessage twilioResponse) {
			EzbobSmsMessage message = EzbobSmsMessage.FromSmsMessage(twilioResponse);
			message.UserId = null;
			message.UnderwriterId = 1; //system id
			message.DateSent = this._dateSent;

			try {
				DB.ExecuteNonQuery("SaveSmsMessage", CommandSpecies.StoredProcedure, DB.CreateTableParameter<EzbobSmsMessage>("Tbl", new List<EzbobSmsMessage> {
					message
				}));
			} catch (Exception ex) {
				Log.Error(ex, "Failed saving twilio SMS send response to DB: {0}", message);
			}

			if (message.Status == null) {
				IsError = true;
				Log.Warn("Failed sending SMS to number:{0}", this._sendMobilePhone);
				return;
			} // if

			Log.Info("Sms message sent to '{0}'. Sid:'{1}'", this._sendMobilePhone, message.Sid);
		} // SaveSms

		private const string UkMobilePrefix = "+44";
		private readonly string m_sMobilePhone;
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
