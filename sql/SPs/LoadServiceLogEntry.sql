IF OBJECT_ID('LoadServiceLogEntry') IS NULL
	EXECUTE('CREATE PROCEDURE LoadServiceLogEntry AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadServiceLogEntry
@EntryID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		Id,
		ServiceType,
		InsertDate,
		ResponseData,
		CustomerId,
		DirectorId
	FROM
		MP_ServiceLog
	WHERE
		Id = @EntryID
END
GO
