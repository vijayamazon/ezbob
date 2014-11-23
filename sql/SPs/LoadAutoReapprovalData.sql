IF OBJECT_ID('LoadAutoReapprovalData') IS NULL
	EXECUTE('CREATE PROCEDURE LoadAutoReapprovalData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadAutoReapprovalData
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @LacrID INT = ISNULL((
		SELECT
			MAX(c.Id)
		FROM
			CashRequests c
			INNER JOIN DecisionHistory h ON c.Id = h.CashRequestId
		WHERE
			c.IdCustomer = @CustomerID
			AND
			c.UnderwriterDecision = 'Approved'
			AND
			h.UnderwriterId != 1
	), 0)

	------------------------------------------------------------------------------

	DECLARE @RejectAfterLacrID INT = CASE WHEN @LacrID = 0 THEN 0 ELSE ISNULL((
		SELECT
			MAX(c.Id)
		FROM
			CashRequests c
		WHERE
			c.IdCustomer = @CustomerID
			AND
			c.UnderwriterDecision = 'Rejected'
			AND
			c.Id > @LacrID
	), 0) END

	------------------------------------------------------------------------------

	DECLARE @LacrTime DATETIME
	DECLARE @EmailSendingBanned BIT
	DECLARE @ManagerApprovedSum INT

	SELECT
		@LacrTime = UnderwriterDecisionDate,
		@EmailSendingBanned = EmailSendingBanned,
		@ManagerApprovedSum = ManagerApprovedSum
	FROM
		CashRequests
	WHERE
		Id = @LacrID

	------------------------------------------------------------------------------

	DECLARE @LateLoanCount INT = ISNULL((
		SELECT
			COUNT(*) 
		FROM
			Loan l
			LEFT JOIN LoanSchedule ls ON l.Id = ls.LoanId 
		WHERE
			l.CustomerId = @CustomerId 
			AND
			ls.[Date] > @LacrTime
			AND
			ls.Status IN ('Late', 'Paid')
	), 0)

	------------------------------------------------------------------------------

	DECLARE @OpenLoanCount INT = ISNULL((
		SELECT
			COUNT(*)
		FROM
			Loan l
		WHERE
			CustomerId = @CustomerID
			AND
			Status != 'PaidOff'
	), 0)

	------------------------------------------------------------------------------

	DECLARE @SumOfCharges DECIMAL(18, 4) = ISNULL((
		SELECT
			SUM(lc.Amount)
		FROM
			Loan l
			LEFT JOIN LoanCharges lc ON lc.LoanId = l.id
		WHERE
			l.Customerid = @CustomerId
			AND
			lc.[Date] > @LacrTime
	), 0)

	------------------------------------------------------------------------------

	DECLARE @TakenLoanAmount DECIMAL(18, 0) = ISNULL((
		SELECT
			SUM(l.LoanAmount)
		FROM
			Loan l
			INNER JOIN CashRequests cr ON l.RequestCashId = cr.Id
		WHERE
			cr.IdCustomer = @CustomerID
			AND
			cr.Id >= @LacrID
	), 0)

	------------------------------------------------------------------------------

	DECLARE @RepaidPrincipal DECIMAL(18, 4) = ISNULL((
		SELECT
			SUM(t.LoanRepayment)
		FROM
			LoanTransaction t
			INNER JOIN Loan l ON t.LoanId = l.Id
			INNER JOIN CashRequests cr ON l.RequestCashId = cr.Id
		WHERE
			cr.IdCustomer = @CustomerID
			AND
			cr.Id >= @LacrID
			AND
			t.Status = 'Done'
			AND
			t.Type LIKE 'Paypoint%'
	), 0)

	------------------------------------------------------------------------------

	DECLARE @SetupFees DECIMAL(18, 2) = ISNULL((
		SELECT
			SUM(t.Fees)
		FROM
			LoanTransaction t
			INNER JOIN Loan l ON t.LoanId = l.Id
			INNER JOIN CashRequests cr ON l.RequestCashId = cr.Id
		WHERE
			cr.IdCustomer = @CustomerID
			AND
			cr.Id >= @LacrID
			AND
			t.Status = 'Done'
			AND
			t.Type LIKE 'Pacnet%'
	), 0)

	------------------------------------------------------------------------------

	SELECT
		RowType            = 'MetaData',
		FraudStatusValue   = c.FraudStatus,
		LacrID             = @LacrID,
		RejectAfterLacrID  = @RejectAfterLacrID,
		LacrTime           = @LacrTime,
		LateLoanCount      = @LateLoanCount,
		OpenLoanCount      = @OpenLoanCount,
		SumOfCharges       = @SumOfCharges,
		ManagerApprovedSum = ISNULL(@ManagerApprovedSum, 0),
		TakenLoanAmount    = @TakenLoanAmount,
		RepaidPrincipal    = @RepaidPrincipal,
		SetupFees          = @SetupFees,
		EmailSendingBanned = @EmailSendingBanned,
		OfferValidUntil    = c.ValidFor,
		OfferStart         = c.ApplyForLoan
	FROM
		Customer c
	WHERE
		c.Id = @CustomerID

	------------------------------------------------------------------------------

	SELECT
		RowType         = 'LatePayment',
		LoanID          = l.Id,
		ScheduleID      = s.Id,
		ScheduleDate    = s.[Date],
		TransactionID   = t.Id,
		TransactionTime = t.PostDate
	FROM
		LoanScheduleTransaction lst
		INNER JOIN Loan l ON lst.LoanID = l.Id
		INNER JOIN LoanSchedule s ON lst.ScheduleID = s.Id
		INNER JOIN LoanTransaction t
			ON lst.TransactionID = t.Id
			AND t.Status = 'Done'
			AND t.Type LIKE 'Paypoint%'
	WHERE
		l.CustomerId = @CustomerID
		AND
		t.PostDate > @LacrTime
		AND
		t.PostDate > s.[Date]
	ORDER BY
		t.PostDate

	------------------------------------------------------------------------------

	SELECT
		RowType            = 'Marketplace',
		MarketplaceID      = m.Id,
		MarketplaceName    = m.DisplayName,
		MarketplaceType    = mt.Name,
		MarketplaceAddTime = m.Created
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType mt ON m.MarketPlaceId = mt.Id
	WHERE
		m.CustomerId = @CustomerID
		AND
		m.Created > @LacrTime
	ORDER BY
		m.Created

	------------------------------------------------------------------------------
END
GO
