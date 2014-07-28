SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianConsumerDataLocation') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianConsumerDataLocation AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianConsumerDataLocation
@ExperianConsumerDataId BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'ExperianConsumerDataLocation' AS DatumType,
		Id,
		ExperianConsumerDataId,
		LocationIdentifier,
		Flat,
		HouseName,
		HouseNumber,
		Street,
		Street2,
		District,
		District2,
		PostTown,
		County,
		Postcode,
		POBox,
		Country,
		SharedLetterbox,
		FormattedLocation,
		LocationCode,
		TimeAtYears,
		TimeAtMonths
	FROM
		ExperianConsumerDataLocation
	WHERE
		ExperianConsumerDataId = @ExperianConsumerDataId
END
GO

