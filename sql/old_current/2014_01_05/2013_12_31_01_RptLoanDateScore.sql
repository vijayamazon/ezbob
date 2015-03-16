IF OBJECT_ID('RptLoanDateScore') IS NULL
	EXECUTE('CREATE PROCEDURE RptLoanDateScore AS SELECT 1')
GO

ALTER PROCEDURE RptLoanDateScore
AS
	SELECT
		c.Id AS CustomerID,
		MAX(CONVERT(DATE, l.Date)) AS LoanIssueDate,
		c.LimitedRefNum
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	GROUP BY
		c.Id,
		c.LimitedRefNum
	ORDER BY
		c.Id
GO
