IF OBJECT_ID('GetServiceLogNonLimitedEntry') IS NULL
	EXECUTE('CREATE PROCEDURE GetServiceLogNonLimitedEntry AS SELECT 1')
GO

ALTER PROCEDURE GetServiceLogNonLimitedEntry
	@ServiceLogId BIGINT
AS
BEGIN	
	SELECT  
		MP_ServiceLog.ResponseData
	FROM 
		MP_ServiceLog
	WHERE
		MP_ServiceLog.Id = @ServiceLogId
END
GO
