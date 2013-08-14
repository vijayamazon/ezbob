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
		TotalPrincipalRepaid NUMERIC(38, 2) NOT NULL,
		TotalInterestRepaid NUMERIC(38, 2) NOT NULL,
		EarnedInterest NUMERIC(38, 2) NOT NULL,
		ExpectedInterest NUMERIC(38, 2) NOT NULL,
		AccruedInterest NUMERIC(38, 2) NOT NULL,
		TotalInterest NUMERIC(38, 2) NOT NULL,
		TotalFeesRepaid NUMERIC(38, 2) NOT NULL,
		TotalCharges NUMERIC(38, 2) NOT NULL,
		BaseInterest DECIMAL(18, 4) NULL,
		DiscountPlan NVARCHAR(512) NOT NULL,
		RowLevel NVARCHAR(32) NOT NULL,
		SortOrder INT NOT NULL
	)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	
	INSERT INTO #t
	SELECT
		l.Id AS LoanID,
		l.Date,
		c.Id AS ClientID,
		c.Name AS ClientEmail,
		c.Fullname AS ClientName,
		lt.Name AS LoanTypeName,
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
		ISNULL(fc.Fees, 0) AS TotalFees,
		ISNULL(fc.Charges, 0) AS TotalCharges,
		l.InterestRate AS BaseInterest,
		dp.Name AS DiscountPlan,
		CASE ISNULL(out.Counter, 0)
			WHEN 1 THEN ''
			ELSE 'unmatched'
		END,
		0
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		INNER JOIN LoanType lt ON l.LoanTypeId = lt.Id
		INNER JOIN (
			SELECT
				s.LoanId,
				COUNT(DISTINCT s.Id) AS Period,
				SUM(s.Interest) AS PlannedInterest,
				SUM(s.AmountDue) AS PlannedRepaid
			FROM
				LoanSchedule s
				INNER JOIN Loan l ON s.LoanId = l.Id
			WHERE
				@DateStart <= l.Date AND l.Date < @DateEnd
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
		 		AND
				@DateStart <= l.Date AND l.Date < @DateEnd
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
		 		AND
				@DateStart <= l.Date AND l.Date < @DateEnd
			GROUP BY
				t.LoanId
		) pay ON l.Id = pay.LoanId
		LEFT JOIN dbo.udfLoanFeesAndCharges(@DateStart, @DateEnd) fc ON l.Id = fc.LoanID
	WHERE
		CONVERT(DATE, @DateStart) <= l.Date AND l.Date < CONVERT(DATE, @DateEnd)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	DECLARE @LoanIDs LoanIdListTable

	------------------------------------------------------------------------------

	INSERT INTO @LoanIDs
	SELECT DISTINCT
		LoanID
	FROM
		#t

	------------------------------------------------------------------------------

	UPDATE #t SET
		EarnedInterest = i.EarnedInterest
	FROM
		#t
		INNER JOIN dbo.udfEarnedInterestForLoans(NULL, NULL, @LoanIDs) i
			ON #t.LoanID = i.LoanID

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	-- A NOTE. UPDATE works in "batch mode" meaning that field
	-- values are changed for entire row at the same time so that assignments in
	-- this UPDATE operator does not affect follwing assignments in THE SAME
	-- operator.

	-- Accrued = Earned - Repaid
	
	-- Expected: until now the field ExpectedInterest only contains sum of planned
	-- interest from LoanSchedule. Accrued should be subtructed from it. Because
	-- of A NOTE it shows Expected - Earned + Repaid instead of Expected - Accrued.

	-- Total = Earned + Expected. Because of A NOTE and some math:
	-- Total = Earned + Expected =
	--       = Earned + (Expected - Accrued) =
	--       = Earned + (Expected - (Earned - Repaid)) =
	--       = Earned + (Expected - Earned + Repaid) =
	--       = Earned + Expected - Earned + Repaid =
	--       = Expected + Repaid

	UPDATE #t SET
		AccruedInterest = EarnedInterest - TotalInterestRepaid,
		ExpectedInterest = ExpectedInterest - EarnedInterest + TotalInterestRepaid,
		TotalInterest = ExpectedInterest + TotalInterestRepaid

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

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
		ISNULL(SUM(TotalPrincipalRepaid), 0),
		ISNULL(SUM(TotalInterestRepaid), 0),
		ISNULL(SUM(EarnedInterest), 0),
		ISNULL(SUM(ExpectedInterest), 0),
		ISNULL(SUM(AccruedInterest), 0),
		ISNULL(SUM(TotalInterest), 0),
		ISNULL(SUM(TotalFeesRepaid), 0),
		ISNULL(SUM(TotalCharges), 0),
		NULL AS BaseInterest,
		'' AS DiscountPlan,
		'total',
		1
	FROM
		#t
	WHERE
		SortOrder = 0

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

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
		TotalPrincipalRepaid,
		TotalInterestRepaid,
		EarnedInterest,
		ExpectedInterest,
		AccruedInterest,
		TotalInterest,
		TotalFeesRepaid,
		TotalCharges,
		BaseInterest,
		DiscountPlan,
		RowLevel
	FROM
		#t
	ORDER BY
		SortOrder DESC,
		Date

	DROP TABLE #t
END
GO
