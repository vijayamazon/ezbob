SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('RptRecords') IS NULL
	EXECUTE('CREATE PROCEDURE RptRecords AS SELECT 1')
GO

ALTER PROCEDURE RptRecords
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------
	--
	-- Create output table
	--
	------------------------------------------------------------------------------

	;WITH eight AS (
		SELECT CONVERT(INT, 1) AS Position
		UNION ALL
		SELECT
			Position + 1
		FROM
			eight
		WHERE
			Position < 8
	), res (OriginID, Origin, Position, BestDay, BestDayAmount, BestMonth, BestMonthAmount) AS (
		SELECT
			co.CustomerOriginID,
			co.Name,
			e.Position,
			CONVERT(DATE, NULL),
			CONVERT(DECIMAL(18, 2), NULL),
			CONVERT(DATE, NULL),
			CONVERT(DECIMAL(18, 2), NULL)
		FROM
			CustomerOrigin co
			INNER JOIN eight e ON 1 = 1
		UNION
		SELECT
			NULL,
			'Total',
			e.Position,
			CONVERT(DATE, NULL),
			CONVERT(DECIMAL(18, 2), NULL),
			CONVERT(DATE, NULL),
			CONVERT(DECIMAL(18, 2), NULL)
		FROM
			eight e
	)	
	SELECT
		*
	INTO
		#res
	FROM
		res

	------------------------------------------------------------------------------
	--
	-- Fill best day data per origin
	--
	------------------------------------------------------------------------------

	;WITH best_days AS (
		SELECT
			OriginID = c.OriginID,
			LoanIssueDay = CONVERT(DATE, l.[Date]),
			IssuedAmount = SUM(l.LoanAmount)
		FROM
			Loan l
			INNER JOIN CashRequests r ON l.RequestCashId = r.Id
			INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
		GROUP BY
			c.OriginID,
			CONVERT(DATE, l.[Date])
	), best_days_ranked AS (
		SELECT
			OriginID,
			LoanIssueDay,
			IssuedAmount,
			Position = ROW_NUMBER() OVER(PARTITION BY OriginID ORDER BY IssuedAmount DESC)
		FROM
			best_days
	)
	UPDATE #res SET
		BestDay = b.LoanIssueDay,
		BestDayAmount = b.IssuedAmount
	FROM
		#res r
		INNER JOIN best_days_ranked b
			ON r.OriginID = b.OriginID
			AND r.Position = b.Position

	------------------------------------------------------------------------------
	--
	-- Fill total best day data
	--
	------------------------------------------------------------------------------

	;WITH best_days AS (
		SELECT
			LoanIssueDay = CONVERT(DATE, l.[Date]),
			IssuedAmount = SUM(l.LoanAmount)
		FROM
			Loan l
			INNER JOIN CashRequests r ON l.RequestCashId = r.Id
			INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
		GROUP BY
			CONVERT(DATE, l.[Date])
	), best_days_ranked AS (
		SELECT
			LoanIssueDay,
			IssuedAmount,
			Position = ROW_NUMBER() OVER(ORDER BY IssuedAmount DESC)
		FROM
			best_days
	)
	UPDATE #res SET
		BestDay = b.LoanIssueDay,
		BestDayAmount = b.IssuedAmount
	FROM
		#res r
		INNER JOIN best_days_ranked b
			ON r.Position = b.Position
	WHERE
		r.OriginID IS NULL

	------------------------------------------------------------------------------
	--
	-- Fill best month data per origin
	--
	------------------------------------------------------------------------------

	;WITH best_months AS (
		SELECT
			OriginID = c.OriginID,
			LoanIssueMonth = dbo.udfMonthStart(l.[Date]),
			IssuedAmount = SUM(l.LoanAmount)
		FROM
			Loan l
			INNER JOIN CashRequests r ON l.RequestCashId = r.Id
			INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
		GROUP BY
			c.OriginID,
			dbo.udfMonthStart(l.[Date])
	), best_months_ranked AS (
		SELECT
			OriginID,
			LoanIssueMonth,
			IssuedAmount,
			Position = ROW_NUMBER() OVER(PARTITION BY OriginID ORDER BY IssuedAmount DESC)
		FROM
			best_months
	)
	UPDATE #res SET
		BestMonth = b.LoanIssueMonth,
		BestMonthAmount = b.IssuedAmount
	FROM
		#res r
		INNER JOIN best_months_ranked b
			ON r.OriginID = b.OriginID
			AND r.Position = b.Position

	------------------------------------------------------------------------------
	--
	-- Fill total best month data
	--
	------------------------------------------------------------------------------

	;WITH best_months AS (
		SELECT
			LoanIssueMonth = dbo.udfMonthStart(l.[Date]),
			IssuedAmount = SUM(l.LoanAmount)
		FROM
			Loan l
			INNER JOIN CashRequests r ON l.RequestCashId = r.Id
			INNER JOIN Customer c ON r.IdCustomer = c.Id AND c.IsTest = 0
		GROUP BY
			dbo.udfMonthStart(l.[Date])
	), best_months_ranked AS (
		SELECT
			LoanIssueMonth,
			IssuedAmount,
			Position = ROW_NUMBER() OVER(ORDER BY IssuedAmount DESC)
		FROM
			best_months
	)
	UPDATE #res SET
		BestMonth = b.LoanIssueMonth,
		BestMonthAmount = b.IssuedAmount
	FROM
		#res r
		INNER JOIN best_months_ranked b
			ON r.Position = b.Position
	WHERE
		r.OriginID IS NULL

	------------------------------------------------------------------------------
	--
	-- Output
	--
	------------------------------------------------------------------------------

	SELECT
		r.Origin,
		r.Position,
		DATENAME(month, r.BestDay) + ' ' + DATENAME(day, r.BestDay) + ' ' + DATENAME(year, r.BestDay) AS BestDay,
		r.BestDayAmount,
		DATENAME(month, r.BestMonth) + ' ' + DATENAME(year, r.BestMonth) AS BestMonth,
		r.BestMonthAmount
	FROM
		#res r
	WHERE
		r.BestDay IS NOT NULL
	ORDER BY
		(CASE WHEN r.OriginID IS NULL THEN 0 ELSE 1 END),
		r.Origin,
		r.Position

	------------------------------------------------------------------------------
	--
	-- Clean up
	--
	------------------------------------------------------------------------------

	DROP TABLE #res
END
GO
