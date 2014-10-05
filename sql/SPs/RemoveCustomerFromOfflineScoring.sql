IF OBJECT_ID('RemoveCustomerFromOfflineScoring') IS NULL
	EXECUTE('CREATE PROCEDURE RemoveCustomerFromOfflineScoring AS SELECT 1')
GO

ALTER PROCEDURE RemoveCustomerFromOfflineScoring
@CustomerId INT
AS
BEGIN
	DELETE FROM OfflineScoring WHERE CustomerId = @CustomerId
END
GO
