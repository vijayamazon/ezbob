IF OBJECT_ID('EmailTemplate') IS NULL
BEGIN
    CREATE TABLE EmailTemplate(
		EmailTemplateID INT IDENTITY(1,1) NOT NULL,
		IsTemplate BIT NOT NULL,
		Type NVARCHAR(250) NOT NULL,
		Template NVARCHAR(MAX) NOT NULL,
		CustomerOriginID INT NULL,
		CONSTRAINT PK_EmailTemplate PRIMARY KEY ([EmailTemplateID]),
		CONSTRAINT FK_EmailTemplate_OriginID FOREIGN KEY ([CustomerOriginID]) REFERENCES [CustomerOrigin] ([CustomerOriginID])
	)
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'EmailTamplateID' AND OBJECT_ID = OBJECT_ID(N'Export_Results'))
BEGIN
	ALTER TABLE Export_Results ADD EmailTamplateID INT NULL
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'AutoAproval')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','AutoAproval','autoapprovesilentnotification',NULL)
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'BankAproval')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','BankAproval','bankbasedapprovalsilentnotification',NULL)
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'BrokerFillForCustomerComplete')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','BrokerFillForCustomerComplete','broker-fill-for-customer-complete','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'BrokerFillForCustomerGreeting')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','BrokerFillForCustomerGreeting','broker-fill-for-customer-greeting','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'BrokerForceResetCustomerPassword')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','BrokerForceResetCustomerPassword','broker-force-reset-customer-password','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'BrokerGreeting')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','BrokerGreeting','broker-greeting','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'BrokerLeadInvitation')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','BrokerLeadInvitation','broker-lead-invitation','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'CustomerLoanStatus')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','CustomerLoanStatus','customerloanstatus','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EasterPromotion2015')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EasterPromotion2015','easter-promotion-2015','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EasterPromotionBrokers2015')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EasterPromotionBrokers2015','easter-promotion-brokers-2015','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EmailHMRCParsingErrors')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EmailHMRCParsingErrors','email-hmrc-parsing-errors','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EsignNotifyDocumentStatus')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EsignNotifyDocumentStatus','esign-notify-document-status',NULL)
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlGreeting')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlGreeting','evl-greeting','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrill2DaysNotice')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrill2DaysNotice','evl-mandrill-2-days-notice','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrill5DaysNotice')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrill5DaysNotice','evl-mandrill-5-days-notice','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillApplicationCompletedUnderReview')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillApplicationCompletedUnderReview','evl-mandrill-application-completed-under-review','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillApplicationIncompletedAml')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillApplicationIncompletedAml','evl-mandrill-application-incompleted-aml','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillApproval1stTime')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillApproval1stTime','evl-mandrill-approval-1st-time','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillApprovalNot1stTime')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillApprovalNot1stTime','evl-mandrill-approval-not-1st-time','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillBrokerLeadInvitation')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillBrokerLeadInvitation','evl-mandrill-broker-lead-invitation','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillBrokerRegistrationComplete')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillBrokerRegistrationComplete','evl-mandrill-broker-registration-complete','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillBrokerRegistrationComplete1')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillBrokerRegistrationComplete1','evl-mandrill-broker-registration-complete-1','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillConfirmYourEmail')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillConfirmYourEmail','evl-mandrill-confirm-your-email','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillDebitCardAuthorizationProblem')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillDebitCardAuthorizationProblem','evl-mandrill-debit-card-authorization-problem','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillEmailChanged')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillEmailChanged','evl-mandrill-email-changed','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillEmailChanged')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillEmailChanged','evl-mandrill-email-changed','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillEzbob20LateFeeWasAddedTo')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillEzbob20LateFeeWasAddedTo','evl-mandrill-ezbob-20-late-fee-was-added-to','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillEzbobLastWarningDebtRecover')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillEzbobLastWarningDebtRecover','evl-mandrill-ezbob-last-warning-debt-recover','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillEzbobMissedPayment')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillEzbobMissedPayment','evl-mandrill-ezbob-missed-payment','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillLoanPaidInFull')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillLoanPaidInFull','evl-mandrill-loan-paid-in-full','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillNewPassword')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillNewPassword','evl-mandrill-new-password','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillPasswordWasRestored')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillPasswordWasRestored','evl-mandrill-password-was-restored','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillProblemWithBankAccount')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillProblemWithBankAccount','evl-mandrill-problem-with-bank-account','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillReAnalyzingCustomer')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillReAnalyzingCustomer','evl-mandrill-re-analyzing-customer','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillRejectionEmail')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillRejectionEmail','evl-mandrill-rejection-email','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillRejectionPartnersEmail')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillRejectionPartnersEmail','evl-mandrill-rejection-partners-email','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillRenewYourEbayToken')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillRenewYourEbayToken','evl-mandrill-renew-your-ebay-token','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillRepaymentConfirmation')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillRepaymentConfirmation','evl-mandrill-repayment-confirmation','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillRolloverAdded')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillRolloverAdded','evl-mandrill-rollover-added','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillTemporaryPassword')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillTemporaryPassword','evl-mandrill-temporary-password','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillTookLoan1stLoan')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillTookLoan1stLoan','evl-mandrill-took-loan-1st-loan','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillTookLoanNot1stLoan')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillTookLoanNot1stLoan','evl-mandrill-took-loan-not-1st-loan','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'EvlMandrillYouMissedYourPayment1')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','EvlMandrillYouMissedYourPayment1','evl-mandrill-you-missed-your-payment-1','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'greeting')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','greeting','greeting','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'GreetingAlibaba')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','GreetingAlibaba','greeting-alibaba','3')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'GreetingCampaign')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','GreetingCampaign','greeting-campaign','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'Mandrill2DaysNotice')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','Mandrill2DaysNotice','mandrill-2-days-notice','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'Mandrill5DaysNotice')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','Mandrill5DaysNotice','mandrill-5-days-notice','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillActionItems')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillActionItems','mandrill-action-items',NULL)
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillAlibabaApplicationCompletedReview')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillAlibabaApplicationCompletedReview','mandrill-alibaba-application-completed-review','3')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillAlibabaApproval1stTime')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillAlibabaApproval1stTime','mandrill-alibaba-approval-1st-time','3')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillAlibabaInternalApprovalEmail')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillAlibabaInternalApprovalEmail','mandrill-alibaba-internal-approval-email','3')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillAlibabaInternalRejectionEmail')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillAlibabaInternalRejectionEmail','mandrill-alibaba-internal-rejection-email','3')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillAlibabaInternalTookLoan')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillAlibabaInternalTookLoan','mandrill-alibaba-internal-took-loan','3')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillAlibabaRejectionEmail')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillAlibabaRejectionEmail','mandrill-alibaba-rejection-email','3')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillAlibabaTookLoan')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillAlibabaTookLoan','mandrill-alibaba-took-loan','3')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillApplicationCompletedUnderReview')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillApplicationCompletedUnderReview','mandrill-application-completed-under-review','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillApplicationIncompletedAml')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillApplicationIncompletedAml','mandrill-application-incompleted-aml','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillApproval1stTime')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillApproval1stTime','mandrill-approval-1st-time','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillApprovalCampaign1stTime')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillApprovalCampaign1stTime','mandrill-approval-campaign-1st-time','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillApprovalNot1stTime')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillApprovalNot1stTime','mandrill-approval-not-1st-time','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillBroker1stClientScratch')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillBroker1stClientScratch','mandrill-broker-1st-client-scratch','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillCaisReport')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillCaisReport','mandrill-cais-report','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillClientFillsApplication')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillClientFillsApplication','mandrill-client-fills-application','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillConfirmYourEmail')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillConfirmYourEmail','mandrill-confirm-your-email','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillDebitCardAuthorizationProblem')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillDebitCardAuthorizationProblem','mandrill-debit-card-authorization-problem','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillEmailChanged')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillEmailChanged','mandrill-email-changed','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillEverlineRefinancEmail')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillEverlineRefinancEmail','mandrill-everlinerefinancemail','2')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillEzbob20pLateFee')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillEzbob20pLateFee','mandrill-ezbob-20p-late-fee','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillEzbobLastWarningDebtRecovery')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillEzbobLastWarningDebtRecovery','mandrill-ezbob-last-warning-debt-recovery','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillEzbobLegalProcessStarting')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillEzbobLegalProcessStarting','mandrill-ezbob-legal-process-starting','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillEzbobMissedPayment')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillEzbobMissedPayment','mandrill-ezbob-missed-payment','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillEzbobPasswordWasRestored')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillEzbobPasswordWasRestored','mandrill-ezbob-password-was-restored','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillEzbobPasswordWasRestoredToStaff')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillEzbobPasswordWasRestoredToStaff','mandrill-ezbob-password-was-restored-to-staff','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillEzbobYouMissedYourPayment')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillEzbobYouMissedYourPayment','mandrill-ezbob-you-missed-your-payment','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillInternalActionItems')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillInternalActionItems','mandrill-internal-action-items',NULL)
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillLoanPaidInFull')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillLoanPaidInFull','mandrill-loan-paid-in-full','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillNewPassword')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillNewPassword','mandrill-new-password','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillNoInformationAboutShops')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillNoInformationAboutShops','mandrill-no-information-about-shops',NULL)
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillPaypointDataDiffers')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillPaypointDataDiffers','mandrill-paypoint-data-differs','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillPaypointScriptException')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillPaypointScriptException','mandrill-paypoint-script-exception','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillProblemWithBankAccount')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillProblemWithBankAccount','mandrill-problem-with-bank-account','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillReAnalyzingCustomer')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillReAnalyzingCustomer','mandrill-re-analyzing-customer','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillRejectionEmail')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillRejectionEmail','mandrill-rejection-email','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillRejectionPartnersEmail')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillRejectionPartnersEmail','mandrill-rejection-partners-email','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillRenewYourEbayToken')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillRenewYourEbayToken','mandrill-renew-your-ebay-token','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillRepaymentConfirmation')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillRepaymentConfirmation','mandrill-repayment-confirmation','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillRolloverAdded')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillRolloverAdded','mandrill-rollover-added','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillSalesNewCustomer')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillSalesNewCustomer','mandrill-sales-new-customer','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillTemporaryPassword')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillTemporaryPassword','mandrill-temporary-password','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillTookLoan1stLoan')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillTookLoan1stLoan','mandrill-took-loan-1st-loan','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillTookLoan1stLoanScratch')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillTookLoan1stLoanScratch','mandrill-took-loan-1st-loan-scratch','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillTookLoanCampaign1stLoan')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillTookLoanCampaign1stLoan','mandrill-took-loan-campaign-1st-loan','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillTookLoanNot1stLoan')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillTookLoanNot1stLoan','mandrill-took-loan-not-1st-loan','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillUnderwriterAddedADebitCard')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillUnderwriterAddedADebitCard','mandrill-underwriter-added-a-debit-card','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillUpdatecmpError')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillUpdatecmpError','mandrill-updatecmp-error','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillUpdateMpErrorCode')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillUpdateMpErrorCode','mandrill-update-mp-error-code','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillUserIsApproved')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillUserIsApproved','mandrill-user-is-approved','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillUserIsApprovedOrReApproved')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillUserIsApprovedOrReApproved','mandrill-user-is-approved-or-re-approved','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillUserIsRejectedByTheStrategy1')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillUserIsRejectedByTheStrategy1','mandrill-user-is-rejected-by-the-strategy-1','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillUserIsWaitingForDecision')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillUserIsWaitingForDecision','mandrill-user-is-waiting-for-decision','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillUserWasEscalated')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillUserWasEscalated','mandrill-user-was-escalated','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillWarningNoticeEzbob40pLateFee')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillWarningNoticeEzbob40pLateFee','mandrill-warning-notice-ezbob-40p-late-fee','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'MandrillWeekendEmailCustomerCare')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','MandrillWeekendEmailCustomerCare','mandrill-weekend-email-customer-care','1')
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'NotEnoughFunds')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','NotEnoughFunds','notenoughfunds',NULL)
END

IF NOT EXISTS (SELECT * FROM EmailTemplate WHERE [Type] = 'VipRequest')
BEGIN
	INSERT INTO EmailTemplate (IsTemplate,Type,Template,CustomerOriginID)
	VALUES ('0','VipRequest','viprequest',NULL)
END