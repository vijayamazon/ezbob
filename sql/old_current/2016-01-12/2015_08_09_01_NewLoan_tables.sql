SET QUOTED_IDENTIFIER ON
GO

SET ANSI_NULLS ON
GO


-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanFormulas') IS NULL
BEGIN
	CREATE TABLE NL_LoanFormulas (
		LoanFormulaID INT NOT NULL,
		FormulaName NVARCHAR(50) NOT NULL,
		IsActive BIT NOT NULL,
		Notes NVARCHAR(255) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_LoanFormulas PRIMARY KEY (LoanFormulaID)
	);
	
	INSERT INTO NL_LoanFormulas (LoanFormulaID, FormulaName, IsActive, Notes) VALUES
		( 1, 'EqualPrincipal', 1, 'keren shava'),
		( 2, 'FixedPayment', 0, 'Fixed Payment, shpitzer');
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'PRIMARY_KEY_CONSTRAINT' AND name = 'PK_PaypointCard')
	ALTER TABLE PaypointCard ADD CONSTRAINT PK_PaypointCard PRIMARY KEY (Id)
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

	INSERT INTO NL_LoanAgreementTemplateTypes (TemplateTypeID, TemplateType) VALUES
		(1, 'GuarantyAgreement'),
		(2, 'PreContractAgreement'),
		(3, 'CreditActAgreement'),
		(4, 'PrivateCompanyLoanAgreement'),
		(5, 'AlibabaGuarantyAgreement'),
		(6, 'AlibabaPreContractAgreement'),
		(7, 'AlibabaCreditActAgreement'),
		(8, 'AlibabaPrivateCompanyLoanAgreement'),
		(9, 'AlibabaCreditFacility')
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

	INSERT INTO NL_CashRequestOrigins (CashRequestOriginID, CashRequestOrigin) VALUES
		( 1, 'FinishedWizard'),
		( 2, 'QuickOffer'),
		( 3, 'RequestCashBtn'),
		( 4, 'NewCreditLineBtn'),
		( 5, 'Other'),
		( 6, 'RequalifyCustomerStrategy'),
		( 7, 'ForcedWizardCompletion'),
		( 8, 'Approved'),
		( 9, 'Manual'),
		(10, 'NewCreditLineSkipAll'),
		(11, 'NewCreditLineSkipAndGoAuto'),
		(12, 'NewCreditLineUpdateAndGoManual'),
		(13, 'NewCreditLineUpdateAndGoAuto')
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
	CREATE TABLE NL_Decisions (
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
		CONSTRAINT FK_NL_Decisions_Decision FOREIGN KEY (DecisionNameID) REFERENCES Decisions (DecisionID),
		CONSTRAINT CHK_NL_Decisions CHECK (		
			(Position >= 1)			
		)	
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_DecisionRejectReasons') IS NULL
BEGIN
	CREATE TABLE NL_DecisionRejectReasons (
		DecisionRejectReasonID INT IDENTITY(1, 1) NOT NULL,
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
		IsDefault BIT NOT NULL,
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
		CONSTRAINT CHK_NL_FundTransferStatuses CHECK (LTRIM(RTRIM(FundTransferStatus)) != '')
	)

	INSERT INTO NL_FundTransferStatuses (FundTransferStatusID, FundTransferStatus) VALUES
		(1, 'Pending'),
		(2, 'Active'),
		(3, 'Deleted')
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanFeeTypes') IS NULL
BEGIN
	CREATE TABLE NL_LoanFeeTypes (
		LoanFeeTypeID INT NOT NULL,
		LoanFeeType NVARCHAR(50) NOT NULL,
		[DefaultAmount] DECIMAL(18, 6) NULL,
		[Description] NVARCHAR(MAX) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_LoanFeeTypes PRIMARY KEY (LoanFeeTypeID)
	);

	INSERT INTO NL_LoanFeeTypes (LoanFeeTypeID, LoanFeeType, [DefaultAmount], [Description]) VALUES
		  ( 1, 'SetupFee',        NULL, 'One-time fee upon loan creation, may be added or didacted from loan')
		, ( 2, 'ServicingFee',    NULL, 'Distributed through the entire loan period. On paying early - not to charge remaining part')
		, ( 3, 'ArrangementFee',  NULL, 'Distributed through the payments. On paying early - all remaned amount need to be charged')
		, ( 4, 'RolloverFee',       50, 'A rollover has been agreed')
		, ( 5, 'AdminFee',          20, 'A fee applied when no payment is received or less than (repayment interest + late payment fee)')				
		, ( 6, 'LatePaymentFee',    20, 'A charge when an instalment is paid after 5 UK working days of the grace period')
		, (7, 'PartialPaymentFee', 45, 'A payment has been made (more than repayment interest + late payment fee but was not made in full)')
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

	INSERT INTO NL_LoanStatuses (LoanStatusID, LoanStatus) VALUES
		  (1, 'Pending')
		, (2, 'Live')
		, (3, 'Late') -- Overdue
		, (4, 'PaidOff') -- Paid
		, (5, 'WriteOff')
		-- , (6, 'Default')
		-- , (7, 'DebtManagement')
		-- , (8, '1-14DaysMissed')
		-- , (9, '15-30DaysMissed')
		-- , (10, '31-45DaysMissed')
		-- , (11, '46-90DaysMissed')
		-- , (12, '60-90DaysMissed')
		-- , (13, '90DaysMissed')
		-- , (14, 'Legal ??? claim process')
		-- , (15, 'Legal - apply for judgment')
		-- , (16, 'Legal: CCJ')
		-- , (17, 'Legal: bailiff')
		-- , (18, 'Legal: charging order')
		-- , (19, 'Collection: Tracing')
		-- , (20, 'Collection: Site Visit')
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
		[Description] NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_RepaymentIntervalTypes PRIMARY KEY (RepaymentIntervalTypeID),
		CONSTRAINT UC_RepaymentIntervalTypes UNIQUE (IsMonthly, LengthInDays),
		CONSTRAINT CHK_RepaymentIntervalTypes_Length CHECK (
			(IsMonthly = 1 AND LengthInDays IS NULL) OR
			(IsMonthly = 0 AND LengthInDays > 0)
		),
		CONSTRAINT CHK_RepaymentIntervalTypes_Description CHECK (LTRIM(RTRIM([Description])) != '')
	)

	INSERT INTO NL_RepaymentIntervalTypes (RepaymentIntervalTypeID, IsMonthly, LengthInDays, [Description]) VALUES
		(1, 1, NULL, 'Month'),
		(2, 0, 1, 'Day'),
		(3, 0, 7, 'Week'),
		(4, 0, 10, '10 days')
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
		MonthlyInterestRate DECIMAL(18, 6) NOT NULL,
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
		CONSTRAINT FK_NL_Offers_Decision FOREIGN KEY (DecisionID) REFERENCES NL_Decisions (DecisionID),
		CONSTRAINT FK_NL_Offers_LoanType FOREIGN KEY (LoanTypeID) REFERENCES LoanType (Id),
		CONSTRAINT FK_NL_Offers_Period FOREIGN KEY (RepaymentIntervalTypeID) REFERENCES NL_RepaymentIntervalTypes (RepaymentIntervalTypeID),
		CONSTRAINT FK_NL_Offers_LoanSource FOREIGN KEY (LoanSourceID) REFERENCES LoanSource (LoanSourceID),
		CONSTRAINT [FK_NL_Offers_NL_DiscountPlans] FOREIGN KEY([DiscountPlanID]) REFERENCES [dbo].[NL_DiscountPlans] ([DiscountPlanID])		
	)
END
GO

 IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type = 'D' and name = 'DF_NL_Offers_LoanType')
	ALTER TABLE [dbo].[NL_Offers] add constraint DF_NL_Offers_LoanType DEFAULT 1 for LoanTypeID	;
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_OfferFees') IS NULL
BEGIN
	CREATE TABLE NL_OfferFees (
		OfferFeeID BIGINT IDENTITY(1, 1) NOT NULL,
		OfferID BIGINT NOT NULL,
		LoanFeeTypeID INT NOT NULL,
		[Percent] DECIMAL(18, 6) NULL,
		AbsoluteAmount DECIMAL(18, 6) NULL,
		OneTimePartPercent DECIMAL(18, 6) NOT NULL CONSTRAINT DF_NL_OfferFees_OneTimePartPercent DEFAULT (1),
		DistributedPartPercent DECIMAL(18, 6) NOT NULL CONSTRAINT DF_NL_OfferFees_DistributedPartPercent DEFAULT (0),
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_OfferFees PRIMARY KEY (OfferFeeID),
		CONSTRAINT FK_NL_OfferFees_Offer FOREIGN KEY (OfferID) REFERENCES NL_Offers (OfferID),
		CONSTRAINT FK_NL_OfferFees_FeeType FOREIGN KEY (LoanFeeTypeID) REFERENCES NL_LoanFeeTypes (LoanFeeTypeID),
		CONSTRAINT CHK_NL_OfferFees CHECK (
			([Percent] IS NOT NULL OR AbsoluteAmount IS NOT NULL)
			AND
			([Percent] IS NULL OR (0 <= [Percent] AND [Percent] <= 1))
			AND
			(AbsoluteAmount IS NULL OR AbsoluteAmount > 0)
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
		LoanStatusID INT NOT NULL,
		LoanFormulaID INT NOT NULL DEFAULT (1),		
		LoanSourceID INT NOT NULL,		
		EzbobBankAccountID INT NULL,
		CreationTime DATETIME NOT NULL,
		Refnum NVARCHAR(50) NOT NULL,				
		Position INT NOT NULL,
		DateClosed DATETIME NULL,	
		PrimaryLoanID BIGINT NULL,	-- in the case of current loan is an auxiliary loan for other main loan (re-scheduled)		
		OldLoanID INT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_Loans PRIMARY KEY (LoanID),
		CONSTRAINT FK_NL_Loans_Offer FOREIGN KEY (OfferID) REFERENCES NL_Offers (OfferID),
		CONSTRAINT FK_NL_Loans_LoanType FOREIGN KEY (LoanTypeID) REFERENCES LoanType (Id),
		CONSTRAINT FK_NL_Loans_Status FOREIGN KEY (LoanStatusID) REFERENCES NL_LoanStatuses (LoanStatusID),
		CONSTRAINT FK_NL_Loans_Formula FOREIGN KEY (LoanFormulaID) REFERENCES NL_LoanFormulas (LoanFormulaID),
		CONSTRAINT FK_NL_Loans_Account FOREIGN KEY (EzbobBankAccountID) REFERENCES NL_EzbobBankAccounts (EzbobBankAccountID),
		CONSTRAINT FK_NL_Loans_Source FOREIGN KEY (LoanSourceID) REFERENCES LoanSource (LoanSourceID),
		CONSTRAINT FK_NL_Loans_Old FOREIGN KEY (OldLoanID) REFERENCES Loan (Id),
		CONSTRAINT CHK_NL_Loans CHECK (		
			(Position >= 1)
			AND 
			(LoanFormulaID in (1,2))
			--AND
			--((LoanFormulaID = 1 AND PaymentPerInterval IS NULL) OR (LoanFormulaID = 2 AND PaymentPerInterval IS NOT NULL))
		)		
	);
	
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
		CONSTRAINT FK_NL_LoanLienLinks_Lien FOREIGN KEY (LoanLienID) REFERENCES LoanLien (Id),
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
		AssignTime DATE NOT NULL,
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
		StartDate DATE NULL,
		EndDate DATE NULL,
		InterestRate DECIMAL(18, 6) NOT NULL,
		ActivationDate DATETIME NULL,
		DeactivationDate DATETIME NULL,
		AssignedByUserID INT NOT NULL,
		DeletedByUserID INT NULL,
		OldID INT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_LoanInterestFreeze PRIMARY KEY (LoanInterestFreezeID),
		CONSTRAINT FK_NL_LoanInterestFreeze_Loan FOREIGN KEY (LoanID) REFERENCES NL_Loans (LoanID),
		CONSTRAINT FK_NL_LoanInterestFreeze_Assigner FOREIGN KEY (AssignedByUserID) REFERENCES Security_User (UserId),
		CONSTRAINT FK_NL_LoanInterestFreeze_Deleter FOREIGN KEY (DeletedByUserID) REFERENCES Security_User (UserId),
		CONSTRAINT FK_NL_LoanInterestFreeze_Old FOREIGN KEY (OldID) REFERENCES [LoanInterestFreeze] (Id)
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
		RepaymentIntervalTypeID INT NOT NULL,
		RepaymentCount INT NOT NULL,
		InterestRate DECIMAL(18, 6) NOT NULL,
		EventTime DATETIME NOT NULL,
		[Description] NVARCHAR(MAX) NOT NULL,		
		RepaymentDate DATETIME NOT NULL,
		PaymentPerInterval DECIMAL(18, 6) NULL,	-- in "fixed payment" formula
		AgreementModel NVARCHAR(MAX) NULL,
		InterestOnlyRepaymentCount INT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_LoanHistory PRIMARY KEY (LoanHistoryID),
		CONSTRAINT FK_NL_LoanHistory_Loan FOREIGN KEY (LoanID) REFERENCES NL_Loans (LoanID),
		CONSTRAINT FK_NL_LoanHistory_Rescheduler FOREIGN KEY (UserID) REFERENCES Security_User (UserId),
		CONSTRAINT FK_NL_LoanHistory_Legal FOREIGN KEY (LoanLegalID) REFERENCES NL_LoanLegals (LoanLegalID),
		CONSTRAINT FK_NL_LoanHistory_Interval FOREIGN KEY (RepaymentIntervalTypeID) REFERENCES NL_RepaymentIntervalTypes (RepaymentIntervalTypeID),
		CONSTRAINT CHK_NL_LoanHistory CHECK (
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
		LoanAgreementTemplateID INT NOT NULL,
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
		[Description] NVARCHAR(70) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_LoanScheduleStatuses PRIMARY KEY (LoanScheduleStatusID),
		CONSTRAINT UC_NL_LoanScheduleStatuses UNIQUE (LoanScheduleStatus)
	);
	INSERT INTO NL_LoanScheduleStatuses (LoanScheduleStatusID, LoanScheduleStatus, [Description]) VALUES
		(1, 'StillToPay', 'Open'),		
		(2, 'Late', 'Late'),		
		(3, 'Paid', 'Paid'),
		(4, 'DeletedOnReschedule', 'Deleted on reshedule (nothing was paid before reschedule)'),
		(5, 'ClosedOnReschedule', 'Closed on reshedule (was partially paid before reschedule)')
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
		Principal DECIMAL (18, 6) NOT NULL,
		InterestRate DECIMAL(18, 6) NOT NULL,
		TwoDaysDueMailSent BIT NOT NULL CONSTRAINT DF_NL_LoanSchedules_TwoDaysSent DEFAULT (0),
		FiveDaysDueMailSent BIT NOT NULL CONSTRAINT DF_NL_LoanSchedules_FiveDaysSent DEFAULT (0),
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

	INSERT INTO NL_PacnetTransactionStatuses (PacnetTransactionStatusID, TransactionStatus) VALUES
		(1, 'Submited'),
		(2, 'ConfigError:MultipleCandidateChannels'),
		(3, 'Error'),
		(4, 'InProgress'),
		(5, 'PaymentByCustomer'),
		(6, 'Done')
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
	);

	INSERT INTO NL_PaymentStatuses (PaymentStatusID, PaymentStatus) VALUES
		(1, 'Error'),
		(2, 'Active'),	
		(3, 'ChargeBack'),	
		(4, 'WrongPayment')		
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
		CreatedByUserID INT NOT NULL,
		DeletionTime DATETIME NULL,		
		DeletedByUserID INT NULL,
		Notes NVARCHAR(MAX) NULL,
		PaymentDestination NVARCHAR(20) NULL,
		LoanID BIGINT NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_Payment PRIMARY KEY (PaymentID),
		CONSTRAINT FK_NL_Payments_Method FOREIGN KEY (PaymentMethodID) REFERENCES LoanTransactionMethod (Id),
		CONSTRAINT FK_NL_Payments_Status FOREIGN KEY (PaymentStatusID) REFERENCES NL_PaymentStatuses (PaymentStatusID),
		CONSTRAINT FK_NL_Payments_Creator FOREIGN KEY (CreatedByUserID) REFERENCES Security_User (UserId),
		CONSTRAINT FK_NL_Payments_Deleter FOREIGN KEY (DeletedByUserID) REFERENCES Security_User (UserId),
		CONSTRAINT FK_NL_Payments_Loan FOREIGN KEY (LoanID) REFERENCES NL_Loans (LoanID),
		CONSTRAINT CHK_NL_Payments CHECK (
			(DeletionTime IS NULL AND DeletedByUserID IS NULL)
			OR
			(DeletionTime >= CreationTime AND DeletedByUserID IS NOT NULL)
		)
	)
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_LoanSchedulePayments') IS NULL
BEGIN
	CREATE TABLE NL_LoanSchedulePayments (
		LoanSchedulePaymentID BIGINT IDENTITY(1, 1) NOT NULL,
		LoanScheduleID BIGINT NOT NULL,
		PaymentID BIGINT NOT NULL,
		PrincipalPaid DECIMAL(18, 6) NOT NULL,
		InterestPaid DECIMAL(18, 6) NOT NULL,
		[ResetPrincipalPaid] [decimal](18, 6) NULL,
		[ResetInterestPaid] [decimal](18, 6) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_LoanSchedulePayments PRIMARY KEY (LoanSchedulePaymentID),
		CONSTRAINT FK_NL_LoanSchedulePayments_Schedule FOREIGN KEY (LoanScheduleID) REFERENCES NL_LoanSchedules (LoanScheduleID),
		CONSTRAINT FK_NL_LoanSchedulePayments_Payment FOREIGN KEY (PaymentID) REFERENCES NL_Payments (PaymentID),
		--CONSTRAINT UC_NL_LoanSchedulePayments UNIQUE (LoanScheduleID, PaymentID),
		--CONSTRAINT CHK_NL_LoanSchedulePayments_Principal CHECK (PrincipalPaid >= 0),
		--CONSTRAINT CHK_NL_LoanSchedulePayments_Interest CHECK (InterestPaid >= 0)	
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
		[ResetAmount] DECIMAL(18, 6) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_LoanFeePayments PRIMARY KEY (LoanFeePaymentID),
		CONSTRAINT FK_LoanFeePayments_Fee FOREIGN KEY (LoanFeeID) REFERENCES NL_LoanFees (LoanFeeID),
		CONSTRAINT FK_LoanFeePayments_Payment FOREIGN KEY (PaymentID) REFERENCES NL_Payments (PaymentID),
		CONSTRAINT UC_LoanFeePayments UNIQUE (LoanFeeID, PaymentID, Amount, [ResetAmount]),
		CONSTRAINT CHK_LoanFeePayments CHECK (Amount >= 0)
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

	INSERT INTO NL_PaypointTransactionStatuses (PaypointTransactionStatusID, TransactionStatus) VALUES
		(1, 'InProgress'),
		(2, 'Done'),
		(3, 'Error'),
		(4, 'Unknown')
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF OBJECT_ID('NL_PaypointTransactions') IS NULL
BEGIN
	CREATE TABLE NL_PaypointTransactions (
		PaypointTransactionID BIGINT IDENTITY(1, 1) NOT NULL,
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
		StopAutoChargeDate DATETIME NULL,					
		PartialAutoCharging BIT NOT NULL CONSTRAINT DF_NL_LoanOptions_PartialAutoCharging DEFAULT (1),
		LatePaymentNotification BIT NOT NULL CONSTRAINT DF_NL_LoanOptions_LatePaymentNotification DEFAULT (1),
		CaisAccountStatus NVARCHAR(50) NULL,
		ManualCaisFlag NVARCHAR(20) NULL,
		EmailSendingAllowed BIT NOT NULL CONSTRAINT DF_NL_LoanOptions_SendEmail DEFAULT (1),
		SmsSendingAllowed BIT NOT NULL CONSTRAINT DF_NL_LoanOptions_SendSms DEFAULT (1),
		MailSendingAllowed BIT NOT NULL CONSTRAINT DF_NL_LoanOptions_SendMail DEFAULT (1),		
		UserID INT,
		InsertDate DATETIME NOT NULL,
		IsActive BIT NOT NULL,
		Notes NVARCHAR(MAX) NULL,
		StopLateFeeFromDate DATETIME NULL,
		StopLateFeeToDate DATETIME NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_NL_LoanOptions PRIMARY KEY (LoanOptionsID),
		CONSTRAINT FK_NL_LoanOptions_Loan FOREIGN KEY (LoanID) REFERENCES NL_Loans (LoanID),
		CONSTRAINT FK_NL_LoanOptions_User FOREIGN KEY (UserID) REFERENCES Security_User (UserId)
	);
END
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------
IF OBJECT_ID('NL_Log') IS NULL
BEGIN
	create TABLE NL_Log (
		LogID BIGINT IDENTITY(1, 1) NOT NULL,
		UserID INT NULL,
		CustomerID INT NULL,
		Args NVARCHAR(MAX) NULL,
		[Result] NVARCHAR(MAX) NULL,
		Sevirity NVARCHAR(MAX) NOT NULL,
		Referrer NVARCHAR(MAX) NOT NULL,
		Description NVARCHAR(MAX) NOT NULL,
		[Exception] NVARCHAR(MAX) NULL,
		Stacktrace NVARCHAR(MAX) NULL,
		TimeStamp DATETIME NOT NULL		
	)
END
GO

-- TODO remove
IF OBJECT_ID('CHK_LoanFeePayments') IS NOT NULL
ALTER TABLE [dbo].[NL_LoanFeePayments] DROP CONSTRAINT [CHK_LoanFeePayments] 
GO

ALTER TABLE [dbo].[NL_LoanFeePayments] WITH CHECK ADD CONSTRAINT [CHK_LoanFeePayments] CHECK  (([Amount]>=(0)))
GO

IF OBJECT_ID('CHK_NL_LoanSchedulePayments_Paid') IS NOT NULL
ALTER TABLE [dbo].[NL_LoanFeePayments] DROP CONSTRAINT [CHK_NL_LoanSchedulePayments_Paid] 
GO

IF OBJECT_ID('CHK_NL_Payments') IS NOT NULL
ALTER TABLE [dbo].[NL_Payments] DROP CONSTRAINT CHK_NL_Payments
GO
ALTER TABLE NL_Payments ADD CONSTRAINT CHK_NL_Payments CHECK (
	(DeletionTime IS NULL AND DeletedByUserID IS NULL)
	OR 
	(DeletionTime >= CreationTime AND DeletedByUserID IS NOT NULL)
	);
GO

IF OBJECT_ID('UQ_LoanID_LoanFeeTypeID_Amount_AssignTime_Disabled') IS NULL
	ALTER TABLE [dbo].[NL_LoanFees] ADD CONSTRAINT UQ_LoanID_LoanFeeTypeID_Amount_AssignTime_Disabled UNIQUE NONCLUSTERED ([LoanID],[LoanFeeTypeID],[Amount],[AssignTime],[DisabledTime]);
GO

