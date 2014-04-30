IF OBJECT_ID('GetExperianAccountsCurrentBalance') IS NULL
	EXECUTE('CREATE PROCEDURE GetExperianAccountsCurrentBalance AS SELECT 1')
GO

ALTER PROCEDURE GetExperianAccountsCurrentBalance
	(@CustomerId INT)
AS
BEGIN
	SELECT 
		SUM(ExperianDL97Accounts.CurrentBalance) AS CurrentBalance
	FROM 
		ExperianDL97Accounts, 
		MP_ExperianDataCache 
	WHERE 
		ExperianDL97Accounts.DataCacheId = MP_ExperianDataCache.Id AND
		MP_ExperianDataCache.CustomerId = @CustomerId AND
		MP_ExperianDataCache.CompanyRefNumber IS NOT NULL
END
GO
