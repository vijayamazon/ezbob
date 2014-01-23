namespace EzBob.Backend.Strategies {
	using System;
	using System.Data;
	using System.Globalization;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Twilio;

	public class GenerateMobileCode : AStrategy
	{
		private readonly string mobilePhone;
		private string accountSid;
		private string authToken;
		private string fromNumber;
		private int maxPerDay;
		private int maxPerNumber;
		private const string UkMobilePrefix = "+44";

		#region constructor

		public GenerateMobileCode(string mobilePhone, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.mobilePhone = mobilePhone;
			ReadConfigurations();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Generate mobile code"; }
		} // Name

		#endregion property Name

		#region property Execute

		public override void Execute()
		{
			DataTable dt = DB.ExecuteReader("GetCurrentMobileCodeCount", CommandSpecies.StoredProcedure, new QueryParameter("Phone", mobilePhone));
			var results = new SafeReader(dt.Rows[0]);
			
			int sentToday = results["SentToday"];
			int sentNumber = results["SentToNumber"];
			if (maxPerDay <= sentToday)
			{
				IsError = true;
				Log.Warn(string.Format("Reached max number of daily SMS messages ({0}). SMS not sent", maxPerDay));
				return;
			}

			if (maxPerNumber <= sentNumber)
			{
				IsError = true;
				Log.Warn(string.Format("Reached max number of SMS messages ({0}) to number:{1}. SMS not sent", maxPerNumber, mobilePhone));
				return;
			}

			GenerateCodeAndSend();
		} // Execute

		private void GenerateCodeAndSend()
		{
			var random = new Random();
			string code = (100000 + random.Next(899999)).ToString(CultureInfo.InvariantCulture);

			DB.ExecuteNonQuery("StoreMobileCode", CommandSpecies.StoredProcedure,
				new QueryParameter("Phone", mobilePhone),
				new QueryParameter("Code", code));

			var twilio = new TwilioRestClient(accountSid, authToken);

			string content = string.Format("Your authentication code is:{0}", code);
			string sendMobilePhone = string.Format("{0}{1}", UkMobilePrefix, mobilePhone.Substring(1));
			var message = twilio.SendSmsMessage(fromNumber, sendMobilePhone, content, "");
			if (message.Status == null)
			{
				IsError = true;
				Log.Warn(string.Format("Failed sending SMS to number:{0}", sendMobilePhone));
				return;
			}

			Log.Info("Sms message sent to '{0}'. Sid:'{1}'", sendMobilePhone, message.Sid);
		}

		private void ReadConfigurations()
		{
			DataTable dt = DB.ExecuteReader("GetTwilioConfigs", CommandSpecies.StoredProcedure);
			DataRow results = dt.Rows[0];
			var sr = new SafeReader(results);
			accountSid = sr["TwilioAccountSid"];
			authToken = sr["TwilioAuthToken"];
			fromNumber = sr["TwilioSendingNumber"];
			maxPerDay = sr["MaxPerDay"];
			maxPerNumber = sr["MaxPerNumber"];
		}

		public bool IsError { get; private set; }

		#endregion property Execute
	} // class GenerateMobileCode
} // namespace
