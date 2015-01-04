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

	;WITH payp AS (
		SELECT
			t.LoanId,
			ISNULL(SUM(ISNULL(t.Amount, 0)), 0) AS TotalRepaid,
			ISNULL(SUM(ISNULL(t.LoanRepayment, 0)), 0) AS PrincipalRepaid,
			ISNULL(SUM(ISNULL(t.Fees, 0)), 0) AS OtherFees,
			ISNULL(SUM(ISNULL(t.Rollover, 0)), 0) AS Rollover
		FROM
			LoanTransaction t
		WHERE
			t.Type = 'PaypointTransaction'
			AND 
			t.Status = 'Done'
		GROUP BY
			t.LoanId
	), pac AS (
		SELECT
			la.LoanId,
			ISNULL(SUM(ISNULL(la.Fees, 0)), 0) AS SetupFees
		FROM
			LoanTransaction la
		WHERE
			la.Type = 'PacnetTransaction'
			AND
			la.Status = 'Done'
		GROUP BY
			la.LoanId
	)
	SELECT
		l.Date AS IssueDate,
		c.Id AS ClientID,
		l.Id AS LoanID,
		c.Fullname AS ClientName,
		c.Name AS ClientEmail,
		l.LoanAmount AS LoanAmount,
		ISNULL(t.TotalRepaid, 0) AS TotalRepaid,
		ISNULL(t.PrincipalRepaid, 0) AS PrincipalRepaid,
		ISNULL(la.SetupFees, 0) AS SetupFees,
		ISNULL(t.OtherFees, 0) AS OtherFees,
		ISNULL(t.Rollover, 0) AS Rollover
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerID = c.Id
		LEFT JOIN pac la
			ON l.Id = la.LoanId
		LEFT JOIN payp t
			ON l.Id = t.LoanId
END
GO
