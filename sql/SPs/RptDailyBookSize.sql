SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('RptDailyBookSize') IS NULL
	EXECUTE('CREATE PROCEDURE RptDailyBookSize AS SELECT 1')
GO

ALTER PROCEDURE RptDailyBookSize
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	SELECT
		CONVERT(DATE, l.[Date]) AS TheDate,
		SUM(l.LoanAmount) AS Issued
	INTO
		#i
	FROM
		Loan l
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		l.[Date] > 'Sep 1 2012'
	GROUP BY
		CONVERT(DATE, l.[Date])

	------------------------------------------------------------------------------

	SELECT
		CONVERT(DATE, t.PostDate) AS TheDate,
		SUM(t.LoanRepayment) AS Repaid
	INTO
		#p
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
	WHERE
		l.[Date] > 'Sep 1 2012'
		AND
		t.Type = 'PaypointTransaction'
		AND
		t.Status = 'Done'
	GROUP BY
		CONVERT(DATE, t.PostDate)

	------------------------------------------------------------------------------

	SELECT
		dbo.udfMinDate(MIN(#i.TheDate), MIN(#p.TheDate)) AS MinDate,
		dbo.udfMaxDate(MAX(#i.TheDate), MAX(#p.TheDate)) AS MaxDate
	INTO
		#d
	FROM
		#i,
		#p

	;WITH d AS (
		SELECT
			#d.MinDate AS TheDate
		FROM
			#d
		UNION ALL
		SELECT
			DATEADD(day, 1, d.TheDate)
		FROM
			d
		WHERE
			d.TheDate < (SELECT MaxDate FROM #d)
	), daily_book_data AS (
		SELECT
			d.TheDate,
			Issued = ISNULL((SELECT SUM(Issued) FROM #i WHERE #i.TheDate <= d.TheDate), 0),
			Repaid = ISNULL((SELECT SUM(Repaid) FROM #p WHERE #p.TheDate <= d.TheDate), 0)
		FROM
			d
	)
	SELECT
		b.TheDate,
		b.Issued,
		b.Repaid,
		BookSize = b.Issued - b.Repaid
	FROM
		daily_book_data b
	ORDER BY
		b.TheDate
	OPTION
		(MAXRECURSION 0)

	------------------------------------------------------------------------------

	DROP TABLE #d
	DROP TABLE #p
	DROP TABLE #i
END
GO