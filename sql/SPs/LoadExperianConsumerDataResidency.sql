SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianConsumerDataResidency') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianConsumerDataResidency AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianConsumerDataResidency
@ExperianConsumerDataId BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'ExperianConsumerDataResidency' AS DatumType,
		Id,
		ExperianConsumerDataId,
		ApplicantIdentifier,
		LocationIdentifier,
		LocationCode,
		ResidencyDateFrom,
		ResidencyDateTo,
		TimeAtYears,
		TimeAtMonths
	FROM
		ExperianConsumerDataResidency r
	WHERE
		r.ExperianConsumerDataId = @ExperianConsumerDataId
END
GO

