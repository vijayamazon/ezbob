SET QUOTED_IDENTIFIER ON
GO

SET ANSI_NULLS ON
GO

IF OBJECT_ID('AV_GetOfferSetupFeeRange') IS NOT NULL
	DROP PROCEDURE AV_GetOfferSetupFeeRange
GO

IF OBJECT_ID('LoadOfferRanges') IS NULL
	DROP PROCEDURE LoadOfferRanges
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('OfferSetupFeeRanges') AND name = 'IsNewLoan')
	DROP TABLE OfferSetupFeeRanges
GO

IF OBJECT_ID('OfferSetupFeeRanges') IS NULL
BEGIN
	CREATE TABLE OfferSetupFeeRanges (
		RangeID INT NOT NULL,
		LoanSizeName NVARCHAR(50) NOT NULL,
		MaxLoanAmount DECIMAL(18, 6) NULL,
		IsNewLoan BIT NOT NULL,
		MinSetupFee DECIMAL(18, 6) NOT NULL,
		MaxSetupFee DECIMAL(18, 6) NOT NULL,
		CONSTRAINT PK_OfferSetupFeeRanges PRIMARY KEY (RangeID),
		CONSTRAINT UC_OfferSetupFeeRanges_Name UNIQUE (LoanSizeName),
		CONSTRAINT UC_OfferSetupFeeRanges_Amount UNIQUE (MaxLoanAmount, IsNewLoan),
		CONSTRAINT CHK_OfferSetupFeeRanges_Name CHECK (LTRIM(RTRIM(LoanSizeName)) != ''),
		CONSTRAINT CHK_OfferSetupFeeRanges_Amount CHECK (MaxLoanAmount IS NULL OR MaxLoanAmount > 0),
		CONSTRAINT CHK_OfferSetupFeeRanges_MinFee CHECK (MinSetupFee >= 0),
		CONSTRAINT CHK_OfferSetupFeeRanges_MaxFee CHECK (MaxSetupFee >= 0),
		CONSTRAINT CHK_OfferSetupFeeRanges_FeeRange CHECK (MaxSetupFee >= MinSetupFee)
	)
END
GO

IF NOT EXISTS (SELECT * FROM OfferSetupFeeRanges)
BEGIN
	INSERT INTO OfferSetupFeeRanges (RangeID, LoanSizeName, MaxLoanAmount, IsNewLoan, MinSetupFee, MaxSetupFee) VALUES
		(1, 'New loan up to 10K', 10000, 1, 0, 0.07),
		(2, 'New loan over 10K',   NULL, 1, 0, 0.05),
		(3, 'Repeating loan',      NULL, 0, 0, 0.03)
END
GO
