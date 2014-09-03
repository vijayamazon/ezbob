namespace EzBob.Backend.Strategies.Misc
{
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
			string sendMobilePhone = string.Format("{0}{1}", UkMobilePrefix, m_sMobilePhone.Substring(1));
			var message = EzbobSmsMessage.FromSmsMessage(twilio.SendSmsMessage(m_sFromNumber, sendMobilePhone, m_sContent, ""));
			message.UserId = m_nUserId;
			message.UnderwriterId = m_nUnderwriterId;

			DB.ExecuteNonQuery("SaveSmsMessage", CommandSpecies.StoredProcedure,
							DB.CreateTableParameter<EzbobSmsMessage>("Tbl", new List<EzbobSmsMessage> { message }));

			if (message.Status == null) {
				Result = false;
				Log.Warn("Failed sending SMS to number:{0}", sendMobilePhone);
				return;
			} // if

			Result = true;
			Log.Info("Sms message sent to '{0}'. Sid:'{1}'", sendMobilePhone, message.Sid);

		} // Execute

		public bool Result { get; private set; }
		private readonly int m_nUserId;
		private readonly int m_nUnderwriterId;
		private readonly string m_sMobilePhone;
		private readonly string m_sContent;
		private const string UkMobilePrefix = "+44";
		private string m_sAccountSid;
		private string m_sAuthToken;
		private string m_sFromNumber;
	} // class GenerateMobileCode
} // namespace
