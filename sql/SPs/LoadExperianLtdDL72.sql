SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianLtdDL72') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianLtdDL72 AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianLtdDL72
@ExperianLtdID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'ExperianLtdDL72' AS DatumType,
		ExperianLtdDL72ID AS ID,
		ExperianLtdID AS ParentID,
		'ExperianLtd' AS ParentType,
		ForeignAddressFlag,
		IsCompany,
		Number,
		LengthOfDirectorship,
		DirectorsAgeYears,
		NumberOfConvictions,
		Prefix,
		FirstName,
		MidName1,
		MidName2,
		LastName,
		Suffix,
		Qualifications,
		Title,
		CompanyName,
		CompanyNumber,
		ShareInfo,
		BirthDate,
		HouseName,
		HouseNumber,
		Street,
		Town,
		County,
		Postcode
	FROM
		ExperianLtdDL72
	WHERE
		ExperianLtdID = @ExperianLtdID
END
GO

