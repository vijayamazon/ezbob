SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianLtdLenderDetails') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianLtdLenderDetails AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianLtdLenderDetails
@ExperianLtdID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'ExperianLtdLenderDetails' AS DatumType,
		'ExperianLtdDL65' AS ParentType,
		ExperianLtdLenderDetailsID AS ID,
		DL65ID AS ParentID,
		LenderName
	FROM
		ExperianLtdLenderDetails ld
		INNER JOIN ExperianLtdDL65 d ON ld.DL65ID = d.ExperianLtdDL65ID
	WHERE
		d.ExperianLtdID = @ExperianLtdID
END
GO

