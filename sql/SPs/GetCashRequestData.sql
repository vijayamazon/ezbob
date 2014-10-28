IF OBJECT_ID('GetCashRequestData') IS NULL
	EXECUTE('CREATE PROCEDURE GetCashRequestData AS SELECT 1')
GO

ALTER PROCEDURE GetCashRequestData
@CustomerId INT
AS
BEGIN
	SELECT TOP 1 InterestRate, ManualSetupFeeAmount, SystemCalculatedSum, ManagerApprovedSum FROM CashRequests WHERE IdCustomer = @CustomerId ORDER BY Id DESC
END
GO
