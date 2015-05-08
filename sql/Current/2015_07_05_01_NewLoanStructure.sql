
IF object_id('LoanAgreementTemplateTypes') IS NULL 
BEGIN
	CREATE TABLE [dbo].[LoanAgreementTemplateTypes](
		[TemplateTypeID] [int] NOT NULL IDENTITY(1,1) ,
		[TemplateType] [nvarchar](50) NOT NULL,
	 CONSTRAINT [PK_LoanAgreementTemplateTypes] PRIMARY KEY CLUSTERED ([TemplateTypeID] ASC )
	) ;
END
GO
	
IF object_id('NL_BlendedLoans') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_BlendedLoans](	
	[BlendedLoanID] [int] NOT NULL,
	[LoanID] [int] NOT NULL, 
 CONSTRAINT [PK_NL_BlendedLoans] PRIMARY KEY CLUSTERED ([BlendedLoanID] ASC, [LoanID] ASC)
) ;
END
GO
	
IF object_id('NL_BlendedOffers') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_BlendedOffers](	
	[BlendedOfferID] [int] NOT NULL,
	[OfferID] [int] NOT NULL, 
 CONSTRAINT [PK_NL_ParentOffers] PRIMARY KEY CLUSTERED ([BlendedOfferID] ASC, [OfferID] ASC)
) ;
END
GO
	
IF object_id('NL_CashRequestOrigins') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_CashRequestOrigins](
	[CashRequestOriginID] [int] NOT NULL IDENTITY(1,1) ,
	[CashRequestOrigin] [nchar](50) NOT NULL,
 CONSTRAINT [PK_NL_CashRequestOrigins] PRIMARY KEY CLUSTERED ([CashRequestOriginID] ASC)
) ;
END
GO
	
IF object_id('NL_CashRequests') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_CashRequests](
	[CashRequestID] [int] NOT NULL IDENTITY(1,1) ,
	[CustomerID] [int] NOT NULL,
	[RequestTime] [datetime] NULL,
	[CashRequestOriginID] [int] NULL,
	[UserID] [int] NULL,	
	[OldCashRequestID] [bigint] not NULL,
 CONSTRAINT [PK_NL_CashRequests] PRIMARY KEY CLUSTERED ([CashRequestID] ASC)
) ;
END
GO
	
IF object_id('NL_Decisions') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_Decisions](
	[DecisionID] [int] NOT NULL IDENTITY(1,1) ,
	[CashRequestID] [int] NOT NULL,
	[UserID] [int] NOT NULL,
	[Position] [int] NOT NULL,
	[DecisionTime] [datetime] NOT NULL,
	[DecisionNameID] [int] NOT NULL,
	[Notes] [nvarchar](max) NULL,
	[IsRepaymentPeriodSelectionAllowed] [bit] NULL,
	[IsAmountSelectionAllowed] [bit] NULL,
	[InterestOnlyRepaymentCount] [int] NULL,
	[SendEmailNotification] [bit] NULL,
 CONSTRAINT [PK_NL_Decisions] PRIMARY KEY CLUSTERED ([DecisionID] ASC) 
) ;
END
GO
	
IF object_id('NL_DiscountPlans') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_DiscountPlans](
	 [DiscountPlanID] [int] NOT NULL identity (1,1),
	 [DiscountPlan] [nchar](80) NOT NULL,
	 [IsDefault] [bit] NULL,
	 [ForbiddenForReuse] [bit] NULL,
   CONSTRAINT [PK_NL_DiscountPlans] PRIMARY KEY CLUSTERED ( [DiscountPlanID] ASC )
 ) ;
END
GO
	
IF object_id('NL_DiscountPlanEntries') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_DiscountPlanEntries](
	[DiscountPlanEntryID] [int] NOT NULL identity (1,1) ,
	[DiscountPlanID] [int] NOT NULL,
	[PaymentOrder] [int] NOT NULL,
	[InterestDiscount] [decimal](18, 6) NOT NULL,
	CONSTRAINT [PK_NL_DiscountPlanEntries] PRIMARY KEY CLUSTERED ( [DiscountPlanEntryID] ASC )	
) ;
END
GO
	
IF object_id('NL_EzbobBankAccounts') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_EzbobBankAccounts](
	[EzbobBankAccountID] [int] NOT NULL IDENTITY(1,1) ,
	[EzbobBankAccount] [nchar](10) NOT NULL,
 CONSTRAINT [PK_NL_EzbobBankAccounts] PRIMARY KEY CLUSTERED ([EzbobBankAccountID] ASC)
) ;
END
GO
	
IF object_id('NL_FundTransfers') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_FundTransfers](
	[FundTransferID] [int] NOT NULL IDENTITY(1,1) ,
	[LoanID] [int] NOT NULL,
	[Amount] [money] NOT NULL,
	[TransferTime] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_NL_FundTransfers] PRIMARY KEY CLUSTERED ([FundTransferID] ASC)
) ;
END
GO
	
IF object_id('NL_LoanAgreements') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_LoanAgreements](
	[LoanAgreementID] [int] NOT NULL IDENTITY(1,1) ,
	[LoanHistoryID] [int] NOT NULL,
	[LoanAgreementName] [nchar](10) NULL,
	[FilePath] [nchar](10) NULL,
	[LoanAgreementTemplateID] [int] NULL,
	[AgreementModel] [nvarchar](max) NULL,
 CONSTRAINT [PK_NL_LoanAgreements] PRIMARY KEY CLUSTERED ([LoanAgreementID] ASC )
);
END
GO
	
IF object_id('NL_LoanFeePayments') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_LoanFeePayments](
	[LoanFeePaymentID] [int] NOT NULL IDENTITY(1,1) ,
	[LoanFeeID] [int] NOT NULL,
	[PaymentID] [int] NOT NULL,
	[Amount] [decimal](18, 6) NOT NULL,	
 CONSTRAINT [PK_LoanFeePayments] PRIMARY KEY CLUSTERED ([LoanFeePaymentID] ASC)
) ;
END
GO
	
IF object_id('NL_LoanFees') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_LoanFees](
	[LoanFeeID] [int] NOT NULL IDENTITY(1,1) ,
	[LoanFeeTypeID] [int] NULL,
	[HistoryID] [int] NULL,
	[UserID] [int] NULL,
	[Amount] [decimal](18, 6) NULL,
	[CreatedTime] [datetime] NULL,
	[AssignTime] [datetime] NULL,	
	[DisabledTime] [datetime] NULL,
	[Description] [nvarchar](max) NULL
 CONSTRAINT [PK_NL_LoanFees] PRIMARY KEY CLUSTERED ([LoanFeeID] ASC)
) ;
END
GO
	
IF object_id('NL_LoanFeeTypes') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_LoanFeeTypes](
	[LoanFeeTypeID] [int] NOT NULL IDENTITY(1,1) ,
	[LoanFeeType] [nchar](50) NOT NULL,
 CONSTRAINT [PK_NL_LoanFeeTypes] PRIMARY KEY CLUSTERED ([LoanFeeTypeID] ASC)
) ;	
END
GO
	
--IF object_id('NL_LoanSources') IS NULL 
--BEGIN	
--CREATE TABLE [dbo].[NL_LoanSources](
--	[LoanSourceID] [int] NOT NULL IDENTITY(1,1) ,
--	[LoanSource] [nchar](10) NOT NULL,
--	[IsDisabled] [bit] NULL,
--	[MaxAmount] [int] NULL,
--	[MaxInterestRate] [decimal](18, 6) NULL,
--	[DefaultRepaymentMonths] [int] NULL,
--	[IsCustomerPeriodSelectionAllowed] [bit] NULL,
--	[MaxEmployeeCount] [int] NULL,
--	[MaxAnnualTurnover] [decimal](18, 6) NULL,
--	[IsDefault] [bit] NULL,
--	[AlertOnCustomerReasonType] [int] NULL,
-- CONSTRAINT [PK_NL_LoanSources] PRIMARY KEY CLUSTERED ([LoanSourceID] ASC)
--);
--END
--GO
	
IF object_id('NL_LoanStatuses') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_LoanStatuses](
	[LoanStatusID] [int] NOT NULL IDENTITY(1,1) ,
	[LoanStatus] [nchar](80) NOT NULL,
 CONSTRAINT [PK_NL_LoanStatuses] PRIMARY KEY CLUSTERED ([LoanStatusID] ASC)
) ;
END
GO
	
IF object_id('NL_RepaymentIntervalTypes') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_RepaymentIntervalTypes](
	[RepaymentIntervalTypeID] [int] NOT NULL IDENTITY(1,1) ,
	[RepaymentIntervalType] [int] NOT NULL,
 CONSTRAINT [PK_RepetitionLengths] PRIMARY KEY CLUSTERED ([RepaymentIntervalTypeID] ASC)
) ;
END
GO

IF object_id('NL_OfferStatuses') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_OfferStatuses](
	[OfferStatusID] [int] NOT NULL IDENTITY(1,1) ,
	[OfferStatus] [nchar](20) NOT NULL,
 CONSTRAINT [PK_NL_OfferStatuses] PRIMARY KEY CLUSTERED ([OfferStatusID] ASC)
) ;
END
GO
	
IF object_id('NL_Offers') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_Offers](
	[OfferID] [int] NOT NULL IDENTITY(1,1) ,
	[DecisionID] [int] NOT NULL,
	[LoanTypeID] [int] NOT NULL default 1,
	[RepaymentIntervalTypeID] [int] NOT NULL default 1,
	[LoanSourceID] [int] NOT NULL default 1,	
	[StartTime] [datetime] NOT NULL,
	[EndTime] [datetime] NOT NULL,	
	[RepaymentCount] [int] NOT NULL,	
	[Amount] [decimal](18, 6) NOT NULL,
	[MonthlyInterestRate] [decimal](18, 6) NOT NULL,
	[CreatedTime] [datetime] NOT NULL,
	[BrokerSetupFeePercent] [money] NULL,
	[Notes] [nvarchar](max) NULL,
	[SetupFeePercent] [decimal](18, 6) NULL,
	[InterestOnlyRepaymentCount] [int] NULL,
	[DiscountPlanID] [int] NULL,
	[IsLoanTypeSelectionAllowed] [bit] NOT NULL DEFAULT 0,
	[EmailSendingBanned] [bit] NOT NULL DEFAULT 0,
  CONSTRAINT [PK_NL_Offers] PRIMARY KEY CLUSTERED ([OfferID] ASC)
);
END
GO
	
IF object_id('NL_LoanLegals') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_LoanLegals](
	[LoanLegalID] [int] NOT NULL IDENTITY(1,1),
	[OfferID] [int] NULL,
	[SignatureTime] [datetime] NOT NULL,
	[CreditActAgreementAgreed] [bit] NULL,
	[PreContractAgreementAgreed] [bit] NULL,
	[PrivateCompanyLoanAgreementAgreed] [bit] NULL,
	[GuarantyAgreementAgreed] [bit] NULL,
	[EUAgreementAgreed] [bit] NULL,
	[COSMEAgreementAgreed] [bit] NULL,
	[NotInBankruptcy] [bit] NULL,
	[SignedName] [nvarchar](128) NULL,
 CONSTRAINT [PK_LoanLegals] PRIMARY KEY CLUSTERED (	[LoanLegalID] ASC)
) ;	
END
GO
	
IF object_id('NL_LoanLienLinks') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_LoanLienLinks](	
	[LoanLienLinkID] [int] IDENTITY(1,1) NOT NULL,
	[LoanID] [int] NOT NULL,
	[LoanLienID] [int] NOT NULL,
	[Amount] [decimal](18, 6) NOT NULL,
 CONSTRAINT [PK_NL_LoanLienLinks] PRIMARY KEY CLUSTERED ([LoanLienLinkID] ASC)
) ;
END
GO
	
IF object_id('NL_LoanSchedules') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_LoanSchedules](
	[LoanScheduleID] [int] NOT NULL IDENTITY(1,1) ,
	[LoanHistoryID] [int] NOT NULL,
	[Position] [int] NOT NULL,
	[PlannedDate] [datetime] NOT NULL,
	[ClosedTime] [datetime] NULL,
	[Principal] [money] NOT NULL,
	[InterestRate] [decimal](18, 6) NOT NULL	
 CONSTRAINT [PK_NL_LoanSchedules] PRIMARY KEY CLUSTERED ([LoanScheduleID] ASC)
) ;
END
GO
	
IF object_id('NL_Loans') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_Loans](
	[LoanID] [int] NOT NULL IDENTITY(1,1) ,
	[OfferID] [int] NULL,
	[LoanTypeID] [int] NOT NULL,
	[RepaymentIntervalTypeID] [int] NULL,
	[LoanStatusID] [int] NULL,
	[EzbobBankAccountID] [int] NULL,
	[LoanSourceID] [int] NULL,
	[Position] [int] NOT NULL,
	[TakenAmount] [money] NULL,
	[CreationTime] [datetime] NOT NULL,
	[IssuedTime] [datetime] NULL,
	[RepaymentCount] [int] NULL,	
	[Refnum] [nchar](10) NOT NULL,
	[DateClosed] [datetime] NULL,	
	[InterestRate] [decimal](18, 6) NULL,
	[InterestOnlyRepaymentCount] [int] NULL
	,[OldLoanID] [int] NOT NULL,	
 CONSTRAINT [PK_NL_Loans] PRIMARY KEY CLUSTERED ([LoanID] ASC)
) ;
END
GO
	
IF object_id('NL_LoanSchedulePayments') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_LoanSchedulePayments](	
	[LoanSchedulePaymentID] [int] NOT NULL,
	[LoanScheduleID] [int] NOT NULL,
	[PaymentID] [int] NOT NULL,
	[PrincipalPaid] [decimal](18, 6) NOT NULL,
	[InterestPaid] [decimal](18, 6) NOT NULL,
 CONSTRAINT [PK_NL_LoanSchedulePayments] PRIMARY KEY CLUSTERED ([LoanSchedulePaymentID] ASC)
) ;
END
GO
	
IF object_id('NL_LoanHistory') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_LoanHistory](
	[LoanHistoryID] [int] NOT NULL IDENTITY(1,1) ,
	[EventTime] [datetime] NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[LoanID] [int] NULL,
	[UserID] [int] NULL,
	[LoanLegalID] [int] NULL,
 CONSTRAINT [PK_NL_LoanHistory] PRIMARY KEY CLUSTERED ([LoanHistoryID] ASC)
) ;
END
GO
	
IF object_id('NL_LoanRollovers') IS NULL 
BEGIN
CREATE TABLE [dbo].[NL_LoanRollovers](
	[LoanRolloverID] [int] NOT NULL IDENTITY(1,1) ,
	[LoanID] [int] NOT NULL,
	[CreatedByUserID] [int] NOT NULL,
	[DeletedByUserID] [int] NULL,	
	[LoanFeeID] [int] NULL,
	[FeeAmount] [money] NOT NULL,
	--[ShiftIntervals] int not null default 1,
	[CreationTime] [datetime] NOT NULL,	
	[ExpirationTime] [datetime] NOT NULL,
	[CustomerActionTime] [datetime] NULL,
	[IsAccepted] [bit] NULL,
	[DeletionTime] [datetime] NULL,	
 CONSTRAINT [PK_NL_Rollovers] PRIMARY KEY CLUSTERED ([LoanRolloverID] ASC)
) ;
END
GO
	
--IF object_id('NL_CollectionLog') IS NULL 
--BEGIN	
--CREATE TABLE [dbo].[NL_CollectionLog](
--	[CollectionLogID] [int]  NOT NULL IDENTITY(1,1),
--	[LoanHistoryID] [int] NOT NULL,
--	[CollectionType] [nvarchar](30) NULL,
--	[Method] [nvarchar](30) NULL,
--	[Comments] [ntext] default null,	
--	[TimeStamp] [datetime] NOT NULL,
-- CONSTRAINT [PK_NL_CollectionLog] PRIMARY KEY CLUSTERED ([CollectionLogID] ASC),
-- CONSTRAINT [FK_NL_CollectionLog_NL_LoanHistory] FOREIGN KEY([LoanHistoryID]) REFERENCES [dbo].[NL_LoanHistory] ([LoanHistoryID])
--) ;
--END
--GO
	
IF object_id('NL_PacnetTransactions') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_PacnetTransactions](
	[PacnetTransactionID] [int] NOT NULL IDENTITY(1,1) ,
	[FundTransferID] [int] NOT NULL,
	[TransactionTime] [datetime] NULL,
	[Amount] [decimal](18, 6) NULL,
	[Notes] [nvarchar](max) NULL,
	[PacnetTransactionStatusID] [int] NULL,
	[TrackingNumber] [nvarchar](100) NULL,
 CONSTRAINT [PK_NL_PacnetTransactions] PRIMARY KEY CLUSTERED (	[PacnetTransactionID] ASC)
) ;
END
GO
	
IF object_id('NL_PacnetTransactionStatuses') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_PacnetTransactionStatuses](
	[PacnetTransactionStatusID] [int] NOT NULL IDENTITY(1,1) ,
	[TransactionStatus] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_NL_PacnetTransactionStatuses] PRIMARY KEY CLUSTERED ([PacnetTransactionStatusID] ASC)
) ;
END
GO

IF object_id('NL_PaymentStatuses') IS NULL 
BEGIN	
CREATE TABLE [dbo].NL_PaymentStatuses(
	[PaymentStatusID] [int] NOT NULL IDENTITY(1,1) ,
	[PaymentStatus] [nvarchar] (60) NOT NULL,		
 CONSTRAINT [PK_NL_PaymentStatuses] PRIMARY KEY CLUSTERED ([PaymentStatusID] ASC)
) ;
END
GO
	
IF object_id('NL_Payments') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_Payments](
	[PaymentID] [int] NOT NULL IDENTITY(1,1) ,
	[PaymentMethodID] [int] NOT NULL,	
	[PaymentStatusID] [int] NOT NULL,	
	[PaymentTime] [datetime] NOT NULL, 
	[IsActive] [bit] NOT NULL,
	[CreationTime] [datetime] NOT NULL default getutcdate(), --real insert datetime
	[CreatedByUserID] [int] NULL,
	[DeletionTime] [datetime] NULL,
	[DeletedByUserID] [int] NULL,
	[Notes] [nvarchar](max) NULL,
 CONSTRAINT [PK_NL_LoanTransactions] PRIMARY KEY CLUSTERED ([PaymentID] ASC)
) ;
END
GO
	
IF object_id('NL_PaypointTransactions') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_PaypointTransactions](
	[PaypointTransactionID] [int] NOT NULL  IDENTITY(1,1) ,
	[PaymentID] [int] NOT NULL,
	[TransactionTime] [datetime] NULL, -- effective payment date
	[Amount] [decimal](18, 6) NULL,
	[Notes] [nvarchar](max) NULL,
	[PaypointTransactionStatusID] [int] NULL,
	[PaypointUniqID] [nvarchar](100) NULL,
	[PaypointCardID] [int] NOT NULL,
	[IP] [nvarchar](32) NULL,
 CONSTRAINT [PK_NL_PaypointTransactions] PRIMARY KEY CLUSTERED ([PaypointTransactionID] ASC )
) ;
END
GO
	
IF object_id('NL_PaypointTransactionStatuses') IS NULL 
BEGIN	
CREATE TABLE [dbo].[NL_PaypointTransactionStatuses](
	[PaypointTransactionStatusID] [int] NOT NULL IDENTITY(1,1) ,
	[TransactionStatus] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_NL_PaypointTransactionStatuses] PRIMARY KEY CLUSTERED (	[PaypointTransactionStatusID] ASC)
) ;
END ;

IF object_id('WriteOffReasons') IS NULL 
BEGIN	
CREATE TABLE [dbo].[WriteOffReasons](
	[WriteOffReasonID] [int] NOT NULL IDENTITY(1,1) ,
	[ReasonName] [nvarchar](100) NOT NULL,
	[PaymentID] [int] NOT NULL ,
 CONSTRAINT [PK_WriteOffReasons] PRIMARY KEY CLUSTERED ( [WriteOffReasonID] ASC)
) ;
END ;


IF object_id('NL_LoanStates') IS NULL BEGIN	
CREATE TABLE [dbo].[NL_LoanStates](
	[LoanStateID] [int] NOT NULL IDENTITY(1,1) ,
	[InsertDate]  [datetime] NOT NULL default getutcdate(),
	[LoanID] [int] NOT NULL , 
	[OutstandingPrincipal] [decimal](18, 6) NOT NULL,
	[OutstandingInterest] [decimal](18, 6) NOT NULL,
	[OutstandingFee] [decimal](18, 6) NOT NULL,	
	[PaidPrincipal] [decimal](18, 6) NOT NULL,
	[PaidInterest] [decimal](18, 6) NOT NULL,
	[PaidFee] [decimal](18, 6) NOT NULL,
	[Balance] [decimal](18, 6) NOT NULL, -- OutstandingPrincipal+OutstandingInterest+OutstandingFee
	[LateDays] int NOT NULL ,
	[Notes] [ntext] NULL,
 CONSTRAINT [PK_NL_LoanState] PRIMARY KEY CLUSTERED ( [LoanStateID] ASC)
) ;
END ;


IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanSource') AND name = 'IsDisabled')
	ALTER TABLE [dbo].[LoanSource] ADD [IsDisabled] [bit] NULL;
GO
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanOptions') AND name = 'UserID')
	ALTER TABLE [dbo].[LoanOptions] ADD [UserID] [int] NULL ; 
GO
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanOptions') AND name = 'InsertDate')
	ALTER TABLE [dbo].[LoanOptions] ADD [InsertDate] [datetime] default getutcdate() not NULL ; 
GO
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanOptions') AND name = 'IsActive')
	ALTER TABLE [dbo].[LoanOptions] ADD [IsActive] [bit] NULL ; 
GO
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanOptions') AND name = 'Comments')
	ALTER TABLE [dbo].[LoanOptions] ADD [Comments] [ntext] NULL ; 
GO
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanOptions') AND name = 'NLLoanID')
	ALTER TABLE [dbo].[LoanOptions] ADD [NLLoanID] [int] NULL ; 
GO
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanBrokerCommission') AND name = 'NLLoanID')
	ALTER TABLE [dbo].[LoanBrokerCommission] ADD [NLLoanID] [int] NULL ; 
GO
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('CollectionLog') AND name = 'LoanHistoryID')
	ALTER TABLE [dbo].[CollectionLog] ADD LoanHistoryID [int] NULL ; 
GO
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('CollectionLog') AND name = 'Comments')
	ALTER TABLE [dbo].[CollectionLog] ADD Comments [ntext] null ; 
GO
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('DecisionTrail') AND name = 'NLDecisionID')
	ALTER TABLE [dbo].[DecisionTrail] ADD NLDecisionID [int] NULL ; 
GO
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('Esignatures') AND name = 'DecisionID')
	ALTER TABLE [dbo].[Esignatures] ADD DecisionID [int] NULL ; 
GO

IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Decisions_NL_CashRequests') BEGIN
ALTER TABLE [dbo].[NL_Decisions]  ADD  CONSTRAINT FK_NL_Decisions_NL_CashRequests FOREIGN KEY([CashRequestID]) REFERENCES [dbo].[NL_CashRequests] ([CashRequestID]);
END ;
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Decisions_DecisionNames') BEGIN
ALTER TABLE [dbo].[NL_Decisions]  ADD  CONSTRAINT FK_NL_Decisions_DecisionNames FOREIGN KEY([DecisionNameID]) REFERENCES [dbo].[Decisions] ([DecisionID]);
END ;
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Decisions_Users') BEGIN
ALTER TABLE [dbo].[NL_Decisions]  ADD  CONSTRAINT FK_NL_Decisions_Users FOREIGN KEY([UserID]) REFERENCES [dbo].[Security_User] ([UserID]);
END ;

IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_BlendedLoans_NL_Loans') BEGIN
ALTER TABLE [dbo].[NL_BlendedLoans] ADD CONSTRAINT FK_NL_BlendedLoans_NL_Loans FOREIGN KEY([LoanID]) REFERENCES [dbo].[NL_Loans] ([LoanID]) ;
END ;

IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_BlendedOffers_NL_Offers') BEGIN
ALTER TABLE [dbo].[NL_BlendedOffers] ADD CONSTRAINT [FK_NL_BlendedOffers_NL_Offers] FOREIGN KEY([OfferID]) REFERENCES [dbo].[NL_Offers] ([OfferID]) ;
END ;

IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_FundTransfers_Loans') BEGIN
ALTER TABLE [dbo].[NL_FundTransfers] ADD CONSTRAINT [FK_FundTransfers_Loans] FOREIGN KEY([LoanID]) REFERENCES [dbo].[NL_Loans] ([LoanID]) ;
END ;

IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanAgreements_LoanHistory') BEGIN
ALTER TABLE [dbo].[NL_LoanAgreements] ADD CONSTRAINT [FK_NL_LoanAgreements_LoanHistory] FOREIGN KEY([LoanHistoryID]) REFERENCES [dbo].[NL_LoanHistory] ([LoanHistoryID]) ;
END ;

IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanAgreements_LoanAgreementTemplate') BEGIN
ALTER TABLE [dbo].[NL_LoanAgreements] ADD CONSTRAINT [FK_NL_LoanAgreements_LoanAgreementTemplate] FOREIGN KEY([LoanAgreementTemplateID]) REFERENCES [dbo].[LoanAgreementTemplate] ([Id]) ;
END
GO

IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanFeePayments_LoanFees') BEGIN
ALTER TABLE [dbo].[NL_LoanFeePayments] ADD CONSTRAINT [FK_LoanFeePayments_LoanFees] FOREIGN KEY([LoanFeeID]) REFERENCES [dbo].[NL_LoanFees] ([LoanFeeID]) ;
END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanFeePayments_LoanTransactions') BEGIN
ALTER TABLE [dbo].[NL_LoanFeePayments] ADD CONSTRAINT [FK_LoanFeePayments_LoanTransactions] FOREIGN KEY([PaymentID]) REFERENCES [dbo].[NL_Payments] ([PaymentID]) ;
END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanFeePayments_LoanTransactions')  BEGIN
ALTER TABLE [dbo].[NL_LoanFeePayments] ADD CONSTRAINT [FK_LoanFeePayments_LoanTransactions] FOREIGN KEY([PaymentID]) REFERENCES [dbo].[NL_Payments] ([PaymentID]) ;
END
GO

IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanFees_LoanFeeTypes') BEGIN
ALTER TABLE [dbo].[NL_LoanFees] ADD CONSTRAINT [FK_LoanFees_LoanFeeTypes] FOREIGN KEY([LoanFeeTypeID]) REFERENCES [dbo].[NL_LoanFeeTypes] ([LoanFeeTypeID]) ;
END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanFees_NL_LoanHistory') BEGIN
ALTER TABLE [dbo].[NL_LoanFees] ADD  CONSTRAINT [FK_NL_LoanFees_NL_LoanHistory] FOREIGN KEY([HistoryID]) REFERENCES [dbo].[NL_LoanHistory] ([LoanHistoryID]) ;
END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanFees_SecurityUser') BEGIN
ALTER TABLE [dbo].[NL_LoanFees] ADD  CONSTRAINT [FK_NL_LoanFees_SecurityUser] FOREIGN KEY([UserID]) REFERENCES [dbo].[Security_User] ([UserId]) ;
END
GO

GO
 IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanScheduleHistory_Security_User') BEGIN
 ALTER TABLE [dbo].[NL_LoanHistory] ADD  CONSTRAINT [FK_NL_LoanScheduleHistory_Security_User] FOREIGN KEY([UserID]) REFERENCES [dbo].[Security_User] ([UserID]) ;
END
GO
 IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanHistory_NL_Loans') BEGIN
 ALTER TABLE [dbo].[NL_LoanHistory] ADD  CONSTRAINT [FK_NL_LoanHistory_NL_Loans] FOREIGN KEY([LoanID]) REFERENCES [dbo].[NL_Loans] ([LoanID]) ;
END
GO
 IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanHistory_NL_LoanLegals') BEGIN
 ALTER TABLE [dbo].[NL_LoanHistory] ADD  CONSTRAINT FK_NL_LoanHistory_NL_LoanLegals FOREIGN KEY([LoanLegalID]) REFERENCES [dbo].[NL_LoanLegals] ([LoanLegalID]) ;
END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_CollectionLog_NL_LoanHistory') BEGIN
 ALTER TABLE [dbo].[CollectionLog] ADD CONSTRAINT FK_CollectionLog_NL_LoanHistory FOREIGN KEY([LoanHistoryID]) REFERENCES [dbo].[NL_LoanHistory] ([LoanHistoryID]) ;
END

IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanSchedules_LoanHistory') BEGIN
ALTER TABLE [dbo].[NL_LoanSchedules] ADD CONSTRAINT [FK_NL_LoanSchedules_LoanHistory] FOREIGN KEY([LoanHistoryID]) REFERENCES [dbo].[NL_LoanHistory] ([LoanHistoryID]) ;
END
GO

IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanLegals_NL_Offers') BEGIN
ALTER TABLE [dbo].[NL_LoanLegals] ADD CONSTRAINT [FK_NL_LoanLegals_NL_Offers] FOREIGN KEY([OfferID]) REFERENCES [dbo].[NL_Offers] ([OfferID]) ;
END
GO

IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanLienLink_LoanLien') BEGIN
ALTER TABLE [dbo].[NL_LoanLienLinks] ADD CONSTRAINT [FK_NL_LoanLienLink_LoanLien] FOREIGN KEY([LoanLienID]) REFERENCES [dbo].[LoanLien] ([Id]) ;
END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanLienLink_NL_Loans') BEGIN
ALTER TABLE [dbo].[NL_LoanLienLinks] ADD CONSTRAINT [FK_NL_LoanLienLink_NL_Loans] FOREIGN KEY([LoanID]) REFERENCES [dbo].[NL_Loans] ([LoanID]) ;
END
GO

IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanOptions_NL_Loans') BEGIN
ALTER TABLE [dbo].[LoanOptions] ADD CONSTRAINT [FK_LoanOptions_NL_Loans] FOREIGN KEY([NLLoanID]) REFERENCES [dbo].[NL_Loans] ([LoanID]) ;
END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanOptions_Security_User') BEGIN
ALTER TABLE [dbo].[LoanOptions] ADD CONSTRAINT [FK_LoanOptions_Security_User] FOREIGN KEY([UserID]) REFERENCES [dbo].[Security_User] ([UserId]) ;
END
GO
 
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanRollovers_Security_User') BEGIN
 ALTER TABLE [dbo].[NL_LoanRollovers] ADD CONSTRAINT [FK_NL_LoanRollovers_Security_User] FOREIGN KEY([CreatedByUserID]) REFERENCES [dbo].[Security_User] ([UserID]) ;
 END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanRollovers_Security_User1') BEGIN
 ALTER TABLE [dbo].[NL_LoanRollovers] ADD CONSTRAINT [FK_NL_LoanRollovers_Security_User1] FOREIGN KEY([DeletedByUserID]) REFERENCES [dbo].[Security_User] ([UserID]) ;
 END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanRollovers_NL_LoanFees') BEGIN
 ALTER TABLE [dbo].[NL_LoanRollovers] ADD CONSTRAINT [FK_NL_LoanRollovers_NL_LoanFees] FOREIGN KEY([LoanFeeID]) REFERENCES [dbo].[NL_LoanFees] ([LoanFeeID]) ;
 END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanRollovers_NL_Loans') BEGIN
 ALTER TABLE [dbo].[NL_LoanRollovers]  ADD  CONSTRAINT [FK_NL_LoanRollovers_NL_Loans] FOREIGN KEY([LoanID]) REFERENCES [dbo].[NL_Loans] ([LoanID]) ;
END
GO

 IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Loans_LoanSource') BEGIN
 ALTER TABLE [dbo].[NL_Loans]   ADD  CONSTRAINT [FK_NL_Loans_LoanSource] FOREIGN KEY([LoanSourceID]) REFERENCES [dbo].[LoanSource] ([LoanSourceID]) ;
 END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_Loans_LoanStatuses') BEGIN
 ALTER TABLE [dbo].[NL_Loans]   ADD  CONSTRAINT [FK_Loans_LoanStatuses] FOREIGN KEY([LoanStatusID]) REFERENCES [dbo].[NL_LoanStatuses] ([LoanStatusID]) ;
 END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_Loans_EzbobBankAccounts') BEGIN
 ALTER TABLE [dbo].[NL_Loans]   ADD  CONSTRAINT [FK_Loans_EzbobBankAccounts] FOREIGN KEY([EzbobBankAccountID]) REFERENCES [dbo].[NL_EzbobBankAccounts] ([EzbobBankAccountID]) ;
 END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Loans_LoanType') BEGIN
 ALTER TABLE [dbo].[NL_Loans]   ADD  CONSTRAINT [FK_NL_Loans_LoanType] FOREIGN KEY([LoanTypeID]) REFERENCES [dbo].[LoanType] ([Id]) ;
 END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Loans_NL_Offers') BEGIN
 ALTER TABLE [dbo].[NL_Loans]   ADD  CONSTRAINT [FK_NL_Loans_NL_Offers] FOREIGN KEY([OfferID]) REFERENCES [dbo].[NL_Offers] ([OfferID]) ;
 END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Loans_NL_RepaymentIntervalTypes') BEGIN
 ALTER TABLE [dbo].[NL_Loans]   ADD  CONSTRAINT [FK_NL_Loans_NL_RepaymentIntervalTypes] FOREIGN KEY([RepaymentIntervalTypeID]) REFERENCES [dbo].[NL_RepaymentIntervalTypes] ([RepaymentIntervalTypeID]) ;
END
GO
--IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Loans_Loan') BEGIN
-- ALTER TABLE [dbo].[NL_Loans] ADD CONSTRAINT [FK_NL_Loans_Loan] FOREIGN KEY([OldLoanID]) REFERENCES [dbo].[Loan] ([Id]) ;
--END
--GO


 IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanScheduleTransaction_LoanSchedules') BEGIN
 ALTER TABLE [dbo].[NL_LoanSchedulePayments]  ADD  CONSTRAINT [FK_LoanScheduleTransaction_LoanSchedules] FOREIGN KEY([LoanScheduleID]) REFERENCES [dbo].[NL_LoanSchedules] ([LoanScheduleID]) ;
END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanScheduleTransaction_LoanTransactions') BEGIN
 ALTER TABLE [dbo].[NL_LoanSchedulePayments] ADD CONSTRAINT [FK_LoanScheduleTransaction_LoanTransactions] FOREIGN KEY([PaymentID]) REFERENCES [dbo].[NL_Payments] ([PaymentID]) ;
END
GO

IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Offers_LoanType') BEGIN
 ALTER TABLE [dbo].[NL_Offers]  ADD  CONSTRAINT [FK_NL_Offers_LoanType] FOREIGN KEY([LoanTypeID]) REFERENCES [dbo].[LoanType] ([Id]) ;
 END
GO
 IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Offers_NL_Decisions') BEGIN
 ALTER TABLE [dbo].[NL_Offers]  ADD  CONSTRAINT [FK_NL_Offers_NL_Decisions] FOREIGN KEY([DecisionID]) REFERENCES [dbo].[NL_Decisions] ([DecisionID]) ;
 END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Offers_LoanSource') BEGIN
 ALTER TABLE [dbo].[NL_Offers]  ADD  CONSTRAINT [FK_NL_Offers_LoanSource] FOREIGN KEY([LoanSourceID]) REFERENCES [dbo].[LoanSource] ([LoanSourceID]) ;
 END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Offers_NL_DiscountPlans') BEGIN
 ALTER TABLE [dbo].[NL_Offers]  ADD  CONSTRAINT [FK_NL_Offers_NL_DiscountPlans] FOREIGN KEY([DiscountPlanID]) REFERENCES [dbo].[NL_DiscountPlans] ([DiscountPlanID]) ;
END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Offers_NL_RepaymentIntervalType') BEGIN
 ALTER TABLE [dbo].[NL_Offers]  ADD  CONSTRAINT [FK_NL_Offers_NL_RepaymentIntervalType] FOREIGN KEY([RepaymentIntervalTypeID]) REFERENCES [dbo].[NL_RepaymentIntervalTypes] ([RepaymentIntervalTypeID]) ;
END
GO
 IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_Esignatures_NL_Decisions') BEGIN
 ALTER TABLE [dbo].[Esignatures]  ADD CONSTRAINT [FK_Esignatures_NL_Decisions] FOREIGN KEY([DecisionID]) REFERENCES [dbo].[NL_Decisions] ([DecisionID]) ;
 END
GO

IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_PacnetTransactions_NL_PacnetTransactionStatuses') BEGIN
 ALTER TABLE [dbo].[NL_PacnetTransactions] ADD CONSTRAINT [FK_NL_PacnetTransactions_NL_PacnetTransactionStatuses] FOREIGN KEY([PacnetTransactionStatusID]) REFERENCES [dbo].[NL_PacnetTransactionStatuses] ([PacnetTransactionStatusID]) ;
END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_PacnetTransactions_FundTransfers') BEGIN
 ALTER TABLE [dbo].[NL_PacnetTransactions] ADD CONSTRAINT [FK_PacnetTransactions_FundTransfers] FOREIGN KEY([FundTransferID]) REFERENCES [dbo].[NL_FundTransfers] ([FundTransferID]) ;
END
GO


IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_Payments_CreatedBySecurity_User') BEGIN
 ALTER TABLE [dbo].[NL_Payments]   ADD  CONSTRAINT [FK_Payments_CreatedBySecurity_User] FOREIGN KEY([CreatedByUserID]) REFERENCES [dbo].[Security_User] ([UserID]) ;
END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Payments_DeletedBySecurity_User') BEGIN
 ALTER TABLE [dbo].[NL_Payments]   ADD  CONSTRAINT [FK_NL_Payments_DeletedBySecurity_User] FOREIGN KEY([DeletedByUserID]) REFERENCES [dbo].[Security_User] ([UserID]) ;
END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_PaymentStatuses') BEGIN
 ALTER TABLE [dbo].[NL_Payments] ADD CONSTRAINT FK_NL_PaymentStatuses FOREIGN KEY([PaymentStatusID]) REFERENCES [dbo].[NL_PaymentStatuses] ([PaymentStatusID]) ;
END
GO

IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_PaypointTransactions_NL_PaypointTransactionStatuses') BEGIN
 ALTER TABLE [dbo].[NL_PaypointTransactions] ADD CONSTRAINT [FK_NL_PaypointTransactions_NL_PaypointTransactionStatuses] FOREIGN KEY([PaypointTransactionStatusID]) REFERENCES [dbo].[NL_PaypointTransactionStatuses] ([PaypointTransactionStatusID]) ;
END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_PaypointTransactions_Payments') BEGIN
 ALTER TABLE [dbo].[NL_PaypointTransactions] ADD CONSTRAINT [FK_PaypointTransactions_Payments] FOREIGN KEY([PaymentID]) REFERENCES [dbo].[NL_Payments] ([PaymentID]) ;
END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'PRIMARY_KEY_CONSTRAINT' and name = 'PK_PaypointCard') BEGIN
 ALTER TABLE [dbo].[PaypointCard] ADD CONSTRAINT [PK_PaypointCard] PRIMARY KEY([Id])  ;
END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_PaypointTransactions_PayPointCard') BEGIN
 ALTER TABLE [dbo].[NL_PaypointTransactions] ADD CONSTRAINT [FK_NL_PaypointTransactions_PayPointCard] FOREIGN KEY([PaypointCardID]) REFERENCES [dbo].[PayPointCard] ([Id]) ;
END
GO


IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_CashRequests_CashRequestOrigins') BEGIN
 ALTER TABLE [dbo].[NL_CashRequests] ADD CONSTRAINT [FK_NL_CashRequests_CashRequestOrigins] FOREIGN KEY([CashRequestOriginID]) REFERENCES [dbo].[NL_CashRequestOrigins] ([CashRequestOriginID]);
END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_CashRequests_Customers') BEGIN
 ALTER TABLE [dbo].[NL_CashRequests] ADD CONSTRAINT [FK_NL_CashRequests_Customers] FOREIGN KEY([CustomerID]) REFERENCES [dbo].[Customer] ([Id]);
END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_CashRequests_Users') BEGIN
 ALTER TABLE [dbo].[NL_CashRequests] ADD CONSTRAINT [FK_NL_CashRequests_Users] FOREIGN KEY([UserID]) REFERENCES [dbo].[Security_User] ([UserID]);
END
GO
--IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_CashRequests_CashRequest') BEGIN
-- ALTER TABLE [dbo].[NL_CashRequests] ADD CONSTRAINT [FK_NL_CashRequests_CashRequest] FOREIGN KEY([OldCashRequestID]) REFERENCES [dbo].[CashRequests] ([Id]);
--END
--GO


IF(select cl.object_id from sys.all_objects ob inner join sys.all_columns cl on ob.object_id = cl.object_id  and ob.name = 'MedalCalculationsAV' and cl.name = 'CashRequestID') IS NULL BEGIN
 ALTER TABLE [dbo].[MedalCalculationsAV] ADD [CashRequestID] int default null;
END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_MedalCalculationsAV_NL_CashRequests') BEGIN
 ALTER TABLE [dbo].[MedalCalculationsAV]  ADD CONSTRAINT [FK_MedalCalculationsAV_NL_CashRequests] FOREIGN KEY([CashRequestID]) REFERENCES [dbo].[NL_CashRequests] ([CashRequestID]);
END
GO

IF(select cl.object_id from sys.all_objects ob inner join sys.all_columns cl on ob.object_id = cl.object_id  and ob.name = 'MedalCalculations' and cl.name = 'CashRequestID') IS NULL BEGIN
 ALTER TABLE [dbo].[MedalCalculations] ADD [CashRequestID] int default null;
END
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_MedalCalculations_NL_CashRequests') BEGIN
 ALTER TABLE [dbo].[MedalCalculations] ADD CONSTRAINT [FK_MedalCalculations_NL_CashRequests] FOREIGN KEY([CashRequestID]) REFERENCES [dbo].[NL_CashRequests] ([CashRequestID]);
END
GO

IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_DiscountPlanEntries_NL_DiscountPlans') BEGIN
 ALTER TABLE [dbo].[NL_DiscountPlanEntries] ADD CONSTRAINT [FK_NL_DiscountPlanEntries_NL_DiscountPlans] FOREIGN KEY([DiscountPlanID]) REFERENCES [dbo].[NL_DiscountPlans] ([DiscountPlanID]);
END
GO

IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_DecisionTrail_NL_Decisions') BEGIN
 ALTER TABLE [dbo].[DecisionTrail] ADD CONSTRAINT [FK_DecisionTrail_NL_Decisions] FOREIGN KEY([NLDecisionID]) REFERENCES [dbo].[NL_Decisions] ([DecisionID]);
END
GO

IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanBrokerCommission_NL_Loan') BEGIN
 ALTER TABLE [dbo].[LoanBrokerCommission] ADD CONSTRAINT [FK_LoanBrokerCommission_NL_Loan] FOREIGN KEY([NLLoanID]) REFERENCES [dbo].[NL_Loans] ([LoanID]) ; 
END
GO

IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_WriteOffReasons_Payments') BEGIN
 ALTER TABLE [dbo].[WriteOffReasons] ADD CONSTRAINT [FK_WriteOffReasons_Payments] FOREIGN KEY([PaymentID]) REFERENCES [dbo].[NL_Payments] ([PaymentID]) ;
END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_LoanStates_NL_Loans') BEGIN
 ALTER TABLE [dbo].[NL_LoanStates] ADD CONSTRAINT [FK_NL_LoanStates_NL_Loans] FOREIGN KEY([LoanID]) REFERENCES [dbo].[NL_Loans] ([LoanID]) ;
END
GO

-- repair Medals table
alter table [dbo].[Medals] alter column [Id] int NOT NULL ;
IF(select object_id from sys.all_objects where name = 'PK_Medals') IS NULL
BEGIN
	alter table [dbo].[Medals] add CONSTRAINT [PK_Medals] PRIMARY KEY CLUSTERED ([Id] ASC );
END;
IF( select Id from dbo.Medals where Medal = 'NoClassification') IS NULL
BEGIN
	insert into [dbo].[Medals] (Medal) values('NoClassification');
END;

-- add column and FK to MedalCalculations/MedalCalculationsAV to Medals(names)
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('MedalCalculations') AND name = 'MedalNameID') 
	BEGIN
		alter table [dbo].[MedalCalculations] add [MedalNameID] [int] NOT NULL default 1;		
	END;
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('MedalCalculationsAV') AND name = 'MedalNameID')
	BEGIN
		alter table [dbo].[MedalCalculationsAV] add [MedalNameID] [int] NOT NULL default 1;
	END;
	
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_MedalCalculations_Medals') BEGIN
 ALTER TABLE [dbo].[MedalCalculations] ADD CONSTRAINT [FK_MedalCalculations_Medals] FOREIGN KEY([MedalNameID]) REFERENCES [dbo].[Medals] ([Id]);	
END
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_MedalCalculationsAV_Medals') BEGIN
 ALTER TABLE [dbo].[MedalCalculationsAV] ADD CONSTRAINT [FK_MedalCalculationsAV_Medals] FOREIGN KEY([MedalNameID])REFERENCES [dbo].[Medals] ([Id]);
END
GO

-- populate [MedalNameID] with appropriate values
UPDATE [dbo].[MedalCalculations] SET [dbo].[MedalCalculations].[MedalNameID] = [dbo].[Medals].Id FROM [dbo].[MedalCalculations] INNER JOIN [dbo].[Medals] ON [dbo].[MedalCalculations].Medal = [dbo].[Medals].Medal;
UPDATE [dbo].[MedalCalculationsAV] SET [dbo].[MedalCalculationsAV].[MedalNameID] = [dbo].[Medals].Id FROM [dbo].[MedalCalculationsAV] INNER JOIN [dbo].[Medals] ON [dbo].[MedalCalculationsAV].Medal = [dbo].[Medals].Medal;

-- add FK_NL_Payments_LoanTransactionMethod 
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Payments_LoanTransactionMethod') BEGIN
 ALTER TABLE [dbo].[NL_Payments] ADD CONSTRAINT [FK_NL_Payments_LoanTransactionMethod] FOREIGN KEY([PaymentMethodID]) REFERENCES [dbo].[LoanTransactionMethod] ([Id]);
END
GO



-- NL_DiscountPlans/NL_DiscountPlanEntries migration

-- SortOrder field defines order of entries (ORDER BY SortOrder ASC, DiscountPlanEntryID DESC) and not number of repayment period. Entry is always related to repetition period. I.e. if the same plan is applied to monthly repaid loan and to weekly repaid loan and an entry in the second position says "-10%" that means that in the former case customer receives 10% discound for the second month while in the latter case customer receives 10% discount for the second week.
-- Value "0.1" in InterestRateDelta means "+10%", value "-0.05" means "-5%".
if OBJECT_ID('tempdb..#discountplanTemp') is not null
	BEGIN
		drop table #discountplanTemp;
	END 
	 
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
        SELECT TOP 1 @Name = Name, @Id = Id, @ValuesStr = ValuesStr, @IsDefault = IsDefault, @ForbiddenForReuse = ForbiddenForReuse FROM #discountplanTemp;		
		if (select DiscountPlan from dbo.[NL_DiscountPlans] where DiscountPlan = @Name ) is null	
		BEGIN	
			INSERT INTO [dbo].[NL_DiscountPlans] ([DiscountPlan], [IsDefault], [ForbiddenForReuse]) VALUES (ltrim(rtrim(@Name)), @IsDefault, @ForbiddenForReuse);
		END
		select @NL_Id = DiscountPlanID from [dbo].[NL_DiscountPlans] where [DiscountPlan] = @Name;	
		if @NL_Id is not null 
		BEGIN					
			if (select COUNT([DiscountPlanEntryID]) from [dbo].[NL_DiscountPlanEntries] where [DiscountPlanID] = @NL_Id group by [DiscountPlanID]) is null			
			BEGIN					
				INSERT INTO [dbo].[NL_DiscountPlanEntries] ([DiscountPlanID], [PaymentOrder], [InterestDiscount]) 
					SELECT 
					@NL_Id, 
					splitted.Id, 
					case 
					when splitted.Data = 0 then 0 
					else CAST(splitted.Data AS float)/ @Percent 
					end					 
					FROM dbo.Split(@ValuesStr, ',') as splitted;
  			END
		END
		delete from #discountplanTemp where ID = @Id;		 
     END
	 drop table #discountplanTemp;
-- ### discount plan/entries migration	 


-- NL_LoanStatuses
IF( select LoanStatusID from dbo.NL_LoanStatuses where LoanStatus = 'Live') IS NULL BEGIN	insert into [dbo].[NL_LoanStatuses] (LoanStatus) values('Live');	END;
IF( select LoanStatusID from dbo.NL_LoanStatuses where LoanStatus = 'Late') IS NULL BEGIN insert into [dbo].[NL_LoanStatuses] (LoanStatus) values('Late');		END;
IF( select LoanStatusID from dbo.NL_LoanStatuses where LoanStatus = 'PaidOff') IS NULL BEGIN insert into [dbo].[NL_LoanStatuses] (LoanStatus) values('PaidOff');	END;
IF( select LoanStatusID from dbo.NL_LoanStatuses where LoanStatus = 'Pending') IS NULL BEGIN	insert into [dbo].[NL_LoanStatuses] (LoanStatus) values('Pending');		END;
IF( select LoanStatusID from dbo.NL_LoanStatuses where LoanStatus = 'Default') IS NULL BEGIN	insert into [dbo].[NL_LoanStatuses] (LoanStatus) values('Default');		END;
IF( select LoanStatusID from dbo.NL_LoanStatuses where LoanStatus = 'WriteOff') IS NULL BEGIN	insert into [dbo].[NL_LoanStatuses] (LoanStatus) values('WriteOff');	END;
-- from [dbo].[CustomerStatuses] ???
IF( select LoanStatusID from dbo.NL_LoanStatuses where LoanStatus = 'DebtManagement') IS NULL BEGIN insert into [dbo].[NL_LoanStatuses] (LoanStatus) values('DebtManagement'); END;
IF( select LoanStatusID from dbo.NL_LoanStatuses where LoanStatus = '1-14DaysMissed') IS NULL BEGIN insert into [dbo].[NL_LoanStatuses] (LoanStatus) values('1-14DaysMissed'); END;
IF( select LoanStatusID from dbo.NL_LoanStatuses where LoanStatus = '15-30DaysMissed') IS NULL BEGIN insert into [dbo].[NL_LoanStatuses] (LoanStatus) values('15-30DaysMissed'); END;
IF( select LoanStatusID from dbo.NL_LoanStatuses where LoanStatus = '31-45DaysMissed') IS NULL BEGIN insert into [dbo].[NL_LoanStatuses] (LoanStatus) values('31-45DaysMissed'); END;
IF( select LoanStatusID from dbo.NL_LoanStatuses where LoanStatus = '46-90DaysMissed') IS NULL BEGIN insert into [dbo].[NL_LoanStatuses] (LoanStatus) values('46-90DaysMissed'); END;
IF( select LoanStatusID from dbo.NL_LoanStatuses where LoanStatus = '60-90DaysMissed') IS NULL BEGIN insert into [dbo].[NL_LoanStatuses] (LoanStatus) values('60-90DaysMissed'); END;
IF( select LoanStatusID from dbo.NL_LoanStatuses where LoanStatus = '90+DaysMissed') IS NULL BEGIN insert into [dbo].[NL_LoanStatuses] (LoanStatus) values('90+DaysMissed'); END;
IF( select LoanStatusID from dbo.NL_LoanStatuses where LoanStatus = 'Legal ??? claim process') IS NULL BEGIN insert into [dbo].[NL_LoanStatuses] (LoanStatus) values('Legal ??? claim process'); END;
IF( select LoanStatusID from dbo.NL_LoanStatuses where LoanStatus = 'Legal - apply for judgment') IS NULL BEGIN insert into [dbo].[NL_LoanStatuses] (LoanStatus) values('Legal - apply for judgment'); END;
IF( select LoanStatusID from dbo.NL_LoanStatuses where LoanStatus = 'Legal: CCJ') IS NULL BEGIN insert into [dbo].[NL_LoanStatuses] (LoanStatus) values('Legal: CCJ'); END;
IF( select LoanStatusID from dbo.NL_LoanStatuses where LoanStatus = 'Legal: bailiff') IS NULL BEGIN insert into [dbo].[NL_LoanStatuses] (LoanStatus) values('Legal: bailiff'); END;
IF( select LoanStatusID from dbo.NL_LoanStatuses where LoanStatus = 'Legal: charging order') IS NULL BEGIN insert into [dbo].[NL_LoanStatuses] (LoanStatus) values('Legal: charging order'); END;
IF( select LoanStatusID from dbo.NL_LoanStatuses where LoanStatus = 'Collection: Tracing') IS NULL BEGIN insert into [dbo].[NL_LoanStatuses] (LoanStatus) values('Collection: Tracing'); END;
IF( select LoanStatusID from dbo.NL_LoanStatuses where LoanStatus = 'Collection: Site Visit') IS NULL BEGIN insert into [dbo].[NL_LoanStatuses] (LoanStatus) values('Collection: Site Visit'); END;




-- NL_LoanFeeTypes
IF( select LoanFeeTypeID from dbo.NL_LoanFeeTypes where LoanFeeType = 'SetupFee') IS NULL BEGIN
	insert into [dbo].[NL_LoanFeeTypes] (LoanFeeType) values('SetupFee');
END;
IF( select LoanFeeTypeID from dbo.NL_LoanFeeTypes where LoanFeeType = 'RolloverFee') IS NULL BEGIN
	insert into [dbo].[NL_LoanFeeTypes] (LoanFeeType) values('RolloverFee');
END;
IF( select LoanFeeTypeID from dbo.NL_LoanFeeTypes where LoanFeeType = 'AdminFee') IS NULL BEGIN
	insert into [dbo].[NL_LoanFeeTypes] (LoanFeeType) values('AdminFee');
END;  

-- NL_CashRequestOrigins
IF( select CashRequestOriginID from dbo.NL_CashRequestOrigins where CashRequestOrigin = 'FinishedWizard') IS NULL BEGIN
	insert into [dbo].[NL_CashRequestOrigins] (CashRequestOrigin) values('FinishedWizard');
END;
IF( select CashRequestOriginID from dbo.NL_CashRequestOrigins where CashRequestOrigin = 'QuickOffer') IS NULL BEGIN
	insert into [dbo].[NL_CashRequestOrigins] (CashRequestOrigin) values('QuickOffer');
END;
IF( select CashRequestOriginID from dbo.NL_CashRequestOrigins where CashRequestOrigin = 'RequestCashBtn') IS NULL BEGIN
	insert into [dbo].[NL_CashRequestOrigins] (CashRequestOrigin) values('RequestCashBtn');
END;
IF( select CashRequestOriginID from dbo.NL_CashRequestOrigins where CashRequestOrigin = 'NewCreditLineBtn') IS NULL BEGIN
	insert into [dbo].[NL_CashRequestOrigins] (CashRequestOrigin) values('NewCreditLineBtn');
END;
IF( select CashRequestOriginID from dbo.NL_CashRequestOrigins where CashRequestOrigin = 'Other') IS NULL BEGIN
	insert into [dbo].[NL_CashRequestOrigins] (CashRequestOrigin) values('Other');
END;
IF( select CashRequestOriginID from dbo.NL_CashRequestOrigins where CashRequestOrigin = 'RequalifyCustomerStrategy') IS NULL BEGIN
	insert into [dbo].[NL_CashRequestOrigins] (CashRequestOrigin) values('RequalifyCustomerStrategy');
END;

-- NL_PacnetTransactionStatuses
IF NOT EXISTS( select PacnetTransactionStatusID from dbo.NL_PacnetTransactionStatuses where [TransactionStatus] = 'Submited') BEGIN
	insert into [dbo].[NL_PacnetTransactionStatuses] ([TransactionStatus]) values('Submited');
END;
IF NOT EXISTS( select PacnetTransactionStatusID from dbo.NL_PacnetTransactionStatuses where [TransactionStatus] = 'ConfigError:MultipleCandidateChannels') BEGIN
	insert into [dbo].[NL_PacnetTransactionStatuses] ([TransactionStatus]) values('ConfigError:MultipleCandidateChannels');
END;
IF NOT EXISTS( select PacnetTransactionStatusID from dbo.NL_PacnetTransactionStatuses where [TransactionStatus] = 'Error')  BEGIN
	insert into [dbo].[NL_PacnetTransactionStatuses] ([TransactionStatus]) values('Error');
END;
IF NOT EXISTS( select PacnetTransactionStatusID from dbo.NL_PacnetTransactionStatuses where [TransactionStatus] = 'InProgress') BEGIN
	insert into [dbo].[NL_PacnetTransactionStatuses] ([TransactionStatus]) values('InProgress');
END;
IF NOT EXISTS( select PacnetTransactionStatusID from dbo.NL_PacnetTransactionStatuses where [TransactionStatus] = 'PaymentByCustomer') BEGIN
	insert into [dbo].[NL_PacnetTransactionStatuses] ([TransactionStatus]) values('PaymentByCustomer');
END;
IF NOT EXISTS( select PacnetTransactionStatusID from dbo.NL_PacnetTransactionStatuses where [TransactionStatus] = 'Done') BEGIN
	insert into [dbo].[NL_PacnetTransactionStatuses] ([TransactionStatus]) values('Done');
END;

-- add new payment method
IF NOT EXISTS( select Id from dbo.LoanTransactionMethod where [Name] = 'Write Off') BEGIN
declare @lastid int;
set @lastid = (select Max(Id) as i from dbo.LoanTransactionMethod);
insert into dbo.LoanTransactionMethod (Id, Name, DisplaySort) values( (@lastid + 1), 'Write Off', 0);	
END;

-- populate LoanAgreementTemplateTypes
IF NOT EXISTS( select [TemplateTypeID] from dbo.LoanAgreementTemplateTypes where TemplateType = 'GuarantyAgreement') BEGIN
	insert into [dbo].[LoanAgreementTemplateTypes] ( [TemplateType]) values('GuarantyAgreement');
END;
IF NOT EXISTS( select [TemplateTypeID] from dbo.LoanAgreementTemplateTypes where [TemplateType] = 'PreContractAgreement') BEGIN
	insert into [dbo].[LoanAgreementTemplateTypes] ( [TemplateType]) values('PreContractAgreement');
END;
IF NOT EXISTS( select [TemplateTypeID] from dbo.LoanAgreementTemplateTypes where [TemplateType] = 'CreditActAgreement') BEGIN
	insert into [dbo].[LoanAgreementTemplateTypes] ( [TemplateType]) values('CreditActAgreement');
END;
IF NOT EXISTS( select [TemplateTypeID] from dbo.LoanAgreementTemplateTypes where [TemplateType] = 'PrivateCompanyLoanAgreement') BEGIN
	insert into [dbo].[LoanAgreementTemplateTypes] ( [TemplateType]) values('PrivateCompanyLoanAgreement');
END;
IF NOT EXISTS( select [TemplateTypeID] from dbo.LoanAgreementTemplateTypes where [TemplateType] = 'AlibabaGuarantyAgreement') BEGIN
	insert into [dbo].[LoanAgreementTemplateTypes] ( [TemplateType]) values('AlibabaGuarantyAgreement');
END;
IF NOT EXISTS( select [TemplateTypeID] from dbo.LoanAgreementTemplateTypes where [TemplateType] = 'AlibabaPreContractAgreement') BEGIN
	insert into [dbo].[LoanAgreementTemplateTypes] ( [TemplateType]) values( 'AlibabaPreContractAgreement');
END;
IF NOT EXISTS( select [TemplateTypeID] from dbo.LoanAgreementTemplateTypes where [TemplateType] = 'AlibabaCreditActAgreement') BEGIN
	insert into [dbo].[LoanAgreementTemplateTypes] ( [TemplateType]) values('AlibabaCreditActAgreement');
END;
IF NOT EXISTS( select [TemplateTypeID] from dbo.LoanAgreementTemplateTypes where [TemplateType] = 'AlibabaPrivateCompanyLoanAgreement') BEGIN
	insert into [dbo].[LoanAgreementTemplateTypes] ( [TemplateType]) values('AlibabaPrivateCompanyLoanAgreement');
END;
IF NOT EXISTS( select [TemplateTypeID] from dbo.LoanAgreementTemplateTypes where [TemplateType] = 'AlibabaCreditFacility') BEGIN
	insert into [dbo].[LoanAgreementTemplateTypes] ( [TemplateType]) values('AlibabaCreditFacility');
END;

-- handle LoanAgreementTemplate and LoanAgreementTemplateTypes 
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanAgreementTemplate') AND name = 'TemplateTypeID')
	ALTER TABLE [dbo].[LoanAgreementTemplate] ADD TemplateTypeID [int] NULL ;	
GO
IF NOT EXISTS (select object_id from sys.all_objects where type = 'D' and name = 'DF_TemplateTypeID')
	alter table [dbo].[LoanAgreementTemplate] add constraint DF_TemplateTypeID default 1 for TemplateTypeID	
GO
IF NOT EXISTS (select object_id from sys.all_objects where type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanAgreementTemplate_LoanAgreementTemplateTypes') BEGIN
	ALTER TABLE [dbo].[LoanAgreementTemplate] ADD CONSTRAINT FK_LoanAgreementTemplate_LoanAgreementTemplateTypes FOREIGN KEY(TemplateTypeID) REFERENCES [dbo].[LoanAgreementTemplateTypes] ([TemplateTypeID]) ;
END ;
IF EXISTS(SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanAgreementTemplate') AND name = 'TemplateTypeID') BEGIN
UPDATE [dbo].[LoanAgreementTemplate] SET [TemplateTypeID] = TemplateType;
END;

-- populate [NL_PaymentStatuses] (enum PaymentStatus)
IF NOT EXISTS( select [PaymentStatusID] from [dbo].[NL_PaymentStatuses] where PaymentStatus = 'PaidOnTime') BEGIN
	insert into [dbo].[NL_PaymentStatuses] ([PaymentStatus]) values('PaidOnTime');
END;
IF NOT EXISTS( select [PaymentStatusID] from [dbo].[NL_PaymentStatuses] where PaymentStatus = 'Late') BEGIN
	insert into [dbo].[NL_PaymentStatuses] ([PaymentStatus]) values('Late');
END;
IF NOT EXISTS( select [PaymentStatusID] from [dbo].[NL_PaymentStatuses] where PaymentStatus = 'Early') BEGIN
	insert into [dbo].[NL_PaymentStatuses] ([PaymentStatus]) values('Early');
END;
IF NOT EXISTS( select [PaymentStatusID] from [dbo].[NL_PaymentStatuses] where PaymentStatus = 'ChargeBack') BEGIN
	insert into [dbo].[NL_PaymentStatuses] ([PaymentStatus]) values('ChargeBack');
END;
IF NOT EXISTS( select [PaymentStatusID] from [dbo].[NL_PaymentStatuses] where PaymentStatus = 'WrongPayment') BEGIN
	insert into [dbo].[NL_PaymentStatuses] ([PaymentStatus]) values('WrongPayment');
END;
IF NOT EXISTS( select [PaymentStatusID] from [dbo].[NL_PaymentStatuses] where PaymentStatus = 'WriteOff ') BEGIN
	insert into [dbo].[NL_PaymentStatuses] ([PaymentStatus]) values('WriteOff');
END;

-- populate NL_PaypointTransactionStatuses (from customer)
IF NOT EXISTS( select TransactionStatus from [dbo].[NL_PaypointTransactionStatuses] where TransactionStatus = 'Done') BEGIN
	insert into [dbo].[NL_PaypointTransactionStatuses] (TransactionStatus) values('Done');
END;
IF NOT EXISTS( select TransactionStatus from [dbo].[NL_PaypointTransactionStatuses] where TransactionStatus = 'Error') BEGIN
	insert into [dbo].[NL_PaypointTransactionStatuses] (TransactionStatus) values('Error');
END;

-- populate NL_PacnetTransactionStatuses (to customer)
IF NOT EXISTS( select TransactionStatus from [dbo].[NL_PacnetTransactionStatuses] where TransactionStatus = 'InProgress') BEGIN
	insert into [dbo].[NL_PacnetTransactionStatuses] (TransactionStatus) values('InProgress');
END;
IF NOT EXISTS( select TransactionStatus from [dbo].[NL_PacnetTransactionStatuses] where TransactionStatus = 'Done') BEGIN
	insert into [dbo].[NL_PacnetTransactionStatuses] (TransactionStatus) values('Done');
END;
IF NOT EXISTS( select TransactionStatus from [dbo].[NL_PacnetTransactionStatuses] where TransactionStatus = 'Error') BEGIN
	insert into [dbo].[NL_PacnetTransactionStatuses] (TransactionStatus) values('Error');
END;     

-- NL_RepaymentIntervalTypes
IF NOT EXISTS( select RepaymentIntervalType from [dbo].[NL_RepaymentIntervalTypes] where RepaymentIntervalType = 30 ) BEGIN -- 'Month'
	insert into [dbo].[NL_RepaymentIntervalTypes] (RepaymentIntervalType) values(30);
END;     
 IF NOT EXISTS( select RepaymentIntervalType from [dbo].[NL_RepaymentIntervalTypes] where RepaymentIntervalType = 1) BEGIN -- 'Day'
	insert into [dbo].[NL_RepaymentIntervalTypes] (RepaymentIntervalType) values(1);
END;  
 IF NOT EXISTS( select RepaymentIntervalType from [dbo].[NL_RepaymentIntervalTypes] where RepaymentIntervalType = 7) BEGIN -- 'Week'
	insert into [dbo].[NL_RepaymentIntervalTypes] (RepaymentIntervalType) values(7);
END; 
 IF NOT EXISTS( select RepaymentIntervalType from [dbo].[NL_RepaymentIntervalTypes] where RepaymentIntervalType = 10) BEGIN -- 'TenDays'
	insert into [dbo].[NL_RepaymentIntervalTypes] (RepaymentIntervalType) values(10);
END; 		 

-- NL_OfferStatuses
IF NOT EXISTS( select OfferStatus from [dbo].[NL_OfferStatuses] where OfferStatus = 'Live') BEGIN 
	insert into [dbo].[NL_OfferStatuses] (OfferStatus) values('Live');
END;
IF NOT EXISTS( select OfferStatus from [dbo].[NL_OfferStatuses] where OfferStatus = 'Pending') BEGIN -- for offers from "Manual" decision
	insert into [dbo].[NL_OfferStatuses] (OfferStatus) values('Pending');
END;

 	
-- [ConfigurationVariables] Collection_Max_Cancel_Fee for roles: Collector, Underwriter, Manager
IF NOT EXISTS( select [Name] from [dbo].[ConfigurationVariables] where [Name] = 'Collection_Max_Cancel_Fee_Role_Collector' )
insert into [dbo].[ConfigurationVariables] ([Name] ,[Value] ,[Description]) values ('Collection_Max_Cancel_Fee_Role_Collector', 200, 'Maximal amount of late fee cancellation for user in role Collector');
ELSE 
update [dbo].[ConfigurationVariables] set [Value] = 200, [Description]= 'Maximal amount of late fee cancellation for user in role Collector' where [Name] = 'Collection_Max_Cancel_Fee_Role_Collector';
IF NOT EXISTS( select [Name] from [dbo].[ConfigurationVariables] where [Name] = 'Collection_Max_Cancel_Fee_Role_Underwriter' )
insert into [dbo].[ConfigurationVariables] ([Name] ,[Value] ,[Description]) values ('Collection_Max_Cancel_Fee_Role_Underwriter', 1000, 'Maximal amount of late fee cancellation for user in role Underwriter');
ELSE 
update [dbo].[ConfigurationVariables] set [Value] = 1000, [Description]= 'Maximal amount of late fee cancellation for user in role Underwriter' where [Name] = 'Collection_Max_Cancel_Fee_Role_Underwriter';
IF NOT EXISTS( select [Name] from [dbo].[ConfigurationVariables] where [Name] = 'Collection_Max_Cancel_Fee_Role_Manager' )
insert into [dbo].[ConfigurationVariables] ([Name] ,[Value] ,[Description]) values ('Collection_Max_Cancel_Fee_Role_Manager', 5000, 'Maximal amount of late fee cancellation for user in role Manager');
ELSE 
update [dbo].[ConfigurationVariables] set [Value] = 5000, [Description]= 'Maximal amount of late fee cancellation for user in role Manager' where [Name] = 'Collection_Max_Cancel_Fee_Role_Manager';

-- [ConfigurationVariables] Collection_Move_To_Next_Payment_Max_Days (15 days)
IF NOT EXISTS( select [Name] from [dbo].[ConfigurationVariables] where [Name] = 'Collection_Move_To_Next_Payment_Max_Days' ) BEGIN
insert into [dbo].[ConfigurationVariables] ([Name] ,[Value] ,[Description]) values ('Collection_Move_To_Next_Payment_Max_Days', 15,
 'Maximal days when extra principal is being forwarded to the next payment (in case of less than Collection_Move_To_Next_Payment_Max_Principal amount)');
END
ELSE BEGIN
update [dbo].[ConfigurationVariables] set [Value] = 15, [Description]= 'Maximal days when extra principal is being forwarded to the next payment (in case of less than Collection_Move_To_Next_Payment_Max_Principal amount)' where [Name] = 'Collection_Move_To_Next_Payment_Max_Days';
END
-- [ConfigurationVariables] Collection_Move_To_Next_Payment_Max_Principal (100 GBP)
IF NOT EXISTS( select [Name] from [dbo].[ConfigurationVariables] where [Name] = 'Collection_Move_To_Next_Payment_Max_Principal' ) BEGIN
insert into [dbo].[ConfigurationVariables] ([Name] ,[Value] ,[Description]) values ('Collection_Move_To_Next_Payment_Max_Principal', 100, 'Maximal principal amount is being forwarded to the next payment in case of less than Collection_Move_To_Next_Payment_Max_Days days late');
END
ELSE BEGIN
update [dbo].[ConfigurationVariables] set [Value] = 100, [Description]= 'Maximal principal amount is being forwarded to the next payment in case of less than Collection_Move_To_Next_Payment_Max_Days days late' where [Name] = 'Collection_Move_To_Next_Payment_Max_Principal';
END



 