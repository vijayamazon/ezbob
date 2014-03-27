IF OBJECT_ID('DeletePreviousFinancialAccounts') IS NULL
	EXECUTE('CREATE PROCEDURE DeletePreviousFinancialAccounts AS SELECT 1')
GO

ALTER PROCEDURE DeletePreviousFinancialAccounts
	(@CustomerId INT)
AS
BEGIN	
	DELETE FROM FinancialAccounts WHERE CustomerId = @CustomerId
END
GO
