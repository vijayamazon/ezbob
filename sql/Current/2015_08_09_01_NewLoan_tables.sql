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
	CREATE TABLE NL_OfferFees (
		OfferFeeID BIGINT IDENTITY(1, 1) NOT NULL,
		OfferID BIGINT NOT NULL,
		LoanFeeTypeID INT NOT NULL,
		Percent DECIMAL(18, 6) NULL,
		Amount DECIMAL(18, 6) NULL,
		OneTimePartPercent DECIMAL(18, 6) NOT NULL,
		DistributedPartPercent DECIMAL(18, 6) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_OfferFees PRIMARY KEY (OfferFeeID),
		CONSTRAINT FK_NL_OfferFees_Offer FOREIGN KEY (OfferID) REFERENCES NL_Offers (OfferID),
		CONSTRAINT FK_NL_OfferFees_FeeType FOREIGN KEY (LoanFeeTypeID) REFERENCES NL_LoanFeeTypes (LoanFeeTypeID),
		CONSTRAINT CHK_NL_OfferFees CHECK (
			(Percent IS NOT NULL OR Amount IS NOT NULL)
			AND
			(Percent IS NULL OR (0 < Percent AND Percent <= 1))
			AND
			(Amount IS NULL OR Amount > 0)
			AND
			0 <= OneTimePartPercent AND OneTimePartPercent <= 1
			AND
			0 <= DistributedPartPercent AND DistributedPartPercent <= 1
			AND
			OneTimePartPercent + DistributedPartPercent = 1
		)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanLegals') IS NULL
BEGIN
	CREATE TABLE NL_LoanLegals (
		LoanLegalID BIGINT IDENTITY(1, 1) NOT NULL,
		OfferID BIGINT NOT NULL,
		RepaymentPeriod INT NULL,
		Amount DECIMAL(18, 6) NOT NULL,
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
		CONSTRAINT PK_LoanLegals PRIMARY KEY (LoanLegalID),
		CONSTRAINT FK_NL_LoanLegals_Offer FOREIGN KEY (OfferID) REFERENCES NL_Offers (OfferID)
		-- Should check but will not add a constraint: RepaymentPeriod <= period in NL_Offers table
		-- Should check but will not add a constraint: Amount <= amount in NL_Offers table
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_Loans') IS NULL
BEGIN
	CREATE TABLE NL_Loans (
		LoanID BIGINT IDENTITY(1, 1) NOT NULL,
		OfferID BIGINT NOT NULL,
		LoanTypeID INT NOT NULL,
		RepaymentIntervalTypeID INT NOT NULL,
		LoanStatusID INT NOT NULL,
		EzbobBankAccountID INT NULL,
		LoanSourceID INT NOT NULL,
		Position INT NOT NULL,
		InitialLoanAmount DECIMAL (18, 6) NOT NULL,
		CreationTime DATETIME NOT NULL,
		IssuedTime DATETIME NOT NULL,
		RepaymentCount INT NOT NULL,
		Refnum NVARCHAR(50) NOT NULL,
		DateClosed DATETIME NULL,
		InterestRate DECIMAL(18, 6) NOT NULL,
		InterestOnlyRepaymentCount INT NOT NULL,
		OldLoanID BIGINT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_Loans PRIMARY KEY (LoanID),
		CONSTRAINT FK_NL_Loans_Offer FOREIGN KEY (OfferID) REFERENCES NL_Offers (OfferID),
		CONSTRAINT FK_NL_Loans_LoanType FOREIGN KEY (LoanTypeID) REFERENCES LoanType (Id),
		CONSTRAINT FK_NL_Loans_Interval FOREIGN KEY (RepaymentIntervalTypeID) REFERENCES NL_RepaymentIntervalTypes (RepaymentIntervalTypeID),
		CONSTRAINT FK_NL_Loans_Status FOREIGN KEY (LoanStatusID) REFERENCES NL_LoanStatuses (LoanStatusID),
		CONSTRAINT FK_NL_Loans_Account FOREIGN KEY (EzbobBankAccountID) REFERENCES NL_EzbobBankAccouns (EzbobBankAccountID),
		CONSTRAINT FK_NL_Loans_Source FOREIGN KEY (LoanSourceID) REFERENCES LoanSource (LoanSourceID),
		CONSTRAINT FK_NL_Loans_Old FOREIGN KEY (OldLoanID) REFERENCES Loan (Id),
		CONSTRAINT CHK_NL_Loans CHECK (
			Position >= 1
			AND
			InitialLoanAmount > 0
			AND
			RepaymentCount > 0
			AND
			InterestRate >= 0
			AND
			0 <= InterestOnlyRepaymentCount AND InterestOnlyRepaymentCount < RepaymentCount
		)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanLienLinks') IS NULL
BEGIN
	CREATE TABLE NL_LoanLienLinks (
		LoanLienLinkID INT IDENTITY(1, 1) NOT NULL,
		LoanID BIGINT NOT NULL,
		LoanLienID INT NOT NULL,
		Amount DECIMAL(18, 6) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_LoanLienLinks PRIMARY KEY (LoanLienLinkID),
		CONSTRAINT FK_NL_LoanLienLinks_Loan FOREIGN KEY (LoanID) REFERENCES NL_Loans (LoanID),
		CONSTRAINT FK_NL_LoanLienLinks_Lien FOREIGN KEY (LoanLienID) REFERENCES LoanLien (LoanLienID),
		CONSTRAINT CHK_LoanLienLinks CHECK (Amount > 0)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanFees') IS NULL
BEGIN
	CREATE TABLE NL_LoanFees (
		LoanFeeID BIGINT IDENTITY(1, 1) NOT NULL,
		LoanID BIGINT NOT NULL,
		LoanFeeTypeID INT NOT NULL,
		AssignedByUserID INT NOT NULL, -- use 1 for automatically assigned fees.
		Amount DECIMAL(18, 6) NOT NULL,
		CreatedTime DATETIME NOT NULL,
		AssignTime DATETIME NOT NULL,
		DeletedByUserID INT NULL,
		DisabledTime DATETIME NULL,
		Notes NVARCHAR(MAX) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_LoanFees PRIMARY KEY (LoanFeeID),
		CONSTRAINT FK_NL_LoanFees_Loan FOREIGN KEY (LoanID) REFERENCES NL_Loans (LoanID),
		CONSTRAINT FK_NL_LoanFees_Type FOREIGN KEY (LoanFeeTypeID) REFERENCES NL_LoanFeeTypes (LoanFeeTypeID),
		CONSTRAINT FK_NL_LoanFees_Assigner FOREIGN KEY (AssignedByUserID) REFERENCES Security_User (UserId),
		CONSTRAINT FK_NL_LoanFees_Deleter FOREIGN KEY (DeletedByUserID) REFERENCES Security_User (UserId),
		CONSTRAINT CHK_NL_LoanFees CHECK (Amount > 0)
	)
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
	CREATE TABLE NL_LoanInterestFreeze (
		LoanInterestFreezeID BIGINT IDENTITY(1, 1) NOT NULL,
		LoanID BIGINT NOT NULL,
		StartDate DATETIME NULL,
		EndDate DATETIME NULL,
		InterestRate DECIMAL(18, 6) NOT NULL,
		ActivationDate DATETIME NULL,
		DeactivationDate DATETIME NULL,
		AssignedByUserID INT NOT NULL,
		DeletedByUserID INT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_LoanInterestFreeze PRIMARY KEY (LoanInterestFreezeID),
		CONSTRAINT FK_NL_LoanInterestFreeze_Loan FOREIGN KEY (LoanID) REFERENCES NL_Loans (LoanID),
		CONSTRAINT FK_NL_LoanInterestFreeze_Assigner FOREIGN KEY (AssignedByUserID) REFERENCES Security_User (UserId),
		CONSTRAINT FK_NL_LoanInterestFreeze_Deleter FOREIGN KEY (DeletedByUserID) REFERENCES Security_User (UserId)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanHistory') IS NULL
BEGIN
	CREATE TABLE NL_LoanHistory (
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
		CONSTRAINT PK_NL_LoanHistory PRIMARY KEY (LoanHistoryID),
		CONSTRAINT FK_NL_LoanHistory_Loan FOREIGN KEY (LoanID) REFERENCES NL_Loans (LoanID),
		CONSTRAINT FK_NL_LoanHistory_Rescheduler FOREIGN KEY (UserID) REFERENCES Security_User (UserId),
		CONSTRAINT FK_NL_LoanHistory_Legal FOREIGN KEY (LoanLegalID) REFERENCES NL_LoanLegals (LoanLegalID),
		CONSTRAINT CHK_NL_LoanHistory (
			Amount > 0
			AND
			RepaymentCount > 0
			AND
			InterestRate >= 0
		)
	)
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
	CREATE TABLE NL_LoanScheduleStatuses (
		LoanScheduleStatusID INT NOT NULL,
		LoanScheduleStatus NVARCHAR(50) NOT NULL,
		Description NVARCHAR(70) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_LoanScheduleStatuses PRIMARY KEY (LoanScheduleStatusID),
		CONSTRAINT UC_NL_LoanScheduleStatuses UNIQUE (LoanScheduleStatus)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanSchedules') IS NULL
BEGIN
	CREATE TABLE NL_LoanSchedules (
		LoanScheduleID BIGINT IDENTITY(1, 1) NOT NULL,
		LoanHistoryID BIGINT NOT NULL,
		LoanScheduleStatusID INT NOT NULL,
		Position INT NOT NULL,
		PlannedDate DATETIME NOT NULL,
		ClosedTime DATETIME NULL,
		Principal DECIMAL (18,6) NOT NULL,
		InterestRate DECIMAL(18, 6) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_LoanSchedules PRIMARY KEY (LoanScheduleID),
		CONSTRAINT FK_NL_LoanSchedules_Status FOREIGN KEY(LoanScheduleStatusID) REFERENCES NL_LoanScheduleStatuses (LoanScheduleStatusID),
		CONSTRAINT FK_NL_LoanSchedules_History FOREIGN KEY(LoanHistoryID) REFERENCES NL_LoanHistory (LoanHistoryID),
		CONSTRAINT CHK_NL_LoanSchedules CHECK (Principal > 0 AND InterestRate >= 0)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanRollovers') IS NULL
BEGIN
	CREATE TABLE NL_LoanRollovers (
		LoanRolloverID BIGINT IDENTITY(1, 1) NOT NULL,
		LoanHistoryID BIGINT NOT NULL,
		CreatedByUserID INT NOT NULL,
		DeletedByUserID INT NULL,
		LoanFeeID BIGINT NULL,
		FeeAmount DECIMAL (18, 6) NOT NULL,
		CreationTime DATETIME NOT NULL,
		ExpirationTime DATETIME NOT NULL,
		CustomerActionTime DATETIME NULL,
		IsAccepted BIT NULL,
		DeletionTime DATETIME NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_Rollovers PRIMARY KEY (LoanRolloverID),
		CONSTRAINT FK_NL_Rollovers_History FOREIGN KEY (LoanHistoryID) REFERENCES NL_LoanHistory (LoanHistoryID),
		CONSTRAINT FK_NL_Rollovers_Creator FOREIGN KEY (CreatedByUserID) REFERENCES Security_User (UserId),
		CONSTRAINT FK_NL_Rollovers_Deleter FOREIGN KEY (DeletedByUserID) REFERENCES Security_User (UserId),
		CONSTRAINT FK_NL_Rollovers_Fee FOREIGN KEY (LoanFeeID) REFERENCES NL_LoanFees (LoanFeeID)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_PacnetTransactionStatuses') IS NULL
BEGIN
	CREATE TABLE NL_PacnetTransactionStatuses (
		PacnetTransactionStatusID INT NOT NULL,
		TransactionStatus NVARCHAR(100) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_PacnetTransactionStatuses PRIMARY KEY (PacnetTransactionStatusID),
		CONSTRAINT UC_NL_PacnetTransactionStatuses UNIQUE (TransactionStatus)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_PacnetTransactions') IS NULL
BEGIN
	CREATE TABLE NL_PacnetTransactions (
		PacnetTransactionID BIGINT IDENTITY(1, 1) NOT NULL,
		FundTransferID BIGINT NOT NULL,
		TransactionTime DATETIME NOT NULL,
		Amount DECIMAL(18, 6) NOT NULL,
		Notes NVARCHAR(MAX) NULL,
		PacnetTransactionStatusID INT NOT NULL,
		StatusUpdatedTime DATETIME NOT NULL,
		TrackingNumber NVARCHAR(100) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_PacnetTransactions PRIMARY KEY (PacnetTransactionID),
		CONSTRAINT FK_NL_PacnetTransactions_FundTransfer FOREIGN KEY (FundTransferID) REFERENCES NL_FundTransfers (FundTransferID),
		CONSTRAINT FK_NL_PacnetTransactions_Status FOREIGN KEY (PacnetTransactionStatusID) REFERENCES NL_PacnetTransactionStatuses (PacnetTransactionStatusID)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_PaymentStatuses') IS NULL
BEGIN
	CREATE TABLE NL_PaymentStatuses (
		PaymentStatusID INT NOT NULL,
		PaymentStatus NVARCHAR(60) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_PaymentStatuses PRIMARY KEY (PaymentStatusID),
		CONSTRAINT UC_NL_PaymentStatuses UNIQUE (PaymentStatus),
		CONSTRAINT CHK_NL_PaymentStatuses CHECK (LTRIM(RTRIM(PaymentStatus)) != '')
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_Payments') IS NULL
BEGIN
	CREATE TABLE NL_Payments (
		PaymentID BIGINT IDENTITY(1, 1) NOT NULL,
		PaymentMethodID INT NOT NULL,
		PaymentTime DATETIME NOT NULL,
		Amount DECIMAL(18, 6) NOT NULL,
		PaymentStatusID INT NOT NULL,
		CreationTime DATETIME NOT NULL,
		CreatedByUserID INT NULL,
		DeletionTime DATETIME NULL,
		DeletedByUserID INT NULL,
		Notes NVARCHAR(MAX) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_Payment PRIMARY KEY (PaymentID),
		CONSTRAINT FK_NL_Payments_Method FOREIGN KEY (PaymentMethodID) REFERENCES LoanTransactionMethod (Id),
		CONSTRAINT FK_NL_Payments_Status FOREIGN KEY (PaymentStatusID) REFERENCES NL_PaymentStatuses (PaymentStatusID),
		CONSTRAINT FK_NL_Payments_Creator FOREIGN KEY (CreatedByUserID) REFERENCES Security_User (UserId),
		CONSTRAINT FK_NL_Payments_Deleter FOREIGN KEY (DeletedByUserID) REFERENCES Security_User (UserId)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanSchedulePayments') IS NULL
BEGIN
	CREATE TABLE NL_LoanSchedulePayments (
		LoanSchedulePaymentID BIGINT NOT NULL,
		LoanScheduleID BIGINT NOT NULL,
		PaymentID BIGINT NOT NULL,
		PrincipalPaid DECIMAL(18, 6) NOT NULL,
		InterestPaid DECIMAL(18, 6) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_LoanSchedulePayments PRIMARY KEY (LoanSchedulePaymentID),
		CONSTRAINT FK_NL_LoanSchedulePayments_Schedule FOREIGN KEY (LoanScheduleID),
		CONSTRAINT FK_NL_LoanSchedulePayments_Payment FOREIGN KEY (PaymentID),
		CONSTRAINT UC_NL_LoanSchedulePayments UNIQUE (LoanScheduleID, PaymentID),
		CONSTRAINT CHK_NL_LoanSchedulePayments_Principal CHECK (PrincipalPaid >= 0),
		CONSTRAINT CHK_NL_LoanSchedulePayments_Interest CHECK (InterestPaid >= 0),
		CONSTRAINT CHK_NL_LoanSchedulePayments_Paid CHECK (PrincipalPaid + InterestPaid > 0)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanFeePayments') IS NULL
BEGIN
	CREATE TABLE NL_LoanFeePayments (
		LoanFeePaymentID BIGINT IDENTITY(1, 1) NOT NULL,
		LoanFeeID BIGINT NOT NULL,
		PaymentID BIGINT NOT NULL,
		Amount DECIMAL(18, 6) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LoanFeePayments PRIMARY KEY (LoanFeePaymentID),
		CONSTRAINT FK_LoanFeePayments_Fee FOREIGN KEY (LoanFeeID) REFERENCES NL_LoanFess (LoanFeeID),
		CONSTRAINT FK_LoanFeePayments_Payment FOREIGN KEY (PaymentID) REFERENCES NL_Payment (PaymentID),
		CONSTRAINT UC_LoanFeePayments UNIQUE (LoanFeeID, PaymentID),
		CONSTRAINT CHK_LoanFeePayments CHECK (Amount > 0)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_PaypointTransactionStatuses') IS NULL
BEGIN
	CREATE TABLE NL_PaypointTransactionStatuses (
		PaypointTransactionStatusID INT NOT NULL,
		TransactionStatus NVARCHAR(100) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_PaypointTransactionStatuses PRIMARY KEY (PaypointTransactionStatusID),
		CONSTRAINT UC_NL_PaypointTransactionStatuses UNIQUE (TransactionStatus),
		CONSTRAINT CHK_NL_PaypointTransactionStatuses CHECK (LTRIM(RTRIM(TransactionStatus)) != '')
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_PaypointTransactions') IS NULL
BEGIN
	CREATE TABLE NL_PaypointTransactions (
		PaypointTransactionID INT NOT NULL,
		PaymentID BIGINT NOT NULL,
		TransactionTime DATETIME NOT NULL,
		Amount DECIMAL(18, 6) NOT NULL,
		Notes NVARCHAR(MAX) NULL,
		PaypointTransactionStatusID INT NOT NULL,
		PaypointUniqueID NVARCHAR(100) NOT NULL,
		PaypointCardID INT NOT NULL,
		IP NVARCHAR(32) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_PaypointTransactions PRIMARY KEY (PaypointTransactionID),
		CONSTRAINT FK_NL_PaypointTransactions_Payment FOREIGN KEY (PaymentID) REFERENCES NL_Payments (PaymentID),
		CONSTRAINT FK_NL_PaypointTransactions_Status FOREIGN KEY (PaypointTransactionStatusID) REFERENCES NL_PaypointTransactionStatuses (PaypointTransactionStatusID),
		CONSTRAINT FK_NL_PaypointTransactions_Card FOREIGN KEY (PaypointCardID) REFERENCES PayPointCard (Id),
		CONSTRAINT CHK_NL_PaypointTransactions CHECK (Amount > 0)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanStates') IS NULL
BEGIN
	CREATE TABLE NL_LoanStates (
		LoanStateID BIGINT IDENTITY(1, 1) NOT NULL,
		InsertDate DATETIME NOT NULL,
		LoanID BIGINT NOT NULL,
		NumberOfPayments INT NOT NULL,
		OutstandingPrincipal DECIMAL(18, 6) NOT NULL,
		OutstandingInterest DECIMAL(18, 6) NOT NULL,
		OutstandingFee DECIMAL(18, 6) NOT NULL,
		PaidPrincipal DECIMAL(18, 6) NOT NULL,
		PaidInterest DECIMAL(18, 6) NOT NULL,
		PaidFee DECIMAL(18, 6) NOT NULL,
		LateDays INT NOT NULL,
		LatePrincipal DECIMAL(18, 6) NOT NULL,
		LateInterest DECIMAL(18, 6) NOT NULL,
		WrittenOffPrincipal DECIMAL(18, 6) NOT NULL,
		WrittenOffInterest DECIMAL(18, 6) NOT NULL,
		WrittentOffFees DECIMAL(18, 6) NOT NULL,
		Notes NVARCHAR(MAX) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_LoanState PRIMARY KEY (LoanStateID),
		CONSTRAINT FK_NL_LoanState_Loan FOREIGN KEY (LoanID) REFERENCES NL_Loans (LoanID)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID ('NL_LoanOptions') IS NULL
BEGIN
	CREATE TABLE NL_LoanOptions (
		LoanOptionsID BIGINT IDENTITY(1, 1) NOT NULL,
		LoanID BIGINT NOT NULL,
		AutoCharge BIT NOT NULL,
		StopAutoChargeDate DATETIME NULL,
		AutoLateFees BIT NOT NULL,
		StopAutoLateFeesDate DATETIME NULL,
		AutoInterest BIT NOT NULL,
		StopAutoInterestDate DATETIME NULL,
		ReductionFee BIT NOT NULL,
		LatePaymentNotification BIT NOT NULL,
		CaisAccountStatus NVARCHAR(50) NULL,
		ManualCaisFlag NVARCHAR(20) NULL,
		EmailSendingAllowed BIT NOT NULL CONSTRAINT DF_NL_LoanOptions_SendEmail DEFAULT (1),
		MailSendingAllowed BIT NOT NULL CONSTRAINT DF_NL_LoanOptions_SendMail DEFAULT (1),
		SmsSendingAllowed BIT NOT NULL CONSTRAINT DF_NL_LoanOptions_SendSms DEFAULT (1),
		UserID INT,
		InsertDate DATETIME NOT NULL,
		IsActive BIT NOT NULL,
		Notes NVARCHAR(MAX) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_LoanOptions PRIMARY KEY (LoanOptionsID),
		CONSTRAINT FK_NL_LoanOptions_Loan FOREIGN KEY (LoanID) REFERENCES NL_Loans (LoanID),
		CONSTRAINT FK_NL_LoanOptions_User FOREIGN KEY (UserID) REFERENCES Security_User (UserId)
	)
END
GO

