IF OBJECT_ID('MP_VatReturnSummary') IS NULL
BEGIN
	SET QUOTED_IDENTIFIER ON;

	CREATE TABLE MP_VatReturnSummary (
		SummaryID BIGINT IDENTITY(1, 1) NOT NULL,
		CustomerID INT NOT NULL,
		BusinessID INT NOT NULL,
		CreationDate DATETIME NOT NULL,
		IsActive BIT NOT NULL,
		Currency CHAR(3) NOT NULL,
		PctOfAnnualRevenues DECIMAL(18, 6) NULL,
		Revenues  DECIMAL(18, 6) NULL,
		Opex DECIMAL(18, 6) NULL,
		TotalValueAdded DECIMAL(18, 6) NULL,
		PctOfRevenues DECIMAL(18, 6) NULL,
		Salaries DECIMAL(18, 6) NULL,
		Tax DECIMAL(18, 6) NULL,
		Ebida DECIMAL(18, 6) NULL,
		PctOfAnnual DECIMAL(18, 6) NULL,
		ActualLoanRepayment DECIMAL(18, 6) NULL,
		FreeCashFlow DECIMAL(18, 6) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_VatReturnSummary PRIMARY KEY (SummaryID),
		CONSTRAINT FK_VatReturnSummary_Customer FOREIGN KEY (CustomerID) REFERENCES Customer(Id),
		CONSTRAINT FK_VatReturnSummary_Business FOREIGN KEY (BusinessID) REFERENCES Business(Id)
	)
END
GO

IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IDX_VatReturnSummary')
	CREATE NONCLUSTERED INDEX IDX_VatReturnSummary ON MP_VatReturnSummary(CustomerID) WHERE IsActive = 1
GO

IF OBJECT_ID('MP_VatReturnSummaryPeriods') IS NULL
BEGIN
	SET QUOTED_IDENTIFIER ON;

	CREATE TABLE MP_VatReturnSummaryPeriods (
		SummaryPeriodID BIGINT IDENTITY(1, 1) NOT NULL,
		SummaryID BIGINT NOT NULL,
		DateFrom DATETIME NOT NULL,
		DateTo DATETIME NOT NULL,
		PctOfAnnualRevenues DECIMAL(18, 6) NULL,
		Revenues  DECIMAL(18, 6) NULL,
		Opex DECIMAL(18, 6) NULL,
		TotalValueAdded DECIMAL(18, 6) NULL,
		PctOfRevenues DECIMAL(18, 6) NULL,
		Salaries DECIMAL(18, 6) NULL,
		Tax DECIMAL(18, 6) NULL,
		Ebida DECIMAL(18, 6) NULL,
		PctOfAnnual DECIMAL(18, 6) NULL,
		ActualLoanRepayment DECIMAL(18, 6) NULL,
		FreeCashFlow DECIMAL(18, 6) NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_VatReturnSummaryPeriods PRIMARY KEY (SummaryPeriodID),
		CONSTRAINT FK_VatReturnSummaryPeriods FOREIGN KEY (SummaryID) REFERENCES MP_VatReturnSummary(SummaryID)
	)
END
GO
