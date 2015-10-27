SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('GetCashRequestForLoanCommissionDefaults') IS NULL
	EXECUTE('CREATE PROCEDURE GetCashRequestForLoanCommissionDefaults AS SELECT 1')
GO

ALTER PROCEDURE GetCashRequestForLoanCommissionDefaults
@CashRequestID BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		CashRequestID = r.Id,
		r.BrokerSetupFeePercent,
		r.ManualSetupFeePercent,
		IsBrokerCustomer = CONVERT(BIT, CASE WHEN c.BrokerID IS NULL THEN 0 ELSE 1 END),
		FirstLoanDate = (SELECT MIN(l.[Date]) FROM Loan l WHERE l.CustomerID = c.Id)
	FROM
		CashRequests r
		INNER JOIN Customer c ON r.IdCustomer = c.Id
	WHERE
		r.Id = @CashRequestID
END
GO
