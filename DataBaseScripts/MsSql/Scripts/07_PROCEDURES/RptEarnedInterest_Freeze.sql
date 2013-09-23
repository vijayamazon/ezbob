IF OBJECT_ID('RptEarnedInterest_Freeze') IS NOT NULL
	DROP PROCEDURE RptEarnedInterest_Freeze
GO

CREATE PROCEDURE RptEarnedInterest_Freeze
AS
SELECT
	f.LoanId,
	f.StartDate,
	f.EndDate,
	f.InterestRate,
	f.ActivationDate,
	f.DeactivationDate
FROM
	LoanInterestFreeze f
	INNER JOIN Loan l ON f.LoanId = l.Id
	INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
ORDER BY
	f.LoanId,
	(CASE WHEN f.DeactivationDate IS NULL THEN 1 ELSE 0 END),
	f.ActivationDate
GO
