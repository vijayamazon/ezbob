namespace EzBob.Web.Areas.Broker.Models {
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Newtonsoft.Json;

	public class BrokerHomeModel {
		public BrokerHomeModel(FileDescription[] oFileList) {
			FileList = oFileList ?? new FileDescription[0];
			Auth = string.Empty;

			MessageOnStart = string.Empty;
			MessageOnStartSeverity = string.Empty;

			MaxPerNumber = 3;
			MaxPerPage = 10;

			var oCfg = CurrentValues.Instance;

			PasswordPolicyType = oCfg.PasswordPolicyType;
			CaptchaMode = oCfg.CaptchaMode;
			SessionTimeout = oCfg.SessionTimeout;
		} // constructor

		public string ChannelGrabberAccounts {
			get { return JsonConvert.SerializeObject(Integration.ChannelGrabberConfig.Configuration.Instance.Vendors); }
		} // ChannelGrabberAccounts

		public FileDescription[] FileList { get; private set; }
		public string Auth { get; set; }

		public string MessageOnStart { get; set; }
		public string MessageOnStartSeverity { get; set; }

		public int MaxPerNumber { get; set; }
		public int MaxPerPage { get; set; }

		public string PasswordPolicyType { get; private set; }
		public string CaptchaMode { get; private set; }
		public int SessionTimeout { get; private set; }

		public string RootUrl { get; set; }
		public string LogoUrl { get; set; }
		public string EuIconUrl { get; set; }
	} // class BrokerHomeModel
} // namespace
