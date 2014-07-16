SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianLtdDLB5') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianLtdDLB5 AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianLtdDLB5
@ExperianLtdID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'ExperianLtdDLB5' AS DatumType,
		ExperianLtdDLB5ID,
		ExperianLtdID,
		RecordType,
		IssueCompany,
		CurrentpreviousIndicator,
		EffectiveDate,
		ShareClassNumber,
		ShareholdingNumber,
		ShareholderNumber,
		ShareholderType,
		Prefix,
		FirstName,
		MidName1,
		LastName,
		Suffix,
		ShareholderQualifications,
		Title,
		ShareholderCompanyName,
		KgenName,
		ShareholderRegisteredNumber,
		AddressLine1,
		AddressLine2,
		AddressLine3,
		Town,
		County,
		Postcode,
		Country,
		ShareholderPunaPcode,
		ShareholderRMC,
		SuppressionFlag,
		NOCRefNumber,
		LastUpdated
	FROM
		ExperianLtdDLB5
	WHERE
		ExperianLtdID = @ExperianLtdID
END
GO

