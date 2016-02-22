IF OBJECT_ID('GetCashRequestData') IS NULL
	EXECUTE('CREATE PROCEDURE GetCashRequestData AS SELECT 1')
GO

ALTER PROCEDURE GetCashRequestData
@CustomerId INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1
		cr.InterestRate,
		isnull(cr.ManualSetupFeePercent, 0) AS ManualSetupFeePercent,
		isnull(cr.BrokerSetupFeePercent, 0) AS BrokerSetupFeePercent,
		cr.SystemCalculatedSum,
		cr.ManagerApprovedSum,
		cr.Id
	FROM
		CashRequests cr
	WHERE
		IdCustomer = @CustomerId
	ORDER BY
		Id DESC
END

GO

