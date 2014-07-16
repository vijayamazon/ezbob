SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianLtdCreditSummary') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianLtdCreditSummary AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianLtdCreditSummary
@ExperianLtdID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'ExperianLtdCreditSummary' AS DatumType,
		ExperianLtdCreditSummaryID AS ID,
		ExperianLtdID AS ParentID,
		'ExperianLtd' AS ParentType,
		CreditEventType,
		DateOfMostRecentRecordForType
	FROM
		ExperianLtdCreditSummary
	WHERE
		ExperianLtdID = @ExperianLtdID
END
GO

