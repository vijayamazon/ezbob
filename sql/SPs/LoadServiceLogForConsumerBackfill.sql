IF OBJECT_ID('LoadServiceLogForConsumerBackfill') IS NULL
	EXECUTE('CREATE PROCEDURE LoadServiceLogForConsumerBackfill AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadServiceLogForConsumerBackfill
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @LastId BIGINT
	SELECT @LastId = MAX(ServiceLogId) FROM ExperianConsumerData

	SELECT
		Id
	FROM
		MP_ServiceLog
	WHERE
		ServiceType = 'Consumer Request' AND
		Id > @LastId
END
GO
