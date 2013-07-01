using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web.UI;
using Aspose.Cells;
using Ezbob.Logger;
using Html;
using Reports;
using System.Web;
using Ezbob.Database;

namespace EzReportsWeb {
	public partial class Default : Page {
		private static WebReportHandler reportHandler;

		protected void Page_Load(object sender, EventArgs e) {
			if (!IsPostBack) {
				var log = new LegacyLog();

				reportHandler = new WebReportHandler(new SqlConnection(log), log);

				List<Report> reportsList = reportHandler.GetReportsList(HttpContext.Current.User.Identity.Name);

				if (reportsList.Count == 0) {
					divFilter.Visible = false;
					return;
				} // if

				ddlReportTypes.DataTextField = "Title";
				ddlReportTypes.DataValueField = "Title";
				ddlReportTypes.DataSource = reportsList;
				ddlReportTypes.DataBind();
			} // if

			DateTime fDate, tDate;
			bool isDaily;

			GetDates(out fDate, out tDate, out isDaily);

			fromDate.Attributes.Add("max", DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
			toDate.Attributes.Add("max", DateTime.Today.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

			fromDate.Value = fDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
			toDate.Value   = tDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
		} // Page_Load

		protected void btnShowReport_Click(object sender, EventArgs e) {
			DateTime fDate, tDate;
			bool isDaily;

			GetDates(out fDate, out tDate, out isDaily);

			ATag data = reportHandler.GetReportData(ddlReportTypes.SelectedItem, fDate, tDate, isDaily);

			var reportData = new LiteralControl(data.ToString());

			divReportData.Controls.Add(reportData);
		} // btnShowReport_Click

		protected void BtnGetExcelClick(object sender, EventArgs e) {
			DateTime fDate, tDate;
			bool isDaily;

			GetDates(out fDate, out tDate, out isDaily);

			var wb = reportHandler.GetWorkBook(ddlReportTypes.SelectedItem, fDate, tDate, isDaily);

			var filename = (ddlReportTypes.SelectedItem + "_" + fDate.ToString("yyyy-MM-dd") + ".xlsx").Replace(" ", "_");
			var ostream = new MemoryStream();
			wb.Save(ostream, FileFormatType.Excel2007Xlsx);

			Response.Clear();
			Response.ContentType = "Application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
			Response.AddHeader("Content-Disposition", "attachment; filename=" + filename);
			Response.BinaryWrite(ostream.ToArray());
			// myMemoryStream.WriteTo(Response.OutputStream); //works too
			Response.Flush();
			Response.Close();
			Response.End();
			ostream.Close();
		} // BtnGetExcelClick

		private void GetDates(out DateTime fDate, out DateTime tDate, out bool isDaily) {
			fDate = DateTime.Today;
			tDate = DateTime.Today.AddDays(1);

			isDaily = false;

			switch (rblFilter.SelectedValue) {
			case "Daily":
				isDaily = true;
				break;

			case "Weekly":
				fDate = fDate.AddDays(-7);
				break;

			case "Monthly":
				fDate = fDate.AddMonths(-1);
				break;

			case "MonthToDate":
				fDate = new DateTime(fDate.Year, fDate.Month, 1);
				break;

			case "Custom":
				if (!DateTime.TryParse(fromDate.Value, out fDate))
					fDate = DateTime.Today;

				if (!DateTime.TryParse(toDate.Value, out tDate))
					tDate = fDate.AddDays(1);

				if (tDate.DayOfYear - fDate.DayOfYear == 1)
					isDaily = true;

				break;
			} // switch
		} // GetDates

		protected void rblFilter_SelectedIndexChanged(object sender, EventArgs e) {
			divCustomFilter.Visible = rblFilter.SelectedValue == "Custom";
		} // rblFilter_SelectedIndexChanged
	} // class Default
} // namespace EzReportsWeb
