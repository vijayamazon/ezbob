IF OBJECT_ID ('RptEarnedInterest_IssuedLoans') IS NOT NULL
	DROP PROCEDURE RptEarnedInterest_IssuedLoans
GO

CREATE PROCEDURE RptEarnedInterest_IssuedLoans
@DateStart DATE,
@DateEnd DATE
AS
SELECT
	l.Id AS LoanID,
	CONVERT(DATE, l.Date) AS IssueDate,
	t.Amount
FROM
	Loan l
	INNER JOIN Customer c
		ON l.CustomerId = c.Id
		AND c.IsTest = 0
	INNER JOIN LoanTransaction t
		ON l.Id = t.LoanId
		AND t.Status = 'Done'
		AND t.Type = 'PacnetTransaction'
WHERE
	@DateStart <= l.Date AND l.Date < @DateEnd
GO
