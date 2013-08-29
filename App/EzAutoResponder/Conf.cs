namespace EzAutoResponder
{
	using ConfigurationBase;
	using Ezbob.Logger;

	class Conf: Configurations {
		public Conf(ASafeLog oLog = null) : base(Const.GetConfigsSpName, oLog) {
		} // constructor

		public virtual string MailBeeLicenseKey { get; protected set; }
		public virtual string Server { get; protected set; }
		public virtual int Port { get; protected set; }

		public virtual string AutoResponderMails { get; protected set; }
		public virtual string AutoResponderPasswords { get; protected set; }
		
		public virtual int AutoRespondAfterHour { get; protected set; }
		public virtual int AutoRespondBeforeHour { get; protected set; } //Sending autoresponse in the range of 19:00-06:00
		public virtual int AutoRespondCountDays { get; protected set; } //Sending autoresponse once in three days

		public virtual int AutoResponderIdleTimeoutMinutes { get; protected set; }
		public virtual int AutoResponderIdleTimeoutSeconds { get; protected set; }

		public virtual string MandrillAutoResponseTemplate { get; protected set; }
		public virtual string MandrillApiKey { get; protected set; }
	} // class Conf
	
}
