SET QUOTED_IDENTIFIER ON
GO

SET ANSI_NULLS ON
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanAgreementTemplateTypes') IS NULL
BEGIN
	CREATE TABLE NL_LoanAgreementTemplateTypes (
		TemplateTypeID INT NOT NULL,
		TemplateType NVARCHAR(50) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_LoanAgreementTemplateTypes PRIMARY KEY (TemplateTypeID)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_BlendedLoans') IS NULL
BEGIN
	CREATE TABLE NL_BlendedLoans (
		BlendedLoanID BIGINT IDENTITY(1, 1) NOT NULL,
		LoanID BIGINT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_BlendedLoans PRIMARY KEY (BlendedLoanID)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_BlendedOffers') IS NULL
BEGIN
	CREATE TABLE NL_BlendedOffers (
		BlendedOfferID BIGINT IDENTITY(1, 1) NOT NULL,
		OfferID BIGINT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_ParentOffers PRIMARY KEY (BlendedOfferID)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_CashRequestOrigins') IS NULL
BEGIN
	CREATE TABLE NL_CashRequestOrigins (
		CashRequestOriginID INT NOT NULL,
		CashRequestOrigin NVARCHAR(50) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_CashRequestOrigins PRIMARY KEY (CashRequestOriginID)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_CashRequests') IS NULL
BEGIN
	CREATE TABLE NL_CashRequests (
		CashRequestID BIGINT IDENTITY(1, 1) NOT NULL,
		CustomerID INT NOT NULL,
		RequestTime DATETIME NOT NULL,
		CashRequestOriginID INT NOT NULL,
		UserID INT NOT NULL,
		OldCashRequestID BIGINT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_CashRequests PRIMARY KEY (CashRequestID),
		CONSTRAINT FK_NL_CashRequests_Customer FOREIGN KEY (CustomerID) REFERENCES Customer (Id),
		CONSTRAINT FK_NL_CashRequests_Origin FOREIGN KEY (CashRequestOriginID) REFERENCES NL_CashRequestOrigins (CashRequestOriginID),
		CONSTRAINT FK_NL_CashRequests_User FOREIGN KEY (UserID) REFERENCES Security_User (UserId),
		CONSTRAINT FK_NL_CashRequests_Old FOREIGN KEY (OldCashRequestID) REFERENCES CashRequests (Id)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_Decisions') IS NULL
BEGIN
	CREATE TABLE NL_Decisions(
		DecisionID BIGINT IDENTITY(1, 1) NOT NULL,
		CashRequestID BIGINT NOT NULL,
		UserID INT NOT NULL,
		DecisionNameID INT NOT NULL,
		DecisionTime DATETIME NOT NULL,
		Position INT NOT NULL,
		Notes NVARCHAR(MAX) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_Decisions PRIMARY KEY (DecisionID),
		CONSTRAINT FK_NL_Decisions_CashRequest FOREIGN KEY (CashRequestID) REFERENCES NL_CashRequests (CashRequestID),
		CONSTRAINT FK_NL_Decisions_User FOREIGN KEY (UserID) REFERENCES Security_User (UserId),
		CONSTRAINT FK_NL_Decisions_Decision FOREIGN KEY (DecisionNameID) REFERENCES Decisions (DecisionID)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_DecisionRejectReasons') IS NULL
BEGIN
	CREATE TABLE NL_DecisionRejectReasons(
		DecisionRejectReasonID INT NOT NULL,
		DecisionID BIGINT NOT NULL,
		RejectReasonID INT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_DecisionRejectReasons PRIMARY KEY (DecisionRejectReasonID),
		CONSTRAINT FK_NL_DecisionRejectReasons_Decision FOREIGN KEY (DecisionID) REFERENCES NL_Decisions (DecisionID),
		CONSTRAINT FK_NL_DecisionRejectReasons_Reason FOREIGN KEY (RejectReasonID) REFERENCES RejectReason (Id)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_DiscountPlans') IS NULL
BEGIN
	CREATE TABLE NL_DiscountPlans (
		DiscountPlanID INT NOT NULL,
		DiscountPlan NVARCHAR(80) NOT NULL,
		IsDefault BIT NOT NULL,
		IsActive BIT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_DiscountPlans PRIMARY KEY (DiscountPlanID)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_DiscountPlanEntries') IS NULL
BEGIN
	CREATE TABLE NL_DiscountPlanEntries (
		DiscountPlanEntryID INT IDENTITY(1, 1) NOT NULL,
		DiscountPlanID INT NOT NULL,
		PaymentOrder INT NOT NULL,
		InterestDiscount DECIMAL(18, 6) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_DiscountPlanEntries PRIMARY KEY (DiscountPlanEntryID),
		CONSTRAINT PK_NL_DiscountPlanEntries_Plan FOREIGN KEY (DiscountPlanID) REFERENCES NL_DiscountPlans (DiscountPlanID)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_EzbobBankAccounts') IS NULL
BEGIN
	CREATE TABLE NL_EzbobBankAccounts (
		EzbobBankAccountID INT NOT NULL,
		EzbobBankAccount NVARCHAR(10) NOT NULL,
		IsDefault BIT NOT,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_EzbobBankAccounts PRIMARY KEY (EzbobBankAccountID)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_FundTransferStatuses') IS NULL
BEGIN
	CREATE TABLE NL_FundTransferStatuses (
		FundTransferStatusID INT NOT NULL,
		FundTransferStatus NVARCHAR(64) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_FundTransferStatuses PRIMARY KEY (FundTransferStatusID),
		CONSTRAINT UC_NL_FundTransferStatuses UNIQUE (FundTransferStatus),
		CONSTRAINT CHK_NL_FundTransferStatuses CHECK (LTRIM(RTRIM(FundTransferStatus)))
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanFeeTypes') IS NULL
BEGIN
	CREATE TABLE NL_LoanFeeTypes (
		LoanFeeTypeID INT NOT NULL,
		LoanFeeType NVARCHAR(50) NOT NULL,
		DefaultAmount DECIMAL(18, 6) NULL,
		Description NVARCHAR(MAX) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_LoanFeeTypes PRIMARY KEY (LoanFeeTypeID)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanStatuses') IS NULL
BEGIN
	CREATE TABLE NL_LoanStatuses (
		LoanStatusID INT NOT NULL,
		LoanStatus NVARCHAR(80) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_LoanStatuses PRIMARY KEY (LoanStatusID)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_RepaymentIntervalTypes') IS NULL
BEGIN
	CREATE TABLE NL_RepaymentIntervalTypes (
		RepaymentIntervalTypeID INT NOT NULL,
		IsMonthly BIT NOT NULL,
		LengthInDays INT NULL,
		Description NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_RepaymentIntervalTypes PRIMARY KEY (RepaymentIntervalTypeID),
		CONSTRAINT UC_RepaymentIntervalTypes UNIQUE (IsMonthly, LengthInDays),
		CONSTRAINT CHK_RepaymentIntervalTypes_Length CHECK (
			(IsMonthly = 1 AND LengthInDays IS NULL) OR
			(IsMonthly = 0 AND LengthInDays > 0)
		),
		CONSTRAINT CHK_RepaymentIntervalTypes_Description CHECK (LTRIM(RTRIM(Description)) != '')
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_Offers') IS NULL
BEGIN
	CREATE TABLE NL_Offers (
		OfferID BIGINT IDENTITY(1, 1) NOT NULL,
		DecisionID BIGINT NOT NULL,
		LoanTypeID INT NOT NULL,
		RepaymentIntervalTypeID INT NOT NULL,
		LoanSourceID INT NOT NULL,
		StartTime DATETIME NOT NULL,
		EndTime DATETIME NOT NULL,
		RepaymentCount INT NOT NULL,
		Amount DECIMAL(18, 6) NOT NULL,
		MonthlyInterestRate DECIMAL (18, 6) NOT NULL,
		CreatedTime DATETIME NOT NULL,
		BrokerSetupFeePercent DECIMAL(18, 6) NULL,
		SetupFeeAddedToLoan BIT NOT NULL CONSTRAINT DF_Offers_SF_Added DEFAULT (0),
		Notes NVARCHAR(MAX) NULL,
		InterestOnlyRepaymentCount INT NULL,
		DiscountPlanID INT NULL,
		IsLoanTypeSelectionAllowed BIT NOT NULL CONSTRAINT DF_Offers_LoanTypeSelectable DEFAULT (0),
		SendEmailNotification BIT NOT NULL CONSTRAINT DF_Offers_SendEmails DEFAULT (1),
		IsRepaymentPeriodSelectionAllowed BIT CONSTRAINT DF_Offers_PeriodSelectable DEFAULT (0),
		IsAmountSelectionAllowed BIT NOT NULL CONSTRAINT DF_Offers_AmountSelectable DEFAULT (1),
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_Offers PRIMARY KEY (OfferID),
		CONSTRAINT FK_NL_Offers_Decision (DecisionID) REFERENCES NL_Decisions (DecisionID),
		CONSTRAINT FK_NL_Offers_LoanType (LoanTypeID) REFERENCES LoanType (Id),
		CONSTRAINT FK_NL_Offers_Period (RepaymentIntervalTypeID) REFERENCES NL_RepaymentIntervalTypes (RepaymentIntervalTypeID),
		CONSTRAINT FK_NL_Offers_LoanSource (LoanSourceID) REFERENCES LoanSource (LoanSourceID)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_OfferFees') IS NULL
BEGIN
CREATE TABLE NL_OfferFees(
	OfferFeeID BIGINT IDENTITY(1, 1) NOT NULL,
	OfferID BIGINT NOT NULL,
	LoanFeeTypeID INT NOT NULL,
	Percent DECIMAL(18, 6) NULL,
	Amount DECIMAL(18, 6) NULL,
	OneTimePartPercent DECIMAL(18, 6) NULL,
	DistributedPartPercent DECIMAL(18, 6) NULL,
	TimestampCounter ROWVERSION,
  CONSTRAINT PK_NL_OfferFees PRIMARY KEY CLUSTERED (OfferFeeID ASC)
 );
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanLegals') IS NULL
BEGIN
CREATE TABLE NL_LoanLegals(
	LoanLegalID BIGINT IDENTITY(1, 1) NOT NULL,
	OfferID BIGINT NOT NULL,
	RepaymentPeriod INT NULL,
	Amount  DECIMAL(18, 6) NOT NULL,
	SignatureTime DATETIME NOT NULL,
	CreditActAgreementAgreed BIT NULL,
	PreContractAgreementAgreed BIT NULL,
	PrivateCompanyLoanAgreementAgreed BIT NULL,
	GuarantyAgreementAgreed BIT NULL,
	EUAgreementAgreed BIT NULL,
	COSMEAgreementAgreed BIT NULL,
	NotInBankruptcy BIT NULL,
	SignedName NVARCHAR(128) NULL,
	TimestampCounter ROWVERSION,
 CONSTRAINT PK_LoanLegals PRIMARY KEY CLUSTERED (	LoanLegalID ASC)
) ;
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanLienLinks') IS NULL
BEGIN
CREATE TABLE NL_LoanLienLinks(
	LoanLienLinkID INT IDENTITY(1,1) NOT NULL,
	LoanID BIGINT NOT NULL,
	LoanLienID INT NOT NULL,
	Amount DECIMAL(18, 6) NOT NULL,
	TimestampCounter ROWVERSION,
 CONSTRAINT PK_NL_LoanLienLinks PRIMARY KEY CLUSTERED (LoanLienLinkID ASC)
) ;
END
GO


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_Loans') IS NULL
BEGIN
CREATE TABLE NL_Loans(
	LoanID BIGINT IDENTITY(1, 1) NOT NULL,
	OfferID BIGINT NOT NULL,
	LoanTypeID INT NOT NULL,
	RepaymentIntervalTypeID INT NOT NULL,
	LoanStatusID INT NOT NULL,
	EzbobBankAccountID INT NULL,
	LoanSourceID INT NOT NULL,
	Position INT NOT NULL,
	InitialLoanAmount DECIMAL (18,6) NULL,
	CreationTime DATETIME NOT NULL,
	IssuedTime DATETIME NOT NULL,
	RepaymentCount INT NOT NULL,
	Refnum NVARCHAR(50) NOT NULL,
	DateClosed DATETIME NULL,
	InterestRate DECIMAL(18, 6) NOT NULL,
	InterestOnlyRepaymentCount INT NULL,
	OldLoanID BIGINT NOT NULL,
	TimestampCounter ROWVERSION,
 CONSTRAINT PK_NL_Loans PRIMARY KEY CLUSTERED (LoanID ASC)
) ;
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanFees') IS NULL
BEGIN
CREATE TABLE NL_LoanFees(
	LoanFeeID BIGINT IDENTITY(1, 1) NOT NULL,
	LoanID BIGINT NOT NULL,
	LoanFeeTypeID INT NOT NULL,
	AssignedByUserID INT  NULL,
	Amount DECIMAL(18, 6) NOT NULL,
	CreatedTime DATETIME NOT NULL,
	AssignTime DATETIME NOT NULL,
	DeletedByUserID INT NULL,
	DisabledTime DATETIME NULL,
	Notes NVARCHAR(MAX) NULL,
	TimestampCounter ROWVERSION,
 CONSTRAINT PK_NL_LoanFees PRIMARY KEY CLUSTERED (LoanFeeID ASC)
) ;
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_FundTransfers') IS NULL
BEGIN
	CREATE TABLE NL_FundTransfers (
		FundTransferID BIGINT IDENTITY(1, 1) NOT NULL,
		LoanID BIGINT NOT NULL,
		Amount DECIMAL (18,6) NOT NULL,
		TransferTime DATETIME NOT NULL,
		FundTransferStatusID INT NOT NULL,
		LoanTransactionMethodID INT NOT NULL,
		DeletionTime DATETIME NULL,
		DeletedByUserID INT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_FundTransfers PRIMARY KEY (FundTransferID),
		CONSTRAINT FK_NL_FundTransfers_Loan FOREIGN KEY (LoanID) REFERENCES NL_Loans (LoanID),
		CONSTRAINT FK_NL_FundTransfers_Status FOREIGN KEY (FundTransferStatusID) REFERENCES NL_FundTransferStatuses (FundTransferStatusID),
		CONSTRAINT FK_NL_FundTransfers_Method FOREIGN KEY (LoanTransactionMethodID) REFERENCES LoanTransactionMethod (Id),
		CONSTRAINT FK_NL_FundTransfers_User FOREIGN KEY (DeletedByUserID) REFERENCES Security_User (UserId),
		CONSTRAINT CHK_NL_FundTransfers CHECK (
			(DeletionTime IS     NULL AND DeletedByUserID IS     NULL) OR
			(DeletionTime IS NOT NULL AND DeletedByUserID IS NOT NULL)
		)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanInterestFreeze') IS NULL
BEGIN
CREATE TABLE NL_LoanInterestFreeze(
	LoanInterestFreezeID BIGINT IDENTITY(1, 1) NOT NULL,
	LoanID BIGINT NOT NULL,
	StartDate DATETIME NULL,
	EndDate DATETIME NULL,
	InterestRate DECIMAL(18, 6) NOT NULL,
	ActivationDate DATETIME NULL,
	DeactivationDate DATETIME NULL,
	TimestampCounter ROWVERSION,
 CONSTRAINT PK_NL_LoanInterestFreeze PRIMARY KEY CLUSTERED (LoanInterestFreezeID ASC),
) ;
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanSchedulePayments') IS NULL
BEGIN
CREATE TABLE NL_LoanSchedulePayments(
	LoanSchedulePaymentID BIGINT NOT NULL,
	LoanScheduleID BIGINT NOT NULL,
	PaymentID BIGINT NOT NULL,
	PrincipalPaid DECIMAL(18, 6) NOT NULL,
	InterestPaid DECIMAL(18, 6) NOT NULL,
	TimestampCounter ROWVERSION,
 CONSTRAINT PK_NL_LoanSchedulePayments PRIMARY KEY CLUSTERED (LoanSchedulePaymentID ASC)
) ;
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanHistory') IS NULL
BEGIN
CREATE TABLE NL_LoanHistory(
	LoanHistoryID BIGINT IDENTITY(1, 1) NOT NULL,
	LoanID BIGINT NOT NULL,
	UserID INT NULL,
	LoanLegalID BIGINT NULL,
	Amount DECIMAL(18, 6) NOT NULL,
	RepaymentCount INT NOT NULL,
	InterestRate DECIMAL(18, 6) NULL,
	EventTime DATETIME NOT NULL,
	Description NVARCHAR(MAX) NOT NULL,
	AgreementModel NVARCHAR(MAX) NULL,
	TimestampCounter ROWVERSION,
 CONSTRAINT PK_NL_LoanHistory PRIMARY KEY CLUSTERED (LoanHistoryID ASC)
) ;
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanAgreements') IS NULL
BEGIN
	CREATE TABLE NL_LoanAgreements (
		LoanAgreementID BIGINT IDENTITY(1, 1) NOT NULL,
		LoanHistoryID BIGINT NOT NULL,
		LoanAgreementTemplateID INT NULL,
		FilePath NVARCHAR(255) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_LoanAgreements PRIMARY KEY (LoanAgreementID),
		CONSTRAINT FK_NL_LoanAgreements_LoanHistory FOREIGN KEY (LoanHistoryID) REFERENCES NL_LoanHistory (LoanHistoryID),
		CONSTRAINT FK_NL_LoanAgreements_Template FOREIGN KEY (LoanAgreementTemplateID) REFERENCES LoanAgreementTemplate (Id)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanScheduleStatuses') IS NULL
BEGIN
CREATE TABLE NL_LoanScheduleStatuses(
	LoanScheduleStatusID INT NOT NULL,
	LoanScheduleStatus NVARCHAR(50) NOT NULL,
	Description NVARCHAR(70) NOT NULL,
	TimestampCounter ROWVERSION,
 CONSTRAINT PK_NL_LoanScheduleStatuses PRIMARY KEY CLUSTERED (LoanScheduleStatusID ASC)
) ;
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanSchedules') IS NULL
BEGIN
CREATE TABLE NL_LoanSchedules(
	LoanScheduleID BIGINT IDENTITY(1, 1) NOT NULL,
	LoanHistoryID BIGINT NOT NULL,
	LoanScheduleStatusID INT NOT NULL,
	Position INT NOT NULL,
	PlannedDate DATETIME NOT NULL,
	ClosedTime DATETIME NULL,
	Principal DECIMAL (18,6) NOT NULL,
	InterestRate DECIMAL(18, 6) NOT NULL,
	TimestampCounter ROWVERSION,
 CONSTRAINT PK_NL_LoanSchedules PRIMARY KEY CLUSTERED (LoanScheduleID ASC),
 CONSTRAINT FK_NL_LoanSchedules_NL_LoanScheduleStatuses FOREIGN KEY(LoanScheduleStatusID) REFERENCES NL_LoanScheduleStatuses (LoanScheduleStatusID),
 CONSTRAINT FK_NL_LoanSchedules_LoanHistory FOREIGN KEY(LoanHistoryID) REFERENCES NL_LoanHistory (LoanHistoryID)
) ;
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanRollovers') IS NULL
BEGIN
CREATE TABLE NL_LoanRollovers(
	LoanRolloverID BIGINT IDENTITY(1, 1) NOT NULL,
	LoanHistoryID BIGINT NOT NULL,
	CreatedByUserID INT NOT NULL,
	DeletedByUserID INT NULL,
	LoanFeeID BIGINT NULL,
	FeeAmount DECIMAL (18,6) NOT NULL,
	CreationTime DATETIME NOT NULL,
	ExpirationTime DATETIME NOT NULL,
	CustomerActionTime DATETIME NULL,
	IsAccepted BIT NULL,
	DeletionTime DATETIME NULL,
	TimestampCounter ROWVERSION,
 CONSTRAINT PK_NL_Rollovers PRIMARY KEY CLUSTERED (LoanRolloverID ASC)
) ;
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_PacnetTransactions') IS NULL
BEGIN
CREATE TABLE NL_PacnetTransactions(
	PacnetTransactionID BIGINT IDENTITY(1, 1) NOT NULL,
	FundTransferID BIGINT NOT NULL,
	TransactionTime DATETIME NOT NULL,
	Amount DECIMAL(18, 6) NOT NULL,
	Notes NVARCHAR(MAX) NULL,
	PacnetTransactionStatusID INT NOT NULL,
	StatusUpdatedTime DATETIME NOT NULL,
	TrackingNumber NVARCHAR(100) NOT NULL,
	TimestampCounter ROWVERSION,
 CONSTRAINT PK_NL_PacnetTransactions PRIMARY KEY CLUSTERED (	PacnetTransactionID ASC)
) ;
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_PacnetTransactionStatuses') IS NULL
BEGIN
CREATE TABLE NL_PacnetTransactionStatuses(
	PacnetTransactionStatusID INT NOT NULL,
	TransactionStatus NVARCHAR(100) NOT NULL,
	TimestampCounter ROWVERSION,
 CONSTRAINT PK_NL_PacnetTransactionStatuses PRIMARY KEY CLUSTERED (PacnetTransactionStatusID ASC)
) ;
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_PaymentStatuses') IS NULL
BEGIN
CREATE TABLE NL_PaymentStatuses(
	PaymentStatusID INT NOT NULL,
	PaymentStatus NVARCHAR(60) NOT NULL,
	TimestampCounter ROWVERSION,
 CONSTRAINT PK_NL_PaymentStatuses PRIMARY KEY CLUSTERED (PaymentStatusID ASC)
) ;
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_Payments') IS NULL
BEGIN
CREATE TABLE NL_Payments(
	PaymentID BIGINT IDENTITY(1, 1) NOT NULL,
	PaymentMethodID INT NOT NULL,
	PaymentTime DATETIME NOT NULL,
	Amount DECIMAL(18, 6) NOT NULL,
	PaymentStatusID INT NOT NULL,
	CreationTime DATETIME NOT NULL DEFAULT GETUTCDATE(), --real insert DATETIME
	CreatedByUserID INT NULL,
	DeletionTime DATETIME NULL,
	DeletedByUserID INT NULL,
	Notes NVARCHAR(MAX) NULL,
	TimestampCounter ROWVERSION,
 CONSTRAINT PK_NL_Payment PRIMARY KEY CLUSTERED (PaymentID ASC),
 CONSTRAINT FK_NL_Payments_NL_PaymentStatuses FOREIGN KEY(PaymentStatusID) REFERENCES NL_PaymentStatuses (PaymentStatusID)
) ;
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanFeePayments') IS NULL
BEGIN
CREATE TABLE NL_LoanFeePayments(
	LoanFeePaymentID BIGINT IDENTITY(1, 1) NOT NULL,
	LoanFeeID BIGINT NOT NULL,
	PaymentID BIGINT NOT NULL,
	Amount DECIMAL(18, 6) NOT NULL,
	TimestampCounter ROWVERSION,
 CONSTRAINT PK_LoanFeePayments PRIMARY KEY CLUSTERED (LoanFeePaymentID ASC)
) ;
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_PaypointTransactions') IS NULL
BEGIN
CREATE TABLE NL_PaypointTransactions(
	PaypointTransactionID INT NOT NULL,
	PaymentID BIGINT NOT NULL,
	TransactionTime DATETIME NOT NULL,
	Amount DECIMAL(18, 6) NOT NULL,
	Notes NVARCHAR(MAX) NULL,
	PaypointTransactionStatusID INT NOT NULL,
	PaypointUniqID NVARCHAR(100) NOT NULL,
	PaypointCardID INT NOT NULL,
	IP NVARCHAR(32) NULL,
	TimestampCounter ROWVERSION,
 CONSTRAINT PK_NL_PaypointTransactions PRIMARY KEY CLUSTERED (PaypointTransactionID ASC )
) ;
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_PaypointTransactionStatuses') IS NULL
BEGIN
CREATE TABLE NL_PaypointTransactionStatuses(
	PaypointTransactionStatusID INT NOT NULL,
	TransactionStatus NVARCHAR(100) NOT NULL,
	TimestampCounter ROWVERSION,
 CONSTRAINT PK_NL_PaypointTransactionStatuses PRIMARY KEY CLUSTERED (	PaypointTransactionStatusID ASC)
) ;
END ;

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanStates') IS NULL
BEGIN
CREATE TABLE NL_LoanStates(
	LoanStateID BIGINT IDENTITY(1, 1) NOT NULL,
	InsertDate  DATETIME NOT NULL DEFAULT GETUTCDATE(),
	LoanID BIGINT NOT NULL,
	NumberOfPayments INT NOT NULL,
	OutstandingPrincipal DECIMAL(18, 6) NOT NULL,
	OutstandingInterest DECIMAL(18, 6) NOT NULL,
	OutstandingFee DECIMAL(18, 6) NOT NULL,
	PaidPrincipal DECIMAL(18, 6) NOT NULL,
	PaidInterest DECIMAL(18, 6) NOT NULL,
	PaidFee DECIMAL(18, 6) NOT NULL,
	LateDays INT NOT NULL,
	LatePrincipal DECIMAL(18, 6) NULL,
	LateInterest DECIMAL(18, 6) NULL,
	WrittenOffPrincipal DECIMAL(18, 6)  NULL,
	WrittenOffInterest DECIMAL(18, 6) NULL,
	WrittentOffFees DECIMAL(18, 6) NULL,
	Notes ntext NULL,
	TimestampCounter ROWVERSION,
 CONSTRAINT PK_NL_LoanState PRIMARY KEY CLUSTERED ( LoanStateID ASC)
) ;
END ;

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID ('dbo.NL_LoanOptions') IS NULL
BEGIN
CREATE TABLE dbo.NL_LoanOptions(
	LoanOptionsID           BIGINT IDENTITY NOT NULL,
	LoanID                  BIGINT NOT NULL,
	AutoCharge              BIT,
	StopAutoChargeDate      DATETIME,
	AutoLateFees            BIT,
	StopAutoLateFeesDate    DATETIME,
	AutoInterest            BIT,
	StopAutoInterestDate    DATETIME,
	ReductionFee            BIT,
	LatePaymentNotification BIT,
	CaisAccountStatus       NVARCHAR(50),
	ManualCaisFlag          NVARCHAR(20),
	EmailSendingAllowed     BIT DEFAULT (1) NOT NULL,
	MailSendingAllowed      BIT DEFAULT (1) NOT NULL,
	SmsSendingAllowed       BIT DEFAULT (1) NOT NULL,
	UserID              	 INT,
	InsertDate             DATETIME DEFAULT (GETUTCDATE()) NOT NULL,
	IsActive                BIT NOT NULL DEFAULT(0),
	Notes                   NTEXT,
	TimestampCounter ROWVERSION,
	CONSTRAINT PK_NL_LoanOptions PRIMARY KEY (LoanOptionsID),
	CONSTRAINT FK_NL_LoanOptions_NL_Loans FOREIGN KEY (LoanID) REFERENCES dbo.NL_Loans (LoanID),
	CONSTRAINT FK_NL_LoanOptions_Security_User FOREIGN KEY (UserID) REFERENCES dbo.Security_User (UserId)
	);
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------


insert into NL_FundTransferStatuses Pending, Active, Deleted

IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanBrokerCommission') AND name = 'NLLoanID')
	ALTER TABLE LoanBrokerCommission ADD NLLoanID BIGINT NULL ;
GO
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('CollectionLog') AND name = 'LoanHistoryID')
	ALTER TABLE CollectionLog ADD LoanHistoryID BIGINT NULL ;
GO
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('CollectionLog') AND name = 'Comments')
	ALTER TABLE CollectionLog ADD Comments ntext null ;
GO
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('DecisionTrail') AND name = 'NLDecisionID')
	ALTER TABLE DecisionTrail ADD NLDecisionID BIGINT NULL ;
GO
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('Esignatures') AND name = 'DecisionID')
	ALTER TABLE Esignatures ADD DecisionID BIGINT NULL ;
GO
IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('DecisionTrail') AND name = 'NLCashRequestID')
	ALTER TABLE DecisionTrail ADD NLCashRequestID BIGINT NULL ;
GO


IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_CollectionLog_NL_LoanHistory')
 ALTER TABLE CollectionLog ADD CONSTRAINT FK_CollectionLog_NL_LoanHistory FOREIGN KEY(LoanHistoryID) REFERENCES NL_LoanHistory (LoanHistoryID) ;

 IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_Esignatures_NL_Decisions')
 ALTER TABLE Esignatures  ADD CONSTRAINT FK_Esignatures_NL_Decisions FOREIGN KEY(DecisionID) REFERENCES NL_Decisions (DecisionID) ;
GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'PRIMARY_KEY_CONSTRAINT' and name = 'PK_PaypointCard')
 ALTER TABLE PaypointCard ADD CONSTRAINT PK_PaypointCard PRIMARY KEY(Id)  ;
GO

IF(SELECT cl.OBJECT_ID FROM sys.all_objects ob inner join sys.all_columns cl on ob.OBJECT_ID = cl.OBJECT_ID  and ob.name = 'MedalCalculationsAV' and cl.name = 'CashRequestID') IS NULL
 ALTER TABLE MedalCalculationsAV ADD CashRequestID INT DEFAULT null;
GO
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_MedalCalculationsAV_NL_CashRequests')
 ALTER TABLE MedalCalculationsAV  ADD CONSTRAINT FK_MedalCalculationsAV_NL_CashRequests FOREIGN KEY(CashRequestID) REFERENCES NL_CashRequests (CashRequestID);
GO

IF(SELECT cl.OBJECT_ID FROM sys.all_objects ob inner join sys.all_columns cl on ob.OBJECT_ID = cl.OBJECT_ID  and ob.name = 'MedalCalculations' and cl.name = 'CashRequestID') IS NULL
 ALTER TABLE MedalCalculations ADD CashRequestID INT DEFAULT null;
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_MedalCalculations_NL_CashRequests')
 ALTER TABLE MedalCalculations ADD CONSTRAINT FK_MedalCalculations_NL_CashRequests FOREIGN KEY(CashRequestID) REFERENCES NL_CashRequests (CashRequestID);
GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_DecisionTrail_NL_CashRequests')
 ALTER TABLE DecisionTrail ADD CONSTRAINT FK_DecisionTrail_NL_CashRequests FOREIGN KEY(NLCashRequestID) REFERENCES NL_CashRequests (CashRequestID);
GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanBrokerCommission_NL_Loan')
 ALTER TABLE LoanBrokerCommission ADD CONSTRAINT FK_LoanBrokerCommission_NL_Loan FOREIGN KEY(NLLoanID) REFERENCES NL_Loans (LoanID) ;
GO

-- add FK_NL_Payments_LoanTransactionMethod
IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_NL_Payments_LoanTransactionMethod')
 ALTER TABLE NL_Payments ADD CONSTRAINT FK_NL_Payments_LoanTransactionMethod FOREIGN KEY(PaymentMethodID) REFERENCES LoanTransactionMethod (Id);
GO

-- NL_DiscountPlans/NL_DiscountPlanEntries migration

-- SortOrder field defines order of entries (ORDER BY SortOrder ASC, DiscountPlanEntryID DESC) and not number of repayment period. Entry is always related to repetition period. I.e. if the same plan is applied to monthly repaid loan and to weekly repaid loan and an entry in the second position says "-10%" that means that in the former case customer receives 10% discound for the second month while in the latter case customer receives 10% discount for the second week.
-- Value "0.1" in InterestRateDelta means "10%", value "-0.05" means "-5%".
BEGIN TRY
	DROP TABLE #discountplanTemp
END TRY
BEGIN CATCH
END CATCH

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
		if (SELECT DiscountPlan FROM dbo.NL_DiscountPlans WHERE DiscountPlan = @Name ) is null
		BEGIN
			INSERT INTO NL_DiscountPlans (DiscountPlan, IsDefault, IsActive) VALUES (ltrim(rtrim(@Name)), @IsDefault, @ForbiddenForReuse);
		END
		SELECT @NL_Id = DiscountPlanID FROM NL_DiscountPlans WHERE DiscountPlan = @Name;
		if @NL_Id is not null
		BEGIN
			if (SELECT COUNT(DiscountPlanEntryID) FROM NL_DiscountPlanEntries WHERE DiscountPlanID = @NL_Id group by DiscountPlanID) is null
			BEGIN
				INSERT INTO NL_DiscountPlanEntries (DiscountPlanID, PaymentOrder, InterestDiscount)
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

