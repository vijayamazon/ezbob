IF OBJECT_ID (N'dbo.udfSplit') IS NOT NULL
	DROP FUNCTION dbo.udfSplit
GO

CREATE FUNCTION dbo.udfSplit
(
    @String NVARCHAR(4000),
    @Delimiter NCHAR(1)
)
RETURNS TABLE
AS
RETURN
(
    WITH udfSplit(stpos,endpos)
    AS(
        SELECT 0 AS stpos, CHARINDEX(@Delimiter,@String) AS endpos
        UNION ALL
        SELECT endpos+1, CHARINDEX(@Delimiter,@String,endpos+1)
            FROM udfSplit
            WHERE endpos > 0
    )
    SELECT 'Id' = ROW_NUMBER() OVER (ORDER BY (SELECT 1)),
        'Data' = SUBSTRING(@String,stpos,COALESCE(NULLIF(endpos,0),LEN(@String)+1)-stpos)
    FROM udfSplit
)
GO

-----------------------------

IF OBJECT_ID('LoanAgreementTemplateTypes') IS NULL 
BEGIN
	CREATE TABLE [dbo].[LoanAgreementTemplateTypes](
		[TemplateTypeID] [INT] NOT NULL IDENTITY(1,1) ,
		[TemplateType] [nvarchar](50) NOT NULL,
		[TimestampCounter] rowversion NOT NULL,
	 CONSTRAINT [PK_LoanAgreementTemplateTypes] PRIMARY KEY CLUSTERED ([TemplateTypeID] ASC )
	) ;
END
GO
	
IF OBJECT_ID('NL_BlendedLoans') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_BlendedLoans](	
	[BlendedLoanID] [INT] NOT NULL,
	[LoanID] [INT] NOT NULL, 
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_NL_BlendedLoans] PRIMARY KEY CLUSTERED ([BlendedLoanID] ASC, [LoanID] ASC)
) ;
END
GO
	
IF OBJECT_ID('NL_BlendedOffers') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_BlendedOffers](	
	[BlendedOfferID] [INT] NOT NULL,
	[OfferID] [INT] NOT NULL, 
	[TimestampCounter] rowversion,
 CONSTRAINT [PK_NL_ParentOffers] PRIMARY KEY CLUSTERED ([BlendedOfferID] ASC, [OfferID] ASC)
) ;
END
GO
	
IF OBJECT_ID('NL_CashRequestOrigins') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_CashRequestOrigins](
	[CashRequestOriginID] [INT] NOT NULL IDENTITY(1,1) ,
	[CashRequestOrigin] [nchar](50) NOT NULL,
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_NL_CashRequestOrigins] PRIMARY KEY CLUSTERED ([CashRequestOriginID] ASC)
) ;
END
GO
	
IF OBJECT_ID('NL_CashRequests') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_CashRequests](
	[CashRequestID] [INT] NOT NULL IDENTITY(1,1) ,
	[CustomerID] [INT] NOT NULL,
	[RequestTime] [DATETIME] NULL,
	[CashRequestOriginID] [INT] NULL,
	[UserID] [INT] NULL,	
	[OldCashRequestID] [bigint] not NULL,
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_NL_CashRequests] PRIMARY KEY CLUSTERED ([CashRequestID] ASC)
) ;
END
GO

IF OBJECT_ID('NL_DecisionRejectReasons') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_DecisionRejectReasons](
	[DecisionRejectReasonID] [INT] IDENTITY(1,1) NOT NULL,
	[DecisionID] [INT] NULL,
	[RejectReasonID] [INT] NULL,
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_NL_DecisionRejectReasons] PRIMARY KEY CLUSTERED ([DecisionRejectReasonID] ASC) 
) ;
END
GO


	
IF OBJECT_ID('NL_Decisions') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_Decisions](
	[DecisionID] [INT] NOT NULL IDENTITY(1,1) ,
	[CashRequestID] [INT] NOT NULL,
	[UserID] [INT] NOT NULL,
	[Position] [INT] NOT NULL,
	[DecisionTime] [DATETIME] NOT NULL,
	[DecisionNameID] [INT] NOT NULL,
	[Notes] [nvarchar](max) NULL,
	[IsRepaymentPeriodSelectionAllowed] [BIT] NULL,
	[IsAmountSelectionAllowed] [BIT] NULL,
	[InterestOnlyRepaymentCount] [INT] NULL,
	[SendEmailNotification] [BIT] NULL,
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_NL_Decisions] PRIMARY KEY CLUSTERED ([DecisionID] ASC) 
) ;
END
GO
	
IF OBJECT_ID('NL_DiscountPlans') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_DiscountPlans](
	 [DiscountPlanID] [INT] NOT NULL identity (1,1),
	 [DiscountPlan] [nchar](80) NOT NULL,
	 [IsDefault] [BIT] NULL,
	 [IsActive] [BIT] NULL,
	 [TimestampCounter] rowversion NOT NULL,
   CONSTRAINT [PK_NL_DiscountPlans] PRIMARY KEY CLUSTERED ( [DiscountPlanID] ASC )
 ) ;
END
GO
	
IF OBJECT_ID('NL_DiscountPlanEntries') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_DiscountPlanEntries](
	[DiscountPlanEntryID] [INT] NOT NULL identity (1,1) ,
	[DiscountPlanID] [INT] NOT NULL,
	[PaymentOrder] [INT] NOT NULL,
	[InterestDiscount] [DECIMAL](18, 6) NOT NULL,
	[TimestampCounter] rowversion NOT NULL,
	CONSTRAINT [PK_NL_DiscountPlanEntries] PRIMARY KEY CLUSTERED ( [DiscountPlanEntryID] ASC )	
) ;
END
GO
	
IF OBJECT_ID('NL_EzbobBankAccounts') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_EzbobBankAccounts](
	[EzbobBankAccountID] [INT] NOT NULL IDENTITY(1,1) ,
	[EzbobBankAccount] [nchar](10) NOT NULL,	
	[IsDefault] [bit] null,
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_NL_EzbobBankAccounts] PRIMARY KEY CLUSTERED ([EzbobBankAccountID] ASC)
) ;
END
GO
	
IF OBJECT_ID('NL_FundTransfers') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_FundTransfers](
	[FundTransferID] [INT] NOT NULL IDENTITY(1,1) ,
	[LoanID] [INT] NOT NULL,
	[Amount] [DECIMAL] (18,6) NOT NULL,
	[TransferTime] [DATETIME] NOT NULL,
	[IsActive] [BIT] NOT NULL,
	[LoanTransactionMethodID] [INT] NOT NULL,
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_NL_FundTransfers] PRIMARY KEY CLUSTERED ([FundTransferID] ASC)
) ;
END
GO
	
IF OBJECT_ID('NL_LoanAgreements') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_LoanAgreements](
	[LoanAgreementID] [INT] NOT NULL IDENTITY(1,1) ,
	[LoanHistoryID] [INT] NOT NULL,
	[LoanAgreementTemplateID] [INT] NULL,	
	[FilePath] [nvarchar] (255) NULL,		
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_NL_LoanAgreements] PRIMARY KEY CLUSTERED ([LoanAgreementID] ASC )
);
END
GO
	
IF OBJECT_ID('NL_LoanFeePayments') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_LoanFeePayments](
	[LoanFeePaymentID] [INT] NOT NULL IDENTITY(1,1) ,
	[LoanFeeID] [INT] NOT NULL,
	[PaymentID] [INT] NOT NULL,
	[Amount] [DECIMAL](18, 6) NOT NULL,	
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_LoanFeePayments] PRIMARY KEY CLUSTERED ([LoanFeePaymentID] ASC)
) ;
END
GO
	
IF OBJECT_ID('NL_LoanFees') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_LoanFees](
	[LoanFeeID] [INT] NOT NULL IDENTITY(1,1) ,	
	[LoanID] [INT] NOT NULL,
	[LoanFeeTypeID] [INT] NOT NULL,
	[AssignedByUserID] [INT]  NULL,
	[Amount] [DECIMAL](18, 6) NOT NULL,
	[CreatedTime] [DATETIME] NOT NULL,
	[AssignTime] [DATETIME] NOT NULL,	
	[DeletedByUserID] [INT] NULL,
	[DisabledTime] [DATETIME] NULL,
	[Notes] [nvarchar](max) NULL,
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_NL_LoanFees] PRIMARY KEY CLUSTERED ([LoanFeeID] ASC)
) ;
END
GO


IF OBJECT_ID('NL_LoanFeeTypes') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_LoanFeeTypes](
	[LoanFeeTypeID] [INT] NOT NULL IDENTITY(1,1) ,
	[LoanFeeType] [nchar](50) NOT NULL,
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_NL_LoanFeeTypes] PRIMARY KEY CLUSTERED ([LoanFeeTypeID] ASC)
) ;	
END
GO
	
IF OBJECT_ID('NL_LoanStatuses') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_LoanStatuses](
	[LoanStatusID] [INT] NOT NULL IDENTITY(1,1) ,
	[LoanStatus] [nchar](80) NOT NULL,
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_NL_LoanStatuses] PRIMARY KEY CLUSTERED ([LoanStatusID] ASC)
) ;
END
GO
	
IF OBJECT_ID('NL_RepaymentIntervalTypes') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_RepaymentIntervalTypes](
	[RepaymentIntervalTypeID] [INT] NOT NULL IDENTITY(1,1) ,
	[RepaymentIntervalType] [INT] NOT NULL,
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_RepetitionLengths] PRIMARY KEY CLUSTERED ([RepaymentIntervalTypeID] ASC)
) ;
END
GO

IF OBJECT_ID('NL_OfferStatuses') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_OfferStatuses](
	[OfferStatusID] [INT] NOT NULL IDENTITY(1,1) ,
	[OfferStatus] [nchar](20) NOT NULL,
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_NL_OfferStatuses] PRIMARY KEY CLUSTERED ([OfferStatusID] ASC)
) ;
END
GO
	
IF OBJECT_ID('NL_Offers') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_Offers](
	[OfferID] [INT] NOT NULL IDENTITY(1,1) ,
	[DecisionID] [INT] NOT NULL,
	[LoanTypeID] [INT] NOT NULL, --DEFAULT 1,
	[RepaymentIntervalTypeID] [INT] NOT NULL, --DEFAULT 1,
	[LoanSourceID] [INT] NOT NULL, --DEFAULT 1,	
	[StartTime] [DATETIME] NOT NULL,
	[EndTime] [DATETIME] NOT NULL,	
	[RepaymentCount] [INT] NOT NULL,	
	[Amount] [DECIMAL](18, 6) NOT NULL,
	[MonthlyInterestRate] [DECIMAL] (18, 6) NOT NULL,
	[CreatedTime] [DATETIME] NOT NULL,
	[BrokerSetupFeePercent] [DECIMAL](18, 6) NULL,
	[Notes] [nvarchar](max) NULL,
	[SetupFeePercent] [DECIMAL](18, 6) NULL,
	[DistributedSetupFeePercent] [decimal](18, 6) NULL,
	[InterestOnlyRepaymentCount] [INT] NULL,
	[DiscountPlanID] [INT] NULL,
	[IsLoanTypeSelectionAllowed] [BIT] NOT NULL DEFAULT 0,
	[EmailSendingBanned] [BIT] NOT NULL DEFAULT 0,
	[TimestampCounter] rowversion NOT NULL,
  CONSTRAINT [PK_NL_Offers] PRIMARY KEY CLUSTERED ([OfferID] ASC)
);
END
GO
	
IF OBJECT_ID('NL_LoanLegals') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_LoanLegals](
	[LoanLegalID] [INT] NOT NULL IDENTITY(1,1),
	[OfferID] [INT] NOT NULL,
	[RepaymentPeriod] [INT] NULL,
	[Amount]  [DECIMAL](18, 6) NOT NULL,	 
	[SignatureTime] [DATETIME] NOT NULL,
	[CreditActAgreementAgreed] [BIT] NULL,
	[PreContractAgreementAgreed] [BIT] NULL,
	[PrivateCompanyLoanAgreementAgreed] [BIT] NULL,
	[GuarantyAgreementAgreed] [BIT] NULL,
	[EUAgreementAgreed] [BIT] NULL,
	[COSMEAgreementAgreed] [BIT] NULL,
	[NotInBankruptcy] [BIT] NULL,
	[SignedName] [nvarchar](128) NULL,
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_LoanLegals] PRIMARY KEY CLUSTERED (	[LoanLegalID] ASC)
) ;	
END
GO
	
IF OBJECT_ID('NL_LoanLienLinks') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_LoanLienLinks](	
	[LoanLienLinkID] [INT] IDENTITY(1,1) NOT NULL,
	[LoanID] [INT] NOT NULL,
	[LoanLienID] [INT] NOT NULL,
	[Amount] [DECIMAL](18, 6) NOT NULL,
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_NL_LoanLienLinks] PRIMARY KEY CLUSTERED ([LoanLienLinkID] ASC)
) ;
END
GO
	
IF OBJECT_ID('NL_LoanSchedules') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_LoanSchedules](
	[LoanScheduleID] [INT] NOT NULL IDENTITY(1,1) ,
	[LoanHistoryID] [INT] NOT NULL,
	[Position] [INT] NOT NULL,
	[PlannedDate] [DATETIME] NOT NULL,
	[ClosedTime] [DATETIME] NULL,
	[Principal] [DECIMAL] (18,6) NOT NULL,
	[InterestRate] [DECIMAL](18, 6) NOT NULL,	
	--[Fee] [DECIMAL](18, 6)  NULL,	
	[TimestampCounter] rowversion NOT NULL,	
 CONSTRAINT [PK_NL_LoanSchedules] PRIMARY KEY CLUSTERED ([LoanScheduleID] ASC)
) ;
END
GO
	
IF OBJECT_ID('NL_Loans') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_Loans](
	[LoanID] [INT] NOT NULL IDENTITY(1,1) ,
	[OfferID] [INT] NOT NULL,
	[LoanTypeID] [INT] NOT NULL,
	[RepaymentIntervalTypeID] [INT] NOT NULL,
	[LoanStatusID] [INT] NOT NULL,
	[EzbobBankAccountID] [INT] NULL,
	[LoanSourceID] [INT] NOT NULL,
	[Position] [INT] NOT NULL,
	[InitialLoanAmount] [DECIMAL] (18,6) NULL,
	[CreationTime] [DATETIME] NOT NULL,
	[IssuedTime] [DATETIME] NOT NULL,
	[RepaymentCount] [INT] NOT NULL,	
	[Refnum] [nvarchar](50) NOT NULL,
	[DateClosed] [DATETIME] NULL,	
	[InterestRate] [DECIMAL](18, 6) NOT NULL,
	[InterestOnlyRepaymentCount] [INT] NULL,
	[OldLoanID] [INT] NOT NULL,
	[TimestampCounter] rowversion NOT NULL,	
 CONSTRAINT [PK_NL_Loans] PRIMARY KEY CLUSTERED ([LoanID] ASC)
) ;
END
GO
	
IF OBJECT_ID('NL_LoanSchedulePayments') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_LoanSchedulePayments](	
	[LoanSchedulePaymentID] [INT] NOT NULL,
	[LoanScheduleID] [INT] NOT NULL,
	[PaymentID] [INT] NOT NULL,
	[PrincipalPaid] [DECIMAL](18, 6) NOT NULL,
	[InterestPaid] [DECIMAL](18, 6) NOT NULL,
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_NL_LoanSchedulePayments] PRIMARY KEY CLUSTERED ([LoanSchedulePaymentID] ASC)
) ;
END
GO
	
IF OBJECT_ID('NL_LoanHistory') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_LoanHistory](
	[LoanHistoryID] [INT] NOT NULL IDENTITY(1,1) ,
	[LoanID] [INT] NOT NULL,
	[UserID] [INT] NULL,
	[LoanLegalID] [INT] NULL,
	[Amount] [DECIMAL](18, 6) NOT NULL,
	[RepaymentCount] [INT] NOT NULL,
	[InterestRate] [DECIMAL](18, 6) NULL,	
	[EventTime] [DATETIME] NOT NULL,
	[Description] [nvarchar](max) NOT NULL,	
	[AgreementModel] [nvarchar](max) NULL,
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_NL_LoanHistory] PRIMARY KEY CLUSTERED ([LoanHistoryID] ASC)
) ;
END
GO
	
IF OBJECT_ID('NL_LoanRollovers') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_LoanRollovers](
	[LoanRolloverID] [INT] NOT NULL IDENTITY(1,1) ,
	[LoanHistoryID] [INT] NOT NULL,
	[CreatedByUserID] [INT] NOT NULL,
	[DeletedByUserID] [INT] NULL,	
	[LoanFeeID] [INT] NULL,
	[FeeAmount] [DECIMAL] (18,6) NOT NULL,	
	[CreationTime] [DATETIME] NOT NULL,	
	[ExpirationTime] [DATETIME] NOT NULL,
	[CustomerActionTime] [DATETIME] NULL,
	[IsAccepted] [BIT] NULL,
	[DeletionTime] [DATETIME] NULL,	
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_NL_Rollovers] PRIMARY KEY CLUSTERED ([LoanRolloverID] ASC)
) ;
END
GO

	
IF OBJECT_ID('NL_PacnetTransactions') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_PacnetTransactions](
	[PacnetTransactionID] [INT] NOT NULL IDENTITY(1,1) ,
	[FundTransferID] [INT] NOT NULL,	
	[TransactionTime] [DATETIME] NOT NULL,
	[Amount] [DECIMAL](18, 6) NOT NULL,
	[Notes] [nvarchar](max) NULL,
	[PacnetTransactionStatusID] [INT] NOT NULL,
	[StatusUpdatedTime] [DATETIME] NOT NULL,
	[TrackingNumber] [nvarchar](100) NOT NULL,
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_NL_PacnetTransactions] PRIMARY KEY CLUSTERED (	[PacnetTransactionID] ASC)
) ;
END
GO

IF OBJECT_ID('NL_PacnetTransactionStatuses') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_PacnetTransactionStatuses](
	[PacnetTransactionStatusID] [INT] NOT NULL IDENTITY(1,1) ,
	[TransactionStatus] [nvarchar](100) NOT NULL,
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_NL_PacnetTransactionStatuses] PRIMARY KEY CLUSTERED ([PacnetTransactionStatusID] ASC)
) ;
END
GO

--IF OBJECT_ID('NL_PaymentStatuses') IS NULL 
--BEGIN	
--CREATE TABLE [dbo].NL_PaymentStatuses(
--	[PaymentStatusID] [INT] NOT NULL IDENTITY(1,1) ,
--	[PaymentStatus] [nvarchar] (60) NOT NULL,
--	[TimestampCounter] rowversion NOT NULL,		
-- CONSTRAINT [PK_NL_PaymentStatuses] PRIMARY KEY CLUSTERED ([PaymentStatusID] ASC)
--) ;
--END
--GO
	
IF OBJECT_ID('NL_Payments') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_Payments](
	[PaymentID] [INT] NOT NULL IDENTITY(1,1) ,
	[PaymentMethodID] [INT] NOT NULL,		
	[PaymentTime] [DATETIME] NOT NULL, 
	[Amount] [DECIMAL](18, 6) NOT NULL,
	[IsActive] [BIT] NOT NULL,
	[CreationTime] [DATETIME] NOT NULL DEFAULT GETUTCDATE(), --real insert DATETIME
	[CreatedByUserID] [INT] NULL,
	[DeletionTime] [DATETIME] NULL,
	[DeletedByUserID] [INT] NULL,
	[Notes] [nvarchar](max) NULL,
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_NL_LoanTransactions] PRIMARY KEY CLUSTERED ([PaymentID] ASC)
) ;
END
GO

ALTER TABLE [dbo].[NL_Payments] ALTER COLUMN [Amount] [DECIMAL](18, 6) NOT NULL;
	
IF OBJECT_ID('NL_PaypointTransactions') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_PaypointTransactions](
	[PaypointTransactionID] [INT] NOT NULL  IDENTITY(1,1) ,
	[PaymentID] [INT] NOT NULL,
	[TransactionTime] [DATETIME] NOT NULL, 
	[Amount] [DECIMAL](18, 6) NOT NULL,
	[Notes] [nvarchar](max) NULL,
	[PaypointTransactionStatusID] [INT] NOT NULL,
	[PaypointUniqID] [nvarchar](100) NOT NULL,
	[PaypointCardID] [INT] NOT NULL,
	[IP] [nvarchar](32) NULL,
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_NL_PaypointTransactions] PRIMARY KEY CLUSTERED ([PaypointTransactionID] ASC )
) ;
END
GO

IF OBJECT_ID('NL_PaypointTransactionStatuses') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_PaypointTransactionStatuses](
	[PaypointTransactionStatusID] [INT] NOT NULL IDENTITY(1,1) ,
	[TransactionStatus] [nvarchar](100) NOT NULL,
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_NL_PaypointTransactionStatuses] PRIMARY KEY CLUSTERED (	[PaypointTransactionStatusID] ASC)
) ;
END ;

IF OBJECT_ID('WriteOffReasons') IS NULL 
BEGIN	
CREATE TABLE [dbo].[WriteOffReasons](
	[WriteOffReasonID] [INT] NOT NULL IDENTITY(1,1) ,
	[ReasonName] [nvarchar](100) NOT NULL,
	[PaymentID] [INT] NOT NULL ,
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_WriteOffReasons] PRIMARY KEY CLUSTERED ( [WriteOffReasonID] ASC)
) ;
END ;


IF OBJECT_ID('NL_LoanStates') IS NULL BEGIN	
CREATE TABLE [dbo].[NL_LoanStates](
	[LoanStateID] [INT] NOT NULL IDENTITY(1,1) ,
	[InsertDate]  [DATETIME] NOT NULL DEFAULT GETUTCDATE(),
	[LoanID] [INT] NOT NULL , 
	[NumberOfPayments] [INT] NOT NULL ,	
	[OutstandingPrincipal] [DECIMAL](18, 6) NOT NULL,
	[OutstandingInterest] [DECIMAL](18, 6) NOT NULL,
	[OutstandingFee] [DECIMAL](18, 6) NOT NULL,	
	[PaidPrincipal] [DECIMAL](18, 6) NOT NULL,
	[PaidInterest] [DECIMAL](18, 6) NOT NULL,
	[PaidFee] [DECIMAL](18, 6) NOT NULL,	
	[LateDays] int NOT NULL ,		
	[LatePrincipal] [DECIMAL](18, 6) NULL,
	[LateInterest] [DECIMAL](18, 6) NULL,
	[WrittenOffPrincipal] [DECIMAL](18, 6)  NULL,
	[WrittenOffInterest] [DECIMAL](18, 6) NULL,
	[WrittentOffFees] [DECIMAL](18, 6) NULL,	
	[Notes] [ntext] NULL,
	[TimestampCounter] rowversion NOT NULL,
 CONSTRAINT [PK_NL_LoanState] PRIMARY KEY CLUSTERED ( [LoanStateID] ASC)
) ;
END ;

IF OBJECT_ID ('dbo.NL_LoanOptions') IS NULL
BEGIN 
CREATE TABLE dbo.NL_LoanOptions
	(
	  LoanOptionsID           INT IDENTITY NOT NULL
	, LoanID                  INT NOT NULL
	, StopAutoChargeDate      DATETIME
	, StopAutoLateFeesDate    DATETIME
	, AutoInterest            BIT
	, StopAutoInterestDate    DATETIME
	, ReductionFee            BIT
	, LatePaymentNotification BIT
	, CaisAccountStatus       NVARCHAR (50)
	, ManualCaisFlag          NVARCHAR (20)
	, EmailSendingAllowed     BIT DEFAULT (1) NOT NULL
	, MailSendingAllowed      BIT DEFAULT (1) NOT NULL
	, SmsSendingAllowed       BIT DEFAULT (1) NOT NULL
	, UserID                  INT
	, InsertDate              DATETIME DEFAULT (GETUTCDATE()) NOT NULL
	, IsActive                BIT NOT NULL DEFAULT(0)
	, Notes                   NTEXT
	, TimestampCounter rowversion NOT NULL
	, CONSTRAINT PK_NL_LoanOptions PRIMARY KEY (LoanOptionsID)
  	, CONSTRAINT FK_NL_LoanOptions_NL_Loans FOREIGN KEY (LoanID) REFERENCES dbo.NL_Loans (LoanID)
	, CONSTRAINT FK_NL_LoanOptions_Security_User FOREIGN KEY (UserID) REFERENCES dbo.Security_User (UserId)
	)
END 
GO


--ALTER TABLE [dbo].[NL_Offers] ALTER COLUMN LoanSourceID CONSTRAINT  DEFAULT 1;
--ALTER TABLE [dbo].[NL_Offers] ALTER COLUMN LoanTypeID SET DEFAULT 1;
--ALTER TABLE [dbo].[NL_Offers] ALTER COLUMN RepaymentIntervalTypeID SET DEFAULT 1;

ALTER TABLE NL_LoanFees ALTER COLUMN [AssignedByUserID] [INT]  NULL;
ALTER TABLE NL_LoanFees ALTER COLUMN [Amount] [DECIMAL](18, 6)  NOT NULL;
ALTER TABLE NL_LoanFees ALTER COLUMN [CreatedTime] [DATETIME]  NOT NULL;
ALTER TABLE NL_LoanFees ALTER COLUMN [AssignTime] [DATETIME]  NOT NULL;
ALTER TABLE NL_LoanFees ALTER COLUMN [DeletedByUserID] [INT]   NULL;

ALTER TABLE NL_Loans ALTER COLUMN  [Refnum] [nvarchar](50);
ALTER TABLE NL_LoanHistory ALTER COLUMN  [LoanID] [INT] NOT NULL;

ALTER TABLE [NL_PacnetTransactions] ALTER COLUMN  [Amount] [DECIMAL](18, 6) NOT NULL;
ALTER TABLE [NL_PacnetTransactions] ALTER COLUMN  [TransactionTime] [DATETIME] NOT NULL;
ALTER TABLE [NL_PacnetTransactions] ALTER COLUMN  [TrackingNumber] [nvarchar](100) NOT NULL;
ALTER TABLE [NL_PacnetTransactions] ALTER COLUMN  [PacnetTransactionStatusID] [INT] NOT NULL;

ALTER TABLE [dbo].[NL_Payments] ALTER COLUMN [Amount] [DECIMAL](18, 6) NOT NULL;

ALTER TABLE [dbo].[NL_PaypointTransactions] ALTER COLUMN [PaymentID] [INT] NOT NULL;
ALTER TABLE [dbo].[NL_PaypointTransactions] ALTER COLUMN [TransactionTime]  [DATETIME] NOT NULL;



IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanBrokerCommission') AND name = 'NLLoanID')
	ALTER TABLE [dbo].[LoanBrokerCommission] ADD [NLLoanID] [INT] NULL ; 
GO
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('CollectionLog') AND name = 'LoanHistoryID')
	ALTER TABLE [dbo].[CollectionLog] ADD LoanHistoryID [INT] NULL ; 
GO
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('CollectionLog') AND name = 'Comments')
	ALTER TABLE [dbo].[CollectionLog] ADD Comments [ntext] null ; 
GO
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('DecisionTrail') AND name = 'NLDecisionID')
	ALTER TABLE [dbo].[DecisionTrail] ADD NLDecisionID [INT] NULL ; 
GO
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('Esignatures') AND name = 'DecisionID')
	ALTER TABLE [dbo].[Esignatures] ADD DecisionID [INT] NULL ; 
GO
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('DecisionTrail') AND name = 'NLCashRequestID')
	ALTER TABLE [dbo].[DecisionTrail] ADD [NLCashRequestID] [INT] NULL ; 
GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_DecisionRejectReasons_NL_Decision') BEGIN
ALTER TABLE [dbo].[NL_DecisionRejectReasons] ADD CONSTRAINT [FK_NL_DecisionRejectReasons_NL_Decision] FOREIGN KEY([DecisionID]) REFERENCES [dbo].[NL_Decisions] ([DecisionID])
END ;
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_DecisionRejectReasons_RejectReasons') BEGIN
ALTER TABLE [dbo].[NL_DecisionRejectReasons] ADD CONSTRAINT [FK_NL_DecisionRejectReasons_RejectReasons] FOREIGN KEY([RejectReasonID]) REFERENCES [dbo].[RejectReason] ([Id])
END ;


IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Decisions_NL_CashRequests') BEGIN
ALTER TABLE [dbo].[NL_Decisions] ADD CONSTRAINT FK_NL_Decisions_NL_CashRequests FOREIGN KEY([CashRequestID]) REFERENCES [dbo].[NL_CashRequests] ([CashRequestID]);
END ;
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Decisions_DecisionNames') BEGIN
ALTER TABLE [dbo].[NL_Decisions] ADD CONSTRAINT FK_NL_Decisions_DecisionNames FOREIGN KEY([DecisionNameID]) REFERENCES [dbo].[Decisions] ([DecisionID]);
END ;
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Decisions_Users') BEGIN
ALTER TABLE [dbo].[NL_Decisions] ADD CONSTRAINT FK_NL_Decisions_Users FOREIGN KEY([UserID]) REFERENCES [dbo].[Security_User] ([UserID]);
END ;

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_BlendedLoans_NL_Loans') BEGIN
ALTER TABLE [dbo].[NL_BlendedLoans] ADD CONSTRAINT FK_NL_BlendedLoans_NL_Loans FOREIGN KEY([LoanID]) REFERENCES [dbo].[NL_Loans] ([LoanID]) ;
END ;

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_BlendedOffers_NL_Offers') BEGIN
ALTER TABLE [dbo].[NL_BlendedOffers] ADD CONSTRAINT [FK_NL_BlendedOffers_NL_Offers] FOREIGN KEY([OfferID]) REFERENCES [dbo].[NL_Offers] ([OfferID]) ;
END ;

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_FundTransfers_Loans') BEGIN
ALTER TABLE [dbo].[NL_FundTransfers] ADD CONSTRAINT [FK_FundTransfers_Loans] FOREIGN KEY([LoanID]) REFERENCES [dbo].[NL_Loans] ([LoanID]) ;
END ;
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_FundTransfers_LoanTransactionMethod') BEGIN
ALTER TABLE [dbo].[NL_FundTransfers] ADD CONSTRAINT [FK_FundTransfers_LoanTransactionMethod] FOREIGN KEY([LoanTransactionMethodID]) REFERENCES [dbo].[LoanTransactionMethod] ([Id]) ;
END ;

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanAgreements_LoanHistory') BEGIN
ALTER TABLE [dbo].[NL_LoanAgreements] ADD CONSTRAINT [FK_NL_LoanAgreements_LoanHistory] FOREIGN KEY([LoanHistoryID]) REFERENCES [dbo].[NL_LoanHistory] ([LoanHistoryID]) ;
END ;

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanAgreements_LoanAgreementTemplate') BEGIN
ALTER TABLE [dbo].[NL_LoanAgreements] ADD CONSTRAINT [FK_NL_LoanAgreements_LoanAgreementTemplate] FOREIGN KEY([LoanAgreementTemplateID]) REFERENCES [dbo].[LoanAgreementTemplate] ([Id]) ;
END
GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanFeePayments_LoanFees') BEGIN
ALTER TABLE [dbo].[NL_LoanFeePayments] ADD CONSTRAINT [FK_LoanFeePayments_LoanFees] FOREIGN KEY([LoanFeeID]) REFERENCES [dbo].[NL_LoanFees] ([LoanFeeID]) ;
END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanFeePayments_LoanTransactions') BEGIN
ALTER TABLE [dbo].[NL_LoanFeePayments] ADD CONSTRAINT [FK_LoanFeePayments_LoanTransactions] FOREIGN KEY([PaymentID]) REFERENCES [dbo].[NL_Payments] ([PaymentID]) ;
END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanFeePayments_LoanTransactions')  BEGIN
ALTER TABLE [dbo].[NL_LoanFeePayments] ADD CONSTRAINT [FK_LoanFeePayments_LoanTransactions] FOREIGN KEY([PaymentID]) REFERENCES [dbo].[NL_Payments] ([PaymentID]) ;
END
GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanFees_LoanFeeTypes') BEGIN
ALTER TABLE [dbo].[NL_LoanFees] ADD CONSTRAINT [FK_LoanFees_LoanFeeTypes] FOREIGN KEY([LoanFeeTypeID]) REFERENCES [dbo].[NL_LoanFeeTypes] ([LoanFeeTypeID]) ;
END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanFees_NL_Loan') BEGIN
ALTER TABLE [dbo].[NL_LoanFees] ADD  CONSTRAINT [FK_NL_LoanFees_NL_Loan] FOREIGN KEY([LoanID]) REFERENCES [dbo].[NL_Loans] ([LoanID]) ;
END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanFees_AssignUser') BEGIN
ALTER TABLE [dbo].[NL_LoanFees] ADD  CONSTRAINT [FK_NL_LoanFees_AssignUser] FOREIGN KEY([AssignedByUserID]) REFERENCES [dbo].[Security_User] ([UserId]) ;
END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanFees_DeleteUser') BEGIN
ALTER TABLE [dbo].[NL_LoanFees] ADD  CONSTRAINT [FK_NL_LoanFees_DeleteUser] FOREIGN KEY([DeletedByUserID]) REFERENCES [dbo].[Security_User] ([UserId]) ;
END
GO

GO
 IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanScheduleHistory_Security_User') BEGIN
 ALTER TABLE [dbo].[NL_LoanHistory] ADD  CONSTRAINT [FK_NL_LoanScheduleHistory_Security_User] FOREIGN KEY([UserID]) REFERENCES [dbo].[Security_User] ([UserID]) ;
END
GO
 IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanHistory_NL_Loans') BEGIN
 ALTER TABLE [dbo].[NL_LoanHistory] ADD  CONSTRAINT [FK_NL_LoanHistory_NL_Loans] FOREIGN KEY([LoanID]) REFERENCES [dbo].[NL_Loans] ([LoanID]) ;
END
GO
 IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanHistory_NL_LoanLegals') BEGIN
 ALTER TABLE [dbo].[NL_LoanHistory] ADD  CONSTRAINT FK_NL_LoanHistory_NL_LoanLegals FOREIGN KEY([LoanLegalID]) REFERENCES [dbo].[NL_LoanLegals] ([LoanLegalID]) ;
END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_CollectionLog_NL_LoanHistory') BEGIN
 ALTER TABLE [dbo].[CollectionLog] ADD CONSTRAINT FK_CollectionLog_NL_LoanHistory FOREIGN KEY([LoanHistoryID]) REFERENCES [dbo].[NL_LoanHistory] ([LoanHistoryID]) ;
END

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanSchedules_LoanHistory') BEGIN
ALTER TABLE [dbo].[NL_LoanSchedules] ADD CONSTRAINT [FK_NL_LoanSchedules_LoanHistory] FOREIGN KEY([LoanHistoryID]) REFERENCES [dbo].[NL_LoanHistory] ([LoanHistoryID]) ;
END
GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanLegals_NL_Offers') BEGIN
ALTER TABLE [dbo].[NL_LoanLegals] ADD CONSTRAINT [FK_NL_LoanLegals_NL_Offers] FOREIGN KEY([OfferID]) REFERENCES [dbo].[NL_Offers] ([OfferID]) ;
END
GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanLienLink_LoanLien') BEGIN
ALTER TABLE [dbo].[NL_LoanLienLinks] ADD CONSTRAINT [FK_NL_LoanLienLink_LoanLien] FOREIGN KEY([LoanLienID]) REFERENCES [dbo].[LoanLien] ([Id]) ;
END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanLienLink_NL_Loans') BEGIN
ALTER TABLE [dbo].[NL_LoanLienLinks] ADD CONSTRAINT [FK_NL_LoanLienLink_NL_Loans] FOREIGN KEY([LoanID]) REFERENCES [dbo].[NL_Loans] ([LoanID]) ;
END
GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanOptions_NL_Loans') BEGIN
ALTER TABLE [dbo].[NL_LoanOptions] ADD CONSTRAINT [FK_NL_LoanOptions_NL_Loans] FOREIGN KEY([LoanID]) REFERENCES [dbo].[NL_Loans] ([LoanID]) ;
END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanOptions_Security_User') BEGIN
ALTER TABLE [dbo].[NL_LoanOptions] ADD CONSTRAINT [FK_NL_LoanOptions_Security_User] FOREIGN KEY([UserID]) REFERENCES [dbo].[Security_User] ([UserId]) ;
END
GO
 
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanRollovers_Security_User') BEGIN
 ALTER TABLE [dbo].[NL_LoanRollovers] ADD CONSTRAINT [FK_NL_LoanRollovers_Security_User] FOREIGN KEY([CreatedByUserID]) REFERENCES [dbo].[Security_User] ([UserID]) ;
 END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanRollovers_Security_User_Delete') BEGIN
 ALTER TABLE [dbo].[NL_LoanRollovers] ADD CONSTRAINT [FK_NL_LoanRollovers_Security_User_Delete] FOREIGN KEY([DeletedByUserID]) REFERENCES [dbo].[Security_User] ([UserID]) ;
 END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanRollovers_NL_LoanFees') BEGIN
 ALTER TABLE [dbo].[NL_LoanRollovers] ADD CONSTRAINT [FK_NL_LoanRollovers_NL_LoanFees] FOREIGN KEY([LoanFeeID]) REFERENCES [dbo].[NL_LoanFees] ([LoanFeeID]) ;
 END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanRollovers_NL_LoanHistory') BEGIN
 ALTER TABLE [dbo].[NL_LoanRollovers]  ADD  CONSTRAINT [FK_NL_LoanRollovers_NL_LoanHistory] FOREIGN KEY([LoanHistoryID]) REFERENCES [dbo].[NL_LoanHistory] ([LoanHistoryID]) ;
END
GO

 IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Loans_LoanSource') BEGIN
 ALTER TABLE [dbo].[NL_Loans]   ADD  CONSTRAINT [FK_NL_Loans_LoanSource] FOREIGN KEY([LoanSourceID]) REFERENCES [dbo].[LoanSource] ([LoanSourceID]) ;
 END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_Loans_LoanStatuses') BEGIN
 ALTER TABLE [dbo].[NL_Loans]   ADD  CONSTRAINT [FK_Loans_LoanStatuses] FOREIGN KEY([LoanStatusID]) REFERENCES [dbo].[NL_LoanStatuses] ([LoanStatusID]) ;
 END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_Loans_EzbobBankAccounts') BEGIN
 ALTER TABLE [dbo].[NL_Loans]   ADD  CONSTRAINT [FK_Loans_EzbobBankAccounts] FOREIGN KEY([EzbobBankAccountID]) REFERENCES [dbo].[NL_EzbobBankAccounts] ([EzbobBankAccountID]) ;
 END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Loans_LoanType') BEGIN
 ALTER TABLE [dbo].[NL_Loans]   ADD  CONSTRAINT [FK_NL_Loans_LoanType] FOREIGN KEY([LoanTypeID]) REFERENCES [dbo].[LoanType] ([Id]) ;
 END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Loans_NL_Offers') BEGIN
 ALTER TABLE [dbo].[NL_Loans]   ADD  CONSTRAINT [FK_NL_Loans_NL_Offers] FOREIGN KEY([OfferID]) REFERENCES [dbo].[NL_Offers] ([OfferID]) ;
 END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Loans_NL_RepaymentIntervalTypes') BEGIN
 ALTER TABLE [dbo].[NL_Loans]   ADD  CONSTRAINT [FK_NL_Loans_NL_RepaymentIntervalTypes] FOREIGN KEY([RepaymentIntervalTypeID]) REFERENCES [dbo].[NL_RepaymentIntervalTypes] ([RepaymentIntervalTypeID]) ;
END
GO


 IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanScheduleTransaction_LoanSchedules') BEGIN
 ALTER TABLE [dbo].[NL_LoanSchedulePayments]  ADD  CONSTRAINT [FK_LoanScheduleTransaction_LoanSchedules] FOREIGN KEY([LoanScheduleID]) REFERENCES [dbo].[NL_LoanSchedules] ([LoanScheduleID]) ;
END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanScheduleTransaction_LoanTransactions') BEGIN
 ALTER TABLE [dbo].[NL_LoanSchedulePayments] ADD CONSTRAINT [FK_LoanScheduleTransaction_LoanTransactions] FOREIGN KEY([PaymentID]) REFERENCES [dbo].[NL_Payments] ([PaymentID]) ;
END
GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Offers_LoanType') BEGIN
 ALTER TABLE [dbo].[NL_Offers]  ADD  CONSTRAINT [FK_NL_Offers_LoanType] FOREIGN KEY([LoanTypeID]) REFERENCES [dbo].[LoanType] ([Id]) ;
 END
GO
 IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Offers_NL_Decisions') BEGIN
 ALTER TABLE [dbo].[NL_Offers]  ADD  CONSTRAINT [FK_NL_Offers_NL_Decisions] FOREIGN KEY([DecisionID]) REFERENCES [dbo].[NL_Decisions] ([DecisionID]) ;
 END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Offers_LoanSource') BEGIN
 ALTER TABLE [dbo].[NL_Offers]  ADD  CONSTRAINT [FK_NL_Offers_LoanSource] FOREIGN KEY([LoanSourceID]) REFERENCES [dbo].[LoanSource] ([LoanSourceID]) ;
 END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Offers_NL_DiscountPlans') BEGIN
 ALTER TABLE [dbo].[NL_Offers]  ADD  CONSTRAINT [FK_NL_Offers_NL_DiscountPlans] FOREIGN KEY([DiscountPlanID]) REFERENCES [dbo].[NL_DiscountPlans] ([DiscountPlanID]) ;
END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Offers_NL_RepaymentIntervalType') BEGIN
 ALTER TABLE [dbo].[NL_Offers]  ADD  CONSTRAINT [FK_NL_Offers_NL_RepaymentIntervalType] FOREIGN KEY([RepaymentIntervalTypeID]) REFERENCES [dbo].[NL_RepaymentIntervalTypes] ([RepaymentIntervalTypeID]) ;
END
GO
 IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_Esignatures_NL_Decisions') BEGIN
 ALTER TABLE [dbo].[Esignatures]  ADD CONSTRAINT [FK_Esignatures_NL_Decisions] FOREIGN KEY([DecisionID]) REFERENCES [dbo].[NL_Decisions] ([DecisionID]) ;
 END
GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_PacnetTransactions_NL_PacnetTransactionStatuses') BEGIN
 ALTER TABLE [dbo].[NL_PacnetTransactions] ADD CONSTRAINT [FK_NL_PacnetTransactions_NL_PacnetTransactionStatuses] FOREIGN KEY([PacnetTransactionStatusID]) REFERENCES [dbo].[NL_PacnetTransactionStatuses] ([PacnetTransactionStatusID]) ;
END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_PacnetTransactions_FundTransfers') BEGIN
 ALTER TABLE [dbo].[NL_PacnetTransactions] ADD CONSTRAINT [FK_PacnetTransactions_FundTransfers] FOREIGN KEY([FundTransferID]) REFERENCES [dbo].[NL_FundTransfers] ([FundTransferID]) ;
END
GO


IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_Payments_CreatedBySecurity_User') BEGIN
 ALTER TABLE [dbo].[NL_Payments]   ADD  CONSTRAINT [FK_Payments_CreatedBySecurity_User] FOREIGN KEY([CreatedByUserID]) REFERENCES [dbo].[Security_User] ([UserID]) ;
END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Payments_DeletedBySecurity_User') BEGIN
 ALTER TABLE [dbo].[NL_Payments]   ADD  CONSTRAINT [FK_NL_Payments_DeletedBySecurity_User] FOREIGN KEY([DeletedByUserID]) REFERENCES [dbo].[Security_User] ([UserID]) ;
END
GO
--IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Payments_NL_PaymentStatuses') BEGIN
-- ALTER TABLE [dbo].[NL_Payments] ADD CONSTRAINT FK_NL_Payments_NL_PaymentStatuses FOREIGN KEY([PaymentStatusID]) REFERENCES [dbo].[NL_PaymentStatuses] ([PaymentStatusID]) ;
--END
--GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_PaypointTransactions_NL_PaypointTransactionStatuses') BEGIN
 ALTER TABLE [dbo].[NL_PaypointTransactions] ADD CONSTRAINT [FK_NL_PaypointTransactions_NL_PaypointTransactionStatuses] FOREIGN KEY([PaypointTransactionStatusID]) REFERENCES [dbo].[NL_PaypointTransactionStatuses] ([PaypointTransactionStatusID]) ;
END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_PaypointTransactions_Payments') BEGIN
 ALTER TABLE [dbo].[NL_PaypointTransactions] ADD CONSTRAINT [FK_PaypointTransactions_Payments] FOREIGN KEY([PaymentID]) REFERENCES [dbo].[NL_Payments] ([PaymentID]) ;
END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'PRIMARY_KEY_CONSTRAINT' and name = 'PK_PaypointCard') BEGIN
 ALTER TABLE [dbo].[PaypointCard] ADD CONSTRAINT [PK_PaypointCard] PRIMARY KEY([Id])  ;
END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_PaypointTransactions_PayPointCard') BEGIN
 ALTER TABLE [dbo].[NL_PaypointTransactions] ADD CONSTRAINT [FK_NL_PaypointTransactions_PayPointCard] FOREIGN KEY([PaypointCardID]) REFERENCES [dbo].[PayPointCard] ([Id]) ;
END
GO


IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_CashRequests_CashRequestOrigins') BEGIN
 ALTER TABLE [dbo].[NL_CashRequests] ADD CONSTRAINT [FK_NL_CashRequests_CashRequestOrigins] FOREIGN KEY([CashRequestOriginID]) REFERENCES [dbo].[NL_CashRequestOrigins] ([CashRequestOriginID]);
END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_CashRequests_Customers') BEGIN
 ALTER TABLE [dbo].[NL_CashRequests] ADD CONSTRAINT [FK_NL_CashRequests_Customers] FOREIGN KEY([CustomerID]) REFERENCES [dbo].[Customer] ([Id]);
END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_CashRequests_Users') BEGIN
 ALTER TABLE [dbo].[NL_CashRequests] ADD CONSTRAINT [FK_NL_CashRequests_Users] FOREIGN KEY([UserID]) REFERENCES [dbo].[Security_User] ([UserID]);
END
GO


IF(SELECT cl.OBJECT_ID FROM sys.all_objects ob inner join sys.all_columns cl on ob.OBJECT_ID = cl.OBJECT_ID  and ob.name = 'MedalCalculationsAV' and cl.name = 'CashRequestID') IS NULL BEGIN
 ALTER TABLE [dbo].[MedalCalculationsAV] ADD [CashRequestID] int DEFAULT null;
END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_MedalCalculationsAV_NL_CashRequests') BEGIN
 ALTER TABLE [dbo].[MedalCalculationsAV]  ADD CONSTRAINT [FK_MedalCalculationsAV_NL_CashRequests] FOREIGN KEY([CashRequestID]) REFERENCES [dbo].[NL_CashRequests] ([CashRequestID]);
END
GO

IF(SELECT cl.OBJECT_ID FROM sys.all_objects ob inner join sys.all_columns cl on ob.OBJECT_ID = cl.OBJECT_ID  and ob.name = 'MedalCalculations' and cl.name = 'CashRequestID') IS NULL BEGIN
 ALTER TABLE [dbo].[MedalCalculations] ADD [CashRequestID] int DEFAULT null;
END
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_MedalCalculations_NL_CashRequests') BEGIN
 ALTER TABLE [dbo].[MedalCalculations] ADD CONSTRAINT [FK_MedalCalculations_NL_CashRequests] FOREIGN KEY([CashRequestID]) REFERENCES [dbo].[NL_CashRequests] ([CashRequestID]);
END
GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_DiscountPlanEntries_NL_DiscountPlans') BEGIN
 ALTER TABLE [dbo].[NL_DiscountPlanEntries] ADD CONSTRAINT [FK_NL_DiscountPlanEntries_NL_DiscountPlans] FOREIGN KEY([DiscountPlanID]) REFERENCES [dbo].[NL_DiscountPlans] ([DiscountPlanID]);
END
GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_DecisionTrail_NL_CashRequests') BEGIN
 ALTER TABLE [dbo].[DecisionTrail] ADD CONSTRAINT [FK_DecisionTrail_NL_CashRequests] FOREIGN KEY([NLCashRequestID]) REFERENCES [dbo].[NL_CashRequests] ([CashRequestID]);
END
GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanBrokerCommission_NL_Loan') BEGIN
 ALTER TABLE [dbo].[LoanBrokerCommission] ADD CONSTRAINT [FK_LoanBrokerCommission_NL_Loan] FOREIGN KEY([NLLoanID]) REFERENCES [dbo].[NL_Loans] ([LoanID]) ; 
END
GO

-- IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_WriteOffReasons_Payments') BEGIN
 -- ALTER TABLE [dbo].[WriteOffReasons] ADD CONSTRAINT [FK_WriteOffReasons_Payments] FOREIGN KEY([PaymentID]) REFERENCES [dbo].[NL_Payments] ([PaymentID]) ;
-- END
-- GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanStates_NL_Loans') BEGIN
 ALTER TABLE [dbo].[NL_LoanStates] ADD CONSTRAINT [FK_NL_LoanStates_NL_Loans] FOREIGN KEY([LoanID]) REFERENCES [dbo].[NL_Loans] ([LoanID]) ;
END
GO

-- repair Medals table
ALTER TABLE [dbo].[Medals] ALTER COLUMN [Id] int NOT NULL ;
IF(SELECT OBJECT_ID FROM sys.all_objects WHERE name = 'PK_Medals') IS NULL
BEGIN
	ALTER TABLE [dbo].[Medals] add CONSTRAINT [PK_Medals] PRIMARY KEY CLUSTERED ([Id] ASC );
END;
IF( SELECT Id FROM dbo.Medals WHERE Medal = 'NoClassification') IS NULL
BEGIN
	INSERT INTO [dbo].[Medals] (Medal) VALUES('NoClassification');
END;

-- add column and FK to MedalCalculations/MedalCalculationsAV to Medals(names)
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('MedalCalculations') AND name = 'MedalNameID') 
	BEGIN
		ALTER TABLE [dbo].[MedalCalculations] add [MedalNameID] [INT] NOT NULL DEFAULT 1;		
	END;
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('MedalCalculationsAV') AND name = 'MedalNameID')
	BEGIN
		ALTER TABLE [dbo].[MedalCalculationsAV] add [MedalNameID] [INT] NOT NULL DEFAULT 1;
	END;
	
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_MedalCalculations_Medals') BEGIN
 ALTER TABLE [dbo].[MedalCalculations] ADD CONSTRAINT [FK_MedalCalculations_Medals] FOREIGN KEY([MedalNameID]) REFERENCES [dbo].[Medals] ([Id]);	
END
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_MedalCalculationsAV_Medals') BEGIN
 ALTER TABLE [dbo].[MedalCalculationsAV] ADD CONSTRAINT [FK_MedalCalculationsAV_Medals] FOREIGN KEY([MedalNameID])REFERENCES [dbo].[Medals] ([Id]);
END
GO

-- populate [MedalNameID] with appropriate VALUES
UPDATE [dbo].[MedalCalculations] SET [dbo].[MedalCalculations].[MedalNameID] = [dbo].[Medals].Id FROM [dbo].[MedalCalculations] INNER JOIN [dbo].[Medals] ON [dbo].[MedalCalculations].Medal = [dbo].[Medals].Medal;
UPDATE [dbo].[MedalCalculationsAV] SET [dbo].[MedalCalculationsAV].[MedalNameID] = [dbo].[Medals].Id FROM [dbo].[MedalCalculationsAV] INNER JOIN [dbo].[Medals] ON [dbo].[MedalCalculationsAV].Medal = [dbo].[Medals].Medal;

-- add FK_NL_Payments_LoanTransactionMethod 
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Payments_LoanTransactionMethod') BEGIN
 ALTER TABLE [dbo].[NL_Payments] ADD CONSTRAINT [FK_NL_Payments_LoanTransactionMethod] FOREIGN KEY([PaymentMethodID]) REFERENCES [dbo].[LoanTransactionMethod] ([Id]);
END
GO

-- NL_DiscountPlans/NL_DiscountPlanEntries migration

-- SortOrder field defines order of entries (ORDER BY SortOrder ASC, DiscountPlanEntryID DESC) and not number of repayment period. Entry is always related to repetition period. I.e. if the same plan is applied to monthly repaid loan and to weekly repaid loan and an entry in the second position says "-10%" that means that in the former case customer receives 10% discound for the second month while in the latter case customer receives 10% discount for the second week.
-- Value "0.1" in InterestRateDelta means "10%", value "-0.05" means "-5%".
if OBJECT_ID('tempdb..#discountplanTemp') is not null BEGIN drop table #discountplanTemp;END 
	 
	DECLARE @Id INT
	DECLARE @NL_Id INT
	DECLARE @Name NVARCHAR(50)
	DECLARE @ValuesStr NVARCHAR(100)
	DECLARE @IsDefault bit
	Declare @ForbiddenForReuse bit
	Declare @Percent float	

	SELECT Id, Name, ValuesStr, IsDefault, ForbiddenForReuse INTO #discountplanTemp FROM dbo.DiscountPlan; 	
	SET @Percent = 100.00;

    WHILE EXISTS (SELECT * FROM #discountplanTemp)
     BEGIN 
        SELECT TOP 1 @Name = Name, @Id = Id, @VALUESStr = VALUESStr, @IsDefault = IsDefault, @ForbiddenForReuse = ForbiddenForReuse FROM #discountplanTemp;		
		if (SELECT DiscountPlan FROM dbo.[NL_DiscountPlans] WHERE DiscountPlan = @Name ) is null	
		BEGIN	
			INSERT INTO [dbo].[NL_DiscountPlans] ([DiscountPlan], [IsDefault], [IsActive]) VALUES (ltrim(rtrim(@Name)), @IsDefault, @ForbiddenForReuse);
		END
		SELECT @NL_Id = DiscountPlanID FROM [dbo].[NL_DiscountPlans] WHERE [DiscountPlan] = @Name;	
		if @NL_Id is not null 
		BEGIN					
			if (SELECT COUNT([DiscountPlanEntryID]) FROM [dbo].[NL_DiscountPlanEntries] WHERE [DiscountPlanID] = @NL_Id group by [DiscountPlanID]) is null			
			BEGIN
				INSERT INTO [dbo].[NL_DiscountPlanEntries] ([DiscountPlanID], [PaymentOrder], [InterestDiscount]) 
					SELECT 
					@NL_Id, 
					splitted.Id, 
					--case 
					--when splitted.Data = 0 then 0 
					--else 
					CAST(splitted.Data AS float)/ @Percent 
					--end					 
					FROM dbo.udfSplit(@VALUESStr, ',') as splitted;
  			END
		END
		delete FROM #discountplanTemp WHERE ID = @Id;		 
     END
	 drop table #discountplanTemp;
-- ### discount plan/entries migration	 


-- NL_LoanStatuses
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'Live') IS NULL BEGIN	INSERT INTO [dbo].[NL_LoanStatuses] (LoanStatus) VALUES('Live');	END;
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'Late') IS NULL BEGIN INSERT INTO [dbo].[NL_LoanStatuses] (LoanStatus) VALUES('Late');		END;
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'PaidOff') IS NULL BEGIN INSERT INTO [dbo].[NL_LoanStatuses] (LoanStatus) VALUES('PaidOff');	END;
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'Pending') IS NULL BEGIN	INSERT INTO [dbo].[NL_LoanStatuses] (LoanStatus) VALUES('Pending');		END;
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'Default') IS NULL BEGIN	INSERT INTO [dbo].[NL_LoanStatuses] (LoanStatus) VALUES('Default');		END;
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'WriteOff') IS NULL BEGIN	INSERT INTO [dbo].[NL_LoanStatuses] (LoanStatus) VALUES('WriteOff');	END;
-- FROM [dbo].[CustomerStatuses] ???
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'DebtManagement') IS NULL BEGIN INSERT INTO [dbo].[NL_LoanStatuses] (LoanStatus) VALUES('DebtManagement'); END;
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = '1-14DaysMissed') IS NULL BEGIN INSERT INTO [dbo].[NL_LoanStatuses] (LoanStatus) VALUES('1-14DaysMissed'); END;
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = '15-30DaysMissed') IS NULL BEGIN INSERT INTO [dbo].[NL_LoanStatuses] (LoanStatus) VALUES('15-30DaysMissed'); END;
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = '31-45DaysMissed') IS NULL BEGIN INSERT INTO [dbo].[NL_LoanStatuses] (LoanStatus) VALUES('31-45DaysMissed'); END;
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = '46-90DaysMissed') IS NULL BEGIN INSERT INTO [dbo].[NL_LoanStatuses] (LoanStatus) VALUES('46-90DaysMissed'); END;
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = '60-90DaysMissed') IS NULL BEGIN INSERT INTO [dbo].[NL_LoanStatuses] (LoanStatus) VALUES('60-90DaysMissed'); END;
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = '90DaysMissed') IS NULL BEGIN INSERT INTO [dbo].[NL_LoanStatuses] (LoanStatus) VALUES('90DaysMissed'); END;
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'Legal - claim process') IS NULL BEGIN INSERT INTO [dbo].[NL_LoanStatuses] (LoanStatus) VALUES('Legal ??? claim process'); END;
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'Legal - apply for judgment') IS NULL BEGIN INSERT INTO [dbo].[NL_LoanStatuses] (LoanStatus) VALUES('Legal - apply for judgment'); END;
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'Legal: CCJ') IS NULL BEGIN INSERT INTO [dbo].[NL_LoanStatuses] (LoanStatus) VALUES('Legal: CCJ'); END;
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'Legal: bailiff') IS NULL BEGIN INSERT INTO [dbo].[NL_LoanStatuses] (LoanStatus) VALUES('Legal: bailiff'); END;
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'Legal: charging order') IS NULL BEGIN INSERT INTO [dbo].[NL_LoanStatuses] (LoanStatus) VALUES('Legal: charging order'); END;
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'Collection: Tracing') IS NULL BEGIN INSERT INTO [dbo].[NL_LoanStatuses] (LoanStatus) VALUES('Collection: Tracing'); END;
IF( SELECT LoanStatusID FROM dbo.NL_LoanStatuses WHERE LoanStatus = 'Collection: Site Visit') IS NULL BEGIN INSERT INTO [dbo].[NL_LoanStatuses] (LoanStatus) VALUES('Collection: Site Visit'); END;




-- NL_LoanFeeTypes
IF( SELECT LoanFeeTypeID FROM dbo.NL_LoanFeeTypes WHERE LoanFeeType = 'SetupFee') IS NULL BEGIN
	INSERT INTO [dbo].[NL_LoanFeeTypes] (LoanFeeType) VALUES('SetupFee');
END;
IF( SELECT LoanFeeTypeID FROM dbo.NL_LoanFeeTypes WHERE LoanFeeType = 'RolloverFee') IS NULL BEGIN
	INSERT INTO [dbo].[NL_LoanFeeTypes] (LoanFeeType) VALUES('RolloverFee');
END;
IF( SELECT LoanFeeTypeID FROM dbo.NL_LoanFeeTypes WHERE LoanFeeType = 'AdminFee') IS NULL BEGIN
	INSERT INTO [dbo].[NL_LoanFeeTypes] (LoanFeeType) VALUES('AdminFee');
END;  

-- NL_CashRequestOrigins
IF( SELECT CashRequestOriginID FROM dbo.NL_CashRequestOrigins WHERE CashRequestOrigin = 'FinishedWizard') IS NULL BEGIN
	INSERT INTO [dbo].[NL_CashRequestOrigins] (CashRequestOrigin) VALUES('FinishedWizard');
END;
IF( SELECT CashRequestOriginID FROM dbo.NL_CashRequestOrigins WHERE CashRequestOrigin = 'QuickOffer') IS NULL BEGIN
	INSERT INTO [dbo].[NL_CashRequestOrigins] (CashRequestOrigin) VALUES('QuickOffer');
END;
IF( SELECT CashRequestOriginID FROM dbo.NL_CashRequestOrigins WHERE CashRequestOrigin = 'RequestCashBtn') IS NULL BEGIN
	INSERT INTO [dbo].[NL_CashRequestOrigins] (CashRequestOrigin) VALUES('RequestCashBtn');
END;
IF( SELECT CashRequestOriginID FROM dbo.NL_CashRequestOrigins WHERE CashRequestOrigin = 'NewCreditLineBtn') IS NULL BEGIN
	INSERT INTO [dbo].[NL_CashRequestOrigins] (CashRequestOrigin) VALUES('NewCreditLineBtn');
END;
IF( SELECT CashRequestOriginID FROM dbo.NL_CashRequestOrigins WHERE CashRequestOrigin = 'Other') IS NULL BEGIN
	INSERT INTO [dbo].[NL_CashRequestOrigins] (CashRequestOrigin) VALUES('Other');
END;
IF( SELECT CashRequestOriginID FROM dbo.NL_CashRequestOrigins WHERE CashRequestOrigin = 'RequalifyCustomerStrategy') IS NULL BEGIN
	INSERT INTO [dbo].[NL_CashRequestOrigins] (CashRequestOrigin) VALUES('RequalifyCustomerStrategy');
END;

-- NL_PacnetTransactionStatuses
IF NOT EXISTS( SELECT PacnetTransactionStatusID FROM dbo.NL_PacnetTransactionStatuses WHERE [TransactionStatus] = 'Submited') BEGIN
	INSERT INTO [dbo].[NL_PacnetTransactionStatuses] ([TransactionStatus]) VALUES('Submited');
END;
IF NOT EXISTS( SELECT PacnetTransactionStatusID FROM dbo.NL_PacnetTransactionStatuses WHERE [TransactionStatus] = 'ConfigError:MultipleCandidateChannels') BEGIN
	INSERT INTO [dbo].[NL_PacnetTransactionStatuses] ([TransactionStatus]) VALUES('ConfigError:MultipleCandidateChannels');
END;
IF NOT EXISTS( SELECT PacnetTransactionStatusID FROM dbo.NL_PacnetTransactionStatuses WHERE [TransactionStatus] = 'Error')  BEGIN
	INSERT INTO [dbo].[NL_PacnetTransactionStatuses] ([TransactionStatus]) VALUES('Error');
END;
IF NOT EXISTS( SELECT PacnetTransactionStatusID FROM dbo.NL_PacnetTransactionStatuses WHERE [TransactionStatus] = 'InProgress') BEGIN
	INSERT INTO [dbo].[NL_PacnetTransactionStatuses] ([TransactionStatus]) VALUES('InProgress');
END;
IF NOT EXISTS( SELECT PacnetTransactionStatusID FROM dbo.NL_PacnetTransactionStatuses WHERE [TransactionStatus] = 'PaymentByCustomer') BEGIN
	INSERT INTO [dbo].[NL_PacnetTransactionStatuses] ([TransactionStatus]) VALUES('PaymentByCustomer');
END;
IF NOT EXISTS( SELECT PacnetTransactionStatusID FROM dbo.NL_PacnetTransactionStatuses WHERE [TransactionStatus] = 'Done') BEGIN
	INSERT INTO [dbo].[NL_PacnetTransactionStatuses] ([TransactionStatus]) VALUES('Done');
END;

-- add new payment method
IF NOT EXISTS( SELECT Id FROM dbo.LoanTransactionMethod WHERE [Name] = 'Write Off') BEGIN
declare @lastid int;
SET @lastid = (SELECT Max(Id) as i FROM dbo.LoanTransactionMethod);
INSERT INTO dbo.LoanTransactionMethod (Id, Name, DisplaySort) VALUES( (@lastid +1), 'Write Off', 0);	
END;

-- populate LoanAgreementTemplateTypes
IF NOT EXISTS( SELECT [TemplateTypeID] FROM dbo.LoanAgreementTemplateTypes WHERE TemplateType = 'GuarantyAgreement') BEGIN
	INSERT INTO [dbo].[LoanAgreementTemplateTypes] ( [TemplateType]) VALUES('GuarantyAgreement');
END;
IF NOT EXISTS( SELECT [TemplateTypeID] FROM dbo.LoanAgreementTemplateTypes WHERE [TemplateType] = 'PreContractAgreement') BEGIN
	INSERT INTO [dbo].[LoanAgreementTemplateTypes] ( [TemplateType]) VALUES('PreContractAgreement');
END;
IF NOT EXISTS( SELECT [TemplateTypeID] FROM dbo.LoanAgreementTemplateTypes WHERE [TemplateType] = 'CreditActAgreement') BEGIN
	INSERT INTO [dbo].[LoanAgreementTemplateTypes] ( [TemplateType]) VALUES('CreditActAgreement');
END;
IF NOT EXISTS( SELECT [TemplateTypeID] FROM dbo.LoanAgreementTemplateTypes WHERE [TemplateType] = 'PrivateCompanyLoanAgreement') BEGIN
	INSERT INTO [dbo].[LoanAgreementTemplateTypes] ( [TemplateType]) VALUES('PrivateCompanyLoanAgreement');
END;
IF NOT EXISTS( SELECT [TemplateTypeID] FROM dbo.LoanAgreementTemplateTypes WHERE [TemplateType] = 'AlibabaGuarantyAgreement') BEGIN
	INSERT INTO [dbo].[LoanAgreementTemplateTypes] ( [TemplateType]) VALUES('AlibabaGuarantyAgreement');
END;
IF NOT EXISTS( SELECT [TemplateTypeID] FROM dbo.LoanAgreementTemplateTypes WHERE [TemplateType] = 'AlibabaPreContractAgreement') BEGIN
	INSERT INTO [dbo].[LoanAgreementTemplateTypes] ( [TemplateType]) VALUES( 'AlibabaPreContractAgreement');
END;
IF NOT EXISTS( SELECT [TemplateTypeID] FROM dbo.LoanAgreementTemplateTypes WHERE [TemplateType] = 'AlibabaCreditActAgreement') BEGIN
	INSERT INTO [dbo].[LoanAgreementTemplateTypes] ( [TemplateType]) VALUES('AlibabaCreditActAgreement');
END;
IF NOT EXISTS( SELECT [TemplateTypeID] FROM dbo.LoanAgreementTemplateTypes WHERE [TemplateType] = 'AlibabaPrivateCompanyLoanAgreement') BEGIN
	INSERT INTO [dbo].[LoanAgreementTemplateTypes] ( [TemplateType]) VALUES('AlibabaPrivateCompanyLoanAgreement');
END;
IF NOT EXISTS( SELECT [TemplateTypeID] FROM dbo.LoanAgreementTemplateTypes WHERE [TemplateType] = 'AlibabaCreditFacility') BEGIN
	INSERT INTO [dbo].[LoanAgreementTemplateTypes] ( [TemplateType]) VALUES('AlibabaCreditFacility');
END;

-- handle LoanAgreementTemplate and LoanAgreementTemplateTypes 
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanAgreementTemplate') AND name = 'TemplateTypeID')
	ALTER TABLE [dbo].[LoanAgreementTemplate] ADD TemplateTypeID [INT] NULL ;	
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type = 'D' and name = 'DF_TemplateTypeID')
	ALTER TABLE [dbo].[LoanAgreementTemplate] add constraint DF_TemplateTypeID DEFAULT 1 for TemplateTypeID	
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanAgreementTemplate_LoanAgreementTemplateTypes') BEGIN
	ALTER TABLE [dbo].[LoanAgreementTemplate] ADD CONSTRAINT FK_LoanAgreementTemplate_LoanAgreementTemplateTypes FOREIGN KEY(TemplateTypeID) REFERENCES [dbo].[LoanAgreementTemplateTypes] ([TemplateTypeID]) ;
END ;
IF EXISTS(SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanAgreementTemplate') AND name = 'TemplateTypeID') BEGIN
UPDATE [dbo].[LoanAgreementTemplate] SET [TemplateTypeID] = TemplateType;
END;

-- populate [NL_PaymentStatuses] (enum PaymentStatus)
--IF NOT EXISTS( SELECT [PaymentStatusID] FROM [dbo].[NL_PaymentStatuses] WHERE PaymentStatus = 'PaidOnTime') BEGIN
--	INSERT INTO [dbo].[NL_PaymentStatuses] ([PaymentStatus]) VALUES('PaidOnTime');
--END;
--IF NOT EXISTS( SELECT [PaymentStatusID] FROM [dbo].[NL_PaymentStatuses] WHERE PaymentStatus = 'Late') BEGIN
--	INSERT INTO [dbo].[NL_PaymentStatuses] ([PaymentStatus]) VALUES('Late');
--END;
--IF NOT EXISTS( SELECT [PaymentStatusID] FROM [dbo].[NL_PaymentStatuses] WHERE PaymentStatus = 'Early') BEGIN
--	INSERT INTO [dbo].[NL_PaymentStatuses] ([PaymentStatus]) VALUES('Early');
--END;
--IF NOT EXISTS( SELECT [PaymentStatusID] FROM [dbo].[NL_PaymentStatuses] WHERE PaymentStatus = 'ChargeBack') BEGIN
--	INSERT INTO [dbo].[NL_PaymentStatuses] ([PaymentStatus]) VALUES('ChargeBack');
--END;
--IF NOT EXISTS( SELECT [PaymentStatusID] FROM [dbo].[NL_PaymentStatuses] WHERE PaymentStatus = 'WrongPayment') BEGIN
--	INSERT INTO [dbo].[NL_PaymentStatuses] ([PaymentStatus]) VALUES('WrongPayment');
--END;
--IF NOT EXISTS( SELECT [PaymentStatusID] FROM [dbo].[NL_PaymentStatuses] WHERE PaymentStatus = 'WriteOff') BEGIN
--	INSERT INTO [dbo].[NL_PaymentStatuses] ([PaymentStatus]) VALUES('WriteOff');
--END;
--IF NOT EXISTS( SELECT [PaymentStatusID] FROM [dbo].[NL_PaymentStatuses] WHERE PaymentStatus = 'system-repay') BEGIN
--	INSERT INTO [dbo].[NL_PaymentStatuses] ([PaymentStatus]) VALUES('system-repay');
--END;

-- populate NL_PaypointTransactionStatuses (FROM customer)
IF NOT EXISTS( SELECT TransactionStatus FROM [dbo].[NL_PaypointTransactionStatuses] WHERE TransactionStatus = 'Done') BEGIN
	INSERT INTO [dbo].[NL_PaypointTransactionStatuses] (TransactionStatus) VALUES('Done');
END;
IF NOT EXISTS( SELECT TransactionStatus FROM [dbo].[NL_PaypointTransactionStatuses] WHERE TransactionStatus = 'Error') BEGIN
	INSERT INTO [dbo].[NL_PaypointTransactionStatuses] (TransactionStatus) VALUES('Error');
END;
IF NOT EXISTS( SELECT TransactionStatus FROM [dbo].[NL_PaypointTransactionStatuses] WHERE TransactionStatus = 'Unknown') BEGIN
	INSERT INTO [dbo].[NL_PaypointTransactionStatuses] (TransactionStatus) VALUES('Unknown');
END;



-- populate NL_PacnetTransactionStatuses (to customer)
IF NOT EXISTS( SELECT TransactionStatus FROM [dbo].[NL_PacnetTransactionStatuses] WHERE TransactionStatus = 'InProgress') BEGIN
	INSERT INTO [dbo].[NL_PacnetTransactionStatuses] (TransactionStatus) VALUES('InProgress');
END;
IF NOT EXISTS( SELECT TransactionStatus FROM [dbo].[NL_PacnetTransactionStatuses] WHERE TransactionStatus = 'Done') BEGIN
	INSERT INTO [dbo].[NL_PacnetTransactionStatuses] (TransactionStatus) VALUES('Done');
END;
IF NOT EXISTS( SELECT TransactionStatus FROM [dbo].[NL_PacnetTransactionStatuses] WHERE TransactionStatus = 'Error') BEGIN
	INSERT INTO [dbo].[NL_PacnetTransactionStatuses] (TransactionStatus) VALUES('Error');
END;     
IF NOT EXISTS( SELECT TransactionStatus FROM [dbo].[NL_PacnetTransactionStatuses] WHERE TransactionStatus = 'Unknown') BEGIN
	INSERT INTO [dbo].[NL_PacnetTransactionStatuses] (TransactionStatus) VALUES('Unknown');
END;  

-- NL_RepaymentIntervalTypes
IF NOT EXISTS( SELECT RepaymentIntervalType FROM [dbo].[NL_RepaymentIntervalTypes] WHERE RepaymentIntervalType = 30 ) BEGIN -- 'Month'
	INSERT INTO [dbo].[NL_RepaymentIntervalTypes] (RepaymentIntervalType) VALUES(30);
END;     
 IF NOT EXISTS( SELECT RepaymentIntervalType FROM [dbo].[NL_RepaymentIntervalTypes] WHERE RepaymentIntervalType = 1) BEGIN -- 'Day'
	INSERT INTO [dbo].[NL_RepaymentIntervalTypes] (RepaymentIntervalType) VALUES(1);
END;  
 IF NOT EXISTS( SELECT RepaymentIntervalType FROM [dbo].[NL_RepaymentIntervalTypes] WHERE RepaymentIntervalType = 7) BEGIN -- 'Week'
	INSERT INTO [dbo].[NL_RepaymentIntervalTypes] (RepaymentIntervalType) VALUES(7);
END; 
 IF NOT EXISTS( SELECT RepaymentIntervalType FROM [dbo].[NL_RepaymentIntervalTypes] WHERE RepaymentIntervalType = 10) BEGIN -- 'TenDays'
	INSERT INTO [dbo].[NL_RepaymentIntervalTypes] (RepaymentIntervalType) VALUES(10);
END; 		 

-- NL_OfferStatuses
--IF NOT EXISTS( SELECT OfferStatus FROM [dbo].[NL_OfferStatuses] WHERE OfferStatus = 'Live') BEGIN 
--	INSERT INTO [dbo].[NL_OfferStatuses] (OfferStatus) VALUES('Live');
--END;
--IF NOT EXISTS( SELECT OfferStatus FROM [dbo].[NL_OfferStatuses] WHERE OfferStatus = 'Pending') BEGIN -- for offers FROM "Manual" decision
--	INSERT INTO [dbo].[NL_OfferStatuses] (OfferStatus) VALUES('Pending');
--END;

 	
-- [ConfigurationVariables] Collection_Max_Cancel_Fee for roles: Collector, Underwriter, Manager
--IF NOT EXISTS( SELECT [Name] FROM [dbo].[ConfigurationVariables] WHERE [Name] = 'Collection_Max_Cancel_Fee_Role_Collector' )
--INSERT INTO [dbo].[ConfigurationVariables] ([Name] ,[Value] ,[Description]) VALUES ('Collection_Max_Cancel_Fee_Role_Collector', 200, 'Maximal amount of late fee cancellation for user in role Collector');
--ELSE 
--UPDATE [dbo].[ConfigurationVariables] SET [Value] = 200, [Description]= 'Maximal amount of late fee cancellation for user in role Collector' WHERE [Name] = 'Collection_Max_Cancel_Fee_Role_Collector';
--IF NOT EXISTS( SELECT [Name] FROM [dbo].[ConfigurationVariables] WHERE [Name] = 'Collection_Max_Cancel_Fee_Role_Underwriter' )
--INSERT INTO [dbo].[ConfigurationVariables] ([Name] ,[Value] ,[Description]) VALUES ('Collection_Max_Cancel_Fee_Role_Underwriter', 1000, 'Maximal amount of late fee cancellation for user in role Underwriter');
--ELSE 
--UPDATE [dbo].[ConfigurationVariables] SET [Value] = 1000, [Description]= 'Maximal amount of late fee cancellation for user in role Underwriter' WHERE [Name] = 'Collection_Max_Cancel_Fee_Role_Underwriter';
--IF NOT EXISTS( SELECT [Name] FROM [dbo].[ConfigurationVariables] WHERE [Name] = 'Collection_Max_Cancel_Fee_Role_Manager' )
--INSERT INTO [dbo].[ConfigurationVariables] ([Name] ,[Value] ,[Description]) VALUES ('Collection_Max_Cancel_Fee_Role_Manager', 5000, 'Maximal amount of late fee cancellation for user in role Manager');
--ELSE 
--UPDATE [dbo].[ConfigurationVariables] SET [Value] = 5000, [Description]= 'Maximal amount of late fee cancellation for user in role Manager' WHERE [Name] = 'Collection_Max_Cancel_Fee_Role_Manager';

-- [ConfigurationVariables] Collection_Move_To_Next_Payment_Max_Days (15 days)
--IF NOT EXISTS( SELECT [Name] FROM [dbo].[ConfigurationVariables] WHERE [Name] = 'Collection_Move_To_Next_Payment_Max_Days' ) BEGIN
--INSERT INTO [dbo].[ConfigurationVariables] ([Name] ,[Value] ,[Description]) VALUES ('Collection_Move_To_Next_Payment_Max_Days', 15,
-- 'Maximal days when extra principal is being forwarded to the next payment (in case of less than Collection_Move_To_Next_Payment_Max_Principal amount)');
--END
--ELSE BEGIN
--UPDATE [dbo].[ConfigurationVariables] SET [Value] = 15, [Description]= 'Maximal days when extra principal is being forwarded to the next payment (in case of less than Collection_Move_To_Next_Payment_Max_Principal amount)' WHERE [Name] = 'Collection_Move_To_Next_Payment_Max_Days';
--END
---- [ConfigurationVariables] Collection_Move_To_Next_Payment_Max_Principal (100 GBP)
--IF NOT EXISTS( SELECT [Name] FROM [dbo].[ConfigurationVariables] WHERE [Name] = 'Collection_Move_To_Next_Payment_Max_Principal' ) BEGIN
--INSERT INTO [dbo].[ConfigurationVariables] ([Name] ,[Value] ,[Description]) VALUES ('Collection_Move_To_Next_Payment_Max_Principal', 100, 'Maximal principal amount is being forwarded to the next payment in case of less than Collection_Move_To_Next_Payment_Max_Days days late');
--END
--ELSE BEGIN
--UPDATE [dbo].[ConfigurationVariables] SET [Value] = 100, [Description]= 'Maximal principal amount is being forwarded to the next payment in case of less than Collection_Move_To_Next_Payment_Max_Days days late' WHERE [Name] = 'Collection_Move_To_Next_Payment_Max_Principal';
--END

