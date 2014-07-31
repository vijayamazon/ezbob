SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianLtdDL99') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianLtdDL99 AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianLtdDL99
@ExperianLtdID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'ExperianLtdDL99' AS DatumType,
		ExperianLtdDL99ID AS ID,
		ExperianLtdID AS ParentID,
		'ExperianLtd' AS ParentType,
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
	FROM
		ExperianLtdDL99
	WHERE
		ExperianLtdID = @ExperianLtdID
END
GO

