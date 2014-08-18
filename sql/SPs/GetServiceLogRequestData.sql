IF OBJECT_ID('GetServiceLogRequestData') IS NULL
	EXECUTE('CREATE PROCEDURE GetServiceLogRequestData AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetServiceLogRequestData
 @ServiceLogId BIGINT
AS
BEGIN
	SELECT RequestData FROM MP_ServiceLog WHERE Id=@ServiceLogId
END
GO