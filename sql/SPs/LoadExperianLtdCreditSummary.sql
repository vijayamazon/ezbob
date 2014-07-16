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
		ExperianLtdCreditSummaryID,
		ExperianLtdID,
		CreditEventType,
		DateOfMostRecentRecordForType
	FROM
		ExperianLtdCreditSummary
	WHERE
		ExperianLtdID = @ExperianLtdID
END
GO

