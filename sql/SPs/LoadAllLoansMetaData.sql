IF OBJECT_ID('LoadAllLoansMetaData') IS NULL
	EXECUTE('CREATE PROCEDURE LoadAllLoansMetaData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON;
GO

ALTER PROCEDURE LoadAllLoansMetaData
AS
BEGIN
	SET NOCOUNT ON;

	;WITH late_list AS (
		SELECT
			lst.LoanID,
			DATEDIFF(day, s.Date, t.PostDate) AS LateDays
		FROM
			LoanScheduleTransaction lst
			INNER JOIN LoanSchedule s ON lst.ScheduleID = s.Id
			INNER JOIN LoanTransaction t ON lst.TransactionID = t.Id
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
			l.Id AS LoanID,
			ls.LoanSourceName,
			l.[Date] AS LoanDate,
			l.LoanAmount,
			l.Status,
			SUM(t.LoanRepayment) AS RepaidPrincipal
		FROM
			Loan l
			INNER JOIN CashRequests r ON l.RequestCashId = r.Id
			INNER JOIN LoanSource ls ON r.LoanSourceID = ls.LoanSourceID
			LEFT JOIN LoanTransaction t
				ON l.Id = t.LoanID
				AND t.Type = 'PaypointTransaction'
				AND t.Status = 'Done'
		GROUP BY
			r.Id,
			l.Id,
			ls.LoanSourceName,
			l.[Date],
			l.LoanAmount,
			l.Status
	)
	SELECT
		ll.CashRequestID,
		ll.LoanID,
		ll.LoanSourceName,
		ll.LoanDate,
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
