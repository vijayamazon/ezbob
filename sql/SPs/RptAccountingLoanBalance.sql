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
		l.Id AS LoanID,
		l.Status AS LoanStatus,
		c.Id AS ClientID,
		c.Fullname AS ClientName,
		c.Name AS ClientEmail,
		la.Amount AS IssuedAmount,
		la.Fees AS SetupFee,
		lm.Name AS LoanTranMethod,
		ISNULL(t.Amount, 0) AS TotalRepaid,
		ISNULL(t.LoanRepayment, 0) AS RepaidPrincipal,
		ISNULL(t.Interest, 0) AS RepaidInterest,
		ISNULL(t.Fees, 0) AS FeesRepaid,
		ISNULL(t.Rollover, 0) AS RolloverRepaid,
		t.PostDate AS TransactionDate,
		t.Id AS TransactionID,
		ISNULL(lc.Amount, 0) AS FeesEarned,
		lc.Id AS FeesEarnedID,
		lc.Date AS LoanChargeDate,
		c.RefNumber AS CustomerRefNum,
		l.RefNum AS LoanRefNum,
		CONVERT(BIT, CASE l.Position WHEN 0 THEN 1 ELSE 0 END) AS IsFirstLoan
	FROM
		Loan l
		INNER JOIN Customer c
			ON l.CustomerID = c.Id
			AND c.IsTest = 0
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
		l.DateClosed >= 'September 1 2012' -- @DateStart
	ORDER BY
		l.Id,
		lc.Id,
		t.Id
END
GO
