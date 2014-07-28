SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadExperianConsumerDataCaisBalance') IS NULL
	EXECUTE('CREATE PROCEDURE LoadExperianConsumerDataCaisBalance AS SELECT 1')
GO

ALTER PROCEDURE LoadExperianConsumerDataCaisBalance
@ExperianConsumerDataId BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		'ExperianConsumerDataCaisBalance' AS DatumType,
		b.Id,
		ExperianConsumerDataCaisId,
		AccountBalance,
		Status
	FROM
		ExperianConsumerDataCaisBalance b INNER JOIN ExperianConsumerDataCais c ON b.ExperianConsumerDataCaisId = c.Id
	WHERE
		c.ExperianConsumerDataId = @ExperianConsumerDataId
END
GO

