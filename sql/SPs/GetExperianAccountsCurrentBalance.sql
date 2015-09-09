IF OBJECT_ID('GetExperianAccountsCurrentBalance') IS NULL
	EXECUTE('CREATE PROCEDURE GetExperianAccountsCurrentBalance AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetExperianAccountsCurrentBalance
@CustomerId INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		CurrentBalanceSum AS CurrentBalance
	FROM 
		dbo.udfGetCustomerCompanyAnalytics(@CustomerId, NULL, 1, 0, 0)
END
GO
