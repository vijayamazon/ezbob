SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('CashRequestsHistory') IS NULL
BEGIN
	CREATE TABLE CashRequestsHistory (
		CashRequestHistoryID BIGINT IDENTITY(1, 1) NOT NULL,
		EntryTime DATETIME NOT NULL CONSTRAINT DF_CashRequestsHistory_EntryTime DEFAULT (GETUTCDATE()),
		Id BIGINT NULL,
		IdCustomer INT NULL,
		IdUnderwriter INT NULL,
		CreationDate DATETIME NULL,
		SystemDecision VARCHAR(50) NULL,
		UnderwriterDecision VARCHAR(50) NULL,
		SystemDecisionDate DATETIME NULL,
		UnderwriterDecisionDate DATETIME NULL,
		EscalatedDate DATETIME NULL,
		SystemCalculatedSum INT NULL,
		ManagerApprovedSum INT NULL,
		MedalType VARCHAR(50) NULL,
		EscalationReason VARCHAR(200) NULL,
		APR DECIMAL(18, 0) NULL,
		RepaymentPeriod INT NULL,
		ScorePoints DECIMAL(8, 3) NULL,
		ExpirianRating INT NULL,
		AnualTurnover INT NULL,
		InterestRate DECIMAL(18, 4) NULL,
		UseSetupFee INT NULL,
		EmailSendingBanned BIT NULL,
		LoanTypeId INT NULL,
		UnderwriterComment VARCHAR(400) NULL,
		HasLoans BIT NULL,
		LoanTemplate VARCHAR(MAX) NULL,
		IsLoanTypeSelectionAllowed INT NULL,
		DiscountPlanId INT NULL,
		LoanSourceID INT NULL,
		OfferStart DATETIME NULL,
		OfferValidUntil DATETIME NULL,
		IsCustomerRepaymentPeriodSelectionAllowed BIT NULL,
		UseBrokerSetupFee BIT NULL,
		ManualSetupFeeAmount INT NULL,
		ManualSetupFeePercent DECIMAL(18, 4) NULL,
		QuickOfferID INT NULL,
		Originator VARCHAR(30) NULL,
		ApprovedRepaymentPeriod INT NULL,
		AutoDecisionID INT NULL,
		BrokerSetupFeePercent DECIMAL(18, 4) NULL,
		SpreadSetupFee BIT NULL,
		HasApprovalChance BIT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_CashRequestHistory PRIMARY KEY (CashRequestHistoryID)
	)
END
GO
