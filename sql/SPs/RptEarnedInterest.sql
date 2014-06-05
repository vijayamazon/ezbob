IF OBJECT_ID('RptEarnedInterest') IS NULL
BEGIN
	EXECUTE('CREATE PROCEDURE RptEarnedInterest AS SELECT 1')
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[RptEarnedInterest] 
	(@DateStart DATETIME, @DateEnd DATETIME)
AS
BEGIN
	SELECT
		l.Date AS IssueDate,
		c.Id AS ClientID,
		l.Id AS LoanID,
		c.Fullname AS ClientName,
		c.Name AS ClientEmail,
		la.Amount AS LoanAmount,
		ISNULL(SUM(t.Amount), 0) AS TotalRepaid,
		ISNULL(SUM(t.LoanRepayment), 0) AS PrincipalRepaid,
		cs.Name AS CustomerStatus
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerID = c.Id
		LEFT JOIN CustomerStatuses cs ON cs.Id = c.CollectionStatus
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
		la.Amount,
		cs.Name
END

GO
