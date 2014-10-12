IF OBJECT_ID('RptAccountingLoanBalance') IS NULL
	EXECUTE('CREATE PROCEDURE RptAccountingLoanBalance AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptAccountingLoanBalance
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		l.Date AS IssueDate,
		c.Id AS ClientID,
		l.Id AS LoanID,
		c.Fullname AS ClientName,
		c.Name AS ClientEmail,
		la.Amount AS IssuedAmount,
		la.Fees AS SetupFee,
		ISNULL(lc.Amount, 0) AS FeesEarned,
		l.Status AS LoanStatus,
		lm.Name AS LoanTranMethod,
		ISNULL(t.Amount, 0) AS TotalRepaid,
		ISNULL(t.Fees, 0) AS FeesRepaid,
		ISNULL(t.Rollover, 0) AS RolloverRepaid,
		lc.Id AS FeesEarnedID,
		t.Id AS TransactionID,
		cs.Name AS CustomerStatus
	FROM
		Loan l
		INNER JOIN Customer c
			ON l.CustomerID = c.Id
			AND c.IsTest = 0
		LEFT JOIN CustomerStatuses cs
			ON cs.Id = c.CollectionStatus
		INNER JOIN LoanTransaction la
			ON l.Id = la.LoanId
			AND la.Type = 'PacnetTransaction'
			AND la.Status = 'Done'
			AND la.PostDate < @DateEnd
		LEFT JOIN LoanTransaction t
			ON l.Id = t.LoanId
			AND t.Type = 'PaypointTransaction'
			AND t.Status = 'Done'
			AND t.PostDate < @DateEnd
		LEFT JOIN LoanCharges lc
			ON l.Id = lc.LoanId
			AND lc.Date < @DateEnd
		LEFT JOIN LoanTransactionMethod lm
			ON t.LoanTransactionMethodId = lm.Id
	WHERE
		l.DateClosed IS NULL
		OR
		l.DateClosed >= @DateStart
	ORDER BY
		l.Id,
		lc.Id,
		t.Id
END
GO