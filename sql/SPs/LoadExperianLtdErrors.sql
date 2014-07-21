SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianLtdErrors') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianLtdErrors AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianLtdErrors
@ExperianLtdID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'ExperianLtdErrors' AS DatumType,
		ExperianLtdErrorsID AS ID,
		ExperianLtdID AS ParentID,
		'ExperianLtd' AS ParentType,
		ErrorMessage
	FROM
		ExperianLtdErrors
	WHERE
		ExperianLtdID = @ExperianLtdID
END
GO

