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
		private const string UkMobilePrefix = "+44";

		#region constructor

		public GenerateMobileCode(string mobilePhone, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			this.mobilePhone = string.Format("{0}{1}", UkMobilePrefix, mobilePhone.Substring(1));
			ReadConfigurations();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Generate mobile code"; }
		} // Name

		#endregion property Name

		#region property Execute

		public override void Execute() {
			var random = new Random();
			string code = (100000 + random.Next(899999)).ToString(CultureInfo.InvariantCulture);

			DB.ExecuteNonQuery("StoreMobileCode", CommandSpecies.StoredProcedure,
				new QueryParameter("Phone", mobilePhone),
				new QueryParameter("Code", code));
			
			var twilio = new TwilioRestClient(accountSid, authToken);

			string content = string.Format("Your authentication code is:{0}", code);
			// it is working with mobilePhone = "+972544771676" (use "+447866530634" for farley to test it after getting prod account)
			var message = twilio.SendSmsMessage(fromNumber, mobilePhone, content, "");
			Log.Info("Sms message sent to '{0}'. Sid:'{1}'", mobilePhone, message.Sid);
		} // Execute

		private void ReadConfigurations()
		{
			DataTable dt = DB.ExecuteReader("GetTwilioConfigs", CommandSpecies.StoredProcedure);
			DataRow results = dt.Rows[0];
			var sr = new SafeReader(results);
			accountSid = sr["TwilioAccountSid"];
			authToken = sr["TwilioAuthToken"];
			fromNumber = sr["TwilioSendingNumber"];
		}

		#endregion property Execute
	} // class GenerateMobileCode
} // namespace
