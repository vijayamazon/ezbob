SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianConsumerDataApplicant') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianConsumerDataApplicant AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianConsumerDataApplicant
@ExperianConsumerDataId BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'ExperianConsumerDataApplicant' AS DatumType,
		Id,
		ExperianConsumerDataId,
		ApplicantIdentifier,
		Title,
		Forename,
		MiddleName,
		Surname,
		Suffix,
		DateOfBirth,
		Gender
	FROM
		ExperianConsumerDataApplicant a
	WHERE
		a.ExperianConsumerDataId = @ExperianConsumerDataId
END
GO

