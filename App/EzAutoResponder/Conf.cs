namespace EzAutoResponder
{
	using System;
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

		public virtual string AutoRespondWeekendDayBegin { get; protected set; }
		public virtual int AutoRespondWeekendHourBegin { get; protected set; }
		public virtual string AutoRespondWeekendDayEnd { get; protected set; }
		public virtual int AutoRespondWeekendHourEnd { get; protected set; }//Sending autoresponse in weekend Friday 19:00 - Sunday 07:00

		public virtual int AutoRespondCountDays { get; protected set; } //Sending autoresponse once in three days

		public virtual int AutoResponderIdleTimeoutMinutes { get; protected set; }
		public virtual int AutoResponderIdleTimeoutSeconds { get; protected set; }

		public virtual Boolean AutoRespondNightConstraintEnabled { get; protected set; }
		public virtual Boolean AutoRespondWeekendConstraintEnabled { get; protected set; }
		public virtual Boolean AutoRespondCountConstraintEnabled { get; protected set; }

		public virtual string AutoRespondMandrillWeekendTemplate { get; protected set; }
		public virtual string AutoRespondMandrillNightTemplate { get; protected set; }
		public virtual string AutoRespondMandrillApiKey { get; protected set; }
	} // class Conf
	
}
