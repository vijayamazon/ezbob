SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianLtdDL68') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianLtdDL68 AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianLtdDL68
@ExperianLtdID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'ExperianLtdDL68' AS DatumType,
		ExperianLtdDL68ID,
		ExperianLtdID,
		SubsidiaryRegisteredNumber,
		SubsidiaryStatus,
		SubsidiaryLegalStatus,
		SubsidiaryName
	FROM
		ExperianLtdDL68
	WHERE
		ExperianLtdID = @ExperianLtdID
END
GO

