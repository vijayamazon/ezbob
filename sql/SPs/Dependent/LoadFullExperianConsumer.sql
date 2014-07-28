SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadFullExperianConsumer') IS NULL
	EXECUTE('CREATE PROCEDURE LoadFullExperianConsumer AS SELECT 1')
GO

ALTER PROCEDURE LoadFullExperianConsumer
@ServiceLogId BIGINT,
@InsertDate DATETIME = NULL
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ExperianConsumerDataId BIGINT

	-- Main table

	EXECUTE LoadExperianConsumerData @ServiceLogId, @ExperianConsumerDataId OUTPUT

	-- Dependent tables (level 1)
	
	EXECUTE LoadExperianConsumerDataApplicant @ExperianConsumerDataId
	EXECUTE LoadExperianConsumerDataNoc @ExperianConsumerDataId
	EXECUTE LoadExperianConsumerDataLocation @ExperianConsumerDataId
	EXECUTE LoadExperianConsumerDataResidency @ExperianConsumerDataId
	EXECUTE LoadExperianConsumerDataCais @ExperianConsumerDataId

	-- Dependent tables (level 2)

	EXECUTE LoadExperianConsumerDataCaisBalance @ExperianConsumerDataId
	EXECUTE LoadExperianConsumerDataCaisCardHistory @ExperianConsumerDataId

	-- Metadata
	IF @InsertDate IS NOT NULL 
	BEGIN
		SELECT
			'Metadata' AS DatumType,
			@InsertDate AS InsertDate
	END
	ELSE BEGIN
		SELECT
			'Metadata' AS DatumType,
			InsertDate
		FROM
			MP_ServiceLog
		WHERE
			Id = @ServiceLogId
	END
END

GO

