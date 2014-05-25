namespace EzBob.Web.Areas.Underwriter.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using Code.ReportGenerator;
	using EZBob.DatabaseLib.Model.Database.Report;
	using Ezbob.Database;
	using Ezbob.Utils.Extensions;
	using Html;
	using Infrastructure;
	using Infrastructure.Attributes;
	using Models.Reports;
	using global::Reports;

	public class ReportController : Controller
	{
		private readonly ReportUsersMapsRepository _reportsRepository;
		private readonly ReportRepository _reportRepository;
		private readonly IWorkplaceContext _context;
		public ReportController(ReportUsersMapsRepository reportsRepository, IWorkplaceContext context, ReportRepository reportRepository)
		{
			_reportsRepository = reportsRepository;
			_context = context;
			_reportRepository = reportRepository;
		}

		[Ajax]
		[HttpGet]
		public JsonResult GetAll()
		{
			var uwId = _context.UserId;
			var reports = _reportsRepository.GetAllUnderwriterReports(uwId);
			var model = new ReportModel();
			var dates = Enum.GetValues(typeof(ReportDate)).Cast<ReportDate>().Select(x => new {key = x.ToString(), value=x.DescriptionAttr()});
			return Json(new {reports = reports.Select(model.ToModel) , dates = dates}, JsonRequestBehavior.AllowGet);
		}

		[Ajax]
		[HttpPost]
		public JsonResult GetReportDates(int reportId, DateTime from, DateTime to)
		{
			var dbReport = _reportRepository.Get(reportId);

			if (dbReport.Arguments.Count != 1)
			{
				throw new NotImplementedException("Report with multiple argument types is not implemented");
			}

			var oDB = new SqlConnection();
			ReportType type;
			if (!Enum.TryParse(dbReport.Type, out type))
			{
				type = ReportType.RPT_GENERIC;
			}
			var report = new Report
			{
				Title = dbReport.Title,
				Type = type,
				StoredProcedure = dbReport.StoredProcedure,
				Columns = Report.ParseHeaderAndFields(dbReport.Header, dbReport.Fields),
			};

			report.AddArgument(dbReport.Arguments.First().ReportArgument.Name);

			var rptDef = new ReportQuery(report, from, to);
			var oColumnTypes = new List<string>();
			var reportHandler = new ReportHandler(oDB);
			bool isError;

			ATag data = reportHandler.GetReportData(report, rptDef, oColumnTypes, out isError);

			return Json(new { report = data.ToString(), columns = oColumnTypes });

		}
		[Ajax]
		[HttpPost]
		public JsonResult GetReport(int reportId, ReportDate reportDate)
		{
			var dates = ReporDateRanges.GetDates(reportDate);
			return GetReportDates(reportId, dates.From, dates.To);
		}

		public FileResult DownloadReportDates(int reportId, DateTime from, DateTime to)
		{
			var dbReport = _reportRepository.Get(reportId);

			if (dbReport.Arguments.Count != 1)
			{
				throw new NotImplementedException("Report with multiple argument types is not implemented");
			}

			var oDB = new SqlConnection();
			ReportType type;
			if (!Enum.TryParse(dbReport.Type, out type))
			{
				type = ReportType.RPT_GENERIC;
			}
			var report = new Report
			{
				Title = dbReport.Title,
				Type = type,
				StoredProcedure = dbReport.StoredProcedure,
				Columns = Report.ParseHeaderAndFields(dbReport.Header, dbReport.Fields),
			};

			report.AddArgument(dbReport.Arguments.First().ReportArgument.Name);
			var rptDef = new ReportQuery(report, from, to);
			var reportHandler = new ReportHandler(oDB);

			var excel = reportHandler.GetWorkBook(report, rptDef);
			var fc = new FileContentResult(excel.GetAsByteArray(), "Application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
			{
				FileDownloadName = report.Title.Replace(" ", "")
			};

			return fc;
		}

		public FileResult DownloadReport(int reportId, ReportDate reportDate)
		{
			var dates = ReporDateRanges.GetDates(reportDate);
			return DownloadReportDates(reportId, dates.From, dates.To);
		}
	}
}