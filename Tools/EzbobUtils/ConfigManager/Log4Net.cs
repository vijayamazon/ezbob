namespace ConfigManager {
	using System;
	using System.Net;
	using Ezbob.Context;

	public class Log4Net {
		#region constructor

		public Log4Net(Ezbob.Context.Environment oEnv = null) {
			if (oEnv == null) {
				try {
					oEnv = new Ezbob.Context.Environment();
				}
				catch (Exception e) {
					throw new NullReferenceException("Failed to determine current environment.", e);
				} // try
			} // if

			Environment = oEnv;

			string sErrMailRec = string.Empty;

			switch (oEnv.Name) {
			case Name.Dev:
				sErrMailRec = oEnv.UserName;
				break;

			case Name.Qa:
				sErrMailRec = "qa";
				break;

			case Name.Integration:
			case Name.Uat:
				sErrMailRec = "uatmail";
				break;

			case Name.Production:
				sErrMailRec = "ProdLogs";
				break;

			default:
				throw new ArgumentOutOfRangeException("Unsupported environment name: " + oEnv.Name, (Exception)null);
			} // switch

			ErrorMailRecipient = sErrMailRec + "@ezbob.com";

			string ipStr = string.Empty;

			foreach (var address in Dns.GetHostEntry(Dns.GetHostName()).AddressList) {
				string addressStr = address.ToString();

				if (addressStr.StartsWith("192")) {
					ipStr = addressStr;
					break;
				} // if
			} // for each

			MailSubject = oEnv.MachineName + " (" + ipStr + "): EzBob log";
		} // constructor

		#endregion constructor

		public virtual string ErrorMailRecipient { get; private set; } // ErrorMailRecipient

		public virtual string MailSubject { get; private set; } // MailSubject

		public virtual Ezbob.Context.Environment Environment { get; private set; } // Environment

		public virtual Log4Net Init() {
			log4net.GlobalContext.Properties["MailSubject"] = MailSubject;
			log4net.GlobalContext.Properties["ErrorEmailRecipient"] = ErrorMailRecipient;

			log4net.Config.XmlConfigurator.Configure();

			return this;
		} // Init
	} // class Log4Net
} // namespace ConfigManager
