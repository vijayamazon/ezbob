IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptFinancialStats]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptFinancialStats]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptFinancialStats
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	DECLARE @TotalGivenLoanValueOpen NUMERIC(18, 2)
	DECLARE @TotalRepaidPrincipalOpen NUMERIC(18, 2)
	
	DECLARE @TotalGivenLoanValueClose NUMERIC(18, 2)
	DECLARE @TotalRepaidPrincipalClose NUMERIC(18, 2)

	DECLARE @TotalBalanceSum NUMERIC(18, 2)

	DECLARE @InterestReceived NUMERIC(18, 2)

	DECLARE @Defaults NUMERIC(18, 2)
	DECLARE @SetupFee NUMERIC(18, 2)

	DECLARE @PACNET NVARCHAR(32)
	DECLARE @PAYPOINT NVARCHAR(32)
	DECLARE @DONE NVARCHAR(4)

	DECLARE @Indent NVARCHAR(64)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		@DateStart = CONVERT(DATE, @DateStart),
		@DateEnd = CONVERT(DATE, @DateEnd),
		@PACNET = 'PacnetTransaction',
		@PAYPOINT = 'PaypointTransaction',
		@DONE = 'Done',
		@Indent = '&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;'

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	CREATE TABLE #output (
		SortOrder NUMERIC(18, 6) NOT NULL,
		Caption NVARCHAR(128) NOT NULL,
		Value NUMERIC(18, 2) NULL
	)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		@TotalGivenLoanValueOpen = ISNULL( SUM(ISNULL(t.Amount, 0)), 0 )
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PACNET AND t.Status = @DONE
		AND
		t.PostDate < @DateStart

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		@TotalGivenLoanValueClose = ISNULL( SUM(ISNULL(t.Amount, 0)), 0 )
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PACNET AND t.Status = @DONE
		AND
		t.PostDate < @DateEnd

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		@TotalRepaidPrincipalOpen = ISNULL( SUM(ISNULL(t.LoanRepayment, 0)), 0 )
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PAYPOINT AND t.Status = @DONE
		AND
		t.PostDate < @DateStart

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		@TotalRepaidPrincipalClose = ISNULL( SUM(ISNULL(t.LoanRepayment, 0)), 0 )
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PAYPOINT AND t.Status = @DONE
		AND
		t.PostDate < @DateEnd

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SET @TotalBalanceSum = 
		@TotalGivenLoanValueOpen  - @TotalRepaidPrincipalOpen +
		@TotalGivenLoanValueClose - @TotalRepaidPrincipalClose
		
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		@InterestReceived = ISNULL(SUM(t.Interest), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PAYPOINT AND t.Status = @DONE
		AND
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		@Defaults = ISNULL(SUM(t.Amount), 0)
	FROM
		Loan l
		INNER JOIN LoanTransaction t
			ON l.Id = t.LoanId
			AND t.Status = @DONE
			AND t.Type = @PACNET
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0 AND c.CollectionStatus = 4
	WHERE
		@DateStart <= l.Date AND l.Date < @DateEnd

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		@SetupFee = ISNULL(SUM(l.SetupFee), 0)
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		@DateStart <= l.Date AND l.Date < @DateEnd

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	CREATE TABLE #known_tran_time_status (
		TransactionID INT,
		Status INT
	)

	------------------------------------------------------------------------------

	INSERT INTO #known_tran_time_status
	SELECT DISTINCT
		TransactionID,
		CASE StatusBefore
			WHEN 'Late' THEN 1
			WHEN 'Paid' THEN 1
			WHEN 'PaidEarly' THEN 2
			ELSE 3
		END
	FROM
		LoanScheduleTransaction
	UNION
	SELECT DISTINCT
		TransactionID,
		CASE StatusAfter
			WHEN 'Late' THEN 1
			WHEN 'Paid' THEN 1
			WHEN 'PaidEarly' THEN 2
			ELSE 3
		END
	FROM
		LoanScheduleTransaction

	------------------------------------------------------------------------------

	SELECT
		TransactionID,
		MIN(Status) AS Status
	INTO
		#grouped_tran_time_status
	FROM
		#known_tran_time_status
	GROUP BY
		TransactionID

	------------------------------------------------------------------------------

	SELECT
		t.Id AS TransactionID,
		ISNULL(g.Status, 3) AS Status
	INTO
		#tran_time_status
	FROM
		LoanTransaction t
		LEFT JOIN #grouped_tran_time_status g ON t.Id = g.TransactionID

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		1,
		'Opening Balance',
		@TotalGivenLoanValueOpen - @TotalRepaidPrincipalOpen

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		7,
		'Loans Issued #',
		ISNULL(COUNT(*), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PACNET AND t.Status = @DONE
		AND
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		2,
		'Loans Issued Value',
		ISNULL( SUM(ISNULL(t.Amount, 0)), 0 )
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PACNET AND t.Status = @DONE
		AND
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		3.1,
		@Indent + 'Principal Repaid Early',
		ISNULL( SUM(ISNULL(LoanRepayment, 0)), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		INNER JOIN #tran_time_status s ON t.Id = s.TransactionID AND s.Status = 2
	WHERE
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd
		AND
		t.Type = @PAYPOINT AND t.Status = @DONE

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		3.2,
		@Indent + 'Principal Repaid On Time',
		ISNULL( SUM(ISNULL(LoanRepayment, 0)), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		INNER JOIN #tran_time_status s ON t.Id = s.TransactionID AND s.Status = 3
	WHERE
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd
		AND
		t.Type = @PAYPOINT AND t.Status = @DONE

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		3.3,
		@Indent + 'Principal Repaid Late',
		ISNULL( SUM(ISNULL(LoanRepayment, 0)), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		INNER JOIN #tran_time_status s ON t.Id = s.TransactionID AND s.Status = 1
	WHERE
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd
		AND
		t.Type = @PAYPOINT AND t.Status = @DONE

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		3,
		'Principal Repaid',
		SUM(Value)
	FROM
		#output
	WHERE
		SortOrder IN (3.1, 3.2, 3.3)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		5,
		'Defaults',
		@Defaults

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		8,
		'Average Loan Amount',
		ISNULL(AVG(t.Amount), 0)
	FROM
		Loan l
		INNER JOIN LoanTransaction t
			ON l.Id = t.LoanId
			AND t.Status = 'Done'
			AND t.Type = 'PacnetTransaction'
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		@DateStart <= l.Date AND l.Date < @DateEnd

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		9,
		'Interest Received',
		@InterestReceived

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		10,
		'Yield %',
		CASE @TotalBalanceSum
			WHEN 0 THEN 0
			ELSE 100 * 2 * @InterestReceived / @TotalBalanceSum
		END

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		11,
		'Fees Paid',
		@SetupFee + ISNULL(SUM(t.Fees), 0)
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		t.Type = @PAYPOINT AND t.Status = @DONE
		AND
		@DateStart <= t.PostDate AND t.PostDate < @DateEnd

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	VALUES (11.1, 'Setup Fee', @SetupFee)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		Name,
		CONVERT(DECIMAL(18, 6), 0) AS Charge
	INTO
		#c
	FROM
		ConfigurationVariables
	WHERE
		1 = 0

	------------------------------------------------------------------------------

	INSERT INTO #c
	SELECT
		cv.Name,
		CONVERT(DECIMAL(18, 2),
			ISNULL(SUM(CASE
				WHEN ISNULL(AmountPaid, 0) > 0 THEN
					CASE WHEN ISNULL(AmountPaid, 0) < ISNULL(Amount, 0) THEN ISNULL(AmountPaid, 0) ELSE Amount END
				ELSE 0
			END), 0)
		)
	FROM
		LoanCharges ch
		INNER JOIN Loan l ON ch.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
		INNER JOIN ConfigurationVariables cv ON ch.ConfigurationVariableId = cv.Id
	WHERE
		@DateStart <= ch.Date AND ch.Date < @DateEnd
		AND
		cv.Name LIKE '%Charge'
	GROUP BY
		cv.Name
	
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		CONVERT(DECIMAL(18, 6), '12.' + cv.Value),
		@Indent + cv.Name,
		ISNULL(#c.Charge, 0)
	FROM
		ConfigurationVariables cv
		LEFT JOIN #c ON cv.Name = #c.Name
	WHERE
		cv.Name LIKE '%Charge'

	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		12,
		'Total charges',
		ISNULL(SUM(Charge), 0)
	FROM
		#c

	------------------------------------------------------------------------------

	DROP TABLE #c

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		6,
		'Closing Balance',
		@TotalGivenLoanValueClose - @TotalRepaidPrincipalClose - @Defaults

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	INSERT INTO #output
	SELECT
		4,
		'Closing Balance before Defaults',
		@TotalGivenLoanValueClose - @TotalRepaidPrincipalClose

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		SortOrder,
		Caption,
		Value
	FROM
		#output
	ORDER BY
		SortOrder

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	DROP TABLE #tran_time_status
	DROP TABLE #grouped_tran_time_status
	DROP TABLE #known_tran_time_status
	DROP TABLE #output
END
GO
