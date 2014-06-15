IF OBJECT_ID('RptEarnedInterest_ForPeriod') IS NULL
	EXECUTE('CREATE PROCEDURE RptEarnedInterest_ForPeriod AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptEarnedInterest_ForPeriod
@DateStart DATE,
@DateEnd DATE
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		l.Id AS LoanID,
		CONVERT(DATE, l.Date) AS IssueDate,
		t.Amount,
		l.CustomerId
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
		t.Amount,
		l.CustomerId
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
END
GO
