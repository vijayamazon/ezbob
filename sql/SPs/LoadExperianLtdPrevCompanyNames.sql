SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianLtdPrevCompanyNames') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianLtdPrevCompanyNames AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianLtdPrevCompanyNames
@ExperianLtdID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'ExperianLtdPrevCompanyNames' AS DatumType,
		ExperianLtdPrevCompanyNamesID AS ID,
		ExperianLtdID AS ParentID,
		'ExperianLtd' AS ParentType,
		DateChanged,
		OfficeAddress1,
		OfficeAddress2,
		OfficeAddress3,
		OfficeAddress4,
		OfficeAddressPostcode
	FROM
		ExperianLtdPrevCompanyNames
	WHERE
		ExperianLtdID = @ExperianLtdID
END
GO

