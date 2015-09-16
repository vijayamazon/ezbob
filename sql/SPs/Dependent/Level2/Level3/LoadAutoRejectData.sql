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
	--
	-- Select customer fraud status.
	--
	------------------------------------------------------------------------------

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
	--
	-- Select broker id, company type and Experian refnum.
	-- There is no history these items.
	--
	------------------------------------------------------------------------------

	SELECT
		@BrokerID      = c.BrokerID,
		@IsLtd         = CASE WHEN c.TypeOfBusiness IN ('Limited', 'LLP') THEN 1 ELSE 0 END,
		@CompanyRefNum = co.ExperianRefNum
	FROM
		Customer c
		LEFT JOIN Company co ON c.CompanyId = co.Id
	WHERE
		c.Id = @CustomerID

	------------------------------------------------------------------------------
	--
	-- Select company score and incorporation date.
	--
	------------------------------------------------------------------------------

	DECLARE @CompanyScore INT = 0
	DECLARE @IncorporationDate DATETIME = NULL

	------------------------------------------------------------------------------

	EXECUTE GetCompanyHistoricalScoreAndIncorporationDate
		@CustomerId,
		0,
		@Now,
		@CompanyScore OUTPUT,
		@IncorporationDate OUTPUT
	
	------------------------------------------------------------------------------
	--
	-- Select "director part" of consumer score.
	--
	------------------------------------------------------------------------------

	DECLARE @ConsumerScore INT = ISNULL((
		SELECT
			MaxScore
		FROM
			dbo.udfGetCustomerScoreAnalytics(@CustomerID, @Now)
	), 0)

	------------------------------------------------------------------------------
	--
	-- Select number of files uploaded via Company Files marketplace.
	--
	------------------------------------------------------------------------------

	DECLARE @CompanyFilesCount INT = ISNULL((
		SELECT
			COUNT(*)
		FROM
			MP_CompanyFilesMetaData f
			INNER JOIN MP_CustomerMarketPlace m
				ON m.CustomerId = @CustomerID
				AND (@Now IS NULL OR m.Created < @Now)
				AND ISNULL(m.Disabled, 0) = 0
			INNER JOIN MP_MarketplaceType t
				ON m.MarketPlaceId = t.Id
				AND t.InternalId = '1C077670-6D6C-4CE9-BEBC-C1F9A9723908'
		WHERE
			f.CustomerId = @CustomerID
			AND
			(@Now IS NULL OR f.Created < @Now)
	), 0)

	------------------------------------------------------------------------------
	--
	-- Select time of last Experian consumer request.
	--
	------------------------------------------------------------------------------

	DECLARE @ConsumerServiceLogID BIGINT
	EXEC GetExperianConsumerServiceLog @CustomerID, @ConsumerServiceLogID OUTPUT, @Now

	------------------------------------------------------------------------------

	DECLARE @ConsumerDataTime DATETIME = (SELECT
		l.InsertDate
	FROM
		MP_ServiceLog l
	WHERE
		l.Id = @ConsumerServiceLogID)

	------------------------------------------------------------------------------
	--
	-- Select last company Experian check.
	--
	------------------------------------------------------------------------------

	DECLARE @CompanyServiceLogID BIGINT
	EXECUTE GetExperianCompanyServiceLog @CustomerID, @CompanyServiceLogID OUTPUT, @Now
	
	------------------------------------------------------------------------------
	--
	-- Output: meta data.
	--
	------------------------------------------------------------------------------

	SELECT
		RowType               = 'MetaData',
		ApprovedCrID          = @ApprovedCrID,
		BrokerID              = ISNULL(@BrokerID, 0),
		FraudStatusID         = ISNULL(@FraudStatus, 3), -- under investigation
		CustomerStatusName    = ISNULL(@CustomerStatus, 'Unknown'),
		CustomerStatusEnabled = ISNULL(@CustomerStatusEnabled, 0),
		ConsumerScore         = ISNULL(@ConsumerScore, 0),
		BusinessScore         = @CompanyScore,
		IncorporationDate     = @IncorporationDate,
		CompanyFilesCount     = @CompanyFilesCount,
		IsLtd                 = @IsLtd,
		CompanyRefNum         = ISNULL(@CompanyRefNum, ''),
		ConsumerDataTime      = @ConsumerDataTime,
		ConsumerServiceLogID  = @ConsumerServiceLogID,
		CompanyServiceLogID   = @CompanyServiceLogID

	------------------------------------------------------------------------------
	--
	-- Output: marketplace errors.
	--
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
		(
			RTRIM(LTRIM(ISNULL(mp.UpdateError, ''))) != ''
			OR
			(mp.UpdatingStart IS NOT NULL AND mp.UpdatingEnd IS NULL)
		)
		AND
		ISNULL(mp.Disabled, 0) = 0
		AND
		(@Now IS NULL OR mp.UpdatingEnd < @Now)

	------------------------------------------------------------------------------
	--
	-- Output: marketplace origination data.
	--
	------------------------------------------------------------------------------

	EXECUTE LoadCustomerMarketplaceOriginationTimes @CustomerID, @Now

	------------------------------------------------------------------------------
	--
	-- Output: marketplace turnover data.
	--
	------------------------------------------------------------------------------

	EXECUTE GetCustomerTurnoverForAutoDecision 0, @CustomerID, @Now

	------------------------------------------------------------------------------
	--
	-- This is the end...
	--
	------------------------------------------------------------------------------
END
GO
