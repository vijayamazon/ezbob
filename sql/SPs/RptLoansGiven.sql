IF OBJECT_ID('RptLoansGiven') IS NULL
BEGIN
	EXECUTE('CREATE PROCEDURE RptLoansGiven AS SELECT 1')
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[RptLoansGiven] 
	(@DateStart DATETIME,@DateEnd DATETIME)
AS
BEGIN
	SELECT
		l.Date,
		c.Id AS ClientID,
		l.Id AS LoanID,
		c.Name AS ClientEmail,
		c.Fullname AS ClientName,
		lt.Name AS LoanTypeName,
		CASE WHEN ls.LoanSourceName = 'EU' THEN 'EU' ELSE '' END AS EU,
		ISNULL(out.Fees, 0) AS SetupFee,
		ISNULL(out.Amount, 0) AS LoanAmount,
		s.Period,
		s.PlannedInterest,
		s.PlannedRepaid,
		ISNULL(pay.TotalPrincipalRepaid, 0) AS TotalPrincipalRepaid,
		ISNULL(pay.TotalInterestRepaid, 0) AS TotalInterestRepaid,
		0 AS EarnedInterest,
		ISNULL(exi.ExpectedInterest, 0) AS ExpectedInterest,
		0 AS AccruedInterest,
		0 AS TotalInterest,
		ISNULL(fc.Fees, 0) AS TotalFeesRepaid,
		ISNULL(fc.Charges, 0) AS TotalCharges,
		l.InterestRate AS BaseInterest,
		dp.Name AS DiscountPlan,
		cs.Name AS CustomerStatus,
		CASE ISNULL(out.Counter, 0)
			WHEN 1 THEN ''
			ELSE 'unmatched'
		END AS RowLevel
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		LEFT JOIN CustomerStatuses cs ON cs.Id = c.CollectionStatus
		INNER JOIN LoanType lt ON l.LoanTypeId = lt.Id
		INNER JOIN LoanSource ls ON ls.LoanSourceID = l.LoanSourceID
		INNER JOIN (
			SELECT
				s.LoanId,
				COUNT(DISTINCT s.Id) AS Period,
				SUM(s.Interest) AS PlannedInterest,
				SUM(s.AmountDue) AS PlannedRepaid
			FROM
				LoanSchedule s
				INNER JOIN Loan l ON s.LoanId = l.Id
			GROUP BY
				s.LoanId
		) s ON l.Id = s.LoanId
		INNER JOIN CashRequests cr ON l.RequestCashId = cr.Id
		INNER JOIN DiscountPlan dp ON cr.DiscountPlanId = dp.Id
		LEFT JOIN (
			SELECT
				s.LoanId,
				SUM(s.Interest) AS ExpectedInterest
			FROM
				LoanSchedule s
			WHERE
				s.Date > GETDATE()
			GROUP BY
				s.LoanId
		) exi ON l.Id = exi.LoanId
		LEFT JOIN (
			SELECT
				t.LoanId,
				SUM(t.Amount) AS Amount,
				SUM(t.Fees) AS Fees,
				COUNT(DISTINCT t.Id) AS Counter
			FROM
				LoanTransaction t
				INNER JOIN Loan l ON t.LoanId = l.Id
			WHERE
		 		t.Status = 'Done'
		 		AND
		 		t.Type = 'PacnetTransaction'
			GROUP BY
				t.LoanId
		) out ON l.Id = out.LoanId
		LEFT JOIN (
			SELECT
				t.LoanId,
				SUM(t.LoanRepayment) AS TotalPrincipalRepaid,
				SUM(t.Interest) AS TotalInterestRepaid
			FROM
				LoanTransaction t
				INNER JOIN Loan l ON t.LoanId = l.Id
			WHERE
		 		t.Status = 'Done'
		 		AND
		 		t.Type = 'PaypointTransaction'
			GROUP BY
				t.LoanId
		) pay ON l.Id = pay.LoanId
		LEFT JOIN dbo.udfLoanFeesAndCharges(@DateStart, @DateEnd) fc ON l.Id = fc.LoanID
	WHERE
		CONVERT(DATE, @DateStart) <= l.Date AND l.Date < CONVERT(DATE, @DateEnd)
	ORDER BY
		l.Date
END
GO