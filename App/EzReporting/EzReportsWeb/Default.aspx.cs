namespace EzReportsWeb {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Html;
	using Reports;
	using System.Web.Script.Serialization;

	public partial class Default : Page {
		public static ASafeLog Log;

		public static bool IsAdmin() {
			return bIsAdmin;
		} // IsAdmin

		protected void Page_Load(object sender, EventArgs e) {
			if (Log == null) {
				Log = (ASafeLog)Application["log"];
				Log = new FileLog("EzReportsWeb", bUtcTimeInName: true, bAppend: true, sPath: @"C:\temp\EzReportsWeb\");
			}

			if (oDB == null) {
				oDB = new SqlConnection(Log);
			}

			if (!IsPostBack) {
				reportHandler = new WebReportHandler(oDB, Log);

				if (reportHandler.ReportList.Count == 0) {
					divFilter.Visible = false;
					return;
				} // if

				ddlReportTypes.DataTextField = "Title";
				ddlReportTypes.DataValueField = "Title";
				ddlReportTypes.DataSource = reportHandler.ReportList.Values.OrderBy(x => x.Title);
				ddlReportTypes.DataBind();
			} // if

			if (Session["IsAdmin"] == null) {
				Session["IsAdmin"] = false;
				try {
					Session["IsAdmin"] = oDB.ExecuteScalar<bool>(
						"SELECT IsAdmin FROM ReportUsers WHERE UserName = @uname",
						CommandSpecies.Text,
						new QueryParameter("@uname", HttpContext.Current.User.Identity.Name)
						);
				}
				catch (Exception ex) {
					Log.Error("Failed to retrieve is admin \n{0}", ex);
				}
			}

			bIsAdmin = (bool)Session["IsAdmin"];

			chkIsAdmin.Checked = bIsAdmin;

			if (bIsAdmin)
				InitAdminArea(oDB, IsPostBack);

			divAdminMsg.InnerText = string.Empty;

			DateTime fDate, tDate;
			bool isDaily;

			GetDates(out fDate, out tDate, out isDaily);

			fromDate.Attributes.Add("max", DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
			toDate.Attributes.Add("max", DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

			fromDate.Value = fDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
			toDate.Value = tDate.AddDays(-1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
		} // Page_Load

		protected void ddlReportTypes_OnSelectedIndexChanged(object sender, EventArgs e) {
			AdjustUIFiltersForReport(((DropDownList)sender).SelectedValue);
		} // ddlReportTypes_OnSelectedIndexChanged

		private void AdjustUIFiltersForReport(string selectedReport) {
			Report rpt = reportHandler.GetReport(selectedReport);

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
			Log.Debug("Show report clicked for report: '{0}'", ddlReportTypes.SelectedItem.Text);

			bool isError;
			ATag data = reportHandler.GetReportData(ddlReportTypes.SelectedItem.Text, rptDef, isDaily, oColumnTypes, out isError);

			if (isError)
				ResetBtn_Click(sender, e);

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
			wb.SaveAs(ostream);

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

			case "Todasterday":
				fDate = fDate.AddDays(-1);
				break;

			case "ThisWeek":
				fDate = MiscUtils.FirstDayOfWeek();
				tDate = fDate.AddDays(CultureInfo.CurrentCulture.DateTimeFormat.DayNames.Length);
				break;

			case "LastWeek":
				fDate = MiscUtils.FirstDayOfWeek().AddDays(-CultureInfo.CurrentCulture.DateTimeFormat.DayNames.Length);
				tDate = fDate.AddDays(CultureInfo.CurrentCulture.DateTimeFormat.DayNames.Length);
				break;

			case "LastMonth":
				fDate = new DateTime(fDate.Year, fDate.Month, 1).AddMonths(-1);
				tDate = fDate.AddMonths(1);
				break;

			case "Weekly":
				fDate = fDate.AddDays(-CultureInfo.CurrentCulture.DateTimeFormat.DayNames.Length);
				break;

			case "Monthly":
				fDate = fDate.AddMonths(-1);
				break;

			case "MonthToDate":
				fDate = new DateTime(fDate.Year, fDate.Month, 1);
				break;

			case "Lifetime":
				fDate = new DateTime(2012, 9, 1);
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

			var rpta = new ReportAuthenticationLib.ReportAuthentication(oDB, Log);
			try {
				rpta.AddUserToDb(sUserName, sUserName);
				divAdminMsg.InnerText = "User has been created.";
			}
			catch (Exception ex) {
				divAdminMsg.InnerText = string.Format("Action failed: {0}", ex.Message);
			} // try

			InitAdminArea(oDB);
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

			var rpta = new ReportAuthenticationLib.ReportAuthentication(oDB, Log);

			try {
				rpta.ResetPassword(sUserName, sPassword);
				divAdminMsg.InnerText = "Password has been reset.";
			}
			catch (Exception ex) {
				divAdminMsg.InnerText = string.Format("Action failed: {0}", ex.Message);
			} // try

			InitAdminArea(oDB);
		} // btnAdminResetPass_Click

		protected void btnAdminDropUser_Click(object sender, EventArgs e) {
			divAdminMsg.InnerText = "Performing task...";

			if (selAdminUserDrop.SelectedItem == null) {
				divAdminMsg.InnerText = "User not selected.";
				return;
			} // if

			var rpta = new ReportAuthenticationLib.ReportAuthentication(oDB, Log);

			try {
				int nUserID = Convert.ToInt32(selAdminUserDrop.SelectedItem.Value);
				rpta.DropUser(nUserID);
				divAdminMsg.InnerText = "User has been dropped.";
			}
			catch (Exception ex) {
				divAdminMsg.InnerText = string.Format("Action failed: {0}", ex.Message);
			} // try

			InitAdminArea(oDB);
		} // btnAdminDropUser_Click

		protected void btnPerformPendingActions_Click(object sender, EventArgs e) {
			divAdminMsg.InnerText = "Performing task...";

			var aryActions = txtPendingActionList.Value.Split('\n');

			try {
				foreach (string sActionCode in aryActions) {
					var ary = sActionCode.Split(',');

					if (ary.Length != 3)
						continue;

					int nUserID;
					int nReportID;
					int nEnabled;

					if (!int.TryParse(ary[0], out nUserID))
						continue;

					if (!int.TryParse(ary[1], out nReportID))
						continue;

					if (!int.TryParse(ary[2], out nEnabled))
						continue;

					oDB.ExecuteNonQuery(
						"RptSetUserReportMap",
						CommandSpecies.StoredProcedure,
						new QueryParameter("@UserID", nUserID),
						new QueryParameter("@ReportID", nReportID),
						new QueryParameter("@Enabled", nEnabled)
						);
				} // foreach

				divAdminMsg.InnerText = "User-report mapping has been updated.";
			}
			catch (Exception ex) {
				divAdminMsg.InnerText = string.Format("Action failed: {0}", ex.Message);
			} // try

			InitAdminArea(oDB);
		} // btnPerformPendingActions_Click

		private void InitAdminArea(AConnection oDB, bool bIsPostBack = false) {
			if (!bIsPostBack) {
				var oUsers = new SortedDictionary<string, int>();

				oDB.ForEachRowSafe((sr, bRowsetStart) => {
					int nUserID = sr["Id"];
					string sUserName = sr["Name"];

					oUsers[sUserName] = nUserID;

					return ActionResult.Continue;
				}, "SELECT Id, Name FROM ReportUsers ORDER BY Name", CommandSpecies.Text);

				SetReportUserMap(oDB, oUsers);

				FillUserDropDowns(oUsers);
			} // if
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
			var os = new List<object>();
			var jss = new JavaScriptSerializer();

			oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					int nUserID = sr["UserID"];
					int nReportID = sr["ReportID"];

					os.Add(new { reportId = nReportID, userId = nUserID });

					return ActionResult.Continue;
				},

				"SELECT UserID, ReportID FROM ReportsUsersMap ORDER BY ReportID",
				CommandSpecies.Text
			);

			txtReportUserMap.Value = jss.Serialize(os);

			var aryReports = new List<object>();

			oDB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					int nReportID = sr["Id"];
					string sReportName = sr["Title"];

					aryReports.Add(new { id = nReportID, name = sReportName });

					return ActionResult.Continue;
				},
				"SELECT Id, Title FROM ReportScheduler ORDER BY Title",
				CommandSpecies.Text
			);

			txtReportList.Value = new JavaScriptSerializer().Serialize(aryReports);

			var aryUsers = new List<object>();

			foreach (KeyValuePair<string, int> kv in oUsers)
				aryUsers.Add(new { id = kv.Value, name = kv.Key });

			txtUserList.Value = jss.Serialize(aryUsers);
		} // SetReportUserMap

		protected void ResetBtn_Click(object sender, EventArgs e) {
			reportHandler = new WebReportHandler(oDB, Log);

			if (reportHandler.ReportList.Count == 0) {
				divFilter.Visible = false;
				return;
			} // if

			ddlReportTypes.DataTextField = "Title";
			ddlReportTypes.DataValueField = "Title";
			ddlReportTypes.DataSource = reportHandler.ReportList.Values.OrderBy(x => x.Title);
			ddlReportTypes.DataBind();
		} // ResetBtn_Click

		private static WebReportHandler reportHandler;
		private static AConnection oDB;
		private static bool bIsAdmin;
	} // class Default
} // namespace EzReportsWeb
