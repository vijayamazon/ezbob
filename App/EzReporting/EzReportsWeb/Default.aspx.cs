using System;
using System.Collections.Generic;
using System.Web.UI;
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

				//  fromDate.Attributes.Add("max", DateTime.Today.ToString("yyyy-MM-dd"));
				//  toDate.Attributes.Add("max", DateTime.Today.ToString("yyyy-MM-dd"));
			} // if
		} // Page_Load

		protected void btnShowReport_Click(object sender, EventArgs e) {
			DateTime today = DateTime.Today;//.ToString("yyyy-MM-dd");
			DateTime tomorrow = DateTime.Today.AddDays(1);//.ToString("yyyy-MM-dd");
			DateTime fDate = today;
			DateTime tDate = tomorrow;
			bool isDaily = false;

			switch (rblFilter.SelectedValue) {
			case "Daily":
				isDaily = true;
				break;

			case "Weekly":
				fDate = DateTime.Today.AddDays(-7);//.ToString("yyyy-MM-dd");
				break;

			case "Monthly":
				fDate = DateTime.Today.AddMonths(-1);//.ToString("yyyy-MM-dd");
				break;

			case "Custom":
				DateTime.TryParse(Request.Form["fromDate"], out fDate);
				DateTime.TryParse(Request.Form["toDate"], out tDate);
				if (tDate.DayOfYear - fDate.DayOfYear == 1)
					isDaily = true;

				break;
			} // switch

			ATag data = reportHandler.GetReportData(ddlReportTypes.SelectedItem, fDate.ToString("yyyy-MM-dd"), tDate.ToString("yyyy-MM-dd"), isDaily);

			var reportData = new LiteralControl(data.ToString());

			divReportData.Controls.Add(reportData);
		} // btnShowReport_Click

		protected void rblFilter_SelectedIndexChanged(object sender, EventArgs e) {
			divCustomFilter.Visible = rblFilter.SelectedValue == "Custom";
		} // rblFilter_SelectedIndexChanged

		public string GetFormatedDate() {
			return DateTime.Today.ToString("yyyy-MM-dd");
		} // GetFormatedDate
	} // class Default
} // namespace EzReportsWeb
