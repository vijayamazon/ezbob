IF OBJECT_ID('DeleteExperianDL97Accounts') IS NULL
	EXECUTE('CREATE PROCEDURE DeleteExperianDL97Accounts AS SELECT 1')
GO

ALTER PROCEDURE DeleteExperianDL97Accounts
@CustomerId INT
AS
BEGIN
	DELETE FROM ExperianDL97Accounts WHERE CustomerId = @CustomerId
END
GO
