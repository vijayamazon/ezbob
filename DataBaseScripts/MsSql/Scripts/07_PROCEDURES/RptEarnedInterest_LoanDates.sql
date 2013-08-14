IF OBJECT_ID('RptEarnedInterest_LoanDates') IS NOT NULL
	DROP PROCEDURE RptEarnedInterest_LoanDates
GO

CREATE PROCEDURE RptEarnedInterest_LoanDates
AS
SELECT
	CONVERT(INT, 0),
	s.LoanId,
	CONVERT(DATE, s.Date),
	s.InterestRate
FROM
	LoanSchedule s
	INNER JOIN Loan l ON s.LoanId = l.Id
	INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
UNION
SELECT
	CONVERT(INT, 1),
	t.LoanId,
	CONVERT(DATE, t.PostDate),
	SUM(t.LoanRepayment)
FROM
	LoanTransaction t
	INNER JOIN Loan l ON t.LoanId = l.Id
	INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
WHERE
	t.Status = 'Done'
	AND
	t.Type = 'PaypointTransaction'
	AND
	t.LoanRepayment > 0
GROUP BY
	t.LoanId,
	CONVERT(DATE, t.PostDate)
ORDER BY
	2, 3, 1
GO
