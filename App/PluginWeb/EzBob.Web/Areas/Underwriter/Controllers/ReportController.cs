namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using System.Web.Script.Serialization;
	using Code.ReportGenerator;
	using EZBob.DatabaseLib.Model.Database.Report;
	using Ezbob.Database;
	using Ezbob.Utils.Extensions;
	using Ezbob.Utils.Html;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Models.Reports;
	using global::Reports;

	public class ReportController : Controller {
		private readonly ReportUsersMapsRepository _reportsRepository;
		private readonly ReportRepository _reportRepository;
		private readonly IWorkplaceContext _context;
		public ReportController(ReportUsersMapsRepository reportsRepository, IWorkplaceContext context, ReportRepository reportRepository) {
			_reportsRepository = reportsRepository;
			_context = context;
			_reportRepository = reportRepository;
		}

		[Ajax]
		[HttpGet]
		public JsonResult GetAll() {
			var uwId = _context.UserId;
			var reports = _reportsRepository.GetAllUnderwriterReports(uwId);
			var model = new ReportModel();
			var dates = Enum.GetValues(typeof(ReportDate)).Cast<ReportDate>().Select(x => new { key = x.ToString(), value = x.DescriptionAttr() });
			return Json(new { reports = reports.Select(model.ToModel), dates = dates }, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		public ContentResult GetReportDates(int reportId, DateTime from, DateTime to, string customer, bool? nonCash) {
			var report = GetReport(reportId);
			from = DateTime.SpecifyKind(from, DateTimeKind.Utc);
			to = DateTime.SpecifyKind(to, DateTimeKind.Utc);
			var rptDef = new ReportQuery(report, from, to, customer, nonCash);
			var oColumnTypes = new List<string>();
			AConnection oDB = DbConnectionGenerator.Get();
			var reportHandler = new ReportHandler(oDB);
			bool isError;

			ATag data = reportHandler.GetReportData(report, rptDef, oColumnTypes, out isError);

			var serializer = new JavaScriptSerializer {
				MaxJsonLength = Int32.MaxValue
			};

			return new ContentResult {
				Content = serializer.Serialize(new { report = data.ToString(), columns = oColumnTypes }),
				ContentType = "application/json",
			};
		}

		[Ajax]
		[HttpPost]
		public ContentResult GetReport(int reportId, ReportDate reportDate, string customer, bool? nonCash) {
			var dates = ReporDateRanges.GetDates(reportDate);
			dates.From = DateTime.SpecifyKind(dates.From, DateTimeKind.Utc);
			dates.To = DateTime.SpecifyKind(dates.To, DateTimeKind.Utc);
			return GetReportDates(reportId, dates.From, dates.To, customer, nonCash);
		}

		public FileResult DownloadReportDates(int reportId, DateTime from, DateTime to, string customer, bool? nonCash) {
			var report = GetReport(reportId);
			var rptDef = new ReportQuery(report, from, to, customer, nonCash);
			AConnection oDB = DbConnectionGenerator.Get();
			var reportHandler = new ReportHandler(oDB);

			var excel = reportHandler.GetWorkBook(report, rptDef);
			var fc = new FileContentResult(excel.GetAsByteArray(), "Application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") {
				FileDownloadName = report.Title.Replace(" ", "") + from.ToString("yyyy-MM-dd") + to.ToString("yyyy-MM-dd") + ".xlsx"
			};

			return fc;
		}

		public FileResult DownloadReport(int reportId, ReportDate reportDate, string customer, bool? nonCash) {
			var dates = ReporDateRanges.GetDates(reportDate);
			return DownloadReportDates(reportId, dates.From, dates.To, customer, nonCash);
		}

		[NonAction]
		private Report GetReport(int reportId) {
			var dbReport = _reportRepository.Get(reportId);

			ReportType type;
			if (!Enum.TryParse(dbReport.Type, out type)) {
				type = ReportType.RPT_GENERIC;
			}
			var report = new Report {
				Title = dbReport.Title,
				Type = type,
				StoredProcedure = dbReport.StoredProcedure,
				Columns = Report.ParseHeaderAndFields(dbReport.Header, dbReport.Fields),
			};
			foreach (var arg in dbReport.Arguments) {
				report.AddArgument(arg.ReportArgument.Name);
			}

			return report;
		}
	}
}
