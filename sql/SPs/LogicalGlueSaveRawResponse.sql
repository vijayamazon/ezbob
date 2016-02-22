SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LogicalGlueSaveRawResponse') IS NULL
	EXECUTE('CREATE PROCEDURE LogicalGlueSaveRawResponse AS SELECT 1')
GO

ALTER PROCEDURE LogicalGlueSaveRawResponse
@ServiceLogID BIGINT,
@RawResponse NVARCHAR(MAX)
AS
BEGIN
	UPDATE MP_ServiceLog SET
		ResponseData = @RawResponse
	WHERE
		Id = @ServiceLogID
END
GO
