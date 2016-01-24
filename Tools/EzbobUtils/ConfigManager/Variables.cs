namespace ConfigManager {
	using System.ComponentModel;

	public enum ChannelGrabberRejectPolicy {
		Never,
		ConnectionFail,
	} // enum ChannelGrabberRejectPolicy

	public enum Variables {
		AccountingBrokersEmail,
		AddSageIntervalMinutes,
		AdministrationCharge,
		AgreementPdfConsentPath1,
		AgreementPdfConsentPath2,
		AgreementPdfLoanPath1,
		AgreementPdfLoanPath2,
		AlibabaCurrencyConversionCoefficient,
		AlibabaMailCc,
		AlibabaMailTo,
		AlibabaClientEnvironment,
		AlibabaBaseUrl,
		AlibabaAppSecret,
		AlibabaUrlPath,
		AlibabaBaseUrl_Sandbox,
		AlibabaAppSecret_Sandbox,
		AlibabaUrlPath_Sandbox,
		AllowFinishOfflineWizardWithoutMarketplaces,
		AllowFinishOnlineWizardWithoutMarketplaces,
		AmazonAskvilleLogin,
		AmazonAskvillePassword,
		AmazonEnableRetrying,
		AmazonIterationSettings1CountRequestsExpectError,
		AmazonIterationSettings1Index,
		AmazonIterationSettings1TimeOutAfterRetryingExpiredInMinutes,
		AmazonIterationSettings2CountRequestsExpectError,
		AmazonIterationSettings2Index,
		AmazonIterationSettings2TimeOutAfterRetryingExpiredInMinutes,
		AmazonMinorTimeoutInSeconds,
		AmazonUseLastTimeOut,
		AmountToChargeFrom,
		AskvilleEnabled,
		AspireToMinSetupFee,
		AutoApproveAllowedCaisStatusesWithLoan,
		AutoApproveAllowedCaisStatusesWithoutLoan,
		AutoApproveAvailableFundsOverdraft,
		AutoApproveBusinessScoreThreshold,
		AutoApproveCustomerMaxAge,
		AutoApproveCustomerMinAge,
		AutoApproveExperianScoreThreshold,
		AutoApproveHmrcTurnoverAge,
		AutoApproveHmrcTurnoverDropHalfYearRatio,
		AutoApproveHmrcTurnoverDropQuarterRatio,
		AutoApproveIsSilent,
		AutoApproveMaxAllowedDaysLate,
		AutoApproveMaxAmount,
		AutoApproveMaxDailyApprovals,
		AutoApproveMaxHourlyApprovals,
		AutoApproveMaxLastHourApprovals,
		AutoApproveMaxNumOfOutstandingLoans,
		AutoApproveMaxOutstandingOffers,
		AutoApproveMaxTodayLoans,
		AutoApproveMinMPSeniorityDays,
		AutoApproveMinRepaidPortion,
		AutoApproveMinTurnover1M,
		AutoApproveMinTurnover1Y,
		AutoApproveMinTurnover3M,
		AutoApproveOffHoursMaxDailyApprovals,
		AutoApproveOffHoursMaxHourlyApprovals,
		AutoApproveOffHoursMaxLastHourApprovals,
		AutoApproveOffHoursMaxTodayLoans,
		AutoApproveOffHoursMaxOutstandingOffers,
		AutoApproveOfficeTimeStart,
		AutoApproveOfficeTimeEnd,
		AutoApproveOnlineTurnoverAge,
		AutoApproveOnlineTurnoverDropMonthRatio,
		AutoApproveOnlineTurnoverDropQuarterRatio,
		AutoApproveSilentTemplateName,
		AutoApproveSilentToAddress,
		AutoApproveTurnoverDropQuarterRatio,
		AutoApproveWeekend,
		AutomationExplanationMailReciever,
		AutoReApproveMaxLacrAge,
		AutoReApproveMaxLatePayment,
		AutoReApproveMaxNumOfOutstandingLoans,
		AutoReRejectMaxAllowedLoans,
		AutoReRejectMaxLRDAge,
		AutoReRejectMinRepaidPortion,
		AutoRejectConsumerCheckAge,
		AutoRejectionException_AnualTurnover,
		AutoRejectionException_CreditScore,
		AutomaticTestBrokerMark,
		AutomaticTestCustomerMark,
		AvailableFundsRefreshInterval,

		BackdoorSimpleAutoDecisionEnabled,
		BWABusinessCheck,
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
		BankBasedApprovalNumOfMonthBackForVatCheck,
		BankBasedApprovalNumOfMonthsToLookForDefaults,
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

		CAISPath,
		CAISPath2,
		CallCreditUrl,
		CallCreditUserCompany,
		CallCreditUserName,
		CallCreditPassword,
		CaptchaMode,
		ChannelGrabberCycleCount,
		ChannelGrabberRejectPolicy,
		ChannelGrabberServiceUrl,
		ChannelGrabberSleepTime,
		CheckStoreUniqueness,
		CollectionPeriod1,
		CollectionPeriod2,
		CollectionPeriod3,
		CollectionToAddress,
		CompanyCaisLateAlertLongMonths,
		CompanyCaisLateAlertShortMonths,
		CompanyCaisLateAlertShortPeriodThreshold,
		CompanyFilesSavePath,
		CompanyScoreNonLimitedParserConfiguration,
		CompanyScoreParserConfiguration,
		ConnectionPoolMaxSize,
		ConnectionPoolReuseCount,
		CreditSafeUserName,
		CreditSafePassword,
		CustomerAnalyticsDefaultHistoryYears,
		CustomerSite,
		CustomerStateRefreshInterval,

		DEBUG_SaveVatReturnData,
		DefaultFeedbackValue,
		DefaultLoanCalculator,
		DirectorDetailsNonLimitedParserConfiguration,
		DirectorDetailsParserConfiguration,
		DirectorInfoNonLimitedParserConfiguration,
		DirectorInfoParserConfiguration,
		DisplayEarnedPoints,
		DummyAddressSearchResult,
		DummyPostcodeSearchResult,

		EbayAppId,
		EbayCertId,
		EbayDevId,
		EbayPixelEnabled,
		EbayRuName,
		EbayRuNameEverline,
		EbayRuNameAlibaba,
		EbayServiceType,
		EchoSignApiKey,
		EchoSignDeadline,
		EchoSignEnabledCustomer,
		EchoSignEnabledUnderwriter,
		EchoSignReminder,
		EchoSignUrl,
		EnableAutomaticApproval,
		EnableAutomaticReApproval,
		EnableAutomaticReRejection,
		EnableAutomaticRejection,
		Environment,
		EverlineLoanStatusTestMode,
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
		EzServiceUpdateConfiguration,
		EzbobMailCc,
		EzbobMailTo,
		EzbobTechMailTo,

		FCFFactor,

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
		FreeAgentApiBase,
		FreeAgentExpensesCategoriesRequest,
		FreeAgentCompanyRequest,
		FreeAgentExpensesRequest,
		FreeAgentExpensesRequestDatePart,
		FreeAgentInvoicesRequest,
		FreeAgentInvoicesRequestMonthPart,
		FreeAgentInvoicesRequestNestedPart,
		FreeAgentOAuthAuthorizationEndpoint,
		FreeAgentOAuthIdentifier,
		FreeAgentOAuthSecret,
		FreeAgentOAuthTokenEndpoint,
		FreeAgentUsersRequest,
		FraudAlertMailReciever,

		GetCashSliderStep,
		GetSatisfactionEnabled,
		GoCardlessAppId,
		GoCardlessAppSecret,
		GoCardlessAccessToken,
		GoCardlessMerchantId,
		GoogleAnalyticsCertThumb,
		GoogleTagManagementProd,
		GreetingMailSendViaMandrill,

		HmrcSalariesMultiplier,
		HmrcUploadedFilesSavePath,

		ImailUserName,
		IMailPassword,
		IMailDebugModeEnabled,
		IMailDebugModeEmail,
		IMailSavePath,
		IntervalWaitForAmlCheck,
		IntervalWaitForExperianCompanyCheck,
		IntervalWaitForExperianConsumerCheck,
		IntervalWaitForMarketplacesUpdate,
		InvalidPasswordAttemptsPeriodSeconds,
		InvalidPasswordBlockSeconds,
		InvestorServicingFeePercent,
		InvestorFundsUtilized75,
		InvestorFundsUtilized90,
		IovationAdminAccountName,
		IovationAdminSubscriber,
		IovationAdminPassword,
		IovationAdminUrl,
		IovationEnabled,
		IovationEnvironment,
		IovationSubscriberAccount,
		IovationSubscriberId,
		IovationSubscriberPasscode,
		IovationUrl,
		IovationCheckPeriod,
		IsSmsValidationActive,

		LandRegistryFilePath,
		LandRegistryPassword,
		LandRegistryProd,
		LandRegistryUserName,
		LandingPageEnabled,
		LateBy14DaysMailSendViaMandrill,
		LatePaymentCharge,
		LogOffMode,
		LogicalGlueAuthorizationScheme,
		LogicalGlueCacheAcceptanceDays,
		LogicalGlueHostName,
		LogicalGlueNewCustomerRequestPath,
		LogicalGlueOldCustomerRequestPath,
		LogicalGluePassword,
		LogicalGlueUserName,
		LoginValidationStringForWeb,
		LoginValidity,
		LotteryForBrokers,
		LotteryForCustomers,
		LowCreditScore,

		MaamEmailReceiver,
		MailBeeLicenseKey,
		MailBeeLoginAddress,
		MailBeeLoginPassword,
		MailBeeMailboxReconnectionIntervalSeconds,
		MailBeePort,
		MailBeeServer,
		MailChimpApiKey,
		MailSenderEmail,
		MailSenderName,
		ManagerMaxLoan,
		MandrillEnable,
		MandrillKey,
		MaxCapHomeOwner,
		MaxCapNotHomeOwner,
		MaxLoan,
		MaxPerDay,
		MaxPerNumber,
		MaxSeniorityYears,
		MaxTimeToWaitForPacnetrConfirmation,
		MaxYodleeOtherCategoryAmount,
		MedalDaysOfMpRelevancy,
		MedalMinOffer,
		MinAuthenticationIndexToPassAml,
		MinDectForDefault,
		MinInterestRateToReuse,
		MinLoan,
		MinLoanLifetimeMonths,

		NewLoanRun,
		NHibernateEnableProfiler,
		NotEnoughFundsTemplateName,
		NotEnoughFundsToAddress,
		NumofAllowedActiveLoans,
		NumOfInvalidPasswordAttempts,
		NumberOfMobileCodeAttempts,

		NL_AgreementPdfLoanPath1,
		NL_AgreementPdfLoanPath2,
		NL_AgreementPdfConsentPath1,
		NL_AgreementPdfConsentPath2,

		OfferValidForHours,
		OnlineMedalTurnoverCutoff,
		OtherCharge,

		PacnetBalanceMaxManualChange,
		PacnetBalanceWeekdayLimit,
		PacnetBalanceWeekendLimit,
		PacnetDublinEmails,
		PacnetVancouverEmails,
		PacnetRAVEN_GATEWAY,
		PacnetRAVEN_PREFIX,
		PacnetRAVEN_RAPIVERSION,
		PacnetRAVEN_SECRET,
		PacnetRAVEN_USERNAME,
		PacnetSERVICE_TYPE,
		PartialPaymentCharge,
		PasswordHashCycleCount,
		PasswordPolicyType,
		PasswordValidity,
		PayPalApiAuthenticationMode,
		PayPalApiPassword,
		PayPalApiRequestFormat,
		PayPalApiResponseFormat,
		PayPalApiSignature,
		PayPalApiUsername,
		PayPalDaysPerRequest,
		PayPalEnableRetrying,
		PayPalEnabled,
		PayPalFirstTimeWait,
		PayPalIterationSettings1CountRequestsExpectError,
		PayPalIterationSettings1Index,
		PayPalIterationSettings1TimeOutAfterRetryingExpiredInMinutes,
		PayPalIterationSettings2CountRequestsExpectError,
		PayPalIterationSettings2Index,
		PayPalIterationSettings2TimeOutAfterRetryingExpiredInMinutes,
		PayPalIterationSettings3CountRequestsExpectError,
		PayPalIterationSettings3Index,
		PayPalIterationSettings3TimeOutAfterRetryingExpiredInMinutes,
		PayPalMaxAllowedFailures,
		PayPalMinorTimeoutInSeconds,
		PayPalNumberOfRetries,
		PayPalOpenTimeoutInMinutes,
		PayPalPpApplicationId,
		PayPalSendTimeoutInMinutes,
		PayPalServiceType,
		PayPalTransactionSearchMonthsBack,
		PayPalTrustAll,
		PayPalUseLastTimeOut,
		PayPointValidateName,
		PendingInvestorNoficationReciever,
		PostcodeAnywhereEnabled,
		PostcodeAnywhereKey,
		PostcodeAnywhereMaxBankAccountValidationAttempts,
		PostcodeConnectionKey,

		RecentCustomersToKeep,
		Recon_Paypoint_Include_Five,
		RefreshYodleeEnabled,
		Reject_Defaults_AccountsNum,
		Reject_Defaults_Amount,
		Reject_Defaults_CompanyAccountsNum,
		Reject_Defaults_CompanyAmount,
		Reject_Defaults_CompanyMonthsNum,
		Reject_Defaults_CompanyScore,
		Reject_Defaults_CreditScore,
		Reject_Defaults_MonthsNum,
		Reject_LateLastMonthsNum,
		Reject_LowOfflineAnnualRevenue,
		Reject_LowOfflineQuarterRevenue,
		Reject_Minimal_Seniority,
		Reject_NumOfLateAccounts,
		RejectionCompanyScore,
		RejectionExceptionMaxCompanyScore,
		RejectionExceptionMaxCompanyScoreForMpError,
		RejectionExceptionMaxConsumerScoreForMpError,
		RejectionLastValidLate,
		RejectionPartnersCities,
		ReportDaemonDropboxCredentials,
		ReportDaemonDropboxRootPath,
		ReportsSite,
		ReschedulingDebugMail,
		ReschedulingDebugMailAddress,
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
		SalesForceFakeMode,
		SalesForceConsumerKey,
		SalesForceConsumerSecret,
		SalesForceUserName,
		SalesForcePassword,
		SalesForceToken,
		SalesForceEnvironment,
		SalesForceNumberOfRetries,
		SalesForceRetryWaitSeconds,
		SendCollectionMailOnNewLoan,
		SessionTimeout,
		SetupFeeEnabled,
		SetupFeeFixed,
		SetupFeeMaxFixedPercent,
		SetupFeePercent,
		SkipCodeGenerationNumber,
		SkipCodeGenerationNumberCode,
		SkipServiceOnNewCreditLine,
		SmallLoanScenarioLimit,
		SmsTestModeEnabled,
		SmsApprovedUserEnabled,
		SmsApprovedUserTemplate,
		SpreadSetupFeeCharge,

		TaboolaPixelEnabled,
		TargetsEnabled,
		TargetsEnabledEntrepreneur,
		TeraPeakApiKey,
		TeraPeakUrl,
		TopUpSendingEmail,
		TotalAnnualTurnover,
		TotalThreeMonthTurnover,
		TotalTimeToWaitForAmlCheck,
		TotalTimeToWaitForExperianCompanyCheck,
		TotalTimeToWaitForExperianConsumerCheck,
		TotalTimeToWaitForMarketplacesUpdate,
		TotalsMonthTail,
		TradeTrackerPixelEnabled,
		TrustPilotBccMail,
		TrustPilotBccMailEverline,
		TrustPilotReviewEnabled,
		TwilioAccountSid,
		TwilioAuthToken,
		TwilioSendingNumber,
		TwilioSendingNumberIsrael,

		UnderwriterSite,
		UpdateCompanyDataPeriodDays,
		UpdateConsumerDataPeriodDays,
		UpdateOnReapplyLastDays,

		VerboseConfigurationLogging,
		VipEnabled,
		VipMailReceiver,
		VipMaxRequests,

		WizardAutomationTimeout,
		WizardInstructionsEnabled,
		WizardTopNaviagtionEnabled,

		XMinLoan,

		YodleeAccountPoolSize,
		YodleeAccountPrefix,
		YodleeAddAccountURL,
		YodleeApplicationId,
		YodleeApplicationKey,
		YodleeApplicationToken,
		YodleeBridgetApplicationId,
		YodleeCobrandId,
		YodleeEditAccountURL,
		YodleePassword,
		YodleeSoapServer,
		YodleeTncVersion,
		YodleeUsername
	} // enum Variables
} // namespace ConfigManager
