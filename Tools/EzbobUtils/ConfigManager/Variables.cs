namespace ConfigManager {
	using System.ComponentModel;

	public enum ChannelGrabberRejectPolicy {
		Never,
		ConnectionFail,
	} // enum ChannelGrabberRejectPolicy

	public enum Variables {
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
		AutomaticTestBrokerMark,
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
		BankBasedApprovalNumOfMonthBackForVatCheck,
		BankBasedApprovalPersonalScoreThreshold,
		BankBasedApprovalPersonalScoreThresholdWhenNoCompanyScore,
		BankBasedApprovalSilentTemplateName,
		BankBasedApprovalSilentToAddress,
		BrokerCommissionEnabled,
		BrokerForceCaptcha,
		BrokerInstantOfferEnabled,
		BrokerMaxPerNumber,
		BrokerMaxPerPage,
		BrokerSetupFeeRate,
		BrokerSite,
		BWABusinessCheck,
		CAISPath,
		CAISPath2,
		ChannelGrabberCycleCount,
		ChannelGrabberRejectPolicy,
		ChannelGrabberServiceUrl,
		ChannelGrabberSleepTime,
		CollectionPeriod1,
		CollectionPeriod2,
		CollectionPeriod3,
		CompanyFilesSavePath,
		CompanyScoreNonLimitedParserConfiguration,
		CompanyScoreParserConfiguration,
		CustomerSite,
		CustomerStateRefreshInterval,
		CustomerAnalyticsDefaultHistoryYears,
		DEBUG_SaveVatReturnData,
		DefaultFeedbackValue,
		DirectorDetailsNonLimitedParserConfiguration,
		DirectorDetailsParserConfiguration,
		DirectorInfoNonLimitedParserConfiguration,
		DirectorInfoParserConfiguration,
		DisplayEarnedPoints,
		EchoSignApiKey,
		EchoSignDeadline,
		EchoSignEnabledCustomer,
		EchoSignEnabledUnderwriter,
		EchoSignReminder,
		EchoSignUrl,
		EnableAutomaticApproval,
		EnableAutomaticReApproval,
		EnableAutomaticRejection,
		EnableAutomaticReRejection,
		Environment,
		ExperianAuthTokenService,
		ExperianAuthTokenServiceIdHub,
		ExperianCertificateThumb,
		ExperianESeriesUrl,
		ExperianIdHubService,
		ExperianInteractiveMode,
		ExperianInteractiveService,
		ExperianSecureFtpHostName,
		ExperianSecureFtpUserName,
		ExperianSecureFtpUserPassword,
		ExperianUIdCertificateThumb,
		EzbobMailCc,
		EzbobMailTo,
		EzServiceUpdateConfiguration,
		[Description("Alias Of Joint Applicant")]
		FinancialAccounts_AliasOfJointApplicant,
		[Description("Alias Of Main Applicant")]
		FinancialAccounts_AliasOfMainApplicant,
		[Description("Association Of Joint Applicant")]
		FinancialAccounts_AssociationOfJointApplicant,
		[Description("Association Of Main Applicant")]
		FinancialAccounts_AssociationOfMainApplicant,
		[Description("Joint Applicant")]
		FinancialAccounts_JointApplicant,
		[Description("Main Applicant")]
		FinancialAccounts_MainApplicant,
		[Description("No Match")]
		FinancialAccounts_No_Match,
		[Description("Spare")]
		FinancialAccounts_Spare,
		FinishWizardForApproved,
		FirstOfMonthEnableCustomerMail,
		FirstOfMonthStatusMailCopyTo,
		FirstOfMonthStatusMailEnabled,
		FirstOfMonthStatusMailMandrillTemplateName,
		FreeAgentCompanyRequest,
		FreeAgentExpensesRequest,
		FreeAgentExpensesRequestDatePart,
		FreeAgentInvoicesRequest,
		FreeAgentInvoicesRequestMonthPart,
		FreeAgentOAuthAuthorizationEndpoint,
		FreeAgentOAuthIdentifier,
		FreeAgentOAuthSecret,
		FreeAgentOAuthTokenEndpoint,
		FreeAgentUsersRequest,
		GreetingMailSendViaMandrill,
		GoogleTagManagementProd,
		HmrcUploadedFilesSavePath,
		HmrcSalariesMultiplier,
		IntervalWaitForAmlCheck,
		IntervalWaitForExperianCompanyCheck,
		IntervalWaitForExperianConsumerCheck,
		IntervalWaitForMarketplacesUpdate,
		InvalidPasswordAttemptsPeriodSeconds,
		InvalidPasswordBlockSeconds,
		IsSmsValidationActive,
		IovationUrl,
		IovationSubscriberCode,
		IovationAccountCode,
		LandRegistryProd,
		LateBy14DaysMailSendViaMandrill,
		LatePaymentCharge,
		LoginValidationStringForWeb,
		LoginValidity,
		LogOffMode,
		LowCreditScore,
		MandrillEnable,
		MandrillKey,
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
		PacnetRAVEN_GATEWAY,
		PacnetRAVEN_PREFIX,
		PacnetRAVEN_RAPIVERSION,
		PacnetRAVEN_SECRET,
		PacnetRAVEN_USERNAME,
		PacnetSERVICE_TYPE,
		PartialPaymentCharge,
		PasswordValidity,
		PayPointCardLimitAmount,
		PayPointDebugMode,
		PayPointEnableCardLimit,
		PayPointEnableDebugErrorCodeN,
		PayPointIsValidCard,
		PayPointMid,
		PayPointOptions,
		PayPointRemotePassword,
		PayPointServiceUrl,
		PayPointTemplateUrl,
		PayPointValidateName,
		PayPointVpnPassword,
		PayPointCardExpiryMonths,
		RecentCustomersToKeep,
		Recon_Paypoint_Include_Five,
		Reject_Defaults_AccountsNum,
		Reject_Defaults_Amount,
		Reject_Defaults_CreditScore,
		Reject_Defaults_MonthsNum,
		Reject_Minimal_Seniority,
		RejectionPartnersCities,
		ReportsSite,
		RolloverCharge,
		SageExpendituresRequest,
		SageIncomesRequest,
		SageOAuthAuthorizationEndpoint,
		SageOAuthIdentifier,
		SageOAuthSecret,
		SageOAuthTokenEndpoint,
		SagePaymentStatusesRequest,
		SagePurchaseInvoicesRequest,
		SageRequestForDatesPart,
		SageSalesInvoicesRequest,
		SalesEmail,
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
		VerboseConfigurationLogging,
		VipEnabled,
		VipMaxRequests,
		VipMailReceiver,
		YodleeAccountPrefix,

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
		TrustPilotBccMail,
		AddSageIntervalMinutes,
		FCFFactor,
		EbayServiceType,
		EbayDevId,
		EbayAppId,
		EbayCertId,
		EbayRuName,
		ReportDaemonDropboxCredentials,
		ReportDaemonDropboxRootPath,
		MinAuthenticationIndexToPassAml,
		AvailableFundsRefreshInterval,
		RejectionExceptionMaxConsumerScoreForMpError,
		RejectionExceptionMaxCompanyScoreForMpError,
		RejectionExceptionMaxCompanyScore,
		RejectionCompanyScore,
		Reject_NumOfLateAccounts,
		Reject_LateLastMonthsNum,
		Reject_LowOfflineQuarterRevenue,
		Reject_LowOfflineAnnualRevenue,
		LandRegistryUserName,
		LandRegistryPassword,
		LandRegistryFilePath,
		RejectionLastValidLate,
		Reject_Defaults_CompanyScore,
		Reject_Defaults_CompanyAccountsNum,
		Reject_Defaults_CompanyMonthsNum,
		Reject_Defaults_CompanyAmount
	} // enum Variables
} // namespace ConfigManager
