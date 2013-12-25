using PostcodeAnywhere;
using Scorto.Configuration;

namespace EzBob.Web.Infrastructure
{
	public interface IEzBobConfiguration
	{
		string GreetingStrategyName { get; }
		string FinishWizardStrategyName { get; }
		string CashTransferedStrategyName { get; }
		string GetCashFailedStrategyName { get; }
		string TransferCashFailedStrategyName { get; }
		string PayEarlyStrategyName { get; }
		string PayPointNameValidationFailedStrategyName { get; }
		string RestorePasswordStrategyName { get; }
		string ChangePasswordStrategyName { get; }
		string ThreeInvalidAttemptsStrategyName { get; }
		string ScoringResultStrategyName { get; }
		string FraudCheckerStrategyName { get; }
		string CustomerMarketPlaceStrategyName { get; }
		string PayPalStrategyName { get; }
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
		string ApprovedUserStrategyName { get; }
		string RejectUserStrategyName { get; }
		string MoreAMLInformationStrategyName { get; }
		string MoreAMLandBWAStrategyName { get; }
		string MoreBWAInformationStrategyName { get; }
		string ConfirmationEmailStrategyName { get; }
		string UpdateMarketplacesStrategyName { get; }
		bool ManagementPartEnabled { get; }
		int UpdateOnReapplyLastDays { get; }
		IPostcodeAnywhereConfig PostcodeAnywhereConfig { get; }
		string EmailRolloverAddedStrategyName { get; }
		string RecheckEbayTokenStrategyName { get; }
		bool GetSatisfactionEnabled { get; }
		bool EbayPixelEnabled { get; }
		bool TaboolaPixelEnabled { get; }
		bool TradeTrackerPixelEnabled { get; }
		string CustomerEscalatedStrategyName { get; }
		string ReneweBayTokenStrategyName { get; }
		string PasswordPolicyType { get; }
		string CAISNoUploadStrategyName { get; }
		string DummyPostcodeSearchResult { get; }
		string DummyAddressSearchResult { get; }
		string EmailUnderReviewStrategyName { get; }
		int SessionTimeout { get; }
		// string ChannelGrabberConfigPath { get; }
		bool SkipServiceOnNewCreditLine { get; }
		int PacnetBalanceMaxManualChange { get; }
		int PacnetBalanceWeekendLimit { get; }
		int PacnetBalanceWeekdayLimit { get; }
		string NotEnoughFundsToAddress { get; }
		string NotEnoughFundsTemplateName { get; }
		bool RefreshYodleeEnabled { get; }
		bool WizardInstructionsEnabled { get; }
		bool TwilioEnabled { get; }
	}

	public class EzBobConfiguration : ConfigurationRootWeb, IEzBobConfiguration
	{
		public virtual string GreetingStrategyName { get { return GetValueWithDefault<string>("GreetingStrategyName", "Greeting"); } }
		public virtual string FinishWizardStrategyName { get { return GetValueWithDefault<string>("FinishWizardStrategyName", "FinishWizard"); } }
		public virtual string CashTransferedStrategyName { get { return GetValueWithDefault<string>("CashTransferedStrategyName", "CashTransfered"); } }
		public virtual string GetCashFailedStrategyName { get { return GetValueWithDefault<string>("GetCashFailed4TimesStrategyName", "Get_Cash_Failed"); } }
		public virtual string TransferCashFailedStrategyName { get { return GetValueWithDefault<string>("TransferCashFailedStrategyName", "Transfer_Cash_Failed"); } }
		public virtual string PayEarlyStrategyName { get { return GetValueWithDefault<string>("PayEarlyStrategyName", "PayEarly"); } }
		public virtual string PayPointNameValidationFailedStrategyName { get { return GetValueWithDefault<string>("PayPointNameValidationFailedStrategyName", "PayPointNameValidationFailed"); } }
		public virtual string RestorePasswordStrategyName { get { return GetValueWithDefault<string>("RestorePasswordStrategyName", "RestorePassword"); } }
		public virtual string ChangePasswordStrategyName { get { return GetValueWithDefault<string>("ChangePasswordStrategyName", "ChangePassword"); } }
		public virtual string ThreeInvalidAttemptsStrategyName { get { return GetValueWithDefault<string>("ThreeInvalidAttemptsStrategyName", "Three_Invalid_Attempts_Password"); } }
		public virtual string ApprovedUserStrategyName { get { return GetValueWithDefault<string>("ApprovedUserStrategyName", "Approved_User"); } }
		public virtual string RejectUserStrategyName { get { return GetValueWithDefault<string>("RejectUserStrategyName", "Rejected_User"); } }
		public virtual string MoreAMLInformationStrategyName { get { return GetValueWithDefault<string>("MoreAMLInformationStrategyName", "More_AML_Information"); } }
		public virtual string MoreAMLandBWAStrategyName { get { return GetValueWithDefault<string>("MoreAMLandBWAStrategyName", "More_AML_and_BWA_Information"); } }
		public virtual string MoreBWAInformationStrategyName { get { return GetValueWithDefault<string>("MoreBWAInformationStrategyName", "More_BWA_Information"); } }
		public virtual string ConfirmationEmailStrategyName { get { return GetValueWithDefault<string>("ConfirmationEmailStrategyName", "ConfirmationEmailStrategy"); } }
		public virtual string UpdateMarketplacesStrategyName { get { return GetValueWithDefault<string>("UpdateCustomerMarketplacesName", "Update Customer Data"); } }
		public virtual string CustomerEscalatedStrategyName { get { return GetValueWithDefault<string>("CustomerEscalatedStrategyName", "CustomerEscalated"); } }
		public virtual string ReneweBayTokenStrategyName { get { return GetValueWithDefault<string>("ReneweBayTokenStrategyName", "ReneweBayToken"); } }
		public string PasswordPolicyType { get { return GetValueWithDefault<string>("PasswordPolicyType", "simple"); } }
		public string CAISNoUploadStrategyName
		{
			get { return GetValueWithDefault<string>("CAISNoUploadStrategyName", "CAIS_NO_Upload"); }
		}

		public int UpdateOnReapplyLastDays
		{
			get { return GetValueWithDefault<int>("UpdateOnReapplyLastDays", "14"); }
		}

		public string ScoringResultStrategyName
		{
			get { return GetValueWithDefault<string>("ScoringResultStrategyName", "ScoringResult"); }
		}

		public string FraudCheckerStrategyName
		{
			get { return GetValueWithDefault<string>("FraudCheckerStrategyName", "FraudChecker"); }
		}

		public string CustomerMarketPlaceStrategyName
		{
			get { return GetValueWithDefault<string>("CustomerMarketPlaceStrategyName", "Update Customer Market Place Data"); }
		}

		public string PayPalStrategyName
		{
			get { return GetValueWithDefault<string>("PayPalStrategyName", "PayPal"); }
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

		public string DummyPostcodeSearchResult { get { return this.GetValueWithDefault<string>("DummyPostcodeSearchResult", ""); } }
		public string DummyAddressSearchResult { get { return this.GetValueWithDefault<string>("DummyAddressSearchResult", ""); } }
		public string EmailUnderReviewStrategyName { get { return GetValueWithDefault<string>("EmailUnderReviewStrategyName", "Email Under review"); } }

		// public string ChannelGrabberConfigPath { get { return this.GetValueWithDefault<string>("ChannelGrabberConfigPath", ""); } }

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

		public virtual IPostcodeAnywhereConfig PostcodeAnywhereConfig
		{
			get { return GetConfiguration<PostcodeAnywhereConfig>("PostcodeAnywhereConfig"); }
		}

		public string EmailRolloverAddedStrategyName
		{
			get { return GetValueWithDefault<string>("EmailRolloverAddedStrategyName", "Email Rollover Added"); }
		}

		public string RecheckEbayTokenStrategyName
		{
			get { return GetValueWithDefault<string>("RecheckEbayTokenStrategyName", "ReneweBayToken"); }
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

		public bool TwilioEnabled
		{
			get { return GetValueWithDefault<bool>("TwilioEnabled", "False"); }
		}
	}
}