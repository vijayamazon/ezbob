IF OBJECT_ID('LoadAutoRejectData') IS NULL
	EXECUTE('CREATE PROCEDURE LoadAutoRejectData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadAutoRejectData
@CustomerID INT,
@Now DATETIME = NULL
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------
	--
	-- Find last approval time.
	--
	------------------------------------------------------------------------------

	DECLARE @ApprovedCrID INT = ISNULL((
		SELECT
			MAX(cr.Id)
		FROM
			CashRequests cr
		WHERE
			cr.IdCustomer = @CustomerID
			AND
			cr.UnderwriterDecision = 'Approved'
			AND
			(@Now IS NULL OR cr.UnderwriterDecisionDate < @Now)
	), 0)

	------------------------------------------------------------------------------

	DECLARE
		@BrokerID INT,
		@FraudStatus INT,
		@CustomerStatusID INT,
		@CustomerStatus NVARCHAR(100),
		@CustomerStatusEnabled BIT,
		@IsLtd BIT,
		@CompanyRefNum NVARCHAR(50)

	------------------------------------------------------------------------------
	--
	-- Find customer status.
	--
	------------------------------------------------------------------------------

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
	------------------------------------------------------------------------------

	SELECT
		@BrokerID      = c.BrokerID,
		@FraudStatus   = c.FraudStatus,
		@IsLtd         = CASE WHEN c.TypeOfBusiness IN ('Limited', 'LLP') THEN 1 ELSE 0 END,
		@CompanyRefNum = co.ExperianRefNum
	FROM
		Customer c
		LEFT JOIN Company co ON c.CompanyId = co.Id
	WHERE
		c.Id = @CustomerID

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	DECLARE @CompanyScore INT = 0
	DECLARE @IncorporationDate DATETIME = NULL

	------------------------------------------------------------------------------

	EXECUTE GetCompanyScoreAndIncorporationDate
		@CustomerId,
		0,
		@CompanyScore OUTPUT,
		@IncorporationDate OUTPUT
	
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	DECLARE @ConsumerScore INT = ISNULL((
		SELECT
			MAX(ISNULL(ExperianConsumerScore, 0))
		FROM
			Director
		WHERE
			CustomerId = @CustomerID
			AND
			ExperianConsumerScore IS NOT NULL
	), 0)

	------------------------------------------------------------------------------

	DECLARE @CompanyFilesCount INT = ISNULL((
		SELECT
			COUNT(*)
		FROM
			MP_CompanyFilesMetaData f
		WHERE
			f.CustomerId = @CustomerID
	), 0)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	DECLARE @ServiceLogId BIGINT

	EXEC GetExperianConsumerServiceLog @CustomerID, @ServiceLogId OUTPUT

	------------------------------------------------------------------------------

	DECLARE @ConsumerDataTime DATETIME = (SELECT
		l.InsertDate
	FROM
		MP_ServiceLog l
	WHERE
		l.Id = @ServiceLogId)

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SELECT
		RowType               = 'MetaData',
		ApprovedCrID          = @ApprovedCrID,
		BrokerID              = ISNULL(@BrokerID, 0),
		FraudStatusID         = ISNULL(@FraudStatus, 3), -- under investigation
		CustomerStatusName    = ISNULL(@CustomerStatus, 'Unknown'),
		CustomerStatusEnabled = ISNULL(@CustomerStatusEnabled, 0),
		ConsumerScore         = @ConsumerScore,
		BusinessScore         = @CompanyScore,
		IncorporationDate     = @IncorporationDate,
		CompanyFilesCount     = @CompanyFilesCount,
		IsLtd                 = @IsLtd,
		CompanyRefNum         = ISNULL(@CompanyRefNum, ''),
		ConsumerDataTime      = @ConsumerDataTime

	------------------------------------------------------------------------------

	SELECT
		RowType = 'MpError',
		MpID = mp.Id,
		MpType = mt.Name,
		UpdateError = CASE WHEN mp.UpdatingEnd IS NULL THEN 'Marketplace is being updated now.' ELSE mp.UpdateError END
	FROM
		MP_CustomerMarketPlace mp
		INNER JOIN MP_MarketplaceType mt ON mp.MarketPlaceId = mt.Id
	WHERE
		mp.CustomerId = @CustomerID
		AND
		RTRIM(LTRIM(ISNULL(mp.UpdateError, ''))) != ''
		AND
		ISNULL(mp.Disabled, 0) = 0

	------------------------------------------------------------------------------

	EXECUTE LoadCustomerMarketplaceOriginationTimes @CustomerID

	------------------------------------------------------------------------------

	EXECUTE GetCustomerTurnoverData 0, @CustomerID, 1
	EXECUTE GetCustomerTurnoverData 0, @CustomerID, 3
	EXECUTE GetCustomerTurnoverData 0, @CustomerID, 6
	EXECUTE GetCustomerTurnoverData 0, @CustomerID, 12

	------------------------------------------------------------------------------
END
GO
