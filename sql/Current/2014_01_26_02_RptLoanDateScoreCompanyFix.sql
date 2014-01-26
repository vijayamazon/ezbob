IF OBJECT_ID('RptLoanDateScore') IS NULL
	EXECUTE('CREATE PROCEDURE RptLoanDateScore AS SELECT 1')
GO


ALTER PROCEDURE RptLoanDateScore
AS
	SELECT
		c.Id AS CustomerID,
		MAX(l.Date) AS LoanIssueDate,
		co.ExperianRefNum AS LimitedRefNum,
		co.ExperianRefNum AS NonLimitedRefNum
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		LEFT JOIN Company co ON co.CustomerId = c.Id
	GROUP BY
		c.Id,
		co.ExperianRefNum
	ORDER BY
		c.Id


GO

