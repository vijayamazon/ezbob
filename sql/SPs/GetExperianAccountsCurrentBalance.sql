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
		CustomerAnalyticsCompany 
	WHERE
		CustomerAnalyticsCompany.CustomerID = @CustomerId AND
		CustomerAnalyticsCompany.IsActive = 1
END
GO
