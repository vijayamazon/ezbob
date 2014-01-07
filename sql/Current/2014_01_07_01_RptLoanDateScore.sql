IF OBJECT_ID('RptLoanDateScore') IS NULL
	EXECUTE('CREATE PROCEDURE RptLoanDateScore AS SELECT 1')
GO

ALTER PROCEDURE RptLoanDateScore
AS
	SELECT
		c.Id AS CustomerID,
		MAX(l.Date) AS LoanIssueDate,
		c.LimitedRefNum,
		c.NonLimitedRefNum
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	GROUP BY
		c.Id,
		c.LimitedRefNum,
		c.NonLimitedRefNum
	ORDER BY
		c.Id

GO
