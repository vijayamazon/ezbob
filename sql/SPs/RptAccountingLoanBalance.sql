IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptAccountingLoanBalance]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptAccountingLoanBalance]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[RptAccountingLoanBalance] 
	(@DateStart DATETIME,
@DateEnd DATETIME)
AS
BEGIN
	SELECT
	l.Date AS IssueDate,
	c.Id AS ClientID,
	l.Id AS LoanID,
	c.Fullname AS ClientName,
	c.Name AS ClientEmail,
	la.Amount AS IssuedAmount,
	la.Fees AS SetupFee,
	ISNULL(SUM(lc.Amount), 0) AS FeesEarned,
	l.Status AS LoanStatus,
	lm.Name AS LoanTranMethod,
	ISNULL(SUM(ISNULL(t.Amount, 0)), 0) AS TotalRepaid,
	ISNULL(SUM(ISNULL(t.Fees, 0)), 0) AS FeesRepaid,
	ISNULL(SUM(ISNULL(t.Rollover, 0)), 0) AS RolloverRepaid
FROM
	Loan l
	INNER JOIN Customer c ON l.CustomerID = c.Id
	INNER JOIN LoanTransaction la
		ON l.Id = la.LoanId
		AND la.Type = 'PacnetTransaction'
		AND la.Status = 'Done'
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
GROUP BY
	l.Date,
	c.Id,
	l.Id,
	c.Fullname,
	c.Name,
	la.Amount,
	la.Fees,
	l.Status,
	lm.Name
ORDER BY
	l.Id
END
GO
