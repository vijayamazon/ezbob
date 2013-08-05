<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="EzReportsWeb.Default" %>
<%@ Import Namespace="Reports" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<meta http-equiv="Content-type" content="text/html;charset=UTF-8" />
	<title>Ezbob Report Viewer</title>

	<link rel="icon" type="image/png" href="~/images/favicon32x32.png" />

	<link rel="stylesheet" href="css/combined.css" />

	<%= Report.GetStyle().ToString() %>

	<script src="js/jquery.js"></script>
	<script src="js/jquery.dataTables.min.js"></script>
	<script src="js/admin.js"></script>
	<script src="js/default.js"></script>
</head>

<body>
	<form id="form1" runat="server">
		<div id="pre_header">
			<div class="inner_nopadding">
				<div id="header_description">
					<a id="aToggleAdminArea" href="#" onclick="ToggleAdminArea()">Administration</a>
					<asp:Label ID="Label3" runat="server" Text="Welcome "></asp:Label>
					<asp:LoginName ID="LoginName" runat="server" />
					<asp:LoginStatus ID="LoginStatus" runat="server" LogoutAction="Redirect" LogoutPageUrl="~/Default.aspx" CssClass="button grey medium_btn" />
				</div>
			</div>
		</div>
		<header id="hdReportSelector" runat="server">
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
		<div id="divReportColumnTypes" runat="server"></div>
		<div id="divAdminArea" runat="server">
			<h3>Administration</h3>
			<table class="center">
				<tr>
					<td>User name:</td>
					<td><asp:TextBox runat="server" ID="edtAdminUserName"></asp:TextBox></td>
				</tr>
				<tr>
					<td>Password:</td>
					<td><asp:TextBox runat="server" ID="edtAdminPassword"></asp:TextBox></td>
				</tr>
				<tr>
					<td>Action:</td>
					<td>
						<asp:RadioButtonList runat="server" ID="rblAdminAction">
							<asp:ListItem Value="Reset" Text="Reset password" Selected="True"></asp:ListItem>
							<asp:ListItem Value="Create" Text="Create user"></asp:ListItem>
						</asp:RadioButtonList>
					</td>
				</tr>
				<tr>
					<td colspan="2" class="center">
						<asp:ImageButton ID="btnAdminDo" runat="server" OnClick="btnAdminDo_Click" ImageUrl="~/images/show_report.png" CssClass="center ReportButton" />
					</td>
				</tr>
			</table>

			<h3 runat="server" ID="divAdminMsg"></h3>
		</div>
		<asp:CheckBox runat="server" ID="chkIsAdmin" />
	</form>
</body>
</html>
