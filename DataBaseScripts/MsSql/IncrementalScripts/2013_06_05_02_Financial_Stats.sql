IF OBJECT_ID('RptFinancialStats') IS NOT NULL
	DROP PROCEDURE RptFinancialStats
GO

CREATE PROCEDURE RptFinancialStats
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	DECLARE @TotalGivenLoanValue NUMERIC(18, 2)
	DECLARE @TotalRepaidPrincipal NUMERIC(18, 2)

	SELECT
		@TotalGivenLoanValue = ISNULL(SUM(t.Amount), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = 'PacnetTransaction'
		AND
		t.Status = 'Done'

	SELECT
		@TotalRepaidPrincipal = ISNULL(SUM(t.LoanRepayment), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = 'PaypointTransaction'
		AND
		t.Status = 'Done'

	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------

	CREATE TABLE #output (
		Caption NVARCHAR(128),
		Value NUMERIC(18, 2),
		SortOrder INT IDENTITY NOT NULL
	)
	
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	
	INSERT INTO #output
	SELECT
		'Loans Issues #',
		ISNULL(COUNT(*), 0)
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		l.Date BETWEEN @DateStart AND @DateEnd
	
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	
	INSERT INTO #output
	SELECT
		'Loans Issued Value',
		ISNULL(SUM(l.LoanAmount), 0)
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		l.Date BETWEEN @DateStart AND @DateEnd
	
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	
	INSERT INTO #output
	SELECT
		'Principal Repaid',
		ISNULL(SUM(t.LoanRepayment), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.PostDate BETWEEN @DateStart AND @DateEnd
		AND
		t.Type = 'PaypointTransaction'
		AND
		t.Status = 'Done'
	
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	
	IF OBJECT_ID('LoanScheduleTransaction') IS NULL
		INSERT INTO #output
		SELECT
			'Principal Repaid Early',
			-1
	ELSE
		INSERT INTO #output
		SELECT
			'Principal Repaid Early',
			ISNULL(SUM(t.LoanRepayment), 0)
		FROM
			LoanTransaction t
			INNER JOIN Loan l ON t.LoanId = l.Id
			INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
			INNER JOIN LoanScheduleTransaction lst ON t.Id = lst.TransactionID AND lst.StatusAfter IN ('PaidEarly', 'StillToPay')
		WHERE
			t.PostDate BETWEEN @DateStart AND @DateEnd
			AND
			t.Type = 'PaypointTransaction'
			AND
			t.Status = 'Done'

	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	
	IF OBJECT_ID('LoanScheduleTransaction') IS NULL
		INSERT INTO #output
		SELECT
			'Principal Repaid On Time',
			-1
	ELSE
		INSERT INTO #output
		SELECT
			'Principal Repaid On Time',
			ISNULL(SUM(t.LoanRepayment), 0)
		FROM
			LoanTransaction t
			INNER JOIN Loan l ON t.LoanId = l.Id
			INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
			INNER JOIN LoanScheduleTransaction lst ON t.Id = lst.TransactionID AND lst.StatusAfter IN ('PaidOnTime')
		WHERE
			t.PostDate BETWEEN @DateStart AND @DateEnd
			AND
			t.Type = 'PaypointTransaction'
			AND
			t.Status = 'Done'
	
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	
	IF OBJECT_ID('LoanScheduleTransaction') IS NULL
		INSERT INTO #output
		SELECT
			'Principal Repaid Late',
			-1
	ELSE
		INSERT INTO #output
		SELECT
			'Principal Repaid Late',
			ISNULL(SUM(t.LoanRepayment), 0)
		FROM
			LoanTransaction t
			INNER JOIN Loan l ON t.LoanId = l.Id
			INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
			INNER JOIN LoanScheduleTransaction lst ON t.Id = lst.TransactionID AND lst.StatusAfter IN ('Paid', 'Late')
		WHERE
			t.PostDate BETWEEN @DateStart AND @DateEnd
			AND
			t.Type = 'PaypointTransaction'
			AND
			t.Status = 'Done'

	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	
	INSERT INTO #output
	SELECT
		'Defaults',
		ISNULL(SUM(l.LoanAmount), 0)
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0 AND c.Status = 'Default'
	WHERE
		l.Date BETWEEN @DateStart AND @DateEnd
	
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	
	INSERT INTO #output
	SELECT
		'Average Loan Amount',
		ISNULL(AVG(l.LoanAmount), 0)
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		l.Date BETWEEN @DateStart AND @DateEnd
	
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	
	INSERT INTO #output
	SELECT
		'Yield Interest received / Open balance',
		CASE @TotalGivenLoanValue
			WHEN 0 THEN 0
			ELSE ISNULL(SUM(t.Interest), 0) / @TotalGivenLoanValue
		END
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = 'PaypointTransaction'
		AND
		t.Status = 'Done'
	
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	
	INSERT INTO #output
	SELECT
		'Total money out',
		@TotalGivenLoanValue - @TotalRepaidPrincipal
	
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	
	INSERT INTO #output
	SELECT
		'Interest Paid',
		ISNULL(SUM(t.Interest), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.PostDate BETWEEN @DateStart AND @DateEnd
		AND
		t.Type = 'PaypointTransaction'
		AND
		t.Status = 'Done'

	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	
	INSERT INTO #output
	SELECT
		'Fees Paid',
		ISNULL(SUM(t.Fees), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.PostDate BETWEEN @DateStart AND @DateEnd
		AND
		t.Type = 'PaypointTransaction'
		AND
		t.Status = 'Done'
	
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	
	INSERT INTO #output
	SELECT
		'Setup Fee',
		ISNULL(SUM(l.SetupFee), 0)
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		l.Date BETWEEN @DateStart AND @DateEnd

	INSERT INTO #output
	SELECT
		cv.Name,
		ISNULL(SUM(CASE 
			WHEN AmountPaid > 0 THEN
				CASE WHEN AmountPaid < Amount THEN AmountPaid ELSE Amount END
			ELSE 0
		END), 0)
	FROM
		LoanCharges ch
		INNER JOIN Loan l ON ch.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		INNER JOIN ConfigurationVariables cv ON ch.ConfigurationVariableId = cv.Id
	WHERE
		ch.Date BETWEEN @DateStart AND @DateEnd
	GROUP BY
		cv.Name
	ORDER BY
		cv.Name
	
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	-------------------------------------------------------------------------------
	
	SELECT
		SortOrder AS ID,
		Caption,
		Value
	FROM
		#output
	ORDER BY
		SortOrder
	
	DROP TABLE #output
END
GO

IF NOT EXISTS (SELECT * FROM ReportScheduler WHERE Type = 'RPT_FINANCIAL_STATS')
	INSERT INTO ReportScheduler VALUES('RPT_FINANCIAL_STATS', 'Financial Stats', 'RptFinancialStats', 0, 0, 0, 'Id,Position,Value', 'ID,Caption,Value', 'nimrodk@ezbob.com,alexbo+rpt@ezbob.com', 1)
ELSE
	UPDATE ReportScheduler SET
		Title = 'Financial Stats',
		StoredProcedure = 'RptFinancialStats',
		IsDaily = 0,
		IsWeekly = 0,
		IsMonthly = 0,
		Header = 'Id,Position,Value',
		Fields = 'ID,Caption,Value',
		ToEmail = 'nimrodk@ezbob.com,alexbo+rpt@ezbob.com',
		IsMonthToDate = 1
	WHERE
		Type = 'RPT_FINANCIAL_STATS'
GO
