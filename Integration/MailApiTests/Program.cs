namespace MailApiTests {
	using System.Collections.Generic;
	using Ezbob.Logger;

	class Program {
		static void Main(string[] args) {
			ASafeLog oLog = new ConsoleLog(new FileLog("MailApiTests"));

			var mf = new MandrillFacade(oLog);

			string sResponse = mf.SendEmail(
				"alexbo+mail-api-tests@ezbob.com",
				"Mandrill - Application completed under review",
				new Dictionary<string, string> {
					{ "FirstName", "Alex" },
				}
			);

			oLog.Info("Response content is: {0}", sResponse ?? "-- null --");
		} // Main
	} // class Program
} // namespace
