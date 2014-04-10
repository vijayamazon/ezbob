﻿namespace ConfigManager {
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
		ChannelGrabberCycleCount,
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
		DefaultFeedbackValue,
		DirectorInfoNonLimitedParserConfiguration,
		DirectorInfoParserConfiguration,
		DisplayEarnedPoints,
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
		RecentCustomersToKeep,
		Recon_Paypoint_Include_Five,
		Reject_Defaults_AccountsNum,
		Reject_Defaults_Amount,
		Reject_Defaults_CreditScore,
		Reject_Defaults_MonthsNum,
		Reject_Minimal_Seniority,
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
		UseNewCaisStrategies,
		UseNewFraudCheckerStrategy,
		UseNewMailStrategies,
		UseNewMainStrategy,
		UseNewUpdateCustomerMpsStrategy,
		UseNewUpdateMpStrategy,
		YodleeAccountPrefix,
	} // enum Variables
} // namespace ConfigManager
