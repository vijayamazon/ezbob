namespace ConfigManager {
	using System;
	using System.Net;
	using Ezbob.Context;
	using log4net;

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
			ErrorMailHashtag = string.Empty;

			string sErrMailRec = string.Empty;

			switch (oEnv.Name) {
			case Name.Dev:
				sErrMailRec = oEnv.UserName;

				if (oEnv.UserName.StartsWith("alexb"))
					ErrorMailHashtag = "#devalexboerror";

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
				ErrorMailHashtag = "#proderror";
				break;

			default:
				throw new ArgumentOutOfRangeException("Unsupported environment name: " + oEnv.Name, (Exception)null);
			} // switch

			ErrorMailRecipient = sErrMailRec + "@ezbob.com" + (
				string.IsNullOrWhiteSpace(ErrorMailHashtag) ? string.Empty : ",trigger@recipe.ifttt.com"
			);

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

		public string ErrorMailHashtag { get; private set; } // ErrorMailHashtag

		public virtual string MailSubject { get; private set; } // MailSubject

		public virtual Ezbob.Context.Environment Environment { get; private set; } // Environment

		public virtual Log4Net Init() {
			GlobalContext.Properties["MailSubject"] = MailSubject;
			GlobalContext.Properties["ErrorEmailRecipient"] = ErrorMailRecipient;
			GlobalContext.Properties["ErrorEmailHashtag"] = ErrorMailHashtag;

			log4net.Config.XmlConfigurator.Configure();

			return this;
		} // Init
	} // class Log4Net
} // namespace ConfigManager
