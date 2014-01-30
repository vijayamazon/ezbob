namespace EzReportsWeb
{
	using System;
	using Ezbob.Logger;

	public class Global : System.Web.HttpApplication
	{
		private static readonly ASafeLog Log = new FileLog("EzReportsWeb", bUtcTimeInName: true, bAppend: true, sPath: @"C:\temp\EzReportsWeb\");
		protected void Application_Start(object sender, EventArgs e)
		{
			Application["Log"] = Log;
		}

		protected void Session_Start(object sender, EventArgs e)
		{
			Log.Debug("Session Start");
		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{
		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{
		}

		protected void Application_Error(object sender, EventArgs e)
		{
			Exception ex = Server.GetLastError();

			Log.Debug("++++++++++++++++++++++++++++");
			Log.Error("Exception - \n" + ex);

			Log.Error("An error occurred", ex);
			Log.Error("Requested url: {0}", Request.RawUrl);
			Log.Debug("++++++++++++++++++++++++++++"); 
		}

		protected void Session_End(object sender, EventArgs e)
		{
			Log.Debug("Session_End");
		}

		protected void Application_End(object sender, EventArgs e)
		{
			Log.Debug("Application_End");
		}
	}
}