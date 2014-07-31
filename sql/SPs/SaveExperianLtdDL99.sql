SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveExperianLtdDL99') IS NOT NULL
	DROP PROCEDURE SaveExperianLtdDL99
GO

IF TYPE_ID('ExperianLtdDL99List') IS NOT NULL
	DROP TYPE ExperianLtdDL99List
GO

CREATE TYPE ExperianLtdDL99List AS TABLE (
	ExperianLtdID BIGINT NOT NULL,
	Date DATETIME NULL,
	CredDirLoans DECIMAL(18, 6) NULL,
	Debtors DECIMAL(18, 6) NULL,
	DebtorsDirLoans DECIMAL(18, 6) NULL,
	DebtorsGroupLoans DECIMAL(18, 6) NULL,
	InTngblAssets DECIMAL(18, 6) NULL,
	Inventories DECIMAL(18, 6) NULL,
	OnClDirLoans DECIMAL(18, 6) NULL,
	OtherDebtors DECIMAL(18, 6) NULL,
	PrepayAccRuals DECIMAL(18, 6) NULL,
	RetainedEarnings DECIMAL(18, 6) NULL,
	TngblAssets DECIMAL(18, 6) NULL,
	TotalCash DECIMAL(18, 6) NULL,
	TotalCurrLblts DECIMAL(18, 6) NULL,
	TotalNonCurr DECIMAL(18, 6) NULL,
	TotalShareFund DECIMAL(18, 6) NULL,
	FinDirLoans DECIMAL(18, 6) NULL,
	FinLbltsDirLoans DECIMAL(18, 6) NULL,
	CurrDirLoans DECIMAL(18, 6) NULL,
	TotalCurrAssets DECIMAL(18, 6) NULL
)
GO

CREATE PROCEDURE SaveExperianLtdDL99
@Tbl ExperianLtdDL99List READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO ExperianLtdDL99 (
		ExperianLtdID,
		Date,
		CredDirLoans,
		Debtors,
		DebtorsDirLoans,
		DebtorsGroupLoans,
		InTngblAssets,
		Inventories,
		OnClDirLoans,
		OtherDebtors,
		PrepayAccRuals,
		RetainedEarnings,
		TngblAssets,
		TotalCash,
		TotalCurrLblts,
		TotalNonCurr,
		TotalShareFund,
		FinDirLoans,
		FinLbltsDirLoans,
		CurrDirLoans,
		TotalCurrAssets
	) SELECT
		ExperianLtdID,
		Date,
		CredDirLoans,
		Debtors,
		DebtorsDirLoans,
		DebtorsGroupLoans,
		InTngblAssets,
		Inventories,
		OnClDirLoans,
		OtherDebtors,
		PrepayAccRuals,
		RetainedEarnings,
		TngblAssets,
		TotalCash,
		TotalCurrLblts,
		TotalNonCurr,
		TotalShareFund,
		FinDirLoans,
		FinLbltsDirLoans,
		CurrDirLoans,
		TotalCurrAssets
	FROM @Tbl
END
GO
