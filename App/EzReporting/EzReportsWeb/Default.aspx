<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="EzReportsWeb.Default" %>
<%@ Import Namespace="EzReportsWeb" %>
<%@ Import Namespace="Reports" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<meta http-equiv="Content-type" content="text/html;charset=UTF-8" />
	<title>Ezbob Report Viewer</title>

	<link rel="icon" type="image/png" href="~/images/favicon32x32.png" />

	<link rel="stylesheet" href="~/css/combined.css" />
	<%= Default.IsAdmin() ? "<link rel=stylesheet href=\"/css/admin.css\" />" : "" %>
	<%= Default.IsAdmin() ? "<link rel=stylesheet href=\"/css/jquery.dataTables.css\" />" : "" %>
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
					<asp:Label ID="Label3" runat="server" Text="Welcome " CssClass="float_left"></asp:Label>
					<asp:LoginName ID="LoginName" runat="server" CssClass="float_left" />
					<asp:LoginStatus ID="LoginStatus" runat="server" LogoutAction="Redirect" LogoutPageUrl="~/Default.aspx" CssClass="button grey medium_btn" />
                    <asp:Button runat="server" ID="ResetBtn" Text="Reset" OnClick="ResetBtn_Click" CssClass="button grey medium_btn"/>
				</div>
			</div>
		</div>
		<header id="hdReportSelector" runat="server">
			<div id="divFilter" runat="server">
				<div>
					<asp:Label ID="Label2" runat="server" Text="Label">Select a Report:</asp:Label>
					<asp:DropDownList ID="ddlReportTypes" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlReportTypes_OnSelectedIndexChanged"></asp:DropDownList>

					<asp:ImageButton ID="btnShowReport" runat="server" OnClick="btnShowReport_Click" ImageUrl="~/images/show_report.png" CssClass="ReportButton" />

					<asp:ImageButton ID="BtnGetExcel" runat="server" OnClick="BtnGetExcelClick" ImageUrl="~/images/xls_icon.png" CssClass="ReportButton" />

					<asp:CheckBox runat="server" ID="chkShowNonCash" Text="Show Non-Cash Transactions" Checked="True"/>

					<div id="tdEntries" class="inline"></div>
					<div id="tdShowing" class="inline"></div>
					<div id="tdNavigation" class="inline"></div>
					<div id="tdSearch" class="inline"></div>
				</div>

				<div id="divDateFilter" runat="server">
					<asp:RadioButtonList ID="rblFilter" runat="server" OnSelectedIndexChanged="rblFilter_SelectedIndexChanged"
						CssClass="rbppcol" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostBack="True">
						<asp:ListItem Value="Today" Text="Today" Selected="True"></asp:ListItem>
						<asp:ListItem Value="Yesterday" Text="Yesterday"></asp:ListItem>
						<asp:ListItem Value="Weekly" Text="Weekly"></asp:ListItem>
						<asp:ListItem Value="Monthly" Text="Monthly"></asp:ListItem>
						<asp:ListItem Value="MonthToDate" Text="Month to Date"></asp:ListItem>
						<asp:ListItem Value="Lifetime" Text="Lifetime"></asp:ListItem>
						<asp:ListItem Value="Custom" Text="Custom"></asp:ListItem>
					</asp:RadioButtonList>

					<div id="divCustomFilter" runat="server" visible="false">
						<label for="fromDate">From Date:</label>
						<input type="date" runat="server" id="fromDate" required="required" min="2012-01-01" />&nbsp;
						<label for="toDate">To Date:</label>
						<input type="date" runat="server" id="toDate" required="required" min="2012-01-01" />&nbsp;
					</div>
				</div>

				<div id="divUserKeyField" runat="server" Visible="False">
					<label for="UserKey">User name/id/email:</label>
					<input type="text" id="UserKey" runat="server"/>
				</div>
			</div>
			<div class="inner">
				<a href="http://www.ezbob.com" class="logo_ezbob indent_text" id="ezbob_logo" title="Fast business loans"></a>
			</div>
		</header>
		<div id="divReportData" runat="server"></div>
		<div id="divReportColumnTypes" runat="server"></div>
		<div id="divAdminArea" runat="server">
			<h2>Administration</h2>
			<h3 runat="server" ID="divAdminMsg"></h3>

			<div id="AdminLeftPane">
				<h3>Create User</h3>
				<asp:TextBox runat="server" ID="edtAdminUserName"></asp:TextBox>
				<asp:ImageButton ID="btnAdminCreateUser" runat="server" OnClick="btnAdminCreateUser_Click" ImageUrl="~/images/show_report.png" CssClass="center ReportButton" />

				<h3>Reset Password</h3>
				<asp:DropDownList runat="server" ID="selAdminUserResetPass" />
				<asp:TextBox runat="server" ID="edtAdminPassword"></asp:TextBox>
				<asp:ImageButton ID="btnAdminResetPass" runat="server" OnClick="btnAdminResetPass_Click" ImageUrl="~/images/show_report.png" CssClass="center ReportButton" />

				<h3>Drop User</h3>
				<asp:DropDownList runat="server" ID="selAdminUserDrop"/>
				<asp:ImageButton ID="btnAdminDropUser" runat="server" OnClick="btnAdminDropUser_Click" ImageUrl="~/images/show_report.png" CssClass="center ReportButton" />
			</div>

			<div id="divReportUserMapSection">
				<div id="divReportUserMap"></div>

				<div id="divPendingActions">
					<h3>Pending actions
					<asp:ImageButton ID="btnPerformPendingActions" runat="server" OnClick="btnPerformPendingActions_Click" ImageUrl="~/images/show_report.png" CssClass="center ReportButton" />
					</h3>

					<div id="divPendingActionList"></div>
					<textarea id="txtPendingActionList" runat="server"></textarea>
				</div>
			</div>	

			<div style="display: none;">
				<textarea id="txtReportList" runat="server"></textarea>
				<textarea id="txtUserList" runat="server"></textarea>
				<textarea id="txtReportUserMap" runat="server"></textarea>
			</div>
		</div>
		<asp:CheckBox runat="server" ID="chkIsAdmin" />
	</form>
</body>
</html>
