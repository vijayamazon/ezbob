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
		public string[] Headers { get; set; }
		public string[] Fields { get; set; }
		public string ToEmail { get; set; }
		public bool IsMonthToDate { get; set; }
	} // class Report
} // namespace Reports