IF OBJECT_ID('LoadAutoApprovalData') IS NULL
	EXECUTE('CREATE PROCEDURE LoadAutoApprovalData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadAutoApprovalData
@CustomerID INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Today DATE = CONVERT(DATE, @Now)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	DECLARE
		@OfferValidUntil DATETIME, 
		@OfferStart DATETIME,
		@ValidForHours INT

	------------------------------------------------------------------------------
	
	SELECT 
		@OfferValidUntil = c.ValidFor, 
		@OfferStart = c.ApplyForLoan 
	FROM 
		Customer c
	WHERE 
		c.Id = @CustomerId

	------------------------------------------------------------------------------

	IF @OfferStart IS NULL
	BEGIN
		SET @OfferStart = @Now

		-------------------------------------------------------------------------

		SELECT
			@ValidForHours = CONVERT(INT, cv.Value)
		FROM
			ConfigurationVariables cv
		WHERE
			cv.Name = 'OfferValidForHours'

		-------------------------------------------------------------------------

		SET @OfferValidUntil = DATEADD(hour, @ValidForHours, @Now) 
	END	
	
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	DECLARE @OpenLoanCount INT = ISNULL((
		SELECT
			COUNT(*)
		FROM
			Loan l
		WHERE
			l.CustomerId = @CustomerID
			AND
			l.Status != 'PaidOff'
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
	), 0)

	------------------------------------------------------------------------------

	DECLARE @RepaidPrincipal DECIMAL(18, 4) = ISNULL((
		SELECT
			SUM(t.LoanRepayment)
		FROM
			LoanTransaction t
			INNER JOIN Loan l ON t.LoanId = l.Id
		WHERE
			l.CustomerId = @CustomerID
			AND
			l.Status != 'PaidOff'
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
		WHERE
			l.CustomerId = @CustomerID
			AND
			l.Status != 'PaidOff'
			AND
			t.Status = 'Done'
			AND
			t.Type LIKE 'Pacnet%'
	), 0)

	------------------------------------------------------------------------------

	DECLARE @MaxCrID INT = ISNULL((
		SELECT
			MAX(cr.Id)
		FROM
			CashRequests cr
		WHERE
			cr.IdCustomer = @CustomerID
	), 0)

	------------------------------------------------------------------------------

	DECLARE @EmailSendingBanned BIT = ISNULL((
		SELECT
			cr.EmailSendingBanned
		FROM
			CashRequests cr
		WHERE
			cr.Id = @MaxCrID
	), 1)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	DECLARE @TodayAutoApprovalCount INT = ISNULL((
		SELECT
			COUNT(DISTINCT cr.Id)
		FROM
			CashRequests cr
			INNER JOIN Customer c
				ON cr.IdCustomer = c.Id
				AND c.IsTest = 0
		WHERE
			cr.UnderwriterComment = 'Auto Approval'
			AND
			CONVERT(DATE, cr.CreationDate) = @Today
	), 0)

	------------------------------------------------------------------------------

	DECLARE @TodayLoanSum DECIMAL(18, 0) = ISNULL((
		SELECT
			SUM(l.LoanAmount)
		FROM
			Loan l
		WHERE
			CONVERT(DATE, l.[Date]) = @Today
	), 0)

	------------------------------------------------------------------------------

	DECLARE @CompanyScore INT = ISNULL((
		SELECT
			(CASE WHEN MaxScore IS NULL OR MaxScore < Score THEN MaxScore ELSE Score END)
		FROM
			CustomerAnalyticsCompany
		WHERE
			CustomerID = @CustomerID
			AND
			IsActive = 1
	), 0)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		RowType                = 'MetaData',

		IsBrokerCustomer       = (CASE WHEN c.BrokerID IS NULL THEN 0 ELSE 1 END),
		TodayAutoApprovalCount = @TodayAutoApprovalCount,
		TodayLoanSum           = @TodayLoanSum,

		AmlResult              = c.AMLResult,
		CustomerStatusName     = cs.Name,
		CustomerStatusEnabled  = cs.IsEnabled,
		CompanyScore           = @CompanyScore,
		DateOfBirth            = c.DateOfBirth,
		
		OpenLoanCount          = @OpenLoanCount,
		TakenLoanAmount        = @TakenLoanAmount,
		RepaidPrincipal        = @RepaidPrincipal,
		SetupFees              = @SetupFees,

		OfferValidUntil        = ValidFor,
		OfferStart             = ApplyForLoan,
		EmailSendingBanned     = @EmailSendingBanned
	FROM
		Customer c
		INNER JOIN CustomerStatuses cs ON c.CollectionStatus = cs.Id
	WHERE
		c.Id = @CustomerID

	------------------------------------------------------------------------------

	SELECT
		RowType         = 'Payment',
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
		t.PostDate > s.[Date]
	ORDER BY
		t.PostDate

	------------------------------------------------------------------------------
END
GO
