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
	--
	-- Select offer start time and length.
	--
	------------------------------------------------------------------------------

	DECLARE
		@OfferValidUntil DATETIME, 
		@OfferStart DATETIME,
		@ValidForHours INT,
		@PreviousManualApproveCount INT = 0

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
	--
	-- Select number of customer open loans.
	-- Calculate how much money customer took.
	--
	------------------------------------------------------------------------------

	DECLARE @TakenLoanAmount DECIMAL(18, 0)
	DECLARE @OpenLoanCount INT

	SELECT
		@OpenLoanCount = COUNT(*),
		@TakenLoanAmount = SUM(l.LoanAmount)
	FROM
		Loan l
	WHERE
		l.CustomerId = @CustomerID
		AND (
			(@Now IS NULL AND l.Status != 'PaidOff')
			OR
			(
				@Now IS NOT NULL
				AND
				l.[Date] < @Now
				AND
				(l.DateClosed IS NULL OR l.DateClosed > @Now)
			)
		)

	------------------------------------------------------------------------------
	--
	-- Select total number of loans that customer took.
	--
	------------------------------------------------------------------------------

	DECLARE @TotalLoanCount INT = ISNULL((
		SELECT
			COUNT(*)
		FROM
			Loan l
		WHERE
			l.CustomerId = @CustomerID
			AND
			(@Now IS NULL OR l.[Date] < @Now)
	), 0)

	------------------------------------------------------------------------------
	--
	-- Select principal repaid for open loans.
	--
	------------------------------------------------------------------------------

	DECLARE @RepaidPrincipal DECIMAL(18, 4) = ISNULL((
		SELECT
			SUM(t.LoanRepayment)
		FROM
			LoanTransaction t
			INNER JOIN Loan l ON t.LoanId = l.Id
		WHERE
			l.CustomerId = @CustomerID
			AND (
				(@Now IS NULL AND l.Status != 'PaidOff')
				OR
				(
					@Now IS NOT NULL
					AND
					l.[Date] < @Now
					AND
					(l.DateClosed IS NULL OR l.DateClosed > @Now)
				)
			)
			AND
			t.Status = 'Done'
			AND
			t.Type LIKE 'Paypoint%'
			AND
			(@Now IS NULL OR t.PostDate < @Now)
	), 0)

	------------------------------------------------------------------------------
	--
	-- Select setup fees for open loans.
	--
	------------------------------------------------------------------------------

	DECLARE @SetupFees DECIMAL(18, 2) = ISNULL((
		SELECT
			SUM(t.Fees)
		FROM
			LoanTransaction t
			INNER JOIN Loan l ON t.LoanId = l.Id
		WHERE
			l.CustomerId = @CustomerID
			AND (
				(@Now IS NULL AND l.Status != 'PaidOff')
				OR
				(
					@Now IS NOT NULL
					AND
					l.[Date] < @Now
					AND
					(l.DateClosed IS NULL OR l.DateClosed > @Now)
				)
			)
			AND
			t.Status = 'Done'
			AND
			t.Type LIKE 'Pacnet%'
			AND
			(@Now IS NULL OR t.PostDate < @Now)
	), 0)

	------------------------------------------------------------------------------

	DECLARE @MaxCrID INT = ISNULL((
		SELECT
			MAX(cr.Id)
		FROM
			CashRequests cr
		WHERE
			cr.IdCustomer = @CustomerID
			AND
			(@Now IS NULL OR cr.CreationDate < @Now)
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
	--
	-- Select number of loans auto approved today.
	--
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
			cr.AutoDecisionID = 1 -- Auto Approval
			AND
			CONVERT(DATE, cr.CreationDate) = @Today
	), 0)

	------------------------------------------------------------------------------
	--
	-- Select amount of loans auto approved today.
	--
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
	------------------------------------------------------------------------------

	DECLARE @CompanyScore INT = 0
	DECLARE @IncorporationDate DATETIME = NULL

	------------------------------------------------------------------------------

	EXECUTE GetCompanyHistoricalScoreAndIncorporationDate
		@CustomerId,
		1,
		@Now,
		@CompanyScore OUTPUT,
		@IncorporationDate OUTPUT
	
	------------------------------------------------------------------------------
	--
	-- Select number of rollovers.
	--
	------------------------------------------------------------------------------

	DECLARE @NumOfRollovers INT = ISNULL((
		SELECT
			COUNT(*)
		FROM
			PaymentRollover r
			INNER JOIN LoanSchedule s ON r.LoanScheduleId = s.Id
			INNER JOIN Loan l ON s.LoanId = l.Id
		WHERE
			l.CustomerId = @CustomerID
			AND
			(@Now IS NULL OR r.Created < @Now)
	), 0)

	------------------------------------------------------------------------------

	DECLARE @ServiceLogId BIGINT

	EXEC GetExperianConsumerServiceLog @CustomerID, @ServiceLogId OUTPUT, @Now

	------------------------------------------------------------------------------

	DECLARE @ExperianConsumerDataID BIGINT

	SELECT
		@ExperianConsumerDataID = e.Id
	FROM
		ExperianConsumerData e
	WHERE
		e.ServiceLogId = @ServiceLogId

	------------------------------------------------------------------------------

	DECLARE @ConsumerScore INT = ISNULL((
		SELECT
			MIN(x.ExperianConsumerScore)
		FROM	(
			SELECT ISNULL(d.BureauScore, 0) AS ExperianConsumerScore
			FROM ExperianConsumerData d
			INNER JOIN MP_ServiceLog l ON d.ServiceLogId = l.Id
			WHERE d.Id = @ExperianConsumerDataID
			AND l.InsertDate < @Now

			UNION

			SELECT ISNULL(d.MinScore, 0) AS ExperianConsumerScore
			FROM CustomerAnalyticsDirector d
			WHERE d.CustomerID = @CustomerID
			AND d.AnalyticsDate < @Now
		) x
	), 0)

	------------------------------------------------------------------------------
	--
	-- Find customer status.
	--
	------------------------------------------------------------------------------

	DECLARE
		@CustomerStatusID INT,
		@CustomerStatus NVARCHAR(100),
		@CustomerStatusEnabled BIT

	-- Step 1. Find last new status before the requested date.

	IF @Now IS NOT NULL
	BEGIN
		SELECT TOP 1
			@CustomerStatusID = h.NewStatus
		FROM
			CustomerStatusHistory h
		WHERE
			h.TimeStamp < @Now
			AND
			h.CustomerId = @CustomerID
		ORDER BY
			h.TimeStamp DESC
	END

	------------------------------------------------------------------------------

	-- Step 2. Find first old status before the requested date.

	IF @Now IS NOT NULL AND @CustomerStatusID IS NULL
	BEGIN
		SELECT TOP 1
			@CustomerStatusID = h.PreviousStatus
		FROM
			CustomerStatusHistory h
		WHERE
			h.TimeStamp >= @Now
			AND
			h.CustomerId = @CustomerID
		ORDER BY
			h.TimeStamp ASC
	END

	------------------------------------------------------------------------------

	-- Step 3. Take current status.

	IF @CustomerStatusID IS NULL
	BEGIN
		SELECT
			@CustomerStatusID = c.CollectionStatus
		FROM
			Customer c
		WHERE
			c.Id = @CustomerID
	END

	------------------------------------------------------------------------------

	SELECT
		@CustomerStatus        = s.Name,
		@CustomerStatusEnabled = s.IsEnabled
	FROM
		CustomerStatuses s
	WHERE
		s.Id = @CustomerStatusID

	------------------------------------------------------------------------------
	--
	-- Select customer fraud status.
	--
	------------------------------------------------------------------------------

	DECLARE @FraudStatus INT
	DECLARE @FraudRqID INT

	IF @Now IS NOT NULL
	BEGIN
		SELECT TOP 1
			@FraudRqID = r.Id
		FROM
			FraudRequest r
		WHERE
			r.CustomerId = @CustomerID
			AND
			r.CheckDate < @Now
		ORDER BY
			r.CheckDate DESC

		IF EXISTS (SELECT * FROM FraudDetection WHERE FraudRequestId = @FraudRqID)
			SET @FraudStatus = 2 -- Fraud suspect
	END

	------------------------------------------------------------------------------

	IF @FraudStatus IS NULL
	BEGIN
		SELECT
			@FraudStatus = c.FraudStatus
		FROM
			Customer c
		WHERE
			c.Id = @CustomerID
	END

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SET @PreviousManualApproveCount = ISNULL((
		SELECT
			COUNT(*)
		FROM
			CashRequests r
		WHERE
			r.IdUnderwriter IS NOT NULL
			AND
			r.UnderwriterDecision = 'Approved'
			AND
			r.IdUnderwriter IS NOT NULL
			AND
			r.IdUnderwriter != 1
			AND
			r.UnderwriterDecisionDate < @Now
			AND
			r.IdCustomer = @CustomerID
	), 0)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		RowType                    = 'MetaData',

		FirstName                  = c.FirstName,
		LastName                   = c.Surname,

		IsBrokerCustomer           = CONVERT(BIT, (CASE WHEN c.BrokerID IS NULL THEN 0 ELSE 1 END)),
		IsLimitedCompanyType       = CONVERT(BIT, (CASE WHEN c.TypeOfBusiness IN ('Limited', 'LLP') THEN 1 ELSE 0 END)),
		NumOfTodayAutoApproval     = @TodayAutoApprovalCount,
		TodayLoanSum               = @TodayLoanSum,

		FraudStatusValue           = @FraudStatus,
		AmlResult                  = c.AMLResult,
		CustomerStatusName         = @CustomerStatus,
		CustomerStatusEnabled      = @CustomerStatusEnabled,
		CompanyScore               = ISNULL(@CompanyScore, 0),
		ConsumerScore              = @ConsumerScore,
		IncorporationDate          = @IncorporationDate,
		DateOfBirth                = c.DateOfBirth,

		NumOfDefaultAccounts       = CONVERT(INT, 0), -- done in the code using below LoadExperianConsumerDataCais
		NumOfRollovers             = @NumOfRollovers,

		TotalLoanCount             = @TotalLoanCount,
		OpenLoanCount              = ISNULL(@OpenLoanCount, 0),
		TakenLoanAmount            = ISNULL(@TakenLoanAmount, 0),
		RepaidPrincipal            = @RepaidPrincipal,
		SetupFees                  = @SetupFees,

		OfferValidUntil            = @OfferValidUntil,
		OfferStart                 = @OfferStart,
		EmailSendingBanned         = @EmailSendingBanned,

		ExperianCompanyName        = co.ExperianCompanyName,
		EnteredCompanyName         = co.CompanyName,

		PreviousManualApproveCount = @PreviousManualApproveCount
	FROM
		Customer c
		LEFT JOIN Company co ON c.CompanyId = co.Id
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
		DATEDIFF(day, s.[Date], t.PostDate) > 1
		AND
		(@Now IS NULL OR t.PostDate < @Now)
	ORDER BY
		t.PostDate

	------------------------------------------------------------------------------

	SELECT DISTINCT
		RowType     = 'Cais',
		WorstStatus = ec.WorstStatus
	FROM
		ExperianConsumerDataCais ec
	WHERE
		ec.ExperianConsumerDataId = @ExperianConsumerDataID

	------------------------------------------------------------------------------

	EXECUTE LoadCustomerMarketplaceOriginationTimes @CustomerID, @Now
	
	------------------------------------------------------------------------------

	EXECUTE GetCustomerTurnoverForAutoDecision 1, @CustomerID, @Now

	------------------------------------------------------------------------------

	EXECUTE GetExperianDirectorsNamesForCustomer @CustomerID, @Now

	------------------------------------------------------------------------------

	EXECUTE LoadHmrcBusinessNames @CustomerID, @Now, 0

	------------------------------------------------------------------------------

	DECLARE @ExperianConsumerId BIGINT = dbo.udfLoadExperianConsumerIdForCustomerAndDate(@CustomerId, @Now)

	EXECUTE LoadExperianConsumerDataCais @ExperianConsumerId

	------------------------------------------------------------------------------

	EXECUTE LoadCompanyDissolutionDate @CustomerID, @Now

	------------------------------------------------------------------------------
END
GO
