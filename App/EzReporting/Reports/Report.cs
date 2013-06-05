using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Html;

namespace Reports {
	public enum ReportType {
		RPT_NEW_CLIENT,
		RPT_PLANNED_PAYTMENT,
		RPT_DAILY_STATS,
		RPT_DAILY_COLLECTION,
		RPT_IN_WIZARD,
		RPT_MARKETING,
		RPT_LINGER_CLIENT,
		RPT_BUGS,
		RPT_GENERIC
	} // enum ReportType

	public class Report {
		public string Title { get; set; }
		public ReportType Type { get; set; }
		public string StoredProcedure { get; set; }
		public bool IsWeekly { get; set; }
		public bool IsMonthly { get; set; }
		public bool IsDaily { get; set; }
		public ColumnInfo[] Columns { get; set; }
		public string ToEmail { get; set; }
		public bool IsMonthToDate { get; set; }

		public static ColumnInfo[] ParseHeaderAndFields(string sHeader, string sFields) {
			var columns = new List<ColumnInfo>();

			string[] aryHeader = (sHeader ?? "").Trim().Split(',');
			string[] aryFields = (sFields ?? "").Trim().Split(',');

			if ((aryHeader == null) || (aryFields == null))
				throw new Exception("Header/Fields information not found.");

			if ((aryHeader.Length < 1) || (aryFields.Length < 1))
				throw new Exception("Header/Fields information not specified.");

			if (aryHeader.Length != aryFields.Length)
				throw new Exception("Header/Fields information does not match.");

			for (int i = 0; i < aryHeader.Length; i++)
				columns.Add(new ColumnInfo(aryHeader[i], aryFields[i]));

			return columns.ToArray();
		} // ParseHeadersAndFields

		public static ATag GetStyle() {
			Assembly a = Assembly.GetExecutingAssembly();

			Stream s = a.GetManifestResourceStream("Reports.Report.css");

			StreamReader sr = new StreamReader(s);

			string sStyle = sr.ReadToEnd();

			return new Style(sStyle);
		} // GetStyle
	} // class Report
} // namespace Reports