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
SELECT
	l.Id AS LoanID,
	l.Date,
	c.Id AS ClientID,
	c.Name AS ClientEmail,
	c.FirstName + ' ' + c.MiddleInitial + ' ' + c.Surname AS ClientName,
	lt.Name AS LoanTypeName,
	l.SetupFee,
	l.LoanAmount,
	COUNT(*) AS Period,
	SUM(s.Interest) AS PlannedInterest,
	SUM(s.AmountDue) AS PlannedRepaid
FROM
	Loan l
	INNER JOIN Customer c ON l.CustomerId = c.Id
	INNER JOIN LoanType lt ON l.LoanTypeId = lt.Id
	INNER JOIN LoanSchedule s ON l.Id = s.LoanId
WHERE
	l.Date BETWEEN @DateStart AND @DateEnd
GROUP BY
	l.Id,
	l.Date,
	c.Id,
	c.Name,
	c.FirstName + ' ' + c.MiddleInitial + ' ' + c.Surname,
	lt.Name,
	l.SetupFee,
	l.LoanAmount
ORDER BY
	l.Date
GO
