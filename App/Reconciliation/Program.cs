namespace Reconciliation {
	using System;
	using System.Globalization;
	using System.Text;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Html.Tags;
	using Reports;

	public class Program {
		public static void Main(string[] args) {
			ms_oLog = new LegacyLog();

			if (IsNormalMode(args)) {
				PacnetBalance.PacNetBalance.Logger = ms_oLog;
				PacnetBalance.ParsePacNetText.Logger = ms_oLog;

				var pacnetcfg = new PacnetBalance.Conf(ms_oLog);
				pacnetcfg.Init();

				var pacnet = new PacnetBalance.Processor(pacnetcfg, ms_oLog);

				if (pacnet.Init())
					pacnet.Run();

				pacnet.Done();
			}
			else {
				ms_oLog = new ConsoleLog(ms_oLog);
				ms_oLog.Debug("Date: {0}, rerun mode.", ms_oDate.ToString("MMMM d yyyy H:mm:ss", CultureInfo.InvariantCulture));
			} // if

			var paypointcfg = new PayPointBalance.Conf(ms_oLog);
			paypointcfg.Init();

			var ppb = new PayPointBalance.Processor(paypointcfg, ms_oDate, ms_oLog);

			if (ppb.Init())
				ppb.Run();

			ppb.Done();

			SendReport(ms_oDate, ms_oLog);
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

		private static void SendReport(DateTime oDate, ASafeLog oLog) {
			oLog.Debug("Generating reconciliation report...");

			var oDB = new SqlConnection();

			oLog.Debug("Loading Pacnet report metadata from db...");

			var pacnet = new Report(oDB, "RPT_PACNET_RECONCILIATION");

			oLog.Debug("Loading Paypoint report metadata from db...");

			var paypoint = new Report(oDB, "RPT_PAYPOINT_RECONCILIATION");

			var rh = new BaseReportHandler(oDB, oLog);

			var sender = new ReportDispatcher(oDB, oLog);

			var email = new ReportEmail();

			oLog.Debug("Generating Pacnet report...");

			email.ReportBody.Append(new H2().Append(new Text(pacnet.GetTitle(oDate))));

			email.ReportBody.Append(
				rh.TableReport(new ReportQuery(pacnet, oDate, oDate))
			);

			oLog.Debug("Generating Paypoint report...");

			email.ReportBody.Append(new H2().Append(new Text(paypoint.GetTitle(oDate))));

			email.ReportBody.Append(
				rh.TableReport(new ReportQuery(paypoint, oDate, oDate))
			);

			var sTo = new StringBuilder();
			
			sTo.Append(pacnet.ToEmail);

			if (pacnet.ToEmail != "")
				sTo.Append(",");

			sTo.Append(paypoint.ToEmail);

			oLog.Debug("Sending report...");

			sender.Dispatch(
				"Reconciliation " + oDate.ToString("MMMM d yyyy", CultureInfo.InvariantCulture),
				oDate,
				email.HtmlBody,
				null,
				sTo.ToString()
			);

			oLog.Debug("Reconciliation report generation complete.");
		} // SendReport

		private static ASafeLog ms_oLog;
		private static DateTime ms_oDate;
	} // class Program
} // namespace
