IF OBJECT_ID('LoadAllLoansMetaData') IS NULL
	EXECUTE('CREATE PROCEDURE LoadAllLoansMetaData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON;
GO

ALTER PROCEDURE LoadAllLoansMetaData
@Today DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	;WITH late_list AS (
		SELECT DISTINCT
			s.LoanID,
			DATEDIFF(day, s.Date, @Today) AS LateDays
		FROM
			LoanSchedule s
			INNER JOIN Loan l ON s.LoanId = l.Id
		WHERE
			(l.DateClosed IS NULL OR l.DateClosed >= @Today)
			AND
			s.[Date] < @Today
			AND
			s.Status NOT IN ('PaidOnTime', 'PaidEarly')
			AND (
				s.Status != 'Paid'
				OR (
					s.Status = 'Paid' AND EXISTS (
						SELECT 1
						FROM LoanScheduleTransaction lst
						INNER JOIN LoanTransaction t
							ON lst.TransactionID = t.Id
							AND t.Type = 'PaypointTransaction'
							AND t.Status = 'Done'
							AND t.PostDate >= @Today
						WHERE
							lst.ScheduleID = s.Id
					)
				)
			)
	), max_late AS (
		SELECT
			LoanID,
			MAX(LateDays) AS LateDays
		FROM
			late_list
		GROUP BY
			LoanID
	), loan_list AS (
		SELECT
			r.Id AS CashRequestID,
			l.CustomerID,
			l.Id AS LoanID,
			ls.LoanSourceName,
			l.[Date] AS LoanDate,
			l.DateClosed,
			l.LoanAmount,
			l.Status,
			ISNULL(SUM(t.LoanRepayment), 0) AS RepaidPrincipal
		FROM
			Loan l
			INNER JOIN CashRequests r ON l.RequestCashId = r.Id
			INNER JOIN LoanSource ls ON r.LoanSourceID = ls.LoanSourceID
			INNER JOIN Customer c ON l.CustomerId = c.Id AND c.IsTest = 0
			LEFT JOIN LoanTransaction t
				ON l.Id = t.LoanID
				AND t.Type = 'PaypointTransaction'
				AND t.Status = 'Done'
		WHERE
			l.[Date] >= 'September 4 2012'
			AND
			l.[Date] < @Today
		GROUP BY
			r.Id,
			l.CustomerID,
			l.Id,
			ls.LoanSourceName,
			l.[Date],
			l.DateClosed,
			l.LoanAmount,
			l.Status
	)
	SELECT
		ll.CashRequestID,
		ll.CustomerID,
		ll.LoanID,
		ll.LoanSourceName,
		ll.LoanDate,
		ll.DateClosed,
		ll.LoanAmount,
		ll.Status,
		ll.RepaidPrincipal,
		dbo.udfMaxInt(0, ml.LateDays) AS MaxLateDays
	FROM
		loan_list ll
		LEFT JOIN max_late ml
			ON ll.LoanID = ml.LoanID
END
GO
