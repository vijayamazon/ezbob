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

	DECLARE @TotalLoanCount INT = ISNULL((
		SELECT
			COUNT(*)
		FROM
			Loan l
		WHERE
			l.CustomerId = @CustomerID
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
	------------------------------------------------------------------------------

	DECLARE @CompanyScore INT = 0
	DECLARE @IncorporationDate DATETIME = NULL

	------------------------------------------------------------------------------

	DECLARE @TypeOfBusiness NVARCHAR(100) = ISNULL((
		SELECT
			c.TypeOfBusiness
		FROM
			Customer c
		WHERE
			c.Id = @CustomerID
	), '')

	------------------------------------------------------------------------------

	IF @TypeOfBusiness IN ('Limited', 'LLP')
	BEGIN
		SELECT
			@CompanyScore = (CASE
				WHEN a.MaxScore IS     NULL AND a.Score IS     NULL THEN 0
				WHEN a.MaxScore IS NOT NULL AND a.Score IS     NULL THEN a.MaxScore
				WHEN a.MaxScore IS     NULL AND a.Score IS NOT NULL THEN a.Score
				WHEN a.MaxScore < a.Score THEN a.MaxScore ELSE a.Score
			END),
			@IncorporationDate = a.IncorporationDate
		FROM
			CustomerAnalyticsCompany a
		WHERE
			a.CustomerID = @CustomerID
			AND
			a.IsActive = 1
	END
	ELSE BEGIN
		SELECT
			@CompanyScore = nl.CommercialDelphiScore,
			@IncorporationDate = nl.IncorporationDate
		FROM
			Customer c
			INNER JOIN Company co ON c.CompanyId = co.Id
			INNER JOIN ExperianNonLimitedResults nl ON co.ExperianRefNum = nl.RefNumber
		WHERE
			c.Id = @CustomerID
			AND
			nl.IsActive = 1
	END

	------------------------------------------------------------------------------
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
	), 0)

	------------------------------------------------------------------------------

	DECLARE @ConsumerScore INT = ISNULL((
		SELECT
			MIN(x.ExperianConsumerScore)
		FROM	(
			SELECT ExperianConsumerScore
			FROM Customer
			WHERE Id = @CustomerID
			AND ExperianConsumerScore IS NOT NULL
			
			UNION
			
			SELECT ExperianConsumerScore
			FROM Director
			WHERE CustomerId = @CustomerID
			AND ExperianConsumerScore IS NOT NULL
		) x
	), 0)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	DECLARE @ServiceLogId BIGINT

	EXEC GetExperianConsumerServiceLog @CustomerID, @ServiceLogId OUTPUT

	------------------------------------------------------------------------------

	DECLARE @ExperianConsumerDataID BIGINT

	SELECT
		@ExperianConsumerDataID = e.Id
	FROM
		ExperianConsumerData e
	WHERE
		e.ServiceLogId = @ServiceLogId

	------------------------------------------------------------------------------

	DECLARE @NumOfDefaultAccounts INT = ISNULL((
		SELECT
			COUNT(*)
		FROM
			ExperianConsumerDataCais c
		WHERE
			c.ExperianConsumerDataId = @ExperianConsumerDataID
			AND
			c.AccountStatus = 'F'
	), 0)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		RowType                = 'MetaData',

		IsBrokerCustomer       = CONVERT(BIT, (CASE WHEN c.BrokerID IS NULL THEN 0 ELSE 1 END)),
		NumOfTodayAutoApproval = @TodayAutoApprovalCount,
		TodayLoanSum           = @TodayLoanSum,

		FraudStatusValue       = c.FraudStatus,
		AmlResult              = c.AMLResult,
		CustomerStatusName     = cs.Name,
		CustomerStatusEnabled  = cs.IsEnabled,
		CompanyScore           = ISNULL(@CompanyScore, 0),
		ConsumerScore          = @ConsumerScore,
		IncorporationDate      = @IncorporationDate,
		DateOfBirth            = c.DateOfBirth,

		NumOfDefaultAccounts   = @NumOfDefaultAccounts,
		NumOfRollovers         = @NumOfRollovers,

		TotalLoanCount         = @TotalLoanCount,
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

	SELECT DISTINCT
		RowType     = 'Cais',
		WorstStatus = ec.WorstStatus
	FROM
		ExperianConsumerDataCais ec
	WHERE
		ec.ExperianConsumerDataId = @ExperianConsumerDataID

	------------------------------------------------------------------------------

	EXECUTE LoadCustomerMarketplaceOriginationTimes @CustomerID
	
	------------------------------------------------------------------------------

	EXECUTE GetCustomerTurnoverData @CustomerID, 1
	EXECUTE GetCustomerTurnoverData @CustomerID, 3
	EXECUTE GetCustomerTurnoverData @CustomerID, 12

	------------------------------------------------------------------------------
END
GO
