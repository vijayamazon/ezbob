SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadLastCustomerCashRequest') IS NULL
	EXECUTE('CREATE PROCEDURE LoadLastCustomerCashRequest AS SELECT 1')
GO

ALTER PROCEDURE LoadLastCustomerCashRequest
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1
		CashRequestID = r.Id,
		r.UnderwriterDecision
	FROM
		CashRequests r
	WHERE
		r.IdCustomer = @CustomerID
	ORDER BY
		r.Id DESC
END
GO
