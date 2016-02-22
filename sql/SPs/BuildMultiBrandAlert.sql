SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('BuildMultiBrandAlert') IS NULL
	EXECUTE('CREATE PROCEDURE BuildMultiBrandAlert AS SELECT 1')
GO

ALTER PROCEDURE BuildMultiBrandAlert
@CustomerID INT,
@Now DATETIME
AS
BEGIN
	SELECT DISTINCT
		c.Id,
		c.OriginID
	INTO
		#cst
	FROM
		Customer c
		INNER JOIN Customer cc ON c.Name = cc.Name
	WHERE
		cc.Id = @CustomerID

	------------------------------------------------------------------------------

	SELECT DISTINCT
		RowType = 'origin',
		Origin = o.Name
	FROM
		#cst
		INNER JOIN CustomerOrigin o ON #cst.OriginID = o.CustomerOriginID

	;WITH loans AS (
		SELECT
			LoanID = l.Id,
			IssueTime = l.[Date],
			l.LoanAmount,
			Term = ISNULL(l.CustomerSelectedTerm, ISNULL(r.RepaymentPeriod, r.ApprovedRepaymentPeriod)),
			Origin = o.Name
		FROM
			Loan l
			INNER JOIN CashRequests r ON l.RequestCashId = r.Id
			INNER JOIN Customer c ON r.IdCustomer = c.Id
			INNER JOIN CustomerOrigin o ON c.OriginID = o.CustomerOriginID
			INNER JOIN #cst ON c.Id = #cst.Id
		WHERE
			l.[Date] <= @Now
			AND
			(l.DateClosed IS NULL OR l.DateClosed > @Now)
	), repayments AS (
		SELECT
			LoanID = t.LoanId,
			RepaidAmount = SUM(t.LoanRepayment)
		FROM
			LoanTransaction t
			INNER JOIN loans l ON t.LoanId = l.LoanID
		WHERE
			t.Type = 'PaypointTransaction'
			AND
			t.Status = 'Done'
			AND
			t.PostDate <= @Now
		GROUP BY
			t.LoanId
	) SELECT
		RowType = 'loan',
		l.LoanID,
		l.IssueTime,
		l.LoanAmount,
		l.Term,
		l.Origin,
		RepaidAmount = ISNULL(r.RepaidAmount, 0)
	FROM
		loans l
		LEFT JOIN repayments r ON l.LoanID = r.LoanID
	WHERE
		l.LoanAmount > ISNULL(r.RepaidAmount, 0)
	ORDER BY
		l.IssueTime

	------------------------------------------------------------------------------

	DROP TABLE #cst
END
GO
