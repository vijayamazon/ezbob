IF OBJECT_ID('RemoveCustomerFromMedalCalculations') IS NULL
	EXECUTE('CREATE PROCEDURE RemoveCustomerFromMedalCalculations AS SELECT 1')
GO

ALTER PROCEDURE RemoveCustomerFromMedalCalculations
@CustomerId INT
AS
BEGIN
	DELETE FROM MedalCalculations WHERE CustomerId = @CustomerId
END
GO
