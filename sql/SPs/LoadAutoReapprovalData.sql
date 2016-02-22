IF OBJECT_ID('LoadAutoReapprovalData') IS NULL
	EXECUTE('CREATE PROCEDURE LoadAutoReapprovalData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadAutoReapprovalData
@CustomerID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @LacrID INT = ISNULL((
		SELECT
			MAX(c.Id)
		FROM
			CashRequests c
		WHERE
			c.IdCustomer = @CustomerID
			AND
			c.UnderwriterDecision = 'Approved'
			AND
			c.IdUnderwriter IS NOT NULL
			AND
			c.IdUnderwriter != 1
			AND
			c.UnderwriterDecisionDate < @Now
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
			AND
			c.UnderwriterDecisionDate < @Now
	), 0) END

	------------------------------------------------------------------------------

	DECLARE @LacrTime DATETIME
	DECLARE @EmailSendingBanned BIT
	DECLARE @ManagerApprovedSum INT
	DECLARE @OfferStart DATETIME
	DECLARE @OfferValidUntil DATETIME

	SELECT
		@LacrTime = UnderwriterDecisionDate,
		@EmailSendingBanned = EmailSendingBanned,
		@ManagerApprovedSum = ManagerApprovedSum,
		@OfferStart = OfferStart,
		@OfferValidUntil = OfferValidUntil
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
			@LacrTime < ls.[Date] AND ls.[Date] < @Now
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
			l.CustomerId = @CustomerID
			AND
			l.[Date] < @Now
			AND
			(l.DateClosed IS NULL OR l.DateClosed > @Now)
	), 0)

	------------------------------------------------------------------------------

	DECLARE @TakenLoanAmount DECIMAL(18, 0) = ISNULL((
		SELECT
			SUM(l.LoanAmount)
		FROM
			Loan l
			INNER JOIN CashRequests r ON l.RequestCashId = r.Id
		WHERE
			l.CustomerId = @CustomerID
			AND
			r.Id >= @LacrID
			AND
			l.[Date] < @Now
	), 0)

	------------------------------------------------------------------------------

	DECLARE @SumOfCharges DECIMAL(18, 4) = ISNULL((
		SELECT
			SUM(lc.Amount)
		FROM
			LoanCharges lc
			INNER JOIN Loan l ON lc.LoanId = l.id
			INNER JOIN ConfigurationVariables cv
				ON lc.ConfigurationVariableId = cv.Id
				AND cv.Name != 'SpreadSetupFeeCharge'
		WHERE
			l.Customerid = @CustomerId
			AND
			@LacrTime < lc.[Date]
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
			t.PostDate < @Now
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
			t.PostDate < @Now
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
		OfferValidUntil    = @OfferValidUntil,
		OfferStart         = @OfferStart
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
		@LacrTime < t.PostDate AND t.PostDate < @Now
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
		@LacrTime < m.Created AND m.Created < @Now
	ORDER BY
		m.Created

	------------------------------------------------------------------------------
END
GO
