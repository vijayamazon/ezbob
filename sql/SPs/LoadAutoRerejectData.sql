IF OBJECT_ID('LoadAutoRerejectData') IS NULL
	EXECUTE('CREATE PROCEDURE LoadAutoRerejectData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadAutoRerejectData
@CustomerID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @LastDecisionWasReject BIT = 0
	DECLARE @LastDecisionDate DATETIME = NULL

	;WITH cid AS (
		SELECT TOP 1
			r.Id
		FROM
			CashRequests r
		WHERE
			r.IdCustomer = @CustomerID
			AND
			r.UnderwriterDecision IS NOT NULL
			AND
			r.UnderwriterDecision != 'WaitingForDecision'
			AND
			r.UnderwriterDecisionDate < @Now
		ORDER BY
			r.UnderwriterDecisionDate DESC
	), dec AS (
		SELECT
			ISNULL(r.UnderwriterDecision, '') UW,
			r.UnderwriterDecisionDate AS UwDate
		FROM
			CashRequests r
			INNER JOIN cid ON r.Id = cid.Id
	)
	SELECT
		@LastDecisionWasReject = CASE UW WHEN 'Rejected' THEN 1 ELSE 0 END,
		@LastDecisionDate = UwDate
	FROM
		dec

	------------------------------------------------------------------------------

	DECLARE @LastRejectDate DATETIME = NULL

	;WITH cid AS (
		SELECT TOP 1
			r.Id
		FROM
			CashRequests r
		WHERE
			r.IdCustomer = @CustomerID
			AND
			r.UnderwriterDecision = 'Rejected'
			AND
			ISNULL(r.AutoDecisionID, 5) != 7 -- auto re-reject
			AND
			r.UnderwriterDecisionDate < @Now
		ORDER BY
			r.UnderwriterDecisionDate DESC
	)
	SELECT
		@LastRejectDate = r.UnderwriterDecisionDate
	FROM
		CashRequests r
		INNER JOIN cid ON r.Id = cid.Id

	------------------------------------------------------------------------------

	DECLARE @NumOfOpenLoans INT = ISNULL((
		SELECT
			COUNT(*)
		FROM
			Loan l
		WHERE
			l.CustomerId = @CustomerID
			AND
			l.[Date] < @Now
			AND
			l.Status != 'PaidOff'
	), 0)

	------------------------------------------------------------------------------

	DECLARE @LoanCount INT = ISNULL((
		SELECT
			COUNT(*)
		FROM
			Loan l
		WHERE
			l.CustomerId = @CustomerID
			AND
			l.[Date] < @Now
	), 0)

	------------------------------------------------------------------------------

	DECLARE @TakenLoanAmount DECIMAL(18, 0) = ISNULL((
		SELECT
			SUM(l.LoanAmount)
		FROM
			Loan l
		WHERE
			l.CustomerId = @CustomerID
			AND
			l.Status != 'PaidOff'
			AND
			l.[Date] < @Now
	), 0)

	------------------------------------------------------------------------------

	DECLARE @RepaidPrincipal DECIMAL(18, 4) = ISNULL((
		SELECT
			SUM(ISNULL(t.LoanRepayment, 0))
		FROM
			LoanTransaction t
			INNER JOIN Loan l ON t.LoanId = l.Id
		WHERE
			l.CustomerId = @CustomerID
			AND
			l.Status != 'PaidOff'
			AND
			l.[Date] < @Now
			AND
			t.Status = 'Done'
			AND
			t.Type LIKE 'Paypoint%'
	), 0)

	------------------------------------------------------------------------------

	DECLARE @SetupFees DECIMAL(18, 2) = ISNULL((
		SELECT
			SUM(ISNULL(t.Fees, 0))
		FROM
			LoanTransaction t
			INNER JOIN Loan l ON t.LoanId = l.Id
		WHERE
			l.CustomerId = @CustomerID
			AND
			l.Status != 'PaidOff'
			AND
			l.[Date] < @Now
			AND
			t.Status = 'Done'
			AND
			t.Type LIKE 'Pacnet%'
	), 0)

	------------------------------------------------------------------------------

	SELECT
		RowType               = 'MetaData',
		LoanCount             = @LoanCount,
		TakenLoanAmount       = @TakenLoanAmount,
		RepaidPrincipal       = @RepaidPrincipal,
		SetupFees             = @SetupFees,
		LastDecisionWasReject = @LastDecisionWasReject,
		LastDecisionDate      = @LastDecisionDate,
		LastRejectDate        = @LastRejectDate,
		NumOfOpenLoans        = @NumOfOpenLoans

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
		@LastDecisionDate < m.Created AND m.Created < @Now
	ORDER BY
		m.Created

	------------------------------------------------------------------------------
END
GO
