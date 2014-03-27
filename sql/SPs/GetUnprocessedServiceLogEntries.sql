IF OBJECT_ID('GetUnprocessedServiceLogEntries') IS NULL
	EXECUTE('CREATE PROCEDURE GetUnprocessedServiceLogEntries AS SELECT 1')
GO

ALTER PROCEDURE GetUnprocessedServiceLogEntries
AS
BEGIN	
	DECLARE @MaxServiceLogId INT
	SELECT @MaxServiceLogId = MAX(ServiceLogId) FROM FinancialAccounts
	
	IF @MaxServiceLogId IS NULL
		SET @MaxServiceLogId = 0

	SELECT Id, CustomerId, ResponseData FROM MP_ServiceLog WHERE Id > @MaxServiceLogId AND ServiceType = 'Consumer Request' AND DirectorId IS NULL ORDER BY Id ASC
END
GO
