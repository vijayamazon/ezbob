SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' AND name = 'FK_NL_Payments_LoanTransactionMethod')
	ALTER TABLE NL_Payments DROP CONSTRAINT FK_NL_Payments_LoanTransactionMethod;	
GO

IF EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' AND name = 'FK_Payments_CreatedBySecurity_User')
	ALTER TABLE NL_Payments DROP CONSTRAINT FK_Payments_CreatedBySecurity_User;	
GO

IF EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' AND name = 'FK_NL_Payments_DeletedBySecurity_User')
	ALTER TABLE NL_Payments DROP CONSTRAINT FK_NL_Payments_DeletedBySecurity_User;	
GO

IF EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' AND name = 'FK_LoanScheduleTransaction_LoanTransactions')
	ALTER TABLE NL_LoanSchedulePayments DROP CONSTRAINT FK_LoanScheduleTransaction_LoanTransactions;	
GO

IF EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanFeePayments_LoanTransactions') 
	ALTER TABLE NL_LoanFeePayments DROP CONSTRAINT [FK_LoanFeePayments_LoanTransactions] ;
GO

IF  EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_PaypointTransactions_Payments') 
 ALTER TABLE [NL_PaypointTransactions] DROP CONSTRAINT FK_PaypointTransactions_Payments;
GO

IF EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Payments_NL_PaymentStatuses') 
 ALTER TABLE [NL_Payments] DROP CONSTRAINT FK_NL_Payments_NL_PaymentStatuses ;
GO

IF EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_WriteOffReasons_Payments') 
 ALTER TABLE [WriteOffReasons] DROP CONSTRAINT FK_WriteOffReasons_Payments ;
GO

IF EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' AND name = 'FK_NL_Payments_Loan')
	ALTER TABLE NL_Payments DROP CONSTRAINT FK_NL_Payments_Loan;	
GO

IF EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE name = 'UQ_LoanID_LoanFeeTypeID_Amount_AssignTime')
	ALTER TABLE NL_LoanFees DROP CONSTRAINT UQ_LoanID_LoanFeeTypeID_Amount_AssignTime;	
GO


-------------------------------------------------------------------------------
--
-- Drop procedures
--
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_BlendedLoansSave') IS NOT NULL
	DROP PROCEDURE NL_BlendedLoansSave
GO

IF OBJECT_ID('NL_BlendedOffersSave') IS NOT NULL
	DROP PROCEDURE NL_BlendedOffersSave
GO

IF OBJECT_ID('NL_CashRequestGetByOldID') IS NOT NULL
	DROP PROCEDURE NL_CashRequestGetByOldID
GO

IF OBJECT_ID('NL_CashRequestsSave') IS NOT NULL
	DROP PROCEDURE NL_CashRequestsSave
GO

IF OBJECT_ID('NL_DecisionRejectReasonsSave') IS NOT NULL
	DROP PROCEDURE NL_DecisionRejectReasonsSave
GO

IF OBJECT_ID('NL_DecisionsSave') IS NOT NULL
	DROP PROCEDURE NL_DecisionsSave
GO

IF OBJECT_ID('NL_FundTransfersSave') IS NOT NULL
	DROP PROCEDURE NL_FundTransfersSave
GO

IF OBJECT_ID('NL_LoanAgreementsSave') IS NOT NULL
	DROP PROCEDURE NL_LoanAgreementsSave
GO

IF OBJECT_ID('NL_LoanFeePaymentsSave') IS NOT NULL
	DROP PROCEDURE NL_LoanFeePaymentsSave
GO

IF OBJECT_ID('NL_LoanFeeTypesLoad') IS NOT NULL
	DROP PROCEDURE NL_LoanFeeTypesLoad
GO

IF OBJECT_ID('NL_LoanFeesSave') IS NOT NULL
	DROP PROCEDURE NL_LoanFeesSave
GO

IF OBJECT_ID('NL_LoanHistoryGet') IS NOT NULL
	DROP PROCEDURE NL_LoanHistoryGet
GO

IF OBJECT_ID('NL_LoanHistorySave') IS NOT NULL
	DROP PROCEDURE NL_LoanHistorySave
GO

IF OBJECT_ID('NL_LoanInterestFreezeGet') IS NOT NULL
	DROP PROCEDURE NL_LoanInterestFreezeGet
GO

IF OBJECT_ID('NL_LoanLegalsSave') IS NOT NULL
	DROP PROCEDURE NL_LoanLegalsSave
GO

IF OBJECT_ID('NL_LoanLienLinksSave') IS NOT NULL
	DROP PROCEDURE NL_LoanLienLinksSave
GO

IF OBJECT_ID('NL_LoanOptionsGet') IS NOT NULL
	DROP PROCEDURE NL_LoanOptionsGet
GO

IF OBJECT_ID('NL_SaveLoanOptions') IS NOT NULL
	DROP PROCEDURE NL_SaveLoanOptions
GO

IF OBJECT_ID('NL_LoanRolloversSave') IS NOT NULL
	DROP PROCEDURE NL_LoanRolloversSave
GO

IF OBJECT_ID('NL_LoanSchedulePaymentsSave') IS NOT NULL
	DROP PROCEDURE NL_LoanSchedulePaymentsSave
GO

IF OBJECT_ID('NL_LoanSchedulesGet') IS NOT NULL
	DROP PROCEDURE NL_LoanSchedulesGet
GO

IF OBJECT_ID('NL_LoanSchedulesSave') IS NOT NULL
	DROP PROCEDURE NL_LoanSchedulesSave
GO

IF OBJECT_ID('NL_LoanStateLoad') IS NOT NULL
	DROP PROCEDURE NL_LoanStateLoad
GO

IF OBJECT_ID('NL_LoanStatesSave') IS NOT NULL
	DROP PROCEDURE NL_LoanStatesSave
GO

IF OBJECT_ID('NL_LoanStatusesLoad') IS NOT NULL
	DROP PROCEDURE NL_LoanStatusesLoad
GO

IF OBJECT_ID('NL_LoansFeesGet') IS NOT NULL
	DROP PROCEDURE NL_LoansFeesGet
GO

IF OBJECT_ID('NL_LoansSave') IS NOT NULL
	DROP PROCEDURE NL_LoansSave
GO

IF OBJECT_ID('NL_OfferFeesGet') IS NOT NULL
	DROP PROCEDURE NL_OfferFeesGet
GO

IF OBJECT_ID('NL_OfferFeesSave') IS NOT NULL
	DROP PROCEDURE NL_OfferFeesSave
GO

IF OBJECT_ID('NL_OfferForLoan') IS NOT NULL
	DROP PROCEDURE NL_OfferForLoan
GO

IF OBJECT_ID('NL_OffersGetLast') IS NOT NULL
	DROP PROCEDURE NL_OffersGetLast
GO

IF OBJECT_ID('NL_OffersSave') IS NOT NULL
	DROP PROCEDURE NL_OffersSave
GO

IF OBJECT_ID('NL_PacnetTransactionStatusesLoad') IS NOT NULL
	DROP PROCEDURE NL_PacnetTransactionStatusesLoad
GO

IF OBJECT_ID('NL_PacnetTransactionsSave') IS NOT NULL
	DROP PROCEDURE NL_PacnetTransactionsSave
GO

IF OBJECT_ID('NL_PacnetTransactionsUpdate') IS NOT NULL
	DROP PROCEDURE NL_PacnetTransactionsUpdate
GO

IF OBJECT_ID('NL_PaymentsGet') IS NOT NULL
	DROP PROCEDURE NL_PaymentsGet
GO

IF OBJECT_ID('NL_PaymentsSave') IS NOT NULL
	DROP PROCEDURE NL_PaymentsSave
GO

IF OBJECT_ID('NL_PaypointTransactionStatusesLoad') IS NOT NULL
	DROP PROCEDURE NL_PaypointTransactionStatusesLoad
GO

IF OBJECT_ID('NL_PaypointTransactionsSave') IS NOT NULL
	DROP PROCEDURE NL_PaypointTransactionsSave
GO

IF OBJECT_ID('NL_RepaymentIntervalTypesLoad') IS NOT NULL
	DROP PROCEDURE NL_RepaymentIntervalTypesLoad
GO

IF OBJECT_ID('NL_loansGet') IS NOT NULL
	DROP PROCEDURE NL_loansGet
GO

IF OBJECT_ID('NL_CustomersForAutoCharger') IS NOT NULL
	DROP PROCEDURE NL_CustomersForAutoCharger
GO

IF OBJECT_ID('NL_LateLoanMailDataGet') IS NOT NULL
	DROP PROCEDURE NL_LateLoanMailDataGet
GO

IF OBJECT_ID('NL_CuredLoansGet') IS NOT NULL
	DROP PROCEDURE NL_CuredLoansGet
GO

-------------------------------------------------------------------------------
--
-- Drop new rows in old tables
--
-------------------------------------------------------------------------------

DELETE FROM LoanTransactionMethod WHERE Name IN ('WriteOff', 'Write Off', 'ChargeBack', 'SetupFee Offset', 'SetupFeeOffset', 'WrongPayment', 'SystemRepay')
GO

DECLARE @lastid INT

-- IF NOT EXISTS (SELECT Id FROM LoanTransactionMethod WHERE Name = 'Write Off')
-- BEGIN
	-- SET @lastid = (SELECT Max(Id) FROM LoanTransactionMethod)
	-- INSERT INTO LoanTransactionMethod (Id, Name, DisplaySort) VALUES(@lastid + 1, 'Write Off', 0)
-- END

-------------------------------------------------------------------------------
--
-- Drop new constraints/columns in old tables
--
-------------------------------------------------------------------------------

IF EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanAgreementTemplate_NL_LoanAgreementTemplateTypes')
	ALTER TABLE LoanAgreementTemplate DROP CONSTRAINT FK_LoanAgreementTemplate_NL_LoanAgreementTemplateTypes
GO

IF EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanAgreementTemplate_LoanAgreementTemplateTypes')
	ALTER TABLE LoanAgreementTemplate DROP CONSTRAINT FK_LoanAgreementTemplate_LoanAgreementTemplateTypes
GO

IF OBJECT_ID('DF_LoanAgreementTemplate_TemplateTypeID') IS NOT NULL
	ALTER TABLE LoanAgreementTemplate DROP CONSTRAINT DF_LoanAgreementTemplate_TemplateTypeID
GO

IF OBJECT_ID('DF_TemplateTypeID') IS NOT NULL
	ALTER TABLE LoanAgreementTemplate DROP CONSTRAINT DF_TemplateTypeID
GO

IF EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanAgreementTemplate') AND name = 'TemplateTypeID')
	ALTER TABLE LoanAgreementTemplate DROP COLUMN TemplateTypeID
GO

IF EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' AND name = 'FK_LoanBrokerCommission_NL_Loan')
	ALTER TABLE LoanBrokerCommission DROP CONSTRAINT FK_LoanBrokerCommission_NL_Loan
GO

IF EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' AND name = 'FK_DecisionTrail_NL_CashRequests')
	ALTER TABLE DecisionTrail DROP CONSTRAINT FK_DecisionTrail_NL_CashRequests
GO

IF EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' AND name = 'FK_MedalCalculations_NL_CashRequests')
	ALTER TABLE MedalCalculations DROP CONSTRAINT FK_MedalCalculations_NL_CashRequests
GO

IF EXISTS (SELECT cl.OBJECT_ID FROM sys.all_objects ob inner join sys.all_columns cl on ob.OBJECT_ID = cl.OBJECT_ID AND ob.name = 'MedalCalculations' AND cl.name = 'NLCashRequestID')
	ALTER TABLE MedalCalculations DROP COLUMN NLCashRequestID
GO

IF EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' AND name = 'FK_MedalCalculationsAV_NL_CashRequests')
	ALTER TABLE MedalCalculationsAV DROP CONSTRAINT FK_MedalCalculationsAV_NL_CashRequests
GO

IF EXISTS (SELECT cl.OBJECT_ID FROM sys.all_objects ob inner join sys.all_columns cl on ob.OBJECT_ID = cl.OBJECT_ID AND ob.name = 'MedalCalculationsAV' AND cl.name = 'NLCashRequestID')
	ALTER TABLE MedalCalculationsAV DROP COLUMN NLCashRequestID
GO

IF EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' AND name = 'FK_Esignatures_NL_Decisions')
	ALTER TABLE Esignatures DROP CONSTRAINT FK_Esignatures_NL_Decisions
GO

IF EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' AND name = 'FK_CollectionLog_NL_LoanHistory')
	ALTER TABLE CollectionLog DROP CONSTRAINT FK_CollectionLog_NL_LoanHistory
GO

IF EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' AND name = 'FK_I_InvestorSystemBalance_NL_Loans')
	ALTER TABLE [dbo].[I_InvestorSystemBalance] DROP CONSTRAINT FK_I_InvestorSystemBalance_NL_Loans
GO

IF EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' AND name = 'FK_I_InvestorSystemBalance_NL_Offers')
	ALTER TABLE [dbo].[I_InvestorSystemBalance] DROP CONSTRAINT FK_I_InvestorSystemBalance_NL_Offers
GO

IF EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' AND name = 'FK_I_InvestorSystemBalance_NL_Payments')
	ALTER TABLE [dbo].[I_InvestorSystemBalance] DROP CONSTRAINT FK_I_InvestorSystemBalance_NL_Payments
GO

IF EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' AND name = 'FK_I_OpenPlatformOffer_NL_Offers')
	ALTER TABLE [dbo].[I_OpenPlatformOffer] DROP CONSTRAINT FK_I_OpenPlatformOffer_NL_Offers
GO

IF EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' AND name = 'FK_I_Portfolio_I_NL_Loans')
	ALTER TABLE [dbo].[I_Portfolio] DROP CONSTRAINT [FK_I_Portfolio_I_NL_Loans]
GO

IF EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('DecisionTrail') AND name = 'NLCashRequestID')
	ALTER TABLE DecisionTrail DROP COLUMN NLCashRequestID
GO

IF EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('Esignatures') AND name = 'DecisionID')
	ALTER TABLE Esignatures DROP COLUMN DecisionID
GO

IF EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('DecisionTrail') AND name = 'NLDecisionID')
	ALTER TABLE DecisionTrail DROP COLUMN NLDecisionID
GO

IF EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('CollectionLog') AND name = 'Comments')
	ALTER TABLE CollectionLog DROP COLUMN Comments
GO

IF EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('CollectionLog') AND name = 'LoanHistoryID')
	ALTER TABLE CollectionLog DROP COLUMN LoanHistoryID
GO

IF EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanBrokerCommission') AND name = 'NLLoanID')
	ALTER TABLE LoanBrokerCommission DROP COLUMN NLLoanID
GO

-------------------------------------------------------------------------------
--
-- Drop new tables
--
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_BlendedOffers') IS NOT NULL
	DROP TABLE NL_BlendedOffers
GO

IF OBJECT_ID('NL_BlendedLoans') IS NOT NULL
	DROP TABLE NL_BlendedLoans
GO

IF OBJECT_ID('NL_LoanOptions') IS NOT NULL
	DROP TABLE NL_LoanOptions
GO

IF OBJECT_ID('NL_LoanStates') IS NOT NULL
	DROP TABLE NL_LoanStates
GO

IF OBJECT_ID('NL_PaypointTransactions') IS NOT NULL
	DROP TABLE NL_PaypointTransactions
GO

IF OBJECT_ID('NL_PaypointTransactionStatuses') IS NOT NULL
	DROP TABLE NL_PaypointTransactionStatuses
GO

IF OBJECT_ID('NL_LoanFeePayments') IS NOT NULL
	DROP TABLE NL_LoanFeePayments
GO

IF OBJECT_ID('NL_LoanSchedulePayments') IS NOT NULL
	DROP TABLE NL_LoanSchedulePayments
GO

IF OBJECT_ID('NL_Payments') IS NOT NULL
	DROP TABLE NL_Payments
GO

IF OBJECT_ID('NL_PaymentStatuses') IS NOT NULL
	DROP TABLE NL_PaymentStatuses
GO

IF OBJECT_ID('NL_PacnetTransactions') IS NOT NULL
	DROP TABLE NL_PacnetTransactions
GO

IF OBJECT_ID('NL_PacnetTransactionStatuses') IS NOT NULL
	DROP TABLE NL_PacnetTransactionStatuses
GO

IF OBJECT_ID('NL_LoanRollovers') IS NOT NULL
	DROP TABLE NL_LoanRollovers
GO

IF OBJECT_ID('NL_LoanSchedules') IS NOT NULL
	DROP TABLE NL_LoanSchedules
GO

IF OBJECT_ID('NL_LoanScheduleStatuses') IS NOT NULL
	DROP TABLE NL_LoanScheduleStatuses
GO

IF OBJECT_ID('NL_LoanAgreements') IS NOT NULL
	DROP TABLE NL_LoanAgreements
GO

IF OBJECT_ID('NL_LoanHistory') IS NOT NULL
	DROP TABLE NL_LoanHistory
GO

IF OBJECT_ID('NL_LoanInterestFreeze') IS NOT NULL
	DROP TABLE NL_LoanInterestFreeze
GO

IF OBJECT_ID('NL_FundTransfers') IS NOT NULL
	DROP TABLE NL_FundTransfers
GO

IF OBJECT_ID('NL_LoanFees') IS NOT NULL
	DROP TABLE NL_LoanFees
GO

IF OBJECT_ID('NL_LoanLienLinks') IS NOT NULL
	DROP TABLE NL_LoanLienLinks
GO

IF OBJECT_ID('NL_Loans') IS NOT NULL
	DROP TABLE NL_Loans
GO

IF OBJECT_ID('NL_LoanLegals') IS NOT NULL
	DROP TABLE NL_LoanLegals
GO

IF OBJECT_ID('NL_OfferFees') IS NOT NULL
	DROP TABLE NL_OfferFees
GO

IF OBJECT_ID('NL_Offers') IS NOT NULL
	DROP TABLE NL_Offers
GO

IF OBJECT_ID('NL_RepaymentIntervalTypes') IS NOT NULL
	DROP TABLE NL_RepaymentIntervalTypes
GO

IF OBJECT_ID('NL_LoanStatuses') IS NOT NULL
	DROP TABLE NL_LoanStatuses
GO

IF OBJECT_ID('NL_LoanFeeTypes') IS NOT NULL
	DROP TABLE NL_LoanFeeTypes
GO

IF OBJECT_ID('NL_FundTransferStatuses') IS NOT NULL
	DROP TABLE NL_FundTransferStatuses
GO

IF OBJECT_ID('NL_EzbobBankAccounts') IS NOT NULL
	DROP TABLE NL_EzbobBankAccounts
GO

IF OBJECT_ID('NL_DiscountPlanEntries') IS NOT NULL
	DROP TABLE NL_DiscountPlanEntries
GO

IF OBJECT_ID('NL_DiscountPlans') IS NOT NULL
	DROP TABLE NL_DiscountPlans
GO

IF OBJECT_ID('NL_DecisionRejectReasons') IS NOT NULL
	DROP TABLE NL_DecisionRejectReasons
GO

IF OBJECT_ID('NL_Decisions') IS NOT NULL
	DROP TABLE NL_Decisions
GO

IF OBJECT_ID('NL_CashRequests') IS NOT NULL
	DROP TABLE NL_CashRequests
GO

IF OBJECT_ID('NL_CashRequestOrigins') IS NOT NULL
	DROP TABLE NL_CashRequestOrigins
GO

IF OBJECT_ID('NL_LoanAgreementTemplateTypes') IS NOT NULL
	DROP TABLE NL_LoanAgreementTemplateTypes
GO

IF EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'PRIMARY_KEY_CONSTRAINT' AND name = 'PK_PaypointCard')
	ALTER TABLE PaypointCard DROP CONSTRAINT PK_PaypointCard
GO

IF object_id('NL_OfferStatuses') IS NOT NULL BEGIN
	DROP TABLE [dbo].[NL_OfferStatuses];
END;

-- if object_id('nl_exists') is not null begin
	-- drop procedure [dbo].[nl_exists];
-- end;

IF OBJECT_ID('NL_CancelledSchedulesLoad') IS NOT NULL BEGIN
	DROP PROCEDURE NL_CancelledSchedulesLoad;
END;

IF OBJECT_ID('NL_CancelledSchedulesLoad') IS NOT NULL BEGIN
	DROP PROCEDURE [dbo].[NL_DiscountPlanEntriesGet];
END;

IF OBJECT_ID('NL_LoanPaidSchedulesLoad') IS NOT NULL BEGIN
	DROP PROCEDURE [dbo].[NL_LoanPaidSchedulesLoad];
END;

IF OBJECT_ID('NL_NonPaidSchedulesLoad') IS NOT NULL BEGIN
	DROP PROCEDURE [dbo].[NL_NonPaidSchedulesLoad];
END;

IF OBJECT_ID('NL_NotPaidSchedulesLoad') IS NOT NULL BEGIN
	DROP PROCEDURE [dbo].[NL_NotPaidSchedulesLoad];
END;

IF OBJECT_ID('NL_PaidSchedulesLoad') IS NOT NULL BEGIN
	DROP PROCEDURE [dbo].[NL_PaidSchedulesLoad];
END;


