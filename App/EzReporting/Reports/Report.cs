namespace Reports {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text;
	using System.Text.RegularExpressions;
	using Ezbob.Database;
	using Ezbob.Utils.Html;
	using Ezbob.Utils.Html.Tags;
	using JetBrains.Annotations;

	public class Report {
		#region public const

		public const string DateRangeArg = "DateRange";
		public const string CustomerArg = "Customer";
		public const string ShowNonCashArg = "ShowNonCashTransactions";

		#endregion public const

		#region public static

		#region method GetScheduledReportsList

		public static SortedDictionary<string, Report> GetScheduledReportsList(AConnection oDB) {
			return FillReportArgs(oDB, 
				ReportListStoredProc,
				new QueryParameter("@RptType", "")
			);
		} // GetScheduledReportsList

		#endregion method GetScheduledReportsList

		#region method GetUserReportList

		public static SortedDictionary<string, Report> GetUserReportsList(AConnection oDB, string userName = null) {
			var users = new List<QueryParameter>();

			userName = (userName ?? "").Trim();

			if (userName != string.Empty)
				users.Add(new QueryParameter("@UserName", userName));

			return FillReportArgs(oDB, "RptGetUserReports", users.ToArray());
		} // GetUserReportsList

		#endregion method GetUserReportList

		#endregion public static

		#region constructor

		public Report() {
			Arguments = new SortedDictionary<string, string>();
		} // construtor

		public Report(SafeReader row) : this() {
			Init(row);
		} // constructor

		public Report(AConnection oDB, ReportType nReportTypeName) : this(oDB, nReportTypeName.ToString()) {
		} // constructor

		public Report(AConnection oDB, string sReportTypeName) : this() {
			bool bFound = false;

			oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					if (sr["Type"] == sReportTypeName) {
						Init(sr);
						bFound = true;
					} // if

					return bFound
						? ActionResult.SkipAll
						: ActionResult.Continue;
				},
				ReportListStoredProc,
				CommandSpecies.StoredProcedure,
				new QueryParameter("@RptType", sReportTypeName)
			);

			if (!bFound)
				throw new Exception(string.Format("Report cannot be found by name {0}", sReportTypeName));

			List<ReportArg> args = LoadReportArgs(oDB, sReportTypeName);

			foreach (ReportArg row in args)
				AddArgument(row.ArgumentName);
		} // constructor

		#endregion constructor

		#region method Init

		private void Init(SafeReader row) {
			ReportType type;

			TypeName = row["Type"];

			if (!Enum.TryParse<ReportType>(TypeName, out type))
				type = ReportType.RPT_GENERIC;

			Type = type;
			Title = row["Title"];
			StoredProcedure = row["StoredProcedure"];
			IsDaily = row["IsDaily"];
			IsWeekly = row["IsWeekly"];
			IsMonthly = row["IsMonthly"];
			Columns = Report.ParseHeaderAndFields(row["Header"], row["Fields"]);
			ToEmail = ((string)row["ToEmail"] ?? string.Empty).Trim();
			IsMonthToDate = row["IsMonthToDate"];
		} // Init

		#endregion method Init

		#region method AddArgument

		public void AddArgument(string sArgument) {
			sArgument = (sArgument ?? "").Trim();
			Arguments[sArgument] = sArgument;
		} // AddArgument

		#endregion method AddArgument

		#region properties

		public string Title { get; set; }
		public ReportType Type { get; set; }
		public string TypeName { get; set; }
		public string StoredProcedure { get; set; }
		public bool IsWeekly { get; set; }
		public bool IsMonthly { get; set; }
		public bool IsDaily { get; set; }
		public ColumnInfo[] Columns { get; set; }
		public string ToEmail { get; set; }
		public bool IsMonthToDate { get; set; }

		public SortedDictionary<string, string> Arguments { get; private set; }

		#endregion properties

		#region title related

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

		#endregion title related

		#region date conversion

		public static string DateToString(DateTime oDate) {
			return oDate.ToString("MMMM d yyyy", CultureInfo.InvariantCulture);
		} // DateToString

		public static string DateToMonth(DateTime oDate) {
			return oDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
		} // DateToMonth

		#endregion date conversion

		#region method ParseHeaderAndFields

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

		#endregion method ParseHeaderAndFields

		#region style related

		public static ATag GetStyle() {
			return new Style(ReadStyle());
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

		private static string ReadStyle() {
			string sFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Report.css");

			if (File.Exists(sFilePath)) {
				try {
					return File.ReadAllText(sFilePath);
				}
				// ReSharper disable EmptyGeneralCatchClause
				catch (Exception) {
					// quietly ignore any error
				} // try
				// ReSharper restore EmptyGeneralCatchClause
			} // if

			Assembly a = Assembly.GetExecutingAssembly();

			Stream s = a.GetManifestResourceStream("Reports.Report.css");

			StreamReader sr = new StreamReader(s);

			return sr.ReadToEnd();
		} // ReadStyle

		#endregion style related

		#region private const

		private const string ReportListStoredProc = "RptScheduler_GetReportList";
		private const string ReportArgsStoredProc = "RptScheduler_GetReportArgs";

		#endregion private const

		#region class ReportArg

		private class ReportArg {
			[UsedImplicitly]
			public int ReportID { get; set; }

			[UsedImplicitly]
			public string ReportType { get; set; }

			[UsedImplicitly]
			public int ArgumentID { get; set; }

			[UsedImplicitly]
			public string ArgumentName { get; set; }
		} // ReportArg

		#endregion class ReportArg

		#region private static

		#region method LoadReportArgs

		private static List<ReportArg> LoadReportArgs(AConnection oDB, string sReportTypeName = null) {
			var oParams = new List<QueryParameter>();

			if (!string.IsNullOrWhiteSpace(sReportTypeName))
				oParams.Add(new QueryParameter("@RptType", sReportTypeName));

			return oDB.Fill<ReportArg>(
				ReportArgsStoredProc,
				CommandSpecies.StoredProcedure,
				oParams.ToArray()
			);
		} // LoadReportArgs

		#endregion method LoadReportArgs

		#region method FillReportArgs

		private static SortedDictionary<string, Report> FillReportArgs(AConnection oDB, string sSpName, params QueryParameter[] arySpArgs) {
			var reportList = new SortedDictionary<string, Report>();

			oDB.ForEachRowSafe((sr, bRowsetStart) => {
				var rpt = new Report(sr);

				reportList[rpt.TypeName] = rpt;

				return ActionResult.Continue;
			}, sSpName, CommandSpecies.StoredProcedure, arySpArgs);

			if (reportList.Count > 0) {
				List<ReportArg> args = LoadReportArgs(oDB);

				Report oLastReport = null;
				string sLastType = null;

				foreach (ReportArg row in args) {
					if (sLastType != row.ReportType) {
						if (reportList.ContainsKey(row.ReportType)) {
							sLastType = row.ReportType;
							oLastReport = reportList[sLastType];
						}
						else
							continue;
					} // if

					oLastReport.AddArgument(row.ArgumentName);
				} // for each
			} // if report list is not null

			return reportList;
		} // FillReportArgs 

		#endregion method FillReportArgs

		#endregion private static
	} // class Report
} // namespace Reports