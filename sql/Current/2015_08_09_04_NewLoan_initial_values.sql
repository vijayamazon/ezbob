SET QUOTED_IDENTIFIER ON
GO

SET ANSI_NULLS ON
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

BEGIN TRY
	DROP TABLE #v
END TRY
BEGIN CATCH
END CATCH

-------------------------------------------------------------------------------
--
-- NL_FundTransferStatuses
--
-------------------------------------------------------------------------------

SELECT
	v = FundTransferStatus
INTO
	#v
FROM
	NL_FundTransferStatuses
WHERE
	1 = 0

-------------------------------------------------------------------------------

ALTER TABLE #v ADD id INT IDENTITY (1, 1) NOT NULL

-------------------------------------------------------------------------------

INSERT INTO #v (v) VALUES ('Pending'), ('Active'), ('Deleted')

-------------------------------------------------------------------------------

DECLARE @Last INT = ISNULL((SELECT MAX(FundTransferStatusID) FROM NL_FundTransferStatuses), 0)

-------------------------------------------------------------------------------

INSERT INTO NL_FundTransferStatuses (FundTransferStatusID, FundTransferStatus)
SELECT
	id, v
FROM
	#v
	LEFT JOIN NL_FundTransferStatuses s ON #v.v = s.FundTransferStatus
WHERE
	s.FundTransferStatusID IS NULL

-------------------------------------------------------------------------------

DROP TABLE #v
GO


-- NL_LoanStatuses
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'Pending') IS NULL	INSERT INTO NL_LoanStatuses (LoanStatus) VALUES('Pending');
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'Live') IS NULL	INSERT INTO NL_LoanStatuses (LoanStatus) VALUES('Live');
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'Late') IS NULL INSERT INTO NL_LoanStatuses (LoanStatus) VALUES('Late'); -- Overdue
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'PaidOff') IS NULL INSERT INTO NL_LoanStatuses (LoanStatus) VALUES('PaidOff'); -- Paid
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'WriteOff') IS NULL	INSERT INTO NL_LoanStatuses (LoanStatus) VALUES('WriteOff');
--IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'Default') IS NULL	INSERT INTO NL_LoanStatuses (LoanStatus) VALUES('Default');
-- FROM CustomerStatuses ???
--IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'DebtManagement') IS NULL INSERT INTO NL_LoanStatuses (LoanStatus) VALUES('DebtManagement');
-- IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = '1-14DaysMissed') IS NULL INSERT INTO NL_LoanStatuses (LoanStatus) VALUES('1-14DaysMissed');
-- IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = '15-30DaysMissed') IS NULL INSERT INTO NL_LoanStatuses (LoanStatus) VALUES('15-30DaysMissed');
-- IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = '31-45DaysMissed') IS NULL INSERT INTO NL_LoanStatuses (LoanStatus) VALUES('31-45DaysMissed');
-- IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = '46-90DaysMissed') IS NULL INSERT INTO NL_LoanStatuses (LoanStatus) VALUES('46-90DaysMissed');
-- IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = '60-90DaysMissed') IS NULL INSERT INTO NL_LoanStatuses (LoanStatus) VALUES('60-90DaysMissed');
-- IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = '90DaysMissed') IS NULL INSERT INTO NL_LoanStatuses (LoanStatus) VALUES('90DaysMissed');
-- IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'Legal - claim process') IS NULL INSERT INTO NL_LoanStatuses (LoanStatus) VALUES('Legal ??? claim process');
-- IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'Legal - apply for judgment') IS NULL INSERT INTO NL_LoanStatuses (LoanStatus) VALUES('Legal - apply for judgment');
-- IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'Legal: CCJ') IS NULL INSERT INTO NL_LoanStatuses (LoanStatus) VALUES('Legal: CCJ');
-- IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'Legal: bailiff') IS NULL INSERT INTO NL_LoanStatuses (LoanStatus) VALUES('Legal: bailiff');
-- IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'Legal: charging order') IS NULL INSERT INTO NL_LoanStatuses (LoanStatus) VALUES('Legal: charging order');
-- IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'Collection: Tracing') IS NULL INSERT INTO NL_LoanStatuses (LoanStatus) VALUES('Collection: Tracing');
-- IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'Collection: Site Visit') IS NULL INSERT INTO NL_LoanStatuses (LoanStatus) VALUES('Collection: Site Visit');

-- NL_LoanFeeTypes
IF( SELECT LoanFeeTypeID FROM dbo.NL_LoanFeeTypes WHERE LoanFeeType = 'SetupFee') IS NULL
	INSERT INTO NL_LoanFeeTypes (LoanFeeType, Description) VALUES('SetupFee', 'One-time fee upon loan creation, may be added or didacted from loan');
IF( SELECT LoanFeeTypeID FROM dbo.NL_LoanFeeTypes WHERE LoanFeeType = 'RolloverFee') IS NULL
	INSERT INTO NL_LoanFeeTypes (LoanFeeType, DefaultAmount, Description) VALUES('RolloverFee', 50, 'A rollover has been agreed');
IF( SELECT LoanFeeTypeID FROM dbo.NL_LoanFeeTypes WHERE LoanFeeType = 'AdminFee') IS NULL
	INSERT INTO NL_LoanFeeTypes (LoanFeeType, DefaultAmount, Description) VALUES('AdminFee', 75, 'A fee applied when no payment is received or less than (repayment interest + late payment fee)');
IF( SELECT LoanFeeTypeID FROM dbo.NL_LoanFeeTypes WHERE LoanFeeType = 'ServicingFee') IS NULL
	INSERT INTO NL_LoanFeeTypes (LoanFeeType, Description) VALUES('ServicingFee', 'Distributed through the entire loan period. On paying early - not to charge remaining part');
IF( SELECT LoanFeeTypeID FROM dbo.NL_LoanFeeTypes WHERE LoanFeeType = 'ArrangementFee') IS NULL
	INSERT INTO NL_LoanFeeTypes (LoanFeeType, Description) VALUES('ArrangementFee', 'Distributed through the payments. On paying early - all remaned amount need to be charged');
IF( SELECT LoanFeeTypeID FROM dbo.NL_LoanFeeTypes WHERE LoanFeeType = 'LatePeriod1') IS NULL
	INSERT INTO NL_LoanFeeTypes (LoanFeeType, DefaultAmount, Description) VALUES('LatePeriod1', 7, 'first collection period');
IF( SELECT LoanFeeTypeID FROM dbo.NL_LoanFeeTypes WHERE LoanFeeType = 'LatePeriod2') IS NULL
	INSERT INTO NL_LoanFeeTypes (LoanFeeType, DefaultAmount, Description) VALUES('LatePeriod2', 14, 'second collection period');
IF( SELECT LoanFeeTypeID FROM dbo.NL_LoanFeeTypes WHERE LoanFeeType = 'LatePeriod3') IS NULL
	INSERT INTO NL_LoanFeeTypes (LoanFeeType, DefaultAmount, Description) VALUES('LatePeriod3', 30, 'third collection period');
IF( SELECT LoanFeeTypeID FROM dbo.NL_LoanFeeTypes WHERE LoanFeeType = 'LatePaymentFee') IS NULL
	INSERT INTO NL_LoanFeeTypes (LoanFeeType, DefaultAmount, Description) VALUES('LatePaymentFee', 20, 'A charge when an instalment is paid after 5 UK working days of the grace period');
IF( SELECT LoanFeeTypeID FROM dbo.NL_LoanFeeTypes WHERE LoanFeeType = 'PartialPaymentFee') IS NULL
	INSERT INTO NL_LoanFeeTypes (LoanFeeType, DefaultAmount, Description) VALUES('PartialPaymentFee', 45, 'A payment has been made (more than repayment interest + late payment fee but was not made in full)');

-- NL_CashRequestOrigins
IF( SELECT CashRequestOriginID FROM dbo.NL_CashRequestOrigins WHERE CashRequestOrigin = 'FinishedWizard') IS NULL
	INSERT INTO NL_CashRequestOrigins (CashRequestOrigin) VALUES('FinishedWizard');
IF( SELECT CashRequestOriginID FROM dbo.NL_CashRequestOrigins WHERE CashRequestOrigin = 'QuickOffer') IS NULL
	INSERT INTO NL_CashRequestOrigins (CashRequestOrigin) VALUES('QuickOffer');
IF( SELECT CashRequestOriginID FROM dbo.NL_CashRequestOrigins WHERE CashRequestOrigin = 'RequestCashBtn') IS NULL
	INSERT INTO NL_CashRequestOrigins (CashRequestOrigin) VALUES('RequestCashBtn');
IF( SELECT CashRequestOriginID FROM dbo.NL_CashRequestOrigins WHERE CashRequestOrigin = 'NewCreditLineBtn') IS NULL
	INSERT INTO NL_CashRequestOrigins (CashRequestOrigin) VALUES('NewCreditLineBtn');
IF( SELECT CashRequestOriginID FROM dbo.NL_CashRequestOrigins WHERE CashRequestOrigin = 'Other') IS NULL
	INSERT INTO NL_CashRequestOrigins (CashRequestOrigin) VALUES('Other');
IF( SELECT CashRequestOriginID FROM dbo.NL_CashRequestOrigins WHERE CashRequestOrigin = 'RequalifyCustomerStrategy') IS NULL
	INSERT INTO NL_CashRequestOrigins (CashRequestOrigin) VALUES('RequalifyCustomerStrategy');

-- NL_PacnetTransactionStatuses
IF NOT EXISTS( SELECT PacnetTransactionStatusID FROM dbo.NL_PacnetTransactionStatuses WHERE TransactionStatus = 'Submited')
	INSERT INTO NL_PacnetTransactionStatuses (TransactionStatus) VALUES('Submited');
IF NOT EXISTS( SELECT PacnetTransactionStatusID FROM dbo.NL_PacnetTransactionStatuses WHERE TransactionStatus = 'ConfigError:MultipleCandidateChannels')
	INSERT INTO NL_PacnetTransactionStatuses (TransactionStatus) VALUES('ConfigError:MultipleCandidateChannels');
IF NOT EXISTS( SELECT PacnetTransactionStatusID FROM dbo.NL_PacnetTransactionStatuses WHERE TransactionStatus = 'Error') 
	INSERT INTO NL_PacnetTransactionStatuses (TransactionStatus) VALUES('Error');
IF NOT EXISTS( SELECT PacnetTransactionStatusID FROM dbo.NL_PacnetTransactionStatuses WHERE TransactionStatus = 'InProgress')
	INSERT INTO NL_PacnetTransactionStatuses (TransactionStatus) VALUES('InProgress');
IF NOT EXISTS( SELECT PacnetTransactionStatusID FROM dbo.NL_PacnetTransactionStatuses WHERE TransactionStatus = 'PaymentByCustomer')
	INSERT INTO NL_PacnetTransactionStatuses (TransactionStatus) VALUES('PaymentByCustomer');
IF NOT EXISTS( SELECT PacnetTransactionStatusID FROM dbo.NL_PacnetTransactionStatuses WHERE TransactionStatus = 'Done')
	INSERT INTO NL_PacnetTransactionStatuses (TransactionStatus) VALUES('Done');

-- add new payment methods
IF EXISTS( SELECT Id FROM dbo.LoanTransactionMethod WHERE Name = 'Write Off')
	DELETE FROM dbo.LoanTransactionMethod WHERE Name = 'Write Off';
declare @lastid INT;
IF NOT EXISTS( SELECT Id FROM dbo.LoanTransactionMethod WHERE Name = 'WriteOff')
BEGIN
SET @lastid = (SELECT Max(Id) as i FROM dbo.LoanTransactionMethod);
INSERT INTO dbo.LoanTransactionMethod (Id, Name, DisplaySort) VALUES( (@lastid +1), 'WriteOff', 0);
END;
IF NOT EXISTS( SELECT Id FROM dbo.LoanTransactionMethod WHERE Name = 'ChargeBack')
BEGIN
SET @lastid = (SELECT Max(Id) as i FROM dbo.LoanTransactionMethod);
INSERT INTO dbo.LoanTransactionMethod (Id, Name, DisplaySort) VALUES( (@lastid +1), 'ChargeBack', 0);
END;
IF NOT EXISTS( SELECT Id FROM dbo.LoanTransactionMethod WHERE Name = 'WrongPayment')
BEGIN
SET @lastid = (SELECT Max(Id) as i FROM dbo.LoanTransactionMethod);
INSERT INTO dbo.LoanTransactionMethod (Id, Name, DisplaySort) VALUES( (@lastid +1), 'WrongPayment', 0);
END;
IF NOT EXISTS( SELECT Id FROM dbo.LoanTransactionMethod WHERE Name = 'SystemRepay')
BEGIN
SET @lastid = (SELECT Max(Id) as i FROM dbo.LoanTransactionMethod);
INSERT INTO dbo.LoanTransactionMethod (Id, Name, DisplaySort) VALUES( (@lastid +1), 'SystemRepay', 0);
END;

 -- populate NL_PaymentStatuses (enum NLPaymentStatus)
IF NOT EXISTS( SELECT PaymentStatusID FROM NL_PaymentStatuses WHERE PaymentStatus = 'Pending') -- "InProgress"
	INSERT INTO NL_PaymentStatuses (PaymentStatus) VALUES('Pending');
IF NOT EXISTS( SELECT PaymentStatusID FROM NL_PaymentStatuses WHERE PaymentStatus = 'Active')
	INSERT INTO NL_PaymentStatuses (PaymentStatus) VALUES('Active');
IF NOT EXISTS( SELECT PaymentStatusID FROM NL_PaymentStatuses WHERE PaymentStatus = 'Deleted')
	INSERT INTO NL_PaymentStatuses (PaymentStatus) VALUES('Deleted');
IF NOT EXISTS( SELECT PaymentStatusID FROM NL_PaymentStatuses WHERE PaymentStatus = 'Cancelled')
	INSERT INTO NL_PaymentStatuses (PaymentStatus) VALUES('Cancelled');



-- populate NL_LoanAgreementTemplateTypes
IF NOT EXISTS( SELECT TemplateTypeID FROM dbo.NL_LoanAgreementTemplateTypes WHERE TemplateType = 'GuarantyAgreement')
	INSERT INTO NL_LoanAgreementTemplateTypes ( TemplateType) VALUES('GuarantyAgreement');
IF NOT EXISTS( SELECT TemplateTypeID FROM dbo.NL_LoanAgreementTemplateTypes WHERE TemplateType = 'PreContractAgreement')
	INSERT INTO NL_LoanAgreementTemplateTypes ( TemplateType) VALUES('PreContractAgreement');
IF NOT EXISTS( SELECT TemplateTypeID FROM dbo.NL_LoanAgreementTemplateTypes WHERE TemplateType = 'CreditActAgreement')
	INSERT INTO NL_LoanAgreementTemplateTypes ( TemplateType) VALUES('CreditActAgreement');
IF NOT EXISTS( SELECT TemplateTypeID FROM dbo.NL_LoanAgreementTemplateTypes WHERE TemplateType = 'PrivateCompanyLoanAgreement')
	INSERT INTO NL_LoanAgreementTemplateTypes ( TemplateType) VALUES('PrivateCompanyLoanAgreement');
IF NOT EXISTS( SELECT TemplateTypeID FROM dbo.NL_LoanAgreementTemplateTypes WHERE TemplateType = 'AlibabaGuarantyAgreement')
	INSERT INTO NL_LoanAgreementTemplateTypes ( TemplateType) VALUES('AlibabaGuarantyAgreement');
IF NOT EXISTS( SELECT TemplateTypeID FROM dbo.NL_LoanAgreementTemplateTypes WHERE TemplateType = 'AlibabaPreContractAgreement')
	INSERT INTO NL_LoanAgreementTemplateTypes ( TemplateType) VALUES( 'AlibabaPreContractAgreement');
IF NOT EXISTS( SELECT TemplateTypeID FROM dbo.NL_LoanAgreementTemplateTypes WHERE TemplateType = 'AlibabaCreditActAgreement')
	INSERT INTO NL_LoanAgreementTemplateTypes ( TemplateType) VALUES('AlibabaCreditActAgreement');
IF NOT EXISTS( SELECT TemplateTypeID FROM dbo.NL_LoanAgreementTemplateTypes WHERE TemplateType = 'AlibabaPrivateCompanyLoanAgreement')
	INSERT INTO NL_LoanAgreementTemplateTypes ( TemplateType) VALUES('AlibabaPrivateCompanyLoanAgreement');
IF NOT EXISTS( SELECT TemplateTypeID FROM dbo.NL_LoanAgreementTemplateTypes WHERE TemplateType = 'AlibabaCreditFacility')
	INSERT INTO NL_LoanAgreementTemplateTypes ( TemplateType) VALUES('AlibabaCreditFacility');

-- handle LoanAgreementTemplate and LoanAgreementTemplateTypes
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanAgreementTemplate') AND name = 'TemplateTypeID')
	ALTER TABLE LoanAgreementTemplate ADD TemplateTypeID INT NULL ;
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type = 'D' and name = 'DF_TemplateTypeID')
	ALTER TABLE LoanAgreementTemplate add constraint DF_TemplateTypeID DEFAULT 1 for TemplateTypeID	;
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanAgreementTemplate_NL_LoanAgreementTemplateTypes')
	ALTER TABLE LoanAgreementTemplate ADD CONSTRAINT FK_LoanAgreementTemplate_NL_LoanAgreementTemplateTypes FOREIGN KEY(TemplateTypeID) REFERENCES NL_LoanAgreementTemplateTypes (TemplateTypeID) ;
IF EXISTS(SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanAgreementTemplate') AND name = 'TemplateTypeID')
UPDATE LoanAgreementTemplate SET TemplateTypeID = TemplateType;

-- populate NL_PaypointTransactionStatuses (FROM customer)
IF NOT EXISTS( SELECT TransactionStatus FROM NL_PaypointTransactionStatuses WHERE TransactionStatus = 'Done')
	INSERT INTO NL_PaypointTransactionStatuses (TransactionStatus) VALUES('Done');
IF NOT EXISTS( SELECT TransactionStatus FROM NL_PaypointTransactionStatuses WHERE TransactionStatus = 'Error')
	INSERT INTO NL_PaypointTransactionStatuses (TransactionStatus) VALUES('Error');
IF NOT EXISTS( SELECT TransactionStatus FROM NL_PaypointTransactionStatuses WHERE TransactionStatus = 'Unknown')
	INSERT INTO NL_PaypointTransactionStatuses (TransactionStatus) VALUES('Unknown');


-- NL_RepaymentIntervalTypes
IF NOT EXISTS( SELECT RepaymentIntervalType FROM NL_RepaymentIntervalTypes WHERE RepaymentIntervalType = 30 ) -- 'Month'
	INSERT INTO NL_RepaymentIntervalTypes (RepaymentIntervalType) VALUES(30);
 IF NOT EXISTS( SELECT RepaymentIntervalType FROM NL_RepaymentIntervalTypes WHERE RepaymentIntervalType = 1) -- 'Day'
	INSERT INTO NL_RepaymentIntervalTypes (RepaymentIntervalType) VALUES(1);
 IF NOT EXISTS( SELECT RepaymentIntervalType FROM NL_RepaymentIntervalTypes WHERE RepaymentIntervalType = 7) -- 'Week'
	INSERT INTO NL_RepaymentIntervalTypes (RepaymentIntervalType) VALUES(7);
 IF NOT EXISTS( SELECT RepaymentIntervalType FROM NL_RepaymentIntervalTypes WHERE RepaymentIntervalType = 10) -- 'TenDays'
	INSERT INTO NL_RepaymentIntervalTypes (RepaymentIntervalType) VALUES(10);


--NL_LoanScheduleStatuses
IF NOT EXISTS( SELECT LoanScheduleStatusID FROM NL_LoanScheduleStatuses WHERE LoanScheduleStatus = 'StillToPay' ) -- 'StillToPay'
	INSERT INTO NL_LoanScheduleStatuses (LoanScheduleStatus, Description) VALUES('StillToPay', 'Open');
IF NOT EXISTS( SELECT LoanScheduleStatusID FROM NL_LoanScheduleStatuses WHERE LoanScheduleStatus = 'PaidOnTime' ) -- 'PaidOnTime'
	INSERT INTO NL_LoanScheduleStatuses (LoanScheduleStatus, Description) VALUES('PaidOnTime', 'Paid on time');
IF NOT EXISTS( SELECT LoanScheduleStatusID FROM NL_LoanScheduleStatuses WHERE LoanScheduleStatus = 'Late' ) -- 'Late'
	INSERT INTO NL_LoanScheduleStatuses (LoanScheduleStatus, Description) VALUES('Late', 'Late');
IF NOT EXISTS( SELECT LoanScheduleStatusID FROM NL_LoanScheduleStatuses WHERE LoanScheduleStatus = 'PaidEarly' ) -- 'PaidEarly'
	INSERT INTO NL_LoanScheduleStatuses (LoanScheduleStatus, Description) VALUES('PaidEarly', 'Paid early');
IF NOT EXISTS( SELECT LoanScheduleStatusID FROM NL_LoanScheduleStatuses WHERE LoanScheduleStatus = 'Paid' ) -- 'Paid'
	INSERT INTO NL_LoanScheduleStatuses (LoanScheduleStatus, Description) VALUES('Paid', 'Paid');
IF NOT EXISTS( SELECT LoanScheduleStatusID FROM NL_LoanScheduleStatuses WHERE LoanScheduleStatus = 'DeletedOnReschedule' ) -- 'DeletedOnReschedule'
	INSERT INTO NL_LoanScheduleStatuses (LoanScheduleStatus, Description) VALUES('DeletedOnReschedule', 'Deleted on reshedule (nothing was paid before reschedule)');

IF NOT EXISTS( SELECT LoanScheduleStatusID FROM NL_LoanScheduleStatuses WHERE LoanScheduleStatus = 'ClosedOnReschedule' ) -- 'ClosedOnReschedule'
	INSERT INTO NL_LoanScheduleStatuses (LoanScheduleStatus, Description) VALUES('ClosedOnReschedule', 'Closed on reshedule (was partially paid before reschedule)');

-- NL_OfferStatuses
--IF NOT EXISTS( SELECT OfferStatus FROM NL_OfferStatuses WHERE OfferStatus = 'Live')
--	INSERT INTO NL_OfferStatuses (OfferStatus) VALUES('Live');
--IF NOT EXISTS( SELECT OfferStatus FROM NL_OfferStatuses WHERE OfferStatus = 'Pending') -- for offers FROM "Manual" decision
--	INSERT INTO NL_OfferStatuses (OfferStatus) VALUES('Pending');


-- ConfigurationVariables Collection_Max_Cancel_Fee for roles: Collector, Underwriter, Manager
--IF NOT EXISTS( SELECT Name FROM ConfigurationVariables WHERE Name = 'Collection_Max_Cancel_Fee_Role_Collector' )
--INSERT INTO ConfigurationVariables (Name,Value,Description) VALUES ('Collection_Max_Cancel_Fee_Role_Collector', 200, 'Maximal amount of late fee cancellation for user in role Collector');
--ELSE
--UPDATE ConfigurationVariables SET Value = 200, Description= 'Maximal amount of late fee cancellation for user in role Collector' WHERE Name = 'Collection_Max_Cancel_Fee_Role_Collector';

--IF NOT EXISTS( SELECT Name FROM ConfigurationVariables WHERE Name = 'Collection_Max_Cancel_Fee_Role_Underwriter' )
--INSERT INTO ConfigurationVariables (Name,Value,Description) VALUES ('Collection_Max_Cancel_Fee_Role_Underwriter', 1000, 'Maximal amount of late fee cancellation for user in role Underwriter');
--ELSE
--UPDATE ConfigurationVariables SET Value = 1000, Description= 'Maximal amount of late fee cancellation for user in role Underwriter' WHERE Name = 'Collection_Max_Cancel_Fee_Role_Underwriter';

--IF NOT EXISTS( SELECT Name FROM ConfigurationVariables WHERE Name = 'Collection_Max_Cancel_Fee_Role_Manager' )
--INSERT INTO ConfigurationVariables (Name,Value,Description) VALUES ('Collection_Max_Cancel_Fee_Role_Manager', 5000, 'Maximal amount of late fee cancellation for user in role Manager');
--ELSE
--UPDATE ConfigurationVariables SET Value = 5000, Description= 'Maximal amount of late fee cancellation for user in role Manager' WHERE Name = 'Collection_Max_Cancel_Fee_Role_Manager';

-- ConfigurationVariables Collection_Move_To_Next_Payment_Max_Days (15 days)
--IF NOT EXISTS( SELECT Name FROM ConfigurationVariables WHERE Name = 'Collection_Move_To_Next_Payment_Max_Days' )
--BEGIN
--INSERT INTO ConfigurationVariables (Name,Value,Description) VALUES ('Collection_Move_To_Next_Payment_Max_Days', 15,
-- 'Maximal days when extra principal is being forwarded to the next payment (in case of less than Collection_Move_To_Next_Payment_Max_Principal amount)');
--END
--ELSE BEGIN
--UPDATE ConfigurationVariables SET Value = 15, Description= 'Maximal days when extra principal is being forwarded to the next payment (in case of less than Collection_Move_To_Next_Payment_Max_Principal amount)' WHERE Name = 'Collection_Move_To_Next_Payment_Max_Days';
--END

---- ConfigurationVariables Collection_Move_To_Next_Payment_Max_Principal (100 GBP)
--IF NOT EXISTS( SELECT Name FROM ConfigurationVariables WHERE Name = 'Collection_Move_To_Next_Payment_Max_Principal' )
--BEGIN
--INSERT INTO ConfigurationVariables (Name,Value,Description) VALUES ('Collection_Move_To_Next_Payment_Max_Principal', 100, 'Maximal principal amount is being forwarded to the next payment in case of less than Collection_Move_To_Next_Payment_Max_Days days late');
--END
--ELSE BEGIN
--UPDATE ConfigurationVariables SET Value = 100, Description= 'Maximal principal amount is being forwarded to the next payment in case of less than Collection_Move_To_Next_Payment_Max_Days days late' WHERE Name = 'Collection_Move_To_Next_Payment_Max_Principal';
--END

