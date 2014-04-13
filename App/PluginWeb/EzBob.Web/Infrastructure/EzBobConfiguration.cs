namespace EzBob.Web.Infrastructure
{
	using Scorto.Configuration;

	public interface IEzBobConfiguration
	{
		bool TargetsEnabledEntrepreneur { get; }
		bool ManagementPartEnabled { get; }
		bool GetSatisfactionEnabled { get; }
		int SessionTimeout { get; }
		bool SkipServiceOnNewCreditLine { get; }
		int PacnetBalanceMaxManualChange { get; }
		int PacnetBalanceWeekendLimit { get; }
		int PacnetBalanceWeekdayLimit { get; }
		string NotEnoughFundsToAddress { get; }
		string NotEnoughFundsTemplateName { get; }
		bool RefreshYodleeEnabled { get; }
		bool WizardInstructionsEnabled { get; }
	}

	public class EzBobConfiguration : ConfigurationRootWeb, IEzBobConfiguration
	{
		public bool SkipServiceOnNewCreditLine
		{
			get
			{
				try
				{
					return GetValueWithDefault<bool>("SkipServiceOnNewCreditLine", "false");
				}
				catch (System.Exception)
				{
					return false;
				}
			} // get
		} // SkipServiceOnNewCreditLine
		
		public bool ManagementPartEnabled
		{
			get { return GetValueWithDefault<bool>("ManagementPartEnabled", "false"); }
		}

		public bool TargetsEnabledEntrepreneur
		{
			get { return GetValueWithDefault<bool>("TargetsEnabledEntrepreneur", "false"); }
		}

		public bool GetSatisfactionEnabled
		{
			get { return GetValueWithDefault<bool>("GetSatisfactionEnabled", "True"); }
		}
		
		public int SessionTimeout
		{
			get { return GetValueWithDefault<int>("SessionTimeoutWeb", "6000"); }
		}

		public int PacnetBalanceMaxManualChange
		{
			get { return GetValueWithDefault<int>("PacnetBalanceMaxManualChange", "300000"); }
		}

		public int PacnetBalanceWeekendLimit
		{
			get { return GetValueWithDefault<int>("PacnetBalanceWeekendLimit", "100000"); }
		}

		public int PacnetBalanceWeekdayLimit
		{
			get { return GetValueWithDefault<int>("PacnetBalanceWeekdayLimit", "50000"); }
		}

		public string NotEnoughFundsToAddress
		{
			get { return GetValueWithDefault<string>("NotEnoughFundsToAddress", "dev@ezbob.com"); }
		}

		public string NotEnoughFundsTemplateName
		{
			get { return GetValueWithDefault<string>("NotEnoughFundsTemplateName", "NotEnoughFunds"); }
		}

		public bool RefreshYodleeEnabled
		{
			get { return GetValueWithDefault<bool>("RefreshYodleeEnabled", "False"); }
		}

		public bool WizardInstructionsEnabled
		{
			get { return GetValueWithDefault<bool>("WizardInstructionsEnabled", "False"); }
		}
	}
}