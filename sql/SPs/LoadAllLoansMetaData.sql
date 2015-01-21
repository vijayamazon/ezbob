IF OBJECT_ID('LoadAllLoansMetaData') IS NULL
	EXECUTE('CREATE PROCEDURE LoadAllLoansMetaData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON;
GO

ALTER PROCEDURE LoadAllLoansMetaData
AS
BEGIN
	SET NOCOUNT ON;

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
END
GO
