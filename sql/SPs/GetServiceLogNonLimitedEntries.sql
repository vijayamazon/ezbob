IF OBJECT_ID('GetServiceLogNonLimitedEntries') IS NULL
	EXECUTE('CREATE PROCEDURE GetServiceLogNonLimitedEntries AS SELECT 1')
GO

ALTER PROCEDURE GetServiceLogNonLimitedEntries
AS
BEGIN	
	SELECT 
		MP_ServiceLog.Id, 
		MP_ServiceLog.CustomerId,
		Company.ExperianRefNum
	FROM 
		MP_ServiceLog,
		Company,
		Customer
	WHERE 
		ServiceType = 'E-SeriesNonLimitedData' AND
		MP_ServiceLog.CustomerId = Customer.Id AND
		Customer.CompanyId = Company.Id
	ORDER BY Id ASC
END
GO
