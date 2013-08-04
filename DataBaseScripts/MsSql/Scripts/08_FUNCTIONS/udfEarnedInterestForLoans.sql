IF OBJECT_ID(N'[dbo].udfEarnedInterestForLoans') IS NOT NULL
	DROP FUNCTION [dbo].udfEarnedInterestForLoans
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF TYPE_ID('LoanIdListTable') IS NOT NULL
	DROP TYPE LoanIdListTable
GO

CREATE TYPE LoanIdListTable AS TABLE (LoanID INT NOT NULL)
GO

CREATE FUNCTION udfEarnedInterestForLoans(
	@DateStart DATETIME,
	@DateEnd DATETIME,
	@loan_ids LoanIdListTable READONLY
)
RETURNS @earned_interest TABLE (
	LoanID INT NOT NULL,
	EarnedInterest DECIMAL(18, 4) NOT NULL
)
AS
BEGIN
	--------------------------------------------------------
	--
	-- Declarations.
	--
	--------------------------------------------------------

	-- Relevant loans only.
	DECLARE @loans TABLE (
		LoanID INT NOT NULL,
		CustomerID INT NULL,
		IssueDate DATE NULL,
		LoanAmount DECIMAL(18, 4) NULL
	)

	-- Loan schedule entries for relevant loans. Seqnum is always from 1 to N with step 1.
	DECLARE @sched TABLE (
		Seqnum INT NOT NULL,
		LoanID INT NOT NULL,
		Date DATE NOT NULL,
		Rate DECIMAL(18, 4) NOT NULL
	)

	-- Last scheduled payment for each loan. Used to calculate a rate for those who
	-- didn't pay on time.
	DECLARE @last_sched TABLE (
		Seqnum INT NOT NULL,
		LoanID INT NOT NULL
	)

	-- Interest rate by period (from issuing a loan until the last payment date).
	DECLARE @rates TABLE (
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
	DECLARE @daily TABLE (
		LoanID INT NOT NULL,
		Date DATETIME NOT NULL,
		Principal DECIMAL(18, 4) NOT NULL,
		InterestRate DECIMAL(18, 4) NULL,
		PeriodLen DECIMAL(18, 4) NULL
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
	
	INSERT INTO @loans(LoanID)
	SELECT DISTINCT
		LoanID
	FROM
		@loan_ids

	--------------------------------------------------------
	--
	-- Setting dates.
	--
	--------------------------------------------------------

	IF @DateStart IS NULL
		SELECT
			@DateStart = CONVERT(DATE, MIN(l.Date))
		FROM
			Loan l
			INNER JOIN @loans rl ON l.Id = rl.LoanID

	IF @DateEnd IS NULL
		SET @DateEnd = CONVERT(DATE, DATEADD(day, 1, GETDATE()))

	--------------------------------------------------------
	--
	-- Loading loan amount for relevant loans.
	--
	--------------------------------------------------------

	UPDATE @loans SET
		LoanAmount = l.LoanAmount,
		IssueDate = CONVERT(DATE, l.Date),
		CustomerID = l.CustomerId
	FROM
		@loans rl
		INNER JOIN Loan l ON rl.LoanID = l.Id
		INNER JOIN Customer c
			ON l.CustomerId = c.Id
			AND c.IsTest = 0

	DELETE FROM
		@loans
	WHERE
		CustomerID IS NULL
	
	--------------------------------------------------------
	--
	-- Normalising payment list: setting Seqnum to 1..N.
	--
	--------------------------------------------------------
	
	INSERT INTO @sched
	SELECT
		RANK() OVER (PARTITION BY s.LoanId ORDER BY s.LoanId, s.Date),
		l.LoanID,
		CAST(s.Date AS DATE),
		s.InterestRate
	FROM
		LoanSchedule s
		INNER JOIN @loans l ON s.LoanID = l.LoanId
	
	--------------------------------------------------------
	--
	-- Filling last scheduled payment.
	--
	--------------------------------------------------------
	
	INSERT INTO @last_sched
	SELECT
		MAX(Seqnum),
		LoanID
	FROM
		@sched
	GROUP BY
		LoanID
	
	--------------------------------------------------------
	--
	-- Loading interest rates for period from the first
	-- payment till the last payment.
	--
	--------------------------------------------------------
	
	INSERT INTO @rates
	SELECT
		l1.Seqnum,
		l2.Seqnum,
		l1.LoanID,
		CAST(l1.Date AS DATE),
		l1.Rate,
		CAST(l2.Date AS DATE),
		l2.Rate
	FROM
		@sched l1
		LEFT JOIN @sched l2
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
	
	INSERT INTO @rates
	SELECT
		0,
		1,
		l.LoanID,
		CAST(ol.Date AS DATE),
		NULL,
		r.DateStart,
		r.RateStart
	FROM
		@loans l
		INNER JOIN Loan ol ON l.LoanID = ol.Id
		INNER JOIN @rates r ON l.LoanID = r.LoanId AND r.SeqnumStart = 1
	
	--------------------------------------------------------
	--
	-- At this point:
	--
	-- @loans
	--    Contains relevant loans.
	--
	-- @sched
	--    Contains normalised payment schedule for
	--    relevant loans.
	--
	-- @rates
	--    Each row contains interest rate of specific loan
	--    during specific period (start date inclusive,
	--    end date exclusive).
	--
	-- @daily
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
	
	SET @Date = @DateStart
	
	WHILE @Date < @DateEnd
	BEGIN
		INSERT INTO @daily(LoanID, Date, Principal)
		SELECT
			LoanID,
			@Date,
			LoanAmount
		FROM
			@loans
		WHERE
			@Date > IssueDate
	
		SET @Date = DATEADD(day, 1, @Date)
	END
	
	--------------------------------------------------------
	--
	-- Updating principal with customer payments.
	--
	--------------------------------------------------------
	
	UPDATE @daily SET
		Principal = d.Principal - ISNULL((
			SELECT SUM(t.LoanRepayment)
			FROM LoanTransaction t
			WHERE d.LoanID = t.LoanId
			AND t.LoanRepayment > 0
			AND t.Status = 'Done'
			AND t.Type = 'PaypointTransaction'
			AND d.Date > CONVERT(DATE, t.PostDate)
		), 0)
	FROM
		@daily d
	
	DELETE FROM
		@daily
	WHERE
		Principal = 0
	
	--------------------------------------------------------
	--
	-- Setting daily interest rate.
	--
	--------------------------------------------------------
	
	UPDATE @daily SET
		InterestRate = r.RateEnd,
		PeriodLen = DATEDIFF(day, r.DateStart, r.DateEnd)
	FROM
		@daily d
		INNER JOIN @rates r ON d.LoanID = r.LoanId
	WHERE
		r.DateStart <= d.Date AND d.Date < r.DateEnd

	--------------------------------------------------------
	
	UPDATE @daily SET
		InterestRate = r.RateEnd,
		PeriodLen = DATEDIFF(day, r.DateStart, r.DateEnd)
	FROM
		@daily d
		INNER JOIN @last_sched l ON d.LoanID = l.LoanID
		INNER JOIN @rates r
			ON l.LoanID = r.LoanID
			AND l.Seqnum = r.SeqnumEnd
	WHERE
		 d.Date >= r.DateEnd
	
	--------------------------------------------------------
	--
	-- Building result.
	--
	--------------------------------------------------------
	
	INSERT INTO @earned_interest
	SELECT
		LoanID,
		SUM(Principal * InterestRate / PeriodLen) AS EarnedInterest
	FROM
		@daily
	GROUP BY
		LoanID

	RETURN
END
GO
