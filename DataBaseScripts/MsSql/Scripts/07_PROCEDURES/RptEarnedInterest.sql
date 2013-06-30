IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptEarnedInterest]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptEarnedInterest]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
CREATE PROCEDURE RptEarnedInterest
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	--------------------------------------------------------
	--
	-- Declarations.
	--
	--------------------------------------------------------
	
	-- Relevant loans only.
	CREATE TABLE #loans (
		LoanID INT NOT NULL,
		CustomerID INT NULL,
		IssueDate DATE NULL,
		LoanAmount DECIMAL(18, 4) NULL
	)
	
	-- Loan schedule entries for relevant loans. Seqnum is always from 1 to N with step 1.
	CREATE TABLE #sched (
		Seqnum INT NOT NULL,
		LoanID INT NOT NULL,
		Date DATE NOT NULL,
		Rate DECIMAL(18, 4) NOT NULL
	)
	
	-- Last scheduled payment for each loan. Used to calculate a rate for those who
	-- didn't pay on time.
	CREATE TABLE #last_sched (
		Seqnum INT NOT NULL,
		LoanID INT NOT NULL
	)
	
	-- Interest rate by period (from issuing a loan until the last payment date).
	CREATE TABLE #rates (
		SeqnumStart INT NOT NULL,
		SeqnumEnd INT NOT NULL,
		LoanID INT NOT NULL,
		DateStart DATE NOT NULL,
		RateStart DECIMAL(18, 4) NULL,
		DateEnd DATE NOT NULL,
		RateEnd DECIMAL(18, 4) NOT NULL
	)
	
	-- Principal and interest rate of specific loan on specific date.
	-- Interest rate is divided by period length.
	-- I.e. for every row: principal * rate / periodlen = interest we earn for that loan on that day.
	CREATE TABLE #daily (
		LoanID INT NOT NULL,
		Date DATETIME NOT NULL,
		Principal DECIMAL(18, 4) NOT NULL,
		InterestRate DECIMAL(18, 4) NULL,
		PeriodLen DECIMAL(18, 4) NULL
	)
	
	-- Earned interest storage.
	CREATE TABLE #earned_interest (
		LoanID INT NOT NULL,
		EarnedInterest DECIMAL(18, 4) NOT NULL
	)
	
	-- Total repaid storage.
	CREATE TABLE #total_repaid  (
		LoanID INT NOT NULL,
		Repaid DECIMAL(18, 4) NOT NULL
	)
	
	-- Principal repaid storage.
	CREATE TABLE #principal_repaid  (
		LoanID INT NOT NULL,
		Repaid DECIMAL(18, 4) NOT NULL
	)

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
	--
	-- Ok, let's go to work!
	-- Who the fuck are you?!
	-- The workaholic!
	--
	--                    2 unlimited
	--                    Workaholic
	--
	--------------------------------------------------------
	
	--------------------------------------------------------
	--
	-- Retrieving relevant loans (id only).
	--
	--------------------------------------------------------
	
	INSERT INTO #loans(LoanID)
	SELECT DISTINCT
		Id
	FROM
		Loan
	WHERE
		Date BETWEEN @DateStart AND @DateEnd
	UNION
	SELECT DISTINCT
		LoanId
	FROM
		LoanSchedule
	WHERE
		Date BETWEEN @DateStart AND @DateEnd
	
	--------------------------------------------------------
	--
	-- Loading loan amount for relevant loans.
	--
	--------------------------------------------------------
	
	UPDATE #loans SET
		LoanAmount = Loan.LoanAmount,
		IssueDate = CONVERT(DATE, Loan.Date),
		CustomerID = Loan.CustomerId
	FROM
		Loan,
		Customer
	WHERE
		#loans.LoanID = Loan.Id
		AND
		Loan.CustomerId = Customer.Id
		AND
		Customer.IsTest = 0
	
	DELETE FROM
		#loans
	WHERE
		CustomerID IS NULL
	
	--------------------------------------------------------
	--
	-- Normalising payment list: setting Seqnum to 1..N.
	--
	--------------------------------------------------------
	
	INSERT INTO #sched
	SELECT
		RANK() OVER (PARTITION BY s.LoanId ORDER BY s.LoanId, s.Date),
		l.LoanID,
		CAST(s.Date AS DATE),
		s.InterestRate
	FROM
		LoanSchedule s
		INNER JOIN #loans l ON s.LoanID = l.LoanId
	
	--------------------------------------------------------
	--
	-- Filling last scheduled payment.
	--
	--------------------------------------------------------
	
	INSERT INTO #last_sched
	SELECT
		MAX(Seqnum),
		LoanID
	FROM
		#sched
	GROUP BY
		LoanID
	
	--------------------------------------------------------
	--
	-- Loading interest rates for period from the first
	-- payment till the last payment.
	--
	--------------------------------------------------------
	
	INSERT INTO #rates
	SELECT
		l1.Seqnum,
		l2.Seqnum,
		l1.LoanID,
		CAST(l1.Date AS DATE),
		l1.Rate,
		CAST(l2.Date AS DATE),
		l2.Rate
	FROM
		#sched l1
		LEFT JOIN #sched l2
			ON l1.LoanId = l2.LoanId
			AND l1.Seqnum = l2.Seqnum - 1
	WHERE
		l2.Date IS NOT NULL
	
	--------------------------------------------------------
	--
	-- Loading interest rates for period from loan issue
	-- till the first payment.
	--
	--------------------------------------------------------
	
	INSERT INTO #rates
	SELECT
		0,
		1,
		l.LoanID,
		CAST(ol.Date AS DATE),
		NULL,
		r.DateStart,
		r.RateStart
	FROM
		#loans l
		INNER JOIN Loan ol ON l.LoanID = ol.Id
		INNER JOIN #rates r ON l.LoanID = r.LoanId AND r.SeqnumStart = 1
	
	--------------------------------------------------------
	--
	-- At this point:
	--
	-- #loans
	--    Contains relevant loans.
	--
	-- #sched
	--    Contains normalised payment schedule for
	--    relevant loans.
	--
	-- #rates
	--    Each row contains interest rate of specific loan
	--    during specific period (start date inclusive,
	--    end date exclusive).
	--
	-- #daily
	--    Not used so far.
	--    Now it's time to use it.
	--
	--------------------------------------------------------
	
	--------------------------------------------------------
	--
	-- Filling initial daily data.
	--
	--------------------------------------------------------
	
	DECLARE @Date DATETIME
	
	SET @Date = CONVERT(Date, @DateStart)
	
	WHILE @Date < CONVERT(DATE, @DateEnd)
	BEGIN
		INSERT INTO #daily(LoanID, Date, Principal)
		SELECT
			LoanID,
			@Date,
			LoanAmount
		FROM
			#loans
		WHERE
			@Date >= #loans.IssueDate
	
		SET @Date = DATEADD(day, 1, @Date)
	END
	
	--------------------------------------------------------
	--
	-- Updating principal with customer payments.
	--
	--------------------------------------------------------
	
	UPDATE #daily SET
		#daily.Principal = #daily.Principal - ISNULL((
			SELECT SUM(t.LoanRepayment)
			FROM LoanTransaction t
			WHERE #daily.LoanID = t.LoanId
			AND t.LoanRepayment > 0
			AND t.Status = 'Done'
			AND t.Type = 'PaypointTransaction'
			AND #daily.Date >= CONVERT(DATE, t.PostDate)
		), 0)
	
	DELETE FROM
		#daily
	WHERE
		Principal = 0
	
	--------------------------------------------------------
	--
	-- Setting daily interest rate.
	--
	--------------------------------------------------------
	
	UPDATE #daily SET
		InterestRate = r.RateEnd,
		PeriodLen = DATEDIFF(day, r.DateStart, r.DateEnd)
	FROM
		#rates r
	WHERE
		#daily.LoanID = r.LoanId
		AND
		r.DateStart <= #daily.Date AND #daily.Date < r.DateEnd
	
	UPDATE #daily SET
		InterestRate = r.RateEnd,
		PeriodLen = DATEDIFF(day, r.DateStart, r.DateEnd)
	FROM
		#last_sched l,
		#rates r
	WHERE
		l.LoanID = r.LoanID AND l.Seqnum = r.SeqnumEnd
		AND
		#daily.LoanID = l.LoanID AND #daily.Date >= r.DateEnd
	
	--------------------------------------------------------
	--
	-- Building result.
	--
	--------------------------------------------------------
	
	INSERT INTO #earned_interest
	SELECT
		LoanID,
		SUM(Principal * InterestRate / PeriodLen) AS EarnedInterest
	FROM
		#daily
	GROUP BY
		LoanID
	
	--------------------------------------------------------
	--
	-- Building total repaid.
	--
	--------------------------------------------------------
	
	INSERT INTO #total_repaid
	SELECT
		t.LoanId,
		ISNULL(SUM(t.Amount), 0)
	FROM
		LoanTransaction t
		INNER JOIN #loans l ON t.LoanId = l.LoanID
	WHERE
		t.Type = 'PaypointTransaction'
		AND
		t.Status = 'Done'
	GROUP BY
		t.LoanId
	
	--------------------------------------------------------
	--
	-- Building principal repaid.
	--
	--------------------------------------------------------
	
	INSERT INTO #principal_repaid
	SELECT
		t.LoanId,
		ISNULL(SUM(t.LoanRepayment), 0)
	FROM
		LoanTransaction t
		INNER JOIN #loans l ON t.LoanId = l.LoanID
	WHERE
		t.Type = 'PaypointTransaction'
		AND
		t.Status = 'Done'
	GROUP BY
		t.LoanId
	
	--------------------------------------------------------
	--
	-- Building output.
	--
	--------------------------------------------------------

	INSERT INTO #output
	SELECT
		l.IssueDate,
		c.Id AS ClientID,
		l.LoanID,
		c.FirstName + ' ' + c.MiddleInitial + ' ' + c.Surname AS ClientName,
		c.Name AS ClientEmail,
		i.EarnedInterest,
		l.LoanAmount,
		ISNULL(tr.Repaid, 0) AS TotalRepaid,
		ISNULL(pr.Repaid, 0) AS PrincipalRepaid,
		''
	FROM
		#earned_interest i
		INNER JOIN #loans l ON i.LoanID = l.LoanID
		INNER JOIN Customer c ON l.CustomerID = c.Id
		LEFT JOIN #total_repaid tr ON l.LoanID = tr.LoanID
		LEFT JOIN #principal_repaid pr ON l.LoanID = pr.LoanID

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
	--
	-- Cleanup.
	--
	--------------------------------------------------------

	DROP TABLE #output
	DROP TABLE #principal_repaid
	DROP TABLE #total_repaid
	DROP TABLE #earned_interest
	DROP TABLE #daily
	DROP TABLE #rates
	DROP TABLE #last_sched
	DROP TABLE #sched
	DROP TABLE #loans
END
GO
