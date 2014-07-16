SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianLtdDL52') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianLtdDL52 AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianLtdDL52
@ExperianLtdID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'ExperianLtdDL52' AS DatumType,
		ExperianLtdDL52ID,
		ExperianLtdID,
		NoticeType,
		DateOfNotice
	FROM
		ExperianLtdDL52
	WHERE
		ExperianLtdID = @ExperianLtdID
END
GO


