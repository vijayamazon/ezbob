SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianConsumerDataNoc') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianConsumerDataNoc AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianConsumerDataNoc
@ExperianConsumerDataId BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'ExperianConsumerDataNoc' AS DatumType,
		Id,
		ExperianConsumerDataId,
		Reference,
		TextLine
	FROM
		ExperianConsumerDataNoc
	WHERE
		ExperianConsumerDataId = @ExperianConsumerDataId
END
GO

