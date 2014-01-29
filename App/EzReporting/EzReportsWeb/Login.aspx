<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="EzReportsWeb.Login" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<meta http-equiv="Content-type" content="text/html;charset=UTF-8" />
	<title>Login</title>

	<link rel="icon" type="image/png" href="~/images/favicon32x32.png" />

	<link rel="stylesheet" href="~/css/login.css" />
	<link rel="stylesheet" href="~/css/combined.css" />

	<script src="js/jquery.js"></script>
	<script src="js/login.js"></script>
</head>

<body>
	<form id="form1" runat="server">
		<div id="pre_header">
			<div class="inner_nopadding">
				<div id="header_description">
					<span>Welcome to Ezbob Reporting, Please Login</span>
				</div>
				<%--<div id="header_description">Already have an account?  <a href="/Account/LogOn" class="button grey medium_btn">LOGIN</a></div>--%>
			</div>
		</div>
		<header>
			<div class="innerLogin">
				<a href="http://www.ezbob.com" class="logo_ezbob indent_text" id="ezbob_logo" title="Fast business loans for Ebay and Amazon merchants"></a>
				<div class="clear"></div>
			</div>
		</header>
		<div class="container">
			<div id="divMessage">
				<asp:Label ID="lblMessage" Visible="false" runat="server"></asp:Label>
			</div>
			<div id="divLogin" runat="server" class="well">

				<asp:Login ID="LoginControl" runat="server" DestinationPageUrl="~/Default.aspx"
					OnAuthenticate="LoginControl_Authenticate" DisplayRememberMe="false" TitleText="">
				</asp:Login>
				<asp:Button ID="btnShowChangePassword" Text="Change Password" runat="server" OnClick="btnShowChangePassword_Click" CssClass="button orange" />
			</div>
			<div id="divChangePassword" runat="server" class="well">
				<table class="tableChangePassword">
					<tbody>
						<tr>
							<td></td>
							<td><span style="text-align: center;">Change Password</span></td>
							<td></td>
						</tr>
						<tr>
							<td>
								<label for="txtUserName">User Name:</label>
							</td>
							<td>
								<asp:TextBox ID="txtUserName" runat="server"></asp:TextBox>
							</td>
							<td>
								<asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ErrorMessage="User Name Required" ControlToValidate="txtUserName"></asp:RequiredFieldValidator>
							</td>
						</tr>
						<tr>
							<td>
								<label for="txtOldPassword">Old Password:</label>
							</td>
							<td>
								<asp:TextBox ID="txtOldPassword" TextMode="Password" runat="server"></asp:TextBox>
							</td>
							<td>
								<asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="Old Password Required" ControlToValidate="txtOldPassword"></asp:RequiredFieldValidator>
							</td>
						</tr>
						<tr>
							<td>
								<label for="txtNewPassword1">New Password:</label>
							</td>
							<td>
								<asp:TextBox ID="txtNewPassword1" TextMode="Password" runat="server"></asp:TextBox>
							</td>
							<td>
								<asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ErrorMessage="New Password Required" ControlToValidate="txtNewPassword1"></asp:RequiredFieldValidator>
							</td>
						</tr>
						<tr>
							<td>
								<label for="txtNewPassword2">New Password Again:</label>
							</td>
							<td>
								<asp:TextBox ID="txtNewPassword2" TextMode="Password" runat="server"></asp:TextBox>
							</td>
							<td>
								<asp:CompareValidator ID="CompareValidator1" runat="server" ErrorMessage="New Password And New Password Again don't match" ControlToValidate="txtNewPassword1" ControlToCompare="txtNewPassword2"></asp:CompareValidator>
							</td>
						</tr>
						<tr>
							<td>
								<asp:Button ID="btnChangePassword" Text="Submit" runat="server" OnClick="btnChangePassword_Click" CssClass="button orange topMargin" />&nbsp;
							</td>
							<td>
								<asp:Button ID="btnBack" Text="Back" runat="server" OnClick="btnBack_Click" CausesValidation="false" CssClass="button orange topMargin" />
							</td>
							<td></td>
						</tr>
					</tbody>
				</table>
			</div>
		</div>
	</form>
</body>
</html>
