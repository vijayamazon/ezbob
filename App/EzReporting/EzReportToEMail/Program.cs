using Ezbob.Database;
using Ezbob.Logger;

namespace EzReportToEMail {
	public class Program {
		public static void Main(string[] args) {
			var log = new LegacyLog();

			var reportsHandler = new EmailReportHandler(new SqlConnection(log), log);
			reportsHandler.ExecuteReportHandler();
		} // Main
	} // class Program
} // namespace EzReportToEmail
