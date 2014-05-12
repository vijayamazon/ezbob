IF OBJECT_ID('GetExperianAccountsCurrentBalance') IS NULL
	EXECUTE('CREATE PROCEDURE GetExperianAccountsCurrentBalance AS SELECT 1')
GO

ALTER PROCEDURE GetExperianAccountsCurrentBalance
	(@CustomerId INT)
AS
BEGIN
	SELECT 
		CurrentBalanceSum AS CurrentBalance
	FROM 
		MP_ExperianDataCache 
	WHERE
		MP_ExperianDataCache.CustomerId = @CustomerId AND
		MP_ExperianDataCache.CompanyRefNumber IS NOT NULL
END
GO
