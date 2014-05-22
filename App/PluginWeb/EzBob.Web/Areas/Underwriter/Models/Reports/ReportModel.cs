namespace EzBob.Web.Areas.Underwriter.Models.Reports
{
	using System;
	using System.ComponentModel;
	using System.Globalization;
	using EZBob.DatabaseLib.Model.Database.Report;
	using System.Collections.Generic;
	using System.Linq;
	using Ezbob.Utils;

	public class ReportModel
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public IEnumerable<ArgumentModel> Arguments { get; set; }
		public ReportModel ToModel(DbReport report)
		{
			return new ReportModel
				{
					Id = report.Id,
					Title = report.Title,
					Arguments = report.Arguments.Select(x => new ArgumentModel { Argument = x.ReportArgument.Name })
				};
		}
	}

	public class ArgumentModel
	{
		public string Argument { get; set; }
	}

	public class ReportDateModel
	{
		public ReportDate ReporDate { get; set; }
		public DateTime From { get; set; }
		public DateTime To { get; set; }
		public bool IsDaily { get; set; }
	}

	public enum ReportDate
	{
		[Description("Today")]
		Today,
		[Description("Yesterday")]
		Yesterday,
		[Description("Today and yesterday")]
		Todasterday,
		[Description("Weekly (last 7 days)")]
		Weekly,
		[Description("This week (Sun to Sat)")]
		ThisWeek,
		[Description("Last week (Sun to Sat)")]
		LastWeek,
		[Description("Monthly (last 30 days)")]
		Monthly,
		[Description("Month to Date")]
		MonthToDate,
		[Description("Last calendar month")]
		LastMonth,
		[Description("Lifetime")]
		Lifetime,
		[Description("Custom")]
		Custom
	}

	public static class ReporDateRanges
	{
		public static ReportDateModel GetDates(ReportDate reportDate)
		{
			var date = new ReportDateModel
				{
					From = DateTime.Today,
					To = DateTime.Today.AddDays(1),
					IsDaily = false
				};

			switch (reportDate)
			{
				case ReportDate.Today:
					date.IsDaily = true;
					break;

				case ReportDate.Yesterday:
					date.IsDaily = true;
					date.From = date.From.AddDays(-1);
					date.To = date.To.AddDays(-1);
					break;

				case ReportDate.Todasterday:
					date.From = date.From.AddDays(-1);
					break;

				case ReportDate.ThisWeek:
					date.From = MiscUtils.FirstDayOfWeek();
					date.To = date.From.AddDays(CultureInfo.CurrentCulture.DateTimeFormat.DayNames.Length);
					break;

				case ReportDate.LastWeek:
					date.From = MiscUtils.FirstDayOfWeek().AddDays(-CultureInfo.CurrentCulture.DateTimeFormat.DayNames.Length);
					date.To = date.From.AddDays(CultureInfo.CurrentCulture.DateTimeFormat.DayNames.Length);
					break;

				case ReportDate.LastMonth:
					date.From = new DateTime(date.From.Year, date.From.Month, 1).AddMonths(-1);
					date.To = date.From.AddMonths(1);
					break;

				case ReportDate.Weekly:
					date.From = date.From.AddDays(-CultureInfo.CurrentCulture.DateTimeFormat.DayNames.Length);
					break;

				case ReportDate.Monthly:
					date.From = date.From.AddMonths(-1);
					break;

				case ReportDate.MonthToDate:
					date.From = new DateTime(date.From.Year, date.From.Month, 1);
					break;

				case ReportDate.Lifetime:
					date.From = new DateTime(2012, 9, 1);
					break;

				case ReportDate.Custom:
					return null;
					break;
			} // switch
			return date;
		}
	}
}