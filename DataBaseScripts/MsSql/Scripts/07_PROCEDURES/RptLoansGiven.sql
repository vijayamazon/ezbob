IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptLoansGiven]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[RptLoansGiven]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE RptLoansGiven
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	CREATE TABLE #t (
		LoanID INT NOT NULL,
		Date DATETIME NULL,
		ClientID INT NOT NULL,
		ClientEmail NVARCHAR(128) NOT NULL,
		ClientName NVARCHAR(752) NOT NULL,
		LoanTypeName NVARCHAR(250) NOT NULL,
		SetupFee DECIMAL(18, 4) NOT NULL,
		LoanAmount NUMERIC(18, 0) NOT NULL,
		Period INT NOT NULL,
		PlannedInterest NUMERIC(38, 2) NOT NULL,
		PlannedRepaid NUMERIC(38, 2) NOT NULL,
		RowLevel NVARCHAR(5) NOT NULL
	)

	INSERT INTO #t
	SELECT
		l.Id AS LoanID,
		l.Date,
		c.Id AS ClientID,
		c.Name AS ClientEmail,
		c.FirstName + ' ' + c.MiddleInitial + ' ' + c.Surname AS ClientName,
		lt.Name AS LoanTypeName,
		l.SetupFee,
		l.LoanAmount,
		ISNULL(COUNT(*), 0) AS Period,
		ISNULL(SUM(s.Interest), 0) AS PlannedInterest,
		ISNULL(SUM(s.AmountDue), 0) AS PlannedRepaid,
		''
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerId = c.Id
		INNER JOIN LoanType lt ON l.LoanTypeId = lt.Id
		INNER JOIN LoanSchedule s ON l.Id = s.LoanId
	WHERE
		CONVERT(DATE, @DateStart) <= l.Date AND l.Date < CONVERT(DATE, @DateEnd)
		AND
		c.IsTest = 0
	GROUP BY
		l.Id,
		l.Date,
		c.Id,
		c.Name,
		c.FirstName + ' ' + c.MiddleInitial + ' ' + c.Surname,
		lt.Name,
		l.SetupFee,
		l.LoanAmount

	INSERT INTO #t
	SELECT
		COUNT(DISTINCT LoanID),
		NULL,
		COUNT(DISTINCT ClientID),
		'' AS ClientEmail,
		'Total' AS ClientName,
		'' AS LoanTypeName,
		ISNULL(SUM(SetupFee), 0),
		ISNULL(SUM(LoanAmount), 0),
		ISNULL(AVG(Period), 0),
		ISNULL(SUM(PlannedInterest), 0),
		ISNULL(SUM(PlannedRepaid), 0),
		'total'
	FROM
		#t
	WHERE
		RowLevel = ''

	SELECT
		LoanID,
		Date,
		ClientID,
		ClientEmail,
		ClientName,
		LoanTypeName,
		SetupFee,
		LoanAmount,
		Period,
		PlannedInterest,
		PlannedRepaid,
		RowLevel
	FROM
		#t
	ORDER BY
		RowLevel DESC,
		Date

	DROP TABLE #t
END
GO
