namespace YodleeLib
{
	using System;
	using ConfigManager;
	using log4net;

	public sealed class CobrandContextSingleton
	{
		static CobrandContextSingleton instance = null;
		static readonly object padlock = new object();
		static CobrandContext cobrandContext = null;
		double COBRAND_CONTEXT_TIME_OUT = 3;
		DateTime created = DateTime.Now;
		CobrandLoginService cobrandLoginService;
		private static readonly ILog Log = LogManager.GetLogger(typeof(CobrandContextSingleton));

		CobrandContextSingleton()
		{
			created = created.AddMinutes(-COBRAND_CONTEXT_TIME_OUT);
			string soapServer = CurrentValues.Instance.YodleeSoapServer;
			Environment.SetEnvironmentVariable("com.yodlee.soap.services.url", soapServer);
			cobrandLoginService = new CobrandLoginService();
			cobrandLoginService.Url = soapServer + "/" + cobrandLoginService.GetType().FullName;
		}

		public static CobrandContextSingleton Instance
		{
			get
			{
				lock (padlock)
				{
					return instance ?? (instance = new CobrandContextSingleton());
				}
			}
		}

		public CobrandContext GetCobrandContext()
		{
			DateTime now = DateTime.Now;
			DateTime expired = created.AddMinutes(COBRAND_CONTEXT_TIME_OUT);

			if (now >= expired)
			{
				// Cobrand Context expired, create new one
				cobrandLoginService = new CobrandLoginService();
				string soapServer = CurrentValues.Instance.YodleeSoapServer;
				cobrandLoginService.Url = soapServer + "/" + cobrandLoginService.GetType().FullName;
				// Get Cobrand Credentials from AppSettings (requires App.config file)
				long cobrandId = CurrentValues.Instance.YodleeCobrandId;
				string applicationId = CurrentValues.Instance.YodleeApplicationId;
				string username = CurrentValues.Instance.YodleeUsername;
				string password = CurrentValues.Instance.YodleePassword;
				string tncVersionStr = CurrentValues.Instance.YodleeTncVersion;
				long tncVersion = long.Parse(tncVersionStr);
				// Note you can remove warnings by adding reference 'System.Configuration' from the .NET tab
				// and replacing code "ConfigurationSettings.AppSettings.Get" with "ConfigurationManager.AppSettings"
				// This only works with .NET 2.0 or above.  Leaving code as is for now.s

				var locale = new Locale { country = "US" };
				var cobrandPasswordCredentials = new CobrandPasswordCredentials { password = password, loginName = username };

				// authentication of a cobrand in the Yodlee software platform and returns 
				// a valid CobrandContext if the authentication is successful. This method takes a generic CobrandCredentials argument as the
				// authentication related credentials of the cobrand.
				cobrandContext = cobrandLoginService.loginCobrand(
					cobrandId,
					true,
					applicationId,
					locale,
					tncVersion,
					true,
					cobrandPasswordCredentials);

				Log.DebugFormat("GetCobrandContext cobrand login successful");

				created = DateTime.Now;
				return cobrandContext;
			}

			return cobrandContext;
		}
	}
}
