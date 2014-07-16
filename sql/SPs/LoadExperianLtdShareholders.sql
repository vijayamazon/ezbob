SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianLtdShareholders') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianLtdShareholders AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianLtdShareholders
@ExperianLtdID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'ExperianLtdShareholders' AS DatumType,
		ExperianLtdShareholdersID AS ID,
		ExperianLtdID AS ParentID,
		'ExperianLtd' AS ParentType,
		DescriptionOfShareholder,
		DescriptionOfShareholding,
		RegisteredNumberOfALimitedCompanyWhichIsAShareholder
	FROM
		ExperianLtdShareholders
	WHERE
		ExperianLtdID = @ExperianLtdID
END
GO

