namespace Reports {
	using System;
	using System.IO;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Html;
	using OfficeOpenXml;

	#region class ReportDispatcher

	public class ReportDispatcher : SafeLog {
		#region public

		public const string ToDropbox = "dropbox";

		#region constructor

		public ReportDispatcher(AConnection oDB, ASafeLog oLog = null) : base(oLog) {
			m_oDropbox = new DropboxReportSaver(oDB, oLog);
			m_oDropbox.Init();

			m_oSender = new BaseReportSender(oLog);
		} // constructor

		#endregion constructor

		#region method Dispatch

		public void Dispatch(string subject, DateTime oReportDate, ATag mailBody, ExcelPackage wb, string toAddressStr, string period = "Daily") {
			if ((toAddressStr ?? "").Trim().ToLower() == ReportDispatcher.ToDropbox) {
				if (wb != null) {
					var ostream = new MemoryStream();
					wb.SaveAs(ostream);
					ostream.Close();

					try {
						m_oDropbox.Send(subject, oReportDate, "xlsx", ostream.ToArray());
					}
					catch (Exception e) {
						Error(
							"Failed to upload report to dropbox. Title: {0}, generation date: {1}\nError message: {2}",
							subject, oReportDate, e.Message
						);

						Debug(e);
					} // try
				}
				else
					Warn("Not uploading {0} report to Dropbox: data not generated.", subject);
			}
			else
				m_oSender.Send(subject, mailBody, wb, toAddressStr, period);
		} // Dispatch

		#endregion method Dispatch

		#endregion public

		#region private

		private readonly BaseReportSender m_oSender;
		private readonly DropboxReportSaver m_oDropbox;

		#endregion private
	} // class ReportDispatcher

	#endregion class ReportDispatcher
} // namespace Reports
