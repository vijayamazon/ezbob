IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].RptStatsDaily') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].RptStatsDaily
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE RptStatsDaily
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
	SELECT
		@DateStart = CONVERT(DATE, @DateStart),
		@DateEnd = CONVERT(DATE, @DateEnd)

	------------------------------------------------------------------------------

	CREATE TABLE #output (
		SortOrder INT IDENTITY(1, 1) NOT NULL,
		Caption NVARCHAR(128),
		Counter INT,
		Decision NVARCHAR(128),
		Value DECIMAL(18, 2),
		Css NVARCHAR(128)
	)

	------------------------------------------------------------------------------

	SELECT DISTINCT
		CustomerId
	INTO
		#OldLoanCustomers
	FROM
		Loan
	WHERE
		[Date] < @DateStart
		
	------------------------------------------------------------------------------

	SELECT DISTINCT
		IdCustomer
	INTO
		#TodayClients
	FROM
		CashRequests
	WHERE
		@DateStart <= CreationDate AND CreationDate < @DateEnd

	------------------------------------------------------------------------------

	SELECT DISTINCT
		C.IdCustomer
	INTO
		#PastClients
	FROM
		CashRequests C
		INNER JOIN #TodayClients T ON C.IdCustomer = T.IdCustomer
	WHERE
		CreationDate < @DateStart

	------------------------------------------------------------------------------

	SELECT
		t.IdCustomer
	INTO
		#NewClients
	FROM
		#TodayClients t
		LEFT JOIN #PastClients p ON t.IdCustomer = p.IdCustomer
	WHERE
		p.IdCustomer IS NULL

	------------------------------------------------------------------------------

	SELECT
		C.IdCustomer,
		MIN(Id) OfferId
	INTO
		#NewOffers
	FROM
		CashRequests C
		INNER JOIN #NewClients N ON C.IdCustomer = N.IdCustomer
	GROUP BY
		c.IdCustomer

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Css) VALUES ('Applications', 'total')

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Counter, Decision, Value)
	SELECT
		'Total',
		COUNT(1),
		UnderwriterDecision,
		SUM(ManagerApprovedSum)
	FROM
		CashRequests
	WHERE
		@DateStart <= CreationDate AND CreationDate < @DateEnd
	GROUP BY
		UnderwriterDecision
	ORDER BY
		UnderwriterDecision

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Counter, Decision, Value)
	SELECT
		'New',
		COUNT(1),
		UnderwriterDecision,
		SUM(ManagerApprovedSum)
	FROM
		CashRequests
		INNER JOIN #NewOffers ON CashRequests.Id = #NewOffers.OfferId
	WHERE
		@DateStart <= CreationDate AND CreationDate < @DateEnd
	GROUP BY
		UnderwriterDecision
	ORDER BY
		UnderwriterDecision

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Counter, Decision, Value)
	SELECT
		'Old',
		COUNT(1),
		UnderwriterDecision,
		SUM(ManagerApprovedSum)
	FROM
		CashRequests
		LEFT JOIN #NewOffers ON CashRequests.Id = #NewOffers.OfferId
	WHERE
		@DateStart <= CreationDate AND CreationDate < @DateEnd
		AND
		#NewOffers.OfferId IS NULL
	GROUP BY
		UnderwriterDecision
	ORDER BY
		UnderwriterDecision

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Css) VALUES ('Loans', 'total')

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Counter, Value)
	SELECT
		'Total',
		COUNT(DISTINCT Id),
		SUM(LoanAmount)
	FROM
		Loan
	WHERE
		@DateStart <= [Date] AND [Date] < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Counter, Value)
	SELECT
		'Old',
		COUNT(DISTINCT l.Id),
		SUM(l.LoanAmount)
	FROM
		Loan l
		INNER JOIN #OldLoanCustomers old ON l.CustomerId = old.CustomerID
	WHERE
		@DateStart <= l.[Date] AND l.[Date] < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Counter, Value)
	SELECT
		'New',
		COUNT(DISTINCT l.Id),
		SUM(l.LoanAmount)
	FROM
		Loan l
		LEFT JOIN #OldLoanCustomers old ON l.CustomerId = old.CustomerID
	WHERE
		old.CustomerID IS NULL
		AND
		@DateStart <= l.[Date] AND l.[Date] < @DateEnd

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Css) VALUES ('Payments', 'total')

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Counter, Value)
	SELECT
		'Total',
		COUNT(1) Counter,
		SUM(Amount)
	FROM
		LoanTransaction
	WHERE
		@DateStart  <= PostDate AND PostDate < @DateEnd
		AND
		Type = 'PaypointTransaction'
		AND
		Description = 'payment from customer'

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Css) VALUES ('Registers', 'total')

	------------------------------------------------------------------------------

	INSERT INTO #output(Caption, Counter)
	SELECT
		'Total' Line,
		COUNT(1) Counter
	FROM
		Customer
	WHERE
		@DateStart <= GreetingMailSentDate AND GreetingMailSentDate < @DateEnd

	------------------------------------------------------------------------------

	SELECT
		Caption,
		Counter,
		Decision,
		Value,
		Css
	FROM
		#output
	ORDER BY
		SortOrder

	------------------------------------------------------------------------------

	DROP TABLE #TodayClients
	DROP TABLE #PastClients
	DROP TABLE #NewClients
	DROP TABLE #NewOffers
	DROP TABLE #OldLoanCustomers
	DROP TABLE #output
END
GO

