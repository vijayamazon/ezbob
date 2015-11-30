SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('FindNewUnmatchedPayments') IS NULL
	EXECUTE('CREATE PROCEDURE FindNewUnmatchedPayments AS SELECT 1')
GO

ALTER PROCEDURE FindNewUnmatchedPayments
@DateStart DATETIME = NULL,
@DateEnd DATETIME = NULL,
@IncludeTestCustomers BIT = 0
AS
BEGIN
	SELECT
		PaymentID = t.Id,
		PaymentTimestamp = t.TimestampCounter,
		p.LastKnownTimestamp,
		PaymentTime = t.PostDate,
		Delta = t.Amount - t.LoanRepayment - t.Interest - t.Fees - t.Rollover,
		PaidAmount = t.Amount,
		PaidPrincipal = t.LoanRepayment,
		PaidInterest = t.Interest,
		PaidFees = t.Fees,
		PaidRollover = t.Rollover,
		LoanID = l.Id,
		LoanRefNum = l.RefNum,
		LoanIssueTime = l.[Date],
		LoanAmount = l.LoanAmount,
		LoanInterestRate = l.InterestRate,
		LoanTerm = l.CustomerSelectedTerm,
		CustomerID = c.Id,
		CustomerEmail = c.Name,
		CustomerName = c.FullName
	FROM
		LoanTransaction t
		INNER JOIN Loan l ON t.LoanId = l.Id
		INNER JOIN CashRequests r ON l.RequestCashId = r.Id
		INNER JOIN Customer c ON r.IdCustomer = c.Id
		LEFT JOIN ReportedUnmatchedPayments p ON t.Id = p.PaymentID
	WHERE (
			p.PaymentID IS NULL
			OR
			t.TimestampCounter != p.LastKnownTimestamp
		)
		AND
		t.Amount != t.LoanRepayment + t.Interest + t.Fees + t.Rollover
		AND (
			ISNULL(@IncludeTestCustomers, 0) = 1
			OR
			c.IsTest = 0
		)
		AND
		t.Status = 'Done'
		AND
		t.Type = 'PaypointTransaction'
	ORDER BY
		t.PostDate
END
GO
