IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].RptEarnedInterest') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].RptEarnedInterest
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE RptEarnedInterest
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SELECT
		@DateStart = CONVERT(DATE, @DateStart),
		@DateEnd = CONVERT(DATE, @DateEnd)

	--------------------------------------------------------

	CREATE TABLE #output (
		IssueDate DATE NULL,
		ClientID INT NOT NULL,
		LoanID INT NOT NULL,
		ClientName NVARCHAR(752) NOT NULL,
		ClientEmail NVARCHAR(128) NOT NULL,
		EarnedInterest DECIMAL(18, 4) NOT NULL,
		LoanAmount DECIMAL(18, 4) NOT NULL,
		TotalRepaid DECIMAL(18, 4) NOT NULL,
		PrincipalRepaid DECIMAL(18, 4) NOT NULL,
		RowLevel NVARCHAR(5) NOT NULL
	)

	--------------------------------------------------------

	INSERT INTO #output
	SELECT
		l.Date,
		c.Id AS ClientID,
		l.Id,
		c.Fullname AS ClientName,
		c.Name AS ClientEmail,
		i.EarnedInterest,
		l.LoanAmount,
		ISNULL(SUM(t.Amount), 0) AS TotalRepaid,
		ISNULL(SUM(t.LoanRepayment), 0) AS PrincipalRepaid,
		''
	FROM
		dbo.udfEarnedInterest(@DateStart, @DateEnd) i
		INNER JOIN Loan l ON i.LoanID = l.Id
		INNER JOIN Customer c ON l.CustomerID = c.Id
		LEFT JOIN LoanTransaction t
			ON l.ID = t.LoanId
			AND t.Type = 'PaypointTransaction'
			AND t.Status = 'Done'
	GROUP BY
		l.IssueDate,
		c.Id,
		l.LoanID,
		c.Fullname,
		c.Name,
		i.EarnedInterest,
		l.LoanAmount

	--------------------------------------------------------

	INSERT INTO #output
	SELECT
		NULL,
		COUNT(DISTINCT ClientID),
		COUNT(DISTINCT LoanID),
		'Total',
		'',
		ISNULL(SUM(EarnedInterest), 0),
		ISNULL(SUM(LoanAmount), 0),
		ISNULL(SUM(TotalRepaid), 0),
		ISNULL(SUM(PrincipalRepaid), 0),
		'total'
	FROM
		#output
	WHERE
		RowLevel = ''

	--------------------------------------------------------

	SELECT
		IssueDate,
		ClientID,
		LoanID,
		ClientName,
		ClientEmail,
		EarnedInterest,
		LoanAmount,
		TotalRepaid,
		PrincipalRepaid,
		RowLevel
	FROM
		#output
	ORDER BY
		RowLevel DESC,
		IssueDate,
		ClientName
	
	--------------------------------------------------------

	DROP TABLE #output
END

GO
