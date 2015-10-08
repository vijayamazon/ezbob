SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('Foam_LoadAmounts') IS NULL
	EXECUTE('CREATE PROCEDURE Foam_LoadAmounts AS SELECT 1')
GO

ALTER PROCEDURE Foam_LoadAmounts
@DateFrom DATETIME,
@DateTo DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		RowType = 'Approved',
		RowValue = ISNULL(SUM(r.ManagerApprovedSum), 0)
	FROM
		CashRequests r
		INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
	WHERE
		r.UnderwriterDecisionDate BETWEEN @DateFrom AND @DateTo
		AND
		r.UnderwriterDecision = 'Approved'

	UNION

	SELECT
		RowType = 'Issued',
		RowValue = ISNULL(SUM(l.LoanAmount), 0)
	FROM
		Loan l
		INNER JOIN CashRequests r ON l.RequestCashId = r.Id
		INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
	WHERE
		l.[Date] BETWEEN @DateFrom AND @DateTo
END
GO
