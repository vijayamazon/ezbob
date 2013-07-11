using System;
using System.Globalization;
using System.Text;
using System.Threading;
using Ezbob.Database;
using Ezbob.Logger;
using Html.Tags;
using PacnetBalance;
using Reports;

namespace Reconciliation {
	public class Program {
		public static void Main(string[] args) {
			ms_oLog = new LegacyLog();

			if (IsNormalMode(args)) {
				PacNetBalance.Logger = ms_oLog;
				ParsePacNetText.Logger = ms_oLog;

				// ReSharper disable ObjectCreationAsStatement
				new PacnetMailListener(Run, ms_oLog);
				// ReSharper restore ObjectCreationAsStatement

				Thread.Sleep(Timeout.Infinite);
			}
			else {
				ms_oLog = new ConsoleLog(ms_oLog);

				ms_oLog.Debug("Date: {0}, rerun mode.", ms_oDate.ToString("MMMM d yyyy H:mm:ss", CultureInfo.InvariantCulture));

				Run();
			} // if
		} // Main

		private static bool IsNormalMode(string[] args) {
			if (args.Length > 1)
				if (args[0] == "--rerunfor")
					if (DateTime.TryParseExact(args[1], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out ms_oDate))
						return false;
			
			ms_oDate = DateTime.Today.AddDays(-1);

			ms_oLog.Debug("Date: {0}, normal working mode.", ms_oDate.ToString("MMMM d yyyy H:mm:ss", CultureInfo.InvariantCulture));

			return true;
		} // IsNormalMode

		public static void Run() {
			ProcessPayPoint(ms_oDate, ms_oLog);
			SendReport(ms_oDate, ms_oLog);
		} // Run

		private static void ProcessPayPoint(DateTime oDate, ASafeLog oLog) {
			var ppb = new PayPointBalance.Processor(oDate, oLog);

			if (ppb.Init())
				ppb.Run();

			ppb.Done();
		} // ProcessPayPoint

		private static void SendReport(DateTime oDate, ASafeLog oLog) {
			oLog.Debug("Generating reconciliation report...");

			var oDB = new SqlConnection();

			oLog.Debug("Loading Pacnet report metadata from db...");

			Report pacnet = new Report(oDB, "RPT_PACNET_RECONCILIATION");

			oLog.Debug("Loading Paypoint report metadata from db...");

			Report paypoint = new Report(oDB, "RPT_PAYPOINT_RECONCILIATION");

			var rh = new BaseReportHandler(oDB, oLog);

			var sender = new BaseReportSender(oLog);

			BaseReportSender.MailTemplate template = sender.CreateMailTemplate();

			oLog.Debug("Generating Pacnet report...");

			template.ReportBody.Append(new H2().Append(new Text(pacnet.GetTitle(oDate))));

			template.ReportBody.Append(
				rh.TableReport(pacnet.StoredProcedure, oDate, oDate, pacnet.Columns)
			);

			oLog.Debug("Generating Paypoint report...");

			template.ReportBody.Append(new H2().Append(new Text(paypoint.GetTitle(oDate))));

			template.ReportBody.Append(
				rh.TableReport(paypoint.StoredProcedure, oDate, oDate, paypoint.Columns)
			);

			StringBuilder sTo = new StringBuilder();
			
			sTo.Append(pacnet.ToEmail);

			if (pacnet.ToEmail != "")
				sTo.Append(",");

			sTo.Append(paypoint.ToEmail);

			oLog.Debug("Sending report...");

			sender.Send(
				"Reconciliation " + oDate.ToString("MMMM d yyyy", CultureInfo.InvariantCulture),
				template.HtmlBody,
				sTo.ToString()
			);

			oLog.Debug("Reconciliation report generation complete.");
		} // SendReport

		private static ASafeLog ms_oLog;
		private static DateTime ms_oDate;
	} // class Program
} // namespace
