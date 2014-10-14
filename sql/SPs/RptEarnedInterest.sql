IF OBJECT_ID('RptEarnedInterest') IS NULL
	EXECUTE('CREATE PROCEDURE RptEarnedInterest AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptEarnedInterest
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		l.Date AS IssueDate,
		c.Id AS ClientID,
		l.Id AS LoanID,
		c.Fullname AS ClientName,
		c.Name AS ClientEmail,
		la.Amount AS LoanAmount,
		ISNULL(SUM(ISNULL(t.Amount, 0)), 0) AS TotalRepaid,
		ISNULL(SUM(ISNULL(t.LoanRepayment, 0)), 0) AS PrincipalRepaid,
		ISNULL(SUM(ISNULL(la.Fees, 0)), 0) AS SetupFees,
		ISNULL(SUM(ISNULL(t.Fees, 0)), 0) AS OtherFees,
		ISNULL(SUM(ISNULL(t.Rollover, 0)), 0) AS Rollover
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerID = c.Id
		INNER JOIN LoanTransaction la
			ON l.Id = la.LoanId
			AND la.Type = 'PacnetTransaction'
			AND la.Status = 'Done'
		LEFT JOIN LoanTransaction t
			ON l.ID = t.LoanId
			AND t.Type = 'PaypointTransaction'
			AND t.Status = 'Done'
	GROUP BY
		l.Date,
		c.Id,
		l.ID,
		c.Fullname,
		c.Name,
		la.Amount
END
GO
