namespace Reports {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Html;
	using Ezbob.Utils.Html.Attributes;
	using Ezbob.Utils.Html.Tags;
	using OfficeOpenXml;

	public partial class BaseReportHandler : SafeLog {
		public ATag BuildNotAutoApprovedReport(
			Report report,
			DateTime today,
			DateTime tomorrow,
			List<string> oColumnTypes = null
		) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateNotAutoApprovedReport(report, today, tomorrow);

			return new Body().Add<Class>("Body")
				.Append(new H1().Append(new Text(report.GetTitle(today, oToDate: tomorrow))))
				.Append(new P().Append(TableReport(oData.Key, oData.Value, oColumnTypes: oColumnTypes)));
		} // BuildNotAutoApprovedReport

		public ExcelPackage BuildNotAutoApprovedXls(Report report, DateTime today, DateTime tomorrow) {
			KeyValuePair<ReportQuery, DataTable> oData = CreateNotAutoApprovedReport(report, today, tomorrow);

			return AddSheetToExcel(oData.Value, report.GetTitle(today, oToDate: tomorrow), "RptNotAutoApproved");
		} // BuildNotAutoApprovedXls

		private KeyValuePair<ReportQuery, DataTable> CreateNotAutoApprovedReport(
			Report report,
			DateTime today,
			DateTime tomorrow
		) {
			var rpt = new ReportQuery(report) {
				DateStart = today,
				DateEnd = tomorrow
			};

			var reportData = new NotAutoApprovedReportData();

			rpt.Execute(DB, (sr, rowsetStart) => {
				reportData.Add(sr);
				return ActionResult.Continue;
			});

			var oOutput = reportData.ToTable();

			return new KeyValuePair<ReportQuery, DataTable>(rpt, oOutput);
		} // CreateNotAutoApprovedReport

		private class NotAutoApprovedReportData {
			public NotAutoApprovedReportData() {
				this.cashRequests = new SortedDictionary<long, NotAutoApprovedCashRequest>();
			} // constructor

			public void Add(SafeReader sr) {
				long cashRequestID = sr["CashRequestID"];

				if (this.cashRequests.ContainsKey(cashRequestID))
					this.cashRequests[cashRequestID].Add(sr);
				else
					this.cashRequests[cashRequestID] = new NotAutoApprovedCashRequest(sr);
			} // Add

			public DataTable ToTable() {
				var tbl = new DataTable();

				tbl.Columns.Add("CustomerID", typeof(string));

				tbl.Columns.Add("CashRequestID", typeof(string));
				tbl.Columns.Add("CreationTime", typeof(string));
				tbl.Columns.Add("ManualDecisionTime", typeof(string));
				tbl.Columns.Add("ApprovedAmount", typeof(string));

				tbl.Columns.Add("InterestRate", typeof(string));
				tbl.Columns.Add("Term", typeof(string));
				tbl.Columns.Add("ManualSetupFeePercent", typeof(string));
				tbl.Columns.Add("BrokerSetupFeePercent", typeof(string));

				tbl.Columns.Add("SpreadSetupFee", typeof(string));
				tbl.Columns.Add("Aux0", typeof(string));
				tbl.Columns.Add("Aux1", typeof(string));
				tbl.Columns.Add("Aux2", typeof(string));

				tbl.Columns.Add("Css", typeof(string));

				foreach (var cr in this.cashRequests.Values)
					cr.ToRow(tbl);

				return tbl;
			} // ToTable

			private readonly SortedDictionary<long, NotAutoApprovedCashRequest> cashRequests;
		} // class NotAutoApprovedReportData

		private class NotAutoApprovedCashRequest {
			public NotAutoApprovedCashRequest(SafeReader sr) {
				this.trails = new SortedDictionary<long, NotAutoApprovedTrail>();
				sr.Fill(this);
				Add(sr);
			} // constructor

			public void Add(SafeReader sr) {
				long trailID = sr["TrailID"];

				if (this.trails.ContainsKey(trailID))
					this.trails[trailID].Add(sr);
				else
					this.trails[trailID] = new NotAutoApprovedTrail(sr);
			} // Add

			public int CustomerID { get; set; }
			public long CashRequestID { get; set; }
			public DateTime CreationDate { get; set; }
			[FieldName("UnderwriterDecisionDate")]
			public DateTime ManualDecisionTime { get; set; }
			[FieldName("ManagerApprovedSum")]
			public decimal ApprovedAmount { get; set; }
			public decimal InterestRate { get; set; }
			[FieldName("ApprovedRepaymentPeriod")]
			public int RepaymentPeriod { get; set; }
			public decimal ManualSetupFeePercent { get; set; }
			public decimal BrokerSetupFeePercent { get; set; }
			public bool SpreadSetupFee { get; set; }

			public void ToRow(DataTable tbl) {
				tbl.Rows.Add(
					CustomerID.ToString("G0", gb),

					CashRequestID.ToString("G0", gb),
					CreationDate.ToString("d/MMM/yyyy H:mm:ss", gb),
					ManualDecisionTime.ToString("d/MMM/yyyy H:mm:ss", gb),
					ApprovedAmount.ToString("C0", gb),

					InterestRate.ToString("P2", gb),
					RepaymentPeriod.ToString("G0", gb),
					ManualSetupFeePercent.ToString("P2", gb),
					BrokerSetupFeePercent.ToString("P2", gb),

					SpreadSetupFee ? "spread" : "deduced",
					string.Empty,
					string.Empty,
					string.Empty,

					"total"
				);

				var first = GetFirstTrail();
				var main = GetMainTrail();
				var last = GetLastTrail();

				if (first.IsEmpty && main.IsEmpty && last.IsEmpty)
					return;

				tbl.Rows.Add(
					string.Empty,

					first.GetTrailID("First"),
					first.DecisionTimeStr,
					first.StatusStr,
					first.TagStr,

					main.GetTrailID("Main"),
					main.DecisionTimeStr,
					main.StatusStr,
					main.TagStr,

					last.GetTrailID("Last"),
					last.DecisionTimeStr,
					last.StatusStr,
					last.TagStr,

					string.Empty
				);

				var firstTraces = first.Traces;
				var mainTraces = main.Traces;
				var lastTraces = last.Traces;

				int maxLen = Math.Max(firstTraces.Length, Math.Max(mainTraces.Length, lastTraces.Length));

				for (int i = 0; i < maxLen; i++) {
					NotAutoApprovedTrace f = i < firstTraces.Length ? firstTraces[i] : new NotAutoApprovedTrace();
					NotAutoApprovedTrace m = i < mainTraces.Length ? mainTraces[i] : new NotAutoApprovedTrace();
					NotAutoApprovedTrace l = i < lastTraces.Length ? lastTraces[i] : new NotAutoApprovedTrace();

					tbl.Rows.Add(
						string.Empty,

						string.Empty,
						f.TraceNameStr,
						f.CommentStr,
						string.Empty,

						string.Empty,
						m.TraceNameStr,
						m.Comment,
						string.Empty,

						string.Empty,
						l.TraceNameStr,
						l.CommentStr,
						string.Empty,

						string.Empty
					);
				} // for i
			} // ToRow

			private NotAutoApprovedTrail GetFirstTrail() {
				return this.trails.Values.OrderBy(t => t.DecisionTime).FirstOrDefault() ?? new NotAutoApprovedTrail(null);
			} // GetFirstTrail

			private NotAutoApprovedTrail GetMainTrail() {
				return this.trails.Values.OrderBy(t => t.DecisionTime).FirstOrDefault(t => t.Tag.StartsWith("Main"))
					?? new NotAutoApprovedTrail(null);
			} // GetMainTrail

			private NotAutoApprovedTrail GetLastTrail() {
				return this.trails.Values.OrderBy(t => t.DecisionTime).LastOrDefault() ?? new NotAutoApprovedTrail(null);
			} // GetLastTrail

			private readonly SortedDictionary<long, NotAutoApprovedTrail> trails;

			private static readonly CultureInfo gb = new CultureInfo("en-GB", false);
		} // class NotAutoApprovedCashRequest

		private class NotAutoApprovedTrail {
			public NotAutoApprovedTrail(SafeReader sr) {
				this.traces = new SortedDictionary<int, NotAutoApprovedTrace>();

				if (sr == null) {
					TrailID = 0;
					return;
				} // if

				sr.Fill(this);
				Add(sr);
			} // constructor

			public bool IsEmpty {
				get { return TrailID <= 0; }
			} // IsEmpty

			public void Add(SafeReader sr) {
				var trace = sr.Fill<NotAutoApprovedTrace>();
				this.traces[trace.Position] = trace;
			} // Add

			public long TrailID { get; set; }

			public DateTime DecisionTime { get; set; }

			[FieldName("DecisionStatus")]
			public string Status { get; set; }

			[FieldName("TrailTag")]
			public string RawTag {
				get { return Tag; }
				set {
					string val = (value ?? string.Empty).Trim();

					int pos = val.IndexOf('_', 1);

					if (val.StartsWith("#") && (pos > 0))
						Tag = val.Substring(1, pos - 1);
					else
						Tag = val;
				} // set
			} // RawTag

			public string Tag { get; private set; }

			public string GetTrailID(string name) {
				return IsEmpty ? "N/A " + name : name + " (" + TrailID + ")";
			} // GetTrailID

			public string DecisionTimeStr {
				get { return IsEmpty ? string.Empty : DecisionTime.ToString("d/MMM/yyyy H:mm:ss", gb); }
			} // DecisionTimeStr

			public string StatusStr {
				get { return IsEmpty ? string.Empty : Status; }
			} // StatusStr

			public string TagStr {
				get { return IsEmpty ? string.Empty : Tag; }
			} // TagStr

			public NotAutoApprovedTrace[] Traces {
				get {
					var lst = new List<NotAutoApprovedTrace>();

					foreach (var trace in this.traces.Values) {
						switch (trace.TraceName) {
						case "ReduceOutstandingPrincipal":
						case "AmountOutOfRange":
						case "Complete":
							if (lst.Count < 1)
								lst.Add(trace);

							break;

						default:
							lst.Add(trace);
							break;
						} // switch
					} // for each

					return lst.ToArray();
				} // get
			} // Traces

			private readonly SortedDictionary<int, NotAutoApprovedTrace> traces;

			private static readonly CultureInfo gb = new CultureInfo("en-GB", false);
		} // class NotAutoApprovedTrail

		private class NotAutoApprovedTrace {
			public int Position { get; set; }

			[FieldName("TraceName")]
			public string FullTraceName {
				get { return TraceName; }
				set {
					string val = (value ?? string.Empty);
					int pos = val.LastIndexOf('.');
					TraceName = pos < 0 ? val : val.Substring(pos + 1);
				} // set
			} // FullTraceName

			public string Comment { get; set; }

			public string TraceName { get; private set; }

			public string TraceNameStr { get { return (TraceName ?? string.Empty).Trim(); } }

			public string CommentStr { get { return (Comment ?? string.Empty).Trim(); } }
		} // class NotAutoApprovedTrace
	} // class BaseReportHandler
} // namespace Reports
