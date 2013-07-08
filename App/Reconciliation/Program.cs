using System;
using System.Globalization;
using System.Text;
using Ezbob.Database;
using Ezbob.Logger;
using Html.Tags;
using Reports;

namespace Reconciliation {
	public class Program {
		public static void Main(string[] args) {
			ms_oLog = new ConsoleLog(new LegacyLog());

			PacNetBalance.Logger = ms_oLog;
			ParsePacNetText.Logger = ms_oLog;
			PacNetBalance.Logger = ms_oLog;

			ms_oDate = DateTime.Today.AddDays(-1);

			var ppb = new PayPointBalance(ms_oDate, Conf.PayPointMid, Conf.PayPointVpnPassword, Conf.PayPointRemotePassword, ms_oLog);

			if (ppb.Init())
				ppb.Run();

			ppb.Done();

			SendReport();

			/*

			// ReSharper disable ObjectCreationAsStatement
			new PacnetMailListener(SendReport, oLog);
			// ReSharper restore ObjectCreationAsStatement

			Thread.Sleep(Timeout.Infinite);
			*/
		} // Main

		public static void SendReport() {
			ms_oLog.Debug("Generating reconciliation report...");

			var oDB = new SqlConnection();

			ms_oLog.Debug("Loading Pacnet report metadata from db...");

			Report pacnet = new Report(oDB, "RPT_PACNET_RECONCILIATION");

			ms_oLog.Debug("Loading Paypoint report metadata from db...");

			Report paypoint = new Report(oDB, "RPT_PAYPOINT_RECONCILIATION");

			var rh = new BaseReportHandler(oDB, ms_oLog);

			var sender = new BaseReportSender(ms_oLog);

			BaseReportSender.MailTemplate template = sender.CreateMailTemplate();

			ms_oLog.Debug("Generating Pacnet report...");

			template.ReportBody.Append(new H2().Append(new Text(pacnet.GetTitle(ms_oDate))));

			template.ReportBody.Append(
				rh.TableReport(pacnet.StoredProcedure, ms_oDate, ms_oDate, pacnet.Columns)
			);

			ms_oLog.Debug("Generating Paypoint report...");

			template.ReportBody.Append(new H2().Append(new Text(paypoint.GetTitle(ms_oDate))));

			template.ReportBody.Append(
				rh.TableReport(paypoint.StoredProcedure, ms_oDate, ms_oDate, paypoint.Columns)
			);

			StringBuilder sTo = new StringBuilder();
			
			sTo.Append(pacnet.ToEmail);

			if (pacnet.ToEmail != "")
				sTo.Append(",");

			sTo.Append(paypoint.ToEmail);

			ms_oLog.Debug("Sending report...");

			sender.Send(
				"Reconciliation " + ms_oDate.ToString("MMMM d yyyy", CultureInfo.InvariantCulture),
				template.HtmlBody,
				sTo.ToString()
			);

			ms_oLog.Debug("Reconciliation report generation complete.");
		} // SendReport

		private static ASafeLog ms_oLog;
		private static DateTime ms_oDate;
	} // class Program
} // namespace
