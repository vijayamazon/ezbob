﻿namespace Ezbob.Backend.Strategies.Reports {
	using System;
	using System.Collections.Generic;
	using Ezbob.Utils.Html.Tags;
	using global::Reports;
	using global::Reports.Alibaba;
	using global::Reports.Alibaba.DataSharing;
	using global::Reports.Alibaba.Funnel;

	public class Alibaba : AStrategy {

		public Alibaba(DateTime? oDateEnd, bool bIncludeTestCustomers) {
			m_oDateEnd = oDateEnd;
			m_bIncludeTestCustomers = bIncludeTestCustomers;

			m_oDataSharing = new Report(DB, ReportType.RPT_ALIBABA_DATA_SHARING);
			m_oFunnel = new Report(DB, ReportType.RPT_ALIBABA_FUNNEL);

			m_oSender = new BaseReportSender(Log);
		} // constructor

		public override string Name {
			get { return "Alibaba"; }
		} // Name

		public override void Execute() {
			var lst = new List<Tuple<string, Report, IAlibaba>> {
				new Tuple<string, Report, IAlibaba>("data sharing", m_oDataSharing, new DataSharing(m_bIncludeTestCustomers, DB, Log)),
				new Tuple<string, Report, IAlibaba>("funnel", m_oFunnel, new FunnelCreator(m_oDateEnd, DB, Log)),
			};

			foreach (var tpl in lst) {
				try {
					GenerateAndSend(tpl.Item1, tpl.Item2, tpl.Item3);
				}
				catch (Exception e) {
					Log.Alert(e, "Failed to generate and send Alibaba {0} report.", tpl.Item1);
				} // try
			} // for each
		} // Execute

		private void GenerateAndSend(string sReportName, Report oRptDef, IAlibaba oGenerator) {
			if (oRptDef == null) {
				Log.Alert("Not running Alibaba {0} report: no entry found in ReportScheduler.", sReportName);
				return;
			} // if

			if (oGenerator == null) {
				Log.Alert("Not running Alibaba {0} report: no report generator was provided.", oRptDef.Title);
				return;
			} // if

			oGenerator.Generate();

			string sTitle = oRptDef.GetTitle(m_oDateEnd ?? DateTime.UtcNow);

			var oBody = new Body().Append(new Text("Please find attached " + sTitle + "."));

			m_oSender.Send(sTitle, oBody, oGenerator.Report, oRptDef.ToEmail);
		} // GenerateAndSend

		private readonly Report m_oDataSharing;
		private readonly Report m_oFunnel;

		private readonly DateTime? m_oDateEnd;
		private readonly bool m_bIncludeTestCustomers;

		private readonly BaseReportSender m_oSender;

	} // class Alibaba
} // namespace
