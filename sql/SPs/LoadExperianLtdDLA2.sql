SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianLtdDLA2') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianLtdDLA2 AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianLtdDLA2
@ExperianLtdID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'ExperianLtdDLA2' AS DatumType,
		ExperianLtdDLA2ID AS ID,
		ExperianLtdID AS ParentID,
		'ExperianLtd' AS ParentType,
		Date,
		NumberOfEmployees
	FROM
		ExperianLtdDLA2
	WHERE
		ExperianLtdID = @ExperianLtdID
END
GO


