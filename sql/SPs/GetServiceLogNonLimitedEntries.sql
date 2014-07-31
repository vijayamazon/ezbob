IF OBJECT_ID('GetServiceLogNonLimitedEntries') IS NULL
	EXECUTE('CREATE PROCEDURE GetServiceLogNonLimitedEntries AS SELECT 1')
GO

ALTER PROCEDURE GetServiceLogNonLimitedEntries
AS
BEGIN	
	SELECT 
		MP_ServiceLog.Id, 
		MP_ServiceLog.CustomerId,
		Company.ExperianRefNum,
		MP_ServiceLog.InsertDate
	FROM 
		MP_ServiceLog,
		Company,
		Customer
	WHERE 
		ServiceType = 'E-SeriesNonLimitedData' AND
		MP_ServiceLog.CustomerId = Customer.Id AND
		Customer.CompanyId = Company.Id AND
		Company.ExperianRefNum <> 'NotFound'
	ORDER BY Id ASC
END
GO