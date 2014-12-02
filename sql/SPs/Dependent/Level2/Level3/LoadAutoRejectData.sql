IF OBJECT_ID('LoadAutoRejectData') IS NULL
	EXECUTE('CREATE PROCEDURE LoadAutoRejectData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadAutoRejectData
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

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
	), 0)

	------------------------------------------------------------------------------

	DECLARE
		@BrokerID INT,
		@FraudStatus INT,
		@CustomerStatus NVARCHAR(100),
		@CustomerStatusEnabled BIT,
		@IsLtd BIT,
		@CompanyRefNum NVARCHAR(50)

	------------------------------------------------------------------------------

	SELECT
		@BrokerID              = c.BrokerID,
		@FraudStatus           = c.FraudStatus,
		@CustomerStatus        = s.Name,
		@CustomerStatusEnabled = s.IsEnabled,
		@IsLtd                 = CASE WHEN c.TypeOfBusiness IN ('Limited', 'LLP') THEN 1 ELSE 0 END,
		@CompanyRefNum         = co.ExperianRefNum
	FROM
		Customer c
		INNER JOIN CustomerStatuses s ON c.CollectionStatus = s.Id
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
		CompanyRefNum         = ISNULL(@CompanyRefNum, '')

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
