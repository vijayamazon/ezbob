namespace EzBob.Web.Areas.Broker.Models {
	using ConfigManager;
	using Newtonsoft.Json;

	public class BrokerHomeModel {
		public BrokerHomeModel() {
			Auth = string.Empty;

			MessageOnStart = string.Empty;
			MessageOnStartSeverity = string.Empty;

			MaxPerNumber = 3;
			MaxPerPage = 10;

			var oCfg = CurrentValues.Instance;

			ForceCaptcha = ((string)oCfg.BrokerForceCaptcha).ToLower();
			ForceCaptcha = (
				(ForceCaptcha == "1") || (ForceCaptcha == "yes") || (ForceCaptcha == "true")
			) ? "yes" : "no";

			PasswordPolicyType = oCfg.PasswordPolicyType;
			CaptchaMode = oCfg.CaptchaMode;
			SessionTimeout = oCfg.SessionTimeout;

			TermsID = 0;
			Terms = string.Empty;
		} // constructor

		public string ChannelGrabberAccounts {
			get { return JsonConvert.SerializeObject(Integration.ChannelGrabberConfig.Configuration.Instance.Vendors); }
		} // ChannelGrabberAccounts

		public string Auth { get; set; }

		public string MarketingFileUrlTemplate { get; set; }

		public static string MarketingFileLocation { get { return "/Areas/Broker/Files/"; } }

		public string MessageOnStart { get; set; }
		public string MessageOnStartSeverity { get; set; }

		public string ForceCaptcha { get; set; }
		public int MaxPerNumber { get; set; }
		public int MaxPerPage { get; set; }

		public string PasswordPolicyType { get; private set; }
		public string CaptchaMode { get; private set; }
		public int SessionTimeout { get; private set; }

		public string RootUrl { get; set; }
		public string LogoUrl { get; set; }
		public string EuIconUrl { get; set; }

		public int TermsID { get; set; }
		public string Terms { get; set; }

		public string HasTermsStr {
			get { return TermsID > 0 ? "yes" : "no"; }
		} // HasTermsStr
	} // class BrokerHomeModel
} // namespace
