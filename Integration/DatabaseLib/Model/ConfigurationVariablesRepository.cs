namespace EZBob.DatabaseLib.Model {
	using System;
	using System.Globalization;
	using ApplicationMng.Repository;
	using NHibernate;

	#region enum ConfigurationVariables

	// Please add new values in alphabetic order.

	public enum ConfigurationVariables {
		AdministrationCharge,
		AllowFinishOfflineWizardWithoutMarketplaces,
		AllowFinishOnlineWizardWithoutMarketplaces,
		AmountToChargeFrom,
		AutoApproveCustomerMaxAge,
		AutoApproveCustomerMinAge,
		AutoApproveExperianScoreThreshold,
		AutoApproveIsSilent,
		AutoApproveMaxAllowedDaysLate,
		AutoApproveMaxAmount,
		AutoApproveMaxDailyApprovals,
		AutoApproveMaxNumOfOutstandingLoans,
		AutoApproveMaxOutstandingOffers,
		AutoApproveMaxTodayLoans,
		AutoApproveMinAmount,
		AutoApproveMinMPSeniorityDays,
		AutoApproveMinRepaidPortion,
		AutoApproveMinTurnover1M,
		AutoApproveMinTurnover1Y,
		AutoApproveMinTurnover3M,
		AutoApproveSilentTemplateName,
		AutoApproveSilentToAddress,
		AutomaticTestCustomerMark,
		AutoReApproveMaxNumOfOutstandingLoans,
		AutoRejectionException_AnualTurnover,
		AutoRejectionException_CreditScore,
		BankBasedApprovalBelowAverageRiskMaxBusinessScore,
		BankBasedApprovalBelowAverageRiskMaxPersonalScore,
		BankBasedApprovalBelowAverageRiskMinBusinessScore,
		BankBasedApprovalBelowAverageRiskMinPersonalScore,
		BankBasedApprovalEuCap,
		BankBasedApprovalHomeOwnerCap,
		BankBasedApprovalIsEnabled,
		BankBasedApprovalIsSilent,
		BankBasedApprovalMinAge,
		BankBasedApprovalMinAmlScore,
		BankBasedApprovalMinAnnualizedTurnover,
		BankBasedApprovalMinBusinessScore,
		BankBasedApprovalMinCompanySeniorityDays,
		BankBasedApprovalMinNumberOfDays,
		BankBasedApprovalMinNumberOfPayers,
		BankBasedApprovalMinOffer,
		BankBasedApprovalNotHomeOwnerCap,
		BankBasedApprovalNumOfMonthsToLookForDefaults,
		BankBasedApprovalPersonalScoreThreshold,
		BankBasedApprovalPersonalScoreThresholdWhenNoCompanyScore,
		BankBasedApprovalSilentTemplateName,
		BankBasedApprovalSilentToAddress,
		BrokerCommissionEnabled,
		BrokerMaxPerNumber,
		BrokerMaxPerPage,
		BrokerSetupFeeRate,
		BrokerSite,
		BWABusinessCheck,
		CAISPath,
		CAISPath2,
		CollectionPeriod1,
		CollectionPeriod2,
		CollectionPeriod3,
		CompanyFilesSavePath,
		CompanyScoreNonLimitedParserConfiguration,
		CompanyScoreParserConfiguration,
		CustomerSite,
		CustomerStateRefreshInterval,
		DefaultFeedbackValue,
		DirectorInfoNonLimitedParserConfiguration,
		DirectorInfoParserConfiguration,
		DisplayEarnedPoints,
		EnableAutomaticApproval,
		EnableAutomaticReApproval,
		EnableAutomaticRejection,
		EnableAutomaticReRejection,
		EzbobMailCc,
		EzbobMailTo,
		FinancialAccounts_AliasOfJointApplicant,
		FinancialAccounts_AliasOfMainApplicant,
		FinancialAccounts_AssociationOfJointApplicant,
		FinancialAccounts_AssociationOfMainApplicant,
		FinancialAccounts_JointApplicant,
		FinancialAccounts_MainApplicant,
		FinancialAccounts_No_Match,
		FirstOfMonthEnableCustomerMail,
		FirstOfMonthStatusMailCopyTo,
		FirstOfMonthStatusMailEnabled,
		FirstOfMonthStatusMailMandrillTemplateName,
		GreetingMailSendViaMandrill,
		HmrcUploadedFilesSavePath,
		IntervalWaitForAmlCheck,
		IntervalWaitForExperianCompanyCheck,
		IntervalWaitForExperianConsumerCheck,
		IntervalWaitForMarketplacesUpdate,
		InvalidPasswordAttemptsPeriodSeconds,
		InvalidPasswordBlockSeconds,
		IsSmsValidationActive,
		LandRegistryProd,
		LateBy14DaysMailSendViaMandrill,
		LatePaymentCharge,
		LoginValidationStringForWeb,
		LoginValidity,
		LowCreditScore,
		MandrillEnable,
		MaxCapHomeOwner,
		MaxCapNotHomeOwner,
		MaxPerDay,
		MaxPerNumber,
		MaxYodleeOtherCategoryAmount,
		MinDectForDefault,
		MinInterestRateToReuse,
		MinLoanAmount,
		NumberOfMobileCodeAttempts,
		NumOfInvalidPasswordAttempts,
		OfferValidForHours,
		OtherCharge,
		PartialPaymentCharge,
		PasswordValidity,
		RecentCustomersToKeep,
		Recon_Paypoint_Include_Five,
		Reject_Defaults_AccountsNum,
		Reject_Defaults_Amount,
		Reject_Defaults_CreditScore,
		Reject_Defaults_MonthsNum,
		Reject_Minimal_Seniority,
		ReportsSite,
		RolloverCharge,
		SetupFeeEnabled,
		SetupFeeFixed,
		SetupFeeMaxFixedPercent,
		SetupFeePercent,
		SkipCodeGenerationNumber,
		SkipCodeGenerationNumberCode,
		TotalAnnualTurnover,
		TotalThreeMonthTurnover,
		TotalTimeToWaitForAmlCheck,
		TotalTimeToWaitForExperianCompanyCheck,
		TotalTimeToWaitForExperianConsumerCheck,
		TotalTimeToWaitForMarketplacesUpdate,
		TrustPilotReviewEnabled,
		TwilioAccountSid,
		TwilioAuthToken,
		TwilioSendingNumber,
		UnderwriterSite,
		UpdateCompanyDataPeriodDays,
		UpdateConsumerDataPeriodDays,
		YodleeAccountPrefix,
		TrustPilotBccMail,

		AmazonEnableRetrying,
		AmazonMinorTimeoutInSeconds,
		AmazonUseLastTimeOut,
		AmazonIterationSettings1Index,
		AmazonIterationSettings1CountRequestsExpectError,
		AmazonIterationSettings1TimeOutAfterRetryingExpiredInMinutes,
		AmazonIterationSettings2Index,
		AmazonIterationSettings2CountRequestsExpectError,
		AmazonIterationSettings2TimeOutAfterRetryingExpiredInMinutes,
		AmazonAskvilleLogin,
		AmazonAskvillePassword,
		PostcodeAnywhereMaxBankAccountValidationAttempts,
		PostcodeAnywhereKey,
		PostcodeAnywhereEnabled,
		AgreementPdfLoanPath1,
		AgreementPdfLoanPath2,
		AgreementPdfConsentPath1,
		AgreementPdfConsentPath2,
		NHibernateEnableProfiler,
		CheckStoreUniqueness,
		PostcodeConnectionKey,
		CaptchaMode,
		PasswordPolicyType,
		WizardTopNaviagtionEnabled,
		LandingPageEnabled,
		GetCashSliderStep,
		MinLoan,
		XMinLoan,
		ManagerMaxLoan,
		MaxLoan,
		AskvilleEnabled,
		TargetsEnabled,
		UpdateOnReapplyLastDays,
		DummyPostcodeSearchResult,
		DummyAddressSearchResult,
		EbayPixelEnabled,
		TradeTrackerPixelEnabled,
		TaboolaPixelEnabled,
		PayPalEnabled,
		TargetsEnabledEntrepreneur,
		ManagementPartEnabled,
		GetSatisfactionEnabled,
		NotEnoughFundsToAddress,
		NotEnoughFundsTemplateName,
		WizardInstructionsEnabled,
		RefreshYodleeEnabled,
		PacnetBalanceMaxManualChange,
		PacnetBalanceWeekendLimit,
		PacnetBalanceWeekdayLimit,
		SkipServiceOnNewCreditLine,
		SessionTimeout,
		PayPalTransactionSearchMonthsBack,
		PayPalOpenTimeoutInMinutes,
		PayPalSendTimeoutInMinutes,
		PayPalEnableRetrying,
		PayPalMinorTimeoutInSeconds,
		PayPalUseLastTimeOut,
		PayPalIterationSettings1Index,
		PayPalIterationSettings1CountRequestsExpectError,
		PayPalIterationSettings1TimeOutAfterRetryingExpiredInMinutes,
		PayPalIterationSettings2Index,
		PayPalIterationSettings2CountRequestsExpectError,
		PayPalIterationSettings2TimeOutAfterRetryingExpiredInMinutes,
		PayPalIterationSettings3Index,
		PayPalIterationSettings3CountRequestsExpectError,
		PayPalIterationSettings3TimeOutAfterRetryingExpiredInMinutes,
		PayPalApiAuthenticationMode,
		PayPalPpApplicationId,
		PayPalApiUsername,
		PayPalApiPassword,
		PayPalApiSignature,
		PayPalApiRequestFormat,
		PayPalApiResponseFormat,
		PayPalTrustAll,
		PayPalServiceType,
		PayPalNumberOfRetries,
		PayPalMaxAllowedFailures,
		YodleeSoapServer,
		YodleeCobrandId,
		YodleeApplicationId,
		YodleeUsername,
		YodleePassword,
		YodleeTncVersion,
		YodleeBridgetApplicationId,
		YodleeAccountPoolSize,
		YodleeApplicationToken,
		YodleeApplicationKey,
		YodleeAddAccountURL,
		YodleeEditAccountURL,
		TeraPeakApiKey,
		TeraPeakUrl,
		AddSageIntervalMinutes,
		FCFFactor,
		PricingModelTenurePercents,
		PricingModelDefaultRateCompanyShare,
		PricingModelInterestOnlyPeriod,
		PricingModelCollectionRate,
		PricingModelEuCollectionRate,
		PricingModelCogs,
		PricingModelDebtOutOfTotalCapital,
		PricingModelCostOfDebtPA,
		PricingModelOpexAndCapex,
		PricingModelProfitMarkupPercentsOfRevenue,
		PricingModelSetupFee
	} // enum ConfigurationVariables

	#endregion enum ConfigurationVariables

	#region interface IConfigurationVariablesRepository

	public interface IConfigurationVariablesRepository : IRepository<ConfigurationVariable> {
		ConfigurationVariable GetByName(string name);
		decimal GetByNameAsDecimal(string name);
		int GetByNameAsInt(string name);
		bool GetByNameAsBool(string name);
		void SetByName(string name, string value);
		ConfigurationVariable this[ConfigurationVariables nVariableName] { get; }
	} // IConfigurationVariablesRepository

	#endregion interface IConfigurationVariablesRepository

	#region class ConfigurationVariablesRepository

	public class ConfigurationVariablesRepository : NHibernateRepositoryBase<ConfigurationVariable>, IConfigurationVariablesRepository {
		public ConfigurationVariablesRepository(ISession session) : base(session) {}

		public ConfigurationVariable GetByName(string name) {
			return _session.QueryOver<ConfigurationVariable>().Where(c => c.Name == name).SingleOrDefault<ConfigurationVariable>();
		} // GetByName

		public decimal GetByNameAsDecimal(string name) {
			return Convert.ToDecimal(GetByName(name).Value, CultureInfo.InvariantCulture);
		} // GetByNameAsDecimal

		public int GetByNameAsInt(string name) {
			return Convert.ToInt32(GetByName(name).Value, CultureInfo.InvariantCulture);
		} // GetByNameAsInt

		public bool GetByNameAsBool(string name) {
			string value = GetByName(name).Value;
			return value.ToLower() == "true" || value == "1";
		} // GetByNameAsBool

		public void SetByName(string name, string value) {
			var property = _session.QueryOver<ConfigurationVariable>().Where(c => c.Name == name).SingleOrDefault<ConfigurationVariable>();
			property.Value = value;
			_session.Update(property);
			_session.Flush();
		} // SetByName

		public ConfigurationVariable this[ConfigurationVariables nVariableName] {
			get { return GetByName(nVariableName.ToString()); } // get
		} // indexer
	} // ConfigurationVariablesRepository

	#endregion class ConfigurationVariablesRepository
} // namespace
