using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Aspose.Cells;
using Ezbob.Database;
using Ezbob.Logger;
using Html;
using Reports;
using CheckBox = System.Web.UI.WebControls.CheckBox;

namespace EzReportsWeb {
	public partial class Default : Page {
		private static WebReportHandler reportHandler;
		private static ASafeLog log;
		private static AConnection oDB;
		private static bool bIsAdmin;

		public static bool IsAdmin() {
			return bIsAdmin;
		} // IsAdmin

		protected void Page_Load(object sender, EventArgs e) {
			if (!IsPostBack) {
				log = new LegacyLog();

				oDB = new SqlConnection(log);

				reportHandler = new WebReportHandler(oDB, log);

				SortedDictionary<string, Report> reportsList = reportHandler.GetReportsList(HttpContext.Current.User.Identity.Name);

				if (reportsList.Count == 0) {
					divFilter.Visible = false;
					return;
				} // if

				ddlReportTypes.DataTextField = "Title";
				ddlReportTypes.DataValueField = "Title";
				ddlReportTypes.DataSource = reportsList.Values;
				ddlReportTypes.DataBind();
			} // if

			bIsAdmin = oDB.ExecuteScalar<bool>(
				"SELECT IsAdmin FROM ReportUsers WHERE UserName = @uname",
				CommandSpecies.Text,
				new QueryParameter("@uname", HttpContext.Current.User.Identity.Name)
			);

			chkIsAdmin.Checked = bIsAdmin;

			if (bIsAdmin)
				InitAdminArea(oDB, log, IsPostBack);

			divAdminMsg.InnerText = string.Empty;

			DateTime fDate, tDate;
			bool isDaily;

			GetDates(out fDate, out tDate, out isDaily);

			fromDate.Attributes.Add("max", DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
			toDate.Attributes.Add("max", DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

			fromDate.Value = fDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
			toDate.Value   = tDate.AddDays(-1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

			AdjustUIFiltersForReport();
		} // Page_Load

		protected void ddlReportTypes_OnSelectedIndexChanged(object sender, EventArgs e) {
			AdjustUIFiltersForReport();
		} // ddlReportTypes_OnSelectedIndexChanged

		private void AdjustUIFiltersForReport() {
			Report rpt = reportHandler.GetReport(ddlReportTypes.SelectedValue);

			if (rpt == null)
				return;

			chkShowNonCash.Visible = rpt.Arguments.ContainsKey(Report.ShowNonCashArg);
			divDateFilter.Visible = rpt.Arguments.ContainsKey(Report.DateRangeArg);
			divUserKeyField.Visible = rpt.Arguments.ContainsKey(Report.CustomerArg);
		} // AdjustUIFiltersForReport

		protected void btnShowReport_Click(object sender, EventArgs e) {
			bool isDaily;

			ReportQuery rptDef = CreateReportQuery(out isDaily);

			var oColumnTypes = new List<string>();

			ATag data = reportHandler.GetReportData(ddlReportTypes.SelectedItem, rptDef, isDaily, oColumnTypes);

			var aoColumnDefs = oColumnTypes.Select(
				sType => string.Format("{{ \"sType\": \"{0}\" }}", sType)
			).ToList();

			divReportColumnTypes.Controls.Add(new LiteralControl(
				"[" + string.Join(", ", aoColumnDefs) + "]"
			));

			var reportData = new LiteralControl(data.ToString());

			divReportData.Controls.Add(reportData);
		} // btnShowReport_Click

		private ReportQuery CreateReportQuery(out bool isDaily) {
			DateTime fDate, tDate;

			GetDates(out fDate, out tDate, out isDaily);

			var rq = new ReportQuery {
				DateStart = fDate,
				DateEnd = tDate,
				ShowNonCashTransactions = chkShowNonCash.Checked ? 1 : 0
			};

			string sUserKey = UserKey.Value.Trim();

			if (sUserKey != string.Empty) {
				int nUserID = 0;

				if (int.TryParse(sUserKey, out nUserID))
					rq.UserID = nUserID;
				else
					rq.UserID = null;

				rq.UserNameOrEmail = sUserKey;
			} // if sUserKey is not empty

			return rq;
		} // CreateReportQuery

		protected void BtnGetExcelClick(object sender, EventArgs e) {
			bool isDaily;

			ReportQuery rptDef = CreateReportQuery(out isDaily);

			var wb = reportHandler.GetWorkBook(ddlReportTypes.SelectedItem, rptDef, isDaily);

			var filename = (ddlReportTypes.SelectedItem + "_" + ((DateTime)rptDef.DateStart).ToString("yyyy-MM-dd") + ".xlsx").Replace(" ", "_");
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
			tDate = fDate.AddDays(1);

			isDaily = false;

			switch (rblFilter.SelectedValue) {
			case "Today":
				isDaily = true;
				break;

			case "Yesterday":
				isDaily = true;
				fDate = fDate.AddDays(-1);
				tDate = tDate.AddDays(-1);
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

			case "Lifetime":
				fDate = new DateTime(2012, 5, 1);
				break;

			case "Custom":
				if (!DateTime.TryParse(fromDate.Value, out fDate))
					fDate = DateTime.Today;

				if (DateTime.TryParse(toDate.Value, out tDate)) {
					if (tDate < fDate) {
						DateTime tmp = tDate;
						tDate = fDate;
						fDate = tmp;
					} // if

					tDate = tDate.AddDays(1); // custom end date selected in UI must be included
				}
				else
					tDate = fDate.AddDays(1);

				if (tDate.DayOfYear - fDate.DayOfYear == 1)
					isDaily = true;

				break;
			} // switch
		} // GetDates

		protected void rblFilter_SelectedIndexChanged(object sender, EventArgs e) {
			divCustomFilter.Visible = rblFilter.SelectedValue == "Custom";
		} // rblFilter_SelectedIndexChanged

		protected void btnAdminCreateUser_Click(object sender, EventArgs e) {
			divAdminMsg.InnerText = "Performing task...";

			string sUserName = edtAdminUserName.Text.Trim();

			if (sUserName.Length < 3) {
				divAdminMsg.InnerText = "User name too short.";
				return;
			} // if

			var rpta = new ReportAuthenticationLib.ReportAuthentication(oDB, log);

			try {
				rpta.AddUserToDb(sUserName, sUserName);
				divAdminMsg.InnerText = "User has been created.";
			}
			catch (Exception ex) {
				divAdminMsg.InnerText = string.Format("Action failed: {0}", ex.Message);
			}

			InitAdminArea(oDB, log);
		} // btnAdminCreateUser_Click

		protected void btnAdminResetPass_Click(object sender, EventArgs e) {
			divAdminMsg.InnerText = "Performing task...";

			if (selAdminUserResetPass.SelectedItem == null) {
				divAdminMsg.InnerText = "User not selected.";
				return;
			} // if

			string sUserName = selAdminUserResetPass.SelectedItem.Value;

			string sPassword = edtAdminPassword.Text.Trim();

			if (sPassword.Length < 6) {
				divAdminMsg.InnerText = "Password too short.";
				return;
			} // if

			var rpta = new ReportAuthenticationLib.ReportAuthentication(oDB, log);

			try {
				rpta.ResetPassword(sUserName, sPassword);
				divAdminMsg.InnerText = "Password has been reset.";
			}
			catch (Exception ex) {
				divAdminMsg.InnerText = string.Format("Action failed: {0}", ex.Message);
			}

			InitAdminArea(oDB, log);
		} // btnAdminResetPass_Click

		protected void btnAdminDropUser_Click(object sender, EventArgs e) {
			divAdminMsg.InnerText = "Performing task...";

			if (selAdminUserDrop.SelectedItem == null) {
				divAdminMsg.InnerText = "User not selected.";
				return;
			} // if

			var rpta = new ReportAuthenticationLib.ReportAuthentication(oDB, log);

			try {
				int nUserID = Convert.ToInt32(selAdminUserDrop.SelectedItem.Value);
				rpta.DropUser(nUserID);
				divAdminMsg.InnerText = "User has been dropped.";
			}
			catch (Exception ex) {
				divAdminMsg.InnerText = string.Format("Action failed: {0}", ex.Message);
			}

			InitAdminArea(oDB, log);
		} // btnAdminDropUser_Click

		private void InitAdminArea(AConnection oDB, ASafeLog log, bool bIsPostBack = false) {
			DataTable oDbUsers = oDB.ExecuteReader("SELECT Id, Name FROM ReportUsers ORDER BY Name", CommandSpecies.Text);

			var oUsers = new SortedDictionary<string, int>();

			foreach (DataRow row in oDbUsers.Rows) {
				int nUserID = Convert.ToInt32(row["Id"]);
				string sUserName = row["Name"].ToString();

				oUsers[sUserName] = nUserID;
			} // for each user row

			SetReportUserMap(oDB, oUsers);

			if (!bIsPostBack)
				FillUserDropDowns(oUsers);
		} // InitAdminArea

		private void FillUserDropDowns(SortedDictionary<string, int> oUsers) {
			selAdminUserDrop.Items.Clear();
			selAdminUserResetPass.Items.Clear();

			selAdminUserDrop.Items.Add(new ListItem { Value = "", Text = "" });

			foreach (KeyValuePair<string, int> pair in oUsers) {
				selAdminUserDrop.Items.Add(new ListItem { Value = pair.Value.ToString(), Text = pair.Key });
				selAdminUserResetPass.Items.Add(new ListItem { Value = pair.Value.ToString(), Text = pair.Key });
			} // for each user
		} // FillUserDropDowns

		private void SetReportUserMap(AConnection oDB, SortedDictionary<string, int> oUsers) {
			DataTable oDbMap = oDB.ExecuteReader("SELECT UserID, ReportID FROM ReportsUsersMap ORDER BY ReportID", CommandSpecies.Text);

			var oMap = new SortedDictionary<int, SortedDictionary<int, int>>();

			foreach (DataRow row in oDbMap.Rows) {
				int nUserID = Convert.ToInt32(row["UserID"]);
				int nReportID = Convert.ToInt32(row["ReportID"]);

				if (!oMap.ContainsKey(nReportID))
					oMap[nReportID] = new SortedDictionary<int, int>();

				oMap[nReportID][nUserID] = 1;
			} // for each row

			tblReportUserMap.Caption = "Report - User Map";

			tblReportUserMap.Rows.Clear();

			var oHeaderRow = new TableHeaderRow { TableSection = TableRowSection.TableHeader };
			tblReportUserMap.Rows.Add(oHeaderRow);

			oHeaderRow.Cells.Add(new TableHeaderCell());

			foreach (KeyValuePair<string, int> pair in oUsers)
				oHeaderRow.Cells.Add(new TableHeaderCell { Text = pair.Key.Replace(" ", "<br>") });

			DataTable oDbReports = oDB.ExecuteReader("SELECT Id, Title FROM ReportScheduler ORDER BY Title", CommandSpecies.Text);

			string sRowClass = "odd";

			foreach (DataRow row in oDbReports.Rows) {
				int nReportID = Convert.ToInt32(row["Id"]);
				string sReportTitle = row["Title"].ToString();

				var oReportRow = new TableRow { TableSection = TableRowSection.TableBody };
				tblReportUserMap.Rows.Add(oReportRow);

				oReportRow.CssClass = sRowClass;

				sRowClass = (sRowClass == "odd") ? "even" : "odd";

				oReportRow.Cells.Add(new TableHeaderCell { Text = sReportTitle.Replace(" ", "<br>") });

				foreach (KeyValuePair<string, int> pair in oUsers) {
					int nUserID = pair.Value;
					string sUserName = pair.Key;
					bool bAllowed = oMap.ContainsKey(nReportID) && oMap[nReportID].ContainsKey(nUserID);

					var oCell = new TableCell();
					oReportRow.Cells.Add(oCell);

					var oChk = new CheckBox();
					oCell.Controls.Add(oChk);

					if (bAllowed)
						oCell.CssClass = "checked";

					oCell.ToolTip = sReportTitle + " to " + sUserName;
					oChk.Checked = bAllowed;
					oChk.AutoPostBack = true;

					oChk.Attributes["userid"] = nUserID.ToString();
					oChk.Attributes["reportid"] = nReportID.ToString();

					oChk.CheckedChanged += ReportPermissionTrigger;
				} // for each user
			} // for each report row
		} // SetReportUserMap

		protected void ReportPermissionTrigger(object sender, EventArgs args) {
			var oTarget = (CheckBox)sender;

			int nUserID = Convert.ToInt32(oTarget.Attributes["userid"]);
			int nReportID = Convert.ToInt32(oTarget.Attributes["reportid"]);

			string sQuery = oTarget.Checked
				? "INSERT INTO ReportsUsersMap (UserID, ReportID) VALUES (@UserID, @ReportID)"
				: "DELETE FROM ReportsUsersMap WHERE UserID = @UserID AND ReportID = @ReportID";

			oDB.ExecuteNonQuery(sQuery,
				CommandSpecies.Text,
				new QueryParameter("@UserID", nUserID),
				new QueryParameter("@ReportID", nReportID)
			);

			divAdminMsg.InnerText = oTarget.Checked
				? "Permission granted"
				: "Permission dropped";

			((TableCell)oTarget.Parent).CssClass = oTarget.Checked
				? "checked"
				: "";
		} // ReportPermissionTrigger
	} // class Default
} // namespace EzReportsWeb
