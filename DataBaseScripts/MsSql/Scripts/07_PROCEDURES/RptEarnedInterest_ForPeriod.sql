IF OBJECT_ID ('RptEarnedInterest_ForPeriod') IS NOT NULL
	DROP PROCEDURE RptEarnedInterest_ForPeriod
GO

CREATE PROCEDURE RptEarnedInterest_ForPeriod
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
	l.Date < @DateEnd
UNION
SELECT
	l.Id AS LoanID,
	CONVERT(DATE, l.Date) AS IssueDate,
	t.Amount
FROM
	Loan l
	INNER JOIN LoanSchedule s
		ON l.Id = s.LoanId
	INNER JOIN Customer c
		ON l.CustomerId = c.Id
		AND c.IsTest = 0
	INNER JOIN LoanTransaction t
		ON l.Id = t.LoanId
		AND t.Status = 'Done'
		AND t.Type = 'PacnetTransaction'
WHERE
	@DateStart <= s.Date
GO
