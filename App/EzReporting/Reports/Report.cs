using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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

		public string GetTitle(DateTime oDate, string sSeparator = " ", DateTime? oToDate = null) {
			return GetTitle(sSeparator, DateToString(oDate), " - ", oToDate);
		} // GetTitle

		public string GetMonthTitle(DateTime oDate, DateTime? oToDate = null) {
			return GetTitle(" for ", DateToMonth(oDate), " Until ", oToDate);
		} // GetMonthTitle

		private string GetTitle(string sSeparator, string sDate, string sSeparator2, DateTime? oToDate) {
			var os = new StringBuilder();

			os.Append(Title);
			os.Append(sSeparator);
			os.Append(sDate);

			if (oToDate != null) {
				os.Append(sSeparator2);
				os.Append(DateToString(((DateTime)oToDate).AddDays(-1)));
			} // if

			return os.ToString();
		} // GetTitle

		public static string DateToString(DateTime oDate) {
			return oDate.ToString("MMMM d", CultureInfo.InvariantCulture);
		} // DateToString

		public static string DateToMonth(DateTime oDate) {
			return oDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
		} // DateToMonth

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

		private static string ReadStyle() {
			Assembly a = Assembly.GetExecutingAssembly();

			Stream s = a.GetManifestResourceStream("Reports.Report.css");

			StreamReader sr = new StreamReader(s);

			return sr.ReadToEnd();
		} // ReadStyle

		public static ATag GetStyle() {
			return new global::Html.Tags.Style(ReadStyle());
		} // GetStyle

		public static Dictionary<string, string> ParseStyle() {
			string sStyle = Regex.Replace(ReadStyle().Trim().Replace("\n", " ").Replace("\r", " ").Trim(), @"\s+", " ");

			sStyle = Regex.Replace(sStyle, @"\/\*.*?\*\/", "").Trim();
			
			sStyle = Regex.Replace(sStyle, @"\s+", " ");

			string[] ary = Regex.Split(sStyle, @"\s*[\{\}]\s*").Where(s => s != string.Empty).ToArray();

			var oRes = new Dictionary<string, string>();

			for (int i = 0; i < ary.Length; i += 2) {
				string[] arySelectors = Regex.Split(ary[i], @"\s*,\s*");

				foreach (string sSelector in arySelectors)
					oRes[sSelector] = ary[i + 1];
			} // for

			return oRes;
		} // ParseStyle
	} // class Report
} // namespace Reports