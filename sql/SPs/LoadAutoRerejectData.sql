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

	DECLARE @LmrID INT = ISNULL((
		SELECT
			MAX(c.Id)
		FROM
			CashRequests c
			INNER JOIN DecisionHistory h ON c.Id = h.CashRequestId
		WHERE
			c.IdCustomer = @CustomerID
			AND
			c.UnderwriterDecision = 'Rejected'
			AND
			h.UnderwriterId != 1
			AND
			c.CreationDate < @Now
	), 0)

	------------------------------------------------------------------------------

	DECLARE @LmrTime DATETIME

	SELECT
		@LmrTime = c.UnderwriterDecisionDate
	FROM
		CashRequests c
	WHERE
		c.Id = @LmrID

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
		RowType         = 'MetaData',
		LmrID           = @LmrID,
		LmrTime         = @LmrTime,
		LoanCount       = @LoanCount,
		TakenLoanAmount = @TakenLoanAmount,
		RepaidPrincipal = @RepaidPrincipal,
		SetupFees       = @SetupFees

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
		@LmrTime < m.Created AND	m.Created < @Now
	ORDER BY
		m.Created

	------------------------------------------------------------------------------
END
GO
