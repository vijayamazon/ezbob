<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="EzReportsWeb.Default" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="Reports" %>
<!DOCTYPE html>
<script runat="server">
	protected String GetYesterday() {
		return DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
	}
	protected String GetToday() {
		return DateTime.Today.ToString("yyyy-MM-dd");
	}
</script>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<meta http-equiv="Content-type" content="text/html;charset=UTF-8" />
	<link rel="icon" type="image/png" href="~/images/favicon32x32.png" />
	<title>Ezbob Report Viewer</title>
	<style type="text/css" title="currentStyle">
		@import "css/combined.css";
	</style>
	<script src="js/jquery.js"></script>
	<script src="js/jquery.dataTables.min.js"></script>
	<script>
		$(document).ready(function () {
			var today = new Date();
			var lastWeek = new Date(today.getTime() - 1000 * 60 * 60 * 24 * 7);
			$('#fromDate').val(lastWeek.toJSON().substring(0, 10));
			$('#toDate').val(today.toJSON().substring(0, 10));
			$('#divReportData table').addClass('table table-bordered table-striped blue-header centered');
			$('#tableReportData').dataTable({
				"aLengthMenu": [[10, 25, 50, 100, 200, -1], [10, 25, 50, 100, 200, "All"]],
				"iDisplayLength": 100
			});

			$("#tdEntries").append($("#tableReportData_length"));
			$("#tdSearch").append($("#tableReportData_filter"));
			$("#tdShowing").append($("#tableReportData_info"));
			$("#tdNavigation").append($("#tableReportData_paginate"));
			$("tr").removeAttr("style");
			//$("#btnShowReport").click(function () {

			//    $('#tableReportData').dataTable(
			//    {
			//        "aLengthMenu": [[10, 25, 50, 100, 200, -1], [10, 25, 50, 100, 200, "All"]],
			//        "iDisplayLength": 100
			//    });
			//    $('#divReportData table').addClass('table table-bordered table-striped blue-header centered');
			//    $("#tdEntries").append($("#tableReportData_length"));
			//    $("#tdSearch").append($("#tableReportData_filter"));
			//});
		});
	</script>
	<%= Report.GetStyle().ToString() %>
</head>

<body>
	<form id="form1" runat="server">
		<div id="pre_header">
			<div class="inner_nopadding">
				<div id="header_description">
					<asp:Label ID="Label3" runat="server" Text="Welcome "></asp:Label>
					<asp:LoginName ID="LoginName" runat="server" />
					<asp:LoginStatus ID="LoginStatus" runat="server" LogoutAction="Redirect" LogoutPageUrl="~/Default.aspx" CssClass="button grey medium_btn" />
				</div>
			</div>
		</div>
		<header>
			<div id="divFilter" runat="server">
				<table>
					<tr>
						<td style="vertical-align: middle;">
							<asp:Label ID="Label2" runat="server" Text="Label">Select a Report </asp:Label>&nbsp;
							<asp:DropDownList ID="ddlReportTypes" runat="server"></asp:DropDownList>&nbsp;
						</td>
						<td style="text-align: center;">
							<asp:ImageButton ID="btnShowReport" runat="server" OnClick="btnShowReport_Click" ImageUrl="~/images/show_report.png" CssClass="center ReportButton" />&nbsp;
						</td>
						<td id="tdEntries"></td>
						<td id="tdShowing"></td>
					</tr>
					<tr>
						<td colspan="2">
							<asp:RadioButtonList ID="rblFilter" runat="server" OnSelectedIndexChanged="rblFilter_SelectedIndexChanged"
								CssClass="rbppcol" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostBack="True">
								<asp:ListItem Value="Daily" Text="Daily" Selected="True"></asp:ListItem>
								<asp:ListItem Value="Weekly" Text="Weekly"></asp:ListItem>
								<asp:ListItem Value="Monthly" Text="Monthly"></asp:ListItem>
								<asp:ListItem Value="Custom" Text="Custom"></asp:ListItem>
							</asp:RadioButtonList>&nbsp;
							<div id="divCustomFilter" runat="server" style="display: inline-block;" visible="false">
								<label for="fromDate">From Date:</label>
								<input type="date" runat="server" id="fromDate" required="required" min="2012-01-01" max="<%= GetToday()%>" value="<%= GetYesterday()%>" />&nbsp;
								<label for="toDate">To Date:</label>
								<input type="date" runat="server" id="toDate" required="required" min="2012-01-01" max="<%= GetToday()%>" value="<%= GetToday()%>" />&nbsp;
							</div>
							&nbsp;
						</td>
						<td id="tdSearch"></td>
						<td id="tdNavigation"></td>
					</tr>
				</table>
			</div>
			<div class="inner">
				<a href="http://www.ezbob.com" class="logo_ezbob indent_text" id="ezbob_logo" title="Fast business loans for Ebay and Amazon merchants"></a>
			</div>
		</header>
		<div>
		</div>
		<div id="divReportData" runat="server"></div>
	</form>
</body>
</html>
