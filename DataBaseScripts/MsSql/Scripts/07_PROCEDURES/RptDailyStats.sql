IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptDailyStats]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptDailyStats]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptDailyStats
@DateStart DATETIME,
@DateEnd   DATETIME
AS
BEGIN
	SELECT
		@DateStart = CONVERT(DATE, @DateStart),
		@DateEnd = CONVERT(DATE, @DateEnd)

	IF OBJECT_ID('tempdb..#TodayClients') is not NULL
		DROP TABLE #TodayClients

	IF OBJECT_ID('tempdb..#PastClients') is not NULL
		DROP TABLE #PastClients

	IF OBJECT_ID('tempdb..#NewClients') is not NULL
		DROP TABLE #NewClients

	IF OBJECT_ID('tempdb..#NewOffers') is not NULL
		DROP TABLE #NewOffers

	IF OBJECT_ID('tempdb..#ReportPart1') is not NULL
		DROP TABLE #ReportPart1

	IF OBJECT_ID('tempdb..#ReportPart2') is not NULL
		DROP TABLE #ReportPart2

	SELECT DISTINCT
		IdCustomer
	INTO
		#TodayClients
	FROM
		CashRequests
	WHERE
		@DateStart <= CreationDate AND CreationDate < @DateEnd

	SELECT DISTINCT
		C.IdCustomer
	INTO
		#PastClients
	FROM
		CashRequests C
		INNER JOIN #TodayClients T ON C.IdCustomer = T.IdCustomer
	WHERE
		CreationDate < @DateStart

	SELECT
		t.IdCustomer
	INTO
		#NewClients
	FROM
		#TodayClients t
		LEFT JOIN #PastClients p ON t.IdCustomer = p.IdCustomer
	WHERE
		p.IdCustomer IS NULL

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

	SELECT
		'Applications' Line,
		'Total' Type,
		COUNT(1) Counter,
		UnderwriterDecision,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(ManagerApprovedSum))), 1), 2) Value
	INTO
		#ReportPart1
	FROM
		CashRequests
	WHERE
		@DateStart <= CreationDate AND CreationDate < @DateEnd
	GROUP BY
		UnderwriterDecision

	INSERT INTO #ReportPart1
	SELECT
		'Applications' Line,
		'New' Type,
		COUNT(1) Counter,
		UnderwriterDecision,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(ManagerApprovedSum))), 1), 2) Value
	FROM
		CashRequests
		INNER JOIN #NewOffers ON CashRequests.Id = #NewOffers.OfferId
	WHERE
		@DateStart <= CreationDate AND CreationDate < @DateEnd
	GROUP BY
		UnderwriterDecision

	INSERT INTO #ReportPart1
	SELECT
		'Applications' Line,
		'Old' Type,
		COUNT(1) Counter,
		UnderwriterDecision,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(ManagerApprovedSum))), 1), 2) Value
	FROM
		CashRequests
		LEFT JOIN #NewOffers ON CashRequests.Id = #NewOffers.OfferId
	WHERE
		@DateStart <= CreationDate AND CreationDate < @DateEnd
		AND
		#NewOffers.OfferId IS NULL
	GROUP BY
		UnderwriterDecision

	SELECT
		'Loans' Line,
		'Total' Type,
		COUNT(DISTINCT Id) Counter,
		'' AS UnderwriterDescision,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(LoanAmount))), 1), 2) Value
	INTO
		#ReportPart2
	FROM
		Loan
	WHERE
		@DateStart <= [Date] AND [Date] < @DateEnd

	INSERT INTO #ReportPart2
	SELECT
		'Loans' Line,
		'Old' Type,
		COUNT(DISTINCT l.Id) Counter,
		'' AS UnderwriterDescision,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(l.LoanAmount))), 1), 2) Value
	FROM
		Loan l
		INNER JOIN Loan old ON l.CustomerId = old.CustomerID AND old.[Date] < @DateStart
	WHERE
		@DateStart <= l.[Date] AND l.[Date] < @DateEnd

	INSERT INTO #ReportPart2
	SELECT
		'Loans' Line,
		'New' Type,
		COUNT(DISTINCT l.Id) Counter,
		'' AS UnderwriterDescision,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), SUM(l.LoanAmount))), 1), 2) Value
	FROM
		Loan l
		LEFT JOIN Loan old ON l.CustomerId = old.CustomerID AND old.[Date] < @DateStart
	WHERE
		old.Id IS NULL
		AND
		@DateStart <= l.[Date] AND l.[Date] < @DateEnd

	SELECT
		*
	FROM
		#ReportPart1

	UNION

	SELECT
		*
	FROM
		#ReportPart2

	UNION

	SELECT
		'Payments' Line,
		'' Type,
		COUNT(1) Counter,
		'' AS UnderwriterDescision,
		PARSENAME(CONVERT(VARCHAR, CONVERT(MONEY, CONVERT(DECIMAL(13, 0), sum(Amount))), 1), 2) Value
	FROM
		LoanTransaction
	WHERE
		@DateStart  <= PostDate AND PostDate < @DateEnd
		AND
		Type = 'PaypointTransaction'
		AND
		Description = 'payment from customer'

	UNION

	SELECT
		'Registers' Line,
		'' Type,
		COUNT(1) Counter,
		'' AS UnderwriterDescision,
		'0' Value
	FROM
		Customer
	WHERE
		@DateStart <= GreetingMailSentDate AND GreetingMailSentDate < @DateEnd

	DROP TABLE #TodayClients
	DROP TABLE #PastClients
	DROP TABLE #NewClients
	DROP TABLE #NewOffers
	DROP TABLE #ReportPart1
	DROP TABLE #ReportPart2
END
GO
