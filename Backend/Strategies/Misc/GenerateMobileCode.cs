namespace EzBob.Backend.Strategies.Misc {
	using System;
	using System.Globalization;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Twilio;
	using System.Collections.Generic;

	public class GenerateMobileCode : AStrategy {
		#region public

		#region constructor

		public GenerateMobileCode(string sMobilePhone, AConnection oDb, ASafeLog oLog) : base(oDb, oLog) {
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

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Generate mobile code"; }
		} // Name

		#endregion property Name

		#region property IsError

		public bool IsError { get; private set; }

		#endregion property IsError

		#region method Execute

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

		#endregion method Execute

		#endregion public

		#region private

		#region method GenerateCodeAndSend

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

			string sendMobilePhone = string.Format("{0}{1}", UkMobilePrefix, m_sMobilePhone.Substring(1));


			var message = (EzbobSmsMessage)twilio.SendSmsMessage(m_sFromNumber, sendMobilePhone, content, "");

			message.UserId = null;
			message.UnderwriterId = 1; //system id

			DB.ExecuteNonQuery("SaveSmsMessage", CommandSpecies.StoredProcedure, DB.CreateTableParameter<EzbobSmsMessage>("Tbl", new List<EzbobSmsMessage> { message }));
			

			if (message.Status == null) {
				IsError = true;
				Log.Warn(string.Format("Failed sending SMS to number:{0}", sendMobilePhone));
				return;
			} // if

			Log.Info("Sms message sent to '{0}'. Sid:'{1}'", sendMobilePhone, message.Sid);
		} // GenerateCodeAndSend

		#endregion method GenerateCodeAndSend

		private readonly string m_sMobilePhone;
		private const string UkMobilePrefix = "+44";

		private string m_sAccountSid;
		private string m_sAuthToken;
		private string m_sFromNumber;
		private int m_nMaxPerDay;
		private int m_nMaxPerNumber;
		private string m_sSkipCodeGenerationNumber;

		#endregion private
	} // class GenerateMobileCode
} // namespace
