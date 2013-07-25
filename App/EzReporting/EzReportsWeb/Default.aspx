<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="EzReportsWeb.Default" %>

<%@ Import Namespace="System" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="Reports" %>
<!DOCTYPE html>
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
			$('#divReportData table').addClass('table table-bordered table-striped blue-header centered');
			$('#tableReportData').dataTable({
				aLengthMenu: [[10, 25, 50, 100, 200, -1], [10, 25, 50, 100, 200, "All"]],
				iDisplayLength: 100,
				aaSorting: []
			});

			$("#tdEntries").append($("#tableReportData_length"));
			$("#tdSearch").append($("#tableReportData_filter"));
			$("#tdShowing").append($("#tableReportData_info"));
			$("#tdNavigation").append($("#tableReportData_paginate"));
			$("tr").removeAttr("style");
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
						<td rowspan="2">
							<div>
								<asp:Label ID="Label2" runat="server" Text="Label">Select a Report:</asp:Label>
								<asp:DropDownList ID="ddlReportTypes" runat="server"></asp:DropDownList>
							</div>

							<asp:RadioButtonList ID="rblFilter" runat="server" OnSelectedIndexChanged="rblFilter_SelectedIndexChanged"
								CssClass="rbppcol" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostBack="True">
								<asp:ListItem Value="Today" Text="Today" Selected="True"></asp:ListItem>
								<asp:ListItem Value="Yesterday" Text="Yesterday"></asp:ListItem>
								<asp:ListItem Value="Weekly" Text="Weekly"></asp:ListItem>
								<asp:ListItem Value="Monthly" Text="Monthly"></asp:ListItem>
								<asp:ListItem Value="MonthToDate" Text="Month to Date"></asp:ListItem>
								<asp:ListItem Value="Custom" Text="Custom"></asp:ListItem>
							</asp:RadioButtonList>

							<div id="divCustomFilter" runat="server" visible="false">
								<label for="fromDate">From Date:</label>
								<input type="date" runat="server" id="fromDate" required="required" min="2012-01-01" />&nbsp;
								<label for="toDate">To Date:</label>
								<input type="date" runat="server" id="toDate" required="required" min="2012-01-01" />&nbsp;
							</div>
						</td>

						<td rowspan="2">
							<div>
							<asp:ImageButton ID="btnShowReport" runat="server" OnClick="btnShowReport_Click" ImageUrl="~/images/show_report.png" CssClass="center ReportButton" />
							</div>
							<div>
							<asp:ImageButton ID="BtnGetExcel" runat="server" OnClick="BtnGetExcelClick" ImageUrl="~/images/xls_icon.png" CssClass="center ReportButton" />
							</div>
						</td>

						<td id="tdEntries"></td>
						<td id="tdShowing"></td>
					</tr>
					<tr>
						<td id="tdSearch"></td>
						<td id="tdNavigation"></td>
					</tr>
				</table>
			</div>
			<div class="inner">
				<a href="http://www.ezbob.com" class="logo_ezbob indent_text" id="ezbob_logo" title="Fast business loans for Ebay and Amazon merchants"></a>
			</div>
		</header>
		<div id="divReportData" runat="server"></div>
	</form>
</body>
</html>
