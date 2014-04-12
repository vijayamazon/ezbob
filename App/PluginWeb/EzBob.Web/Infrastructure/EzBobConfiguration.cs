namespace EzBob.Web.Infrastructure
{
	using Scorto.Configuration;

	public interface IEzBobConfiguration
	{
		bool CheckStoreUniqueness { get; }
		string PostcodeConnectionKey { get; }
		string CaptchaMode { get; }
		bool WizardTopNaviagtionEnabled { get; }
		bool LandingPageEnabled { get; }
		int GetCashSliderStep { get; }
		int MinLoan { get; }
		int XMinLoan { get; }
		int ManagerMaxLoan { get; }
		int MaxLoan { get; }
		bool PayPalEnabled { get; }
		bool AskvilleEnabled { get; }
		bool TargetsEnabled { get; }
		bool TargetsEnabledEntrepreneur { get; }
		bool ManagementPartEnabled { get; }
		int UpdateOnReapplyLastDays { get; }
		bool GetSatisfactionEnabled { get; }
		bool EbayPixelEnabled { get; }
		bool TaboolaPixelEnabled { get; }
		bool TradeTrackerPixelEnabled { get; }
		string PasswordPolicyType { get; }
		string DummyPostcodeSearchResult { get; }
		string DummyAddressSearchResult { get; }
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
		public string PasswordPolicyType { get { return GetValueWithDefault<string>("PasswordPolicyType", "simple"); } }
		
		public int UpdateOnReapplyLastDays
		{
			get { return GetValueWithDefault<int>("UpdateOnReapplyLastDays", "14"); }
		}
		
		public bool CheckStoreUniqueness { get { return GetValueWithDefault<bool>("CheckStoreUniqueness", "false"); } }

		public string PostcodeConnectionKey
		{
			get { return GetValueWithDefault<string>("PostcodeConnectionKey", "Please enter postcode data key to environment config"); }
		}

		public string CaptchaMode
		{
			get { return GetValueWithDefault<string>("CaptchaMode", "off"); }
		}

		public string DummyPostcodeSearchResult { get { return GetValueWithDefault<string>("DummyPostcodeSearchResult", ""); } }
		public string DummyAddressSearchResult { get { return GetValueWithDefault<string>("DummyAddressSearchResult", ""); } }
		
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

		public bool WizardTopNaviagtionEnabled
		{
			get { return GetValueWithDefault<bool>("WizardTopNaviagtionEnabled", "false"); }
		}

		public bool LandingPageEnabled
		{
			get { return GetValueWithDefault<bool>("LandingPageEnabled", "false"); }
		}

		public bool ManagementPartEnabled
		{
			get { return GetValueWithDefault<bool>("ManagementPartEnabled", "false"); }
		}

		public int GetCashSliderStep
		{
			get { return GetValueWithDefault<int>("GetCashSliderStep", "5"); }
		}

		public int MinLoan
		{
			get { return GetValueWithDefault<int>("MinLoan", "1000"); }
		}

		public int XMinLoan
		{
			get { return GetValueWithDefault<int>("XMinLoan", "100"); }
		}

		public int MaxLoan
		{
			get { return GetValueWithDefault<int>("MaxLoan", "10000"); }
		}

		public int ManagerMaxLoan
		{
			get { return GetValueWithDefault<int>("ManagerMaxLoan", "40000"); }
		}

		public bool PayPalEnabled
		{
			get { return GetValueWithDefault<bool>("PayPalEnabled", "True"); }
		}

		public bool AskvilleEnabled
		{
			get { return GetValueWithDefault<bool>("AskvilleEnabled", "false"); }

		}

		public bool TargetsEnabled
		{
			get { return GetValueWithDefault<bool>("TargetsEnabled", "false"); }
		}

		public bool TargetsEnabledEntrepreneur
		{
			get { return GetValueWithDefault<bool>("TargetsEnabledEntrepreneur", "false"); }
		}

		public bool GetSatisfactionEnabled
		{
			get { return GetValueWithDefault<bool>("GetSatisfactionEnabled", "True"); }
		}

		public bool EbayPixelEnabled
		{
			get { return GetValueWithDefault<bool>("EbayPixelEnabled", "False"); }
		}

		public bool TaboolaPixelEnabled
		{
			get { return GetValueWithDefault<bool>("TaboolaPixelEnabled", "False"); }
		}

		public bool TradeTrackerPixelEnabled
		{
			get { return GetValueWithDefault<bool>("TradeTrackerPixelEnabled", "False"); }
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