IF OBJECT_ID('GetAllLastMarketplaceStatuses') IS NULL
	EXECUTE('CREATE PROCEDURE GetAllLastMarketplaceStatuses AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetAllLastMarketplaceStatuses
@CustomerId INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @CompanyFiles UNIQUEIDENTIFIER = '1C077670-6D6C-4CE9-BEBC-C1F9A9723908'

	-- Setting the isolation level to avoid deadlocks while 'waiting' in the main strategy
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

	DECLARE @CompanyFilesID INT = (SELECT Id FROM MP_MarketplaceType WHERE InternalId = @CompanyFiles)

	-- Join with table MP_MarketplaceType was not made intentionally.
	-- Without the join the SP uses only one table a time which reduces chance of a dead lock.

	SELECT
		Id,
		dbo.udfGetLastMarketplaceStatus(Id) AS CurrentStatus
	FROM
		MP_CustomerMarketplace
	WHERE
		CustomerId = @CustomerId
		AND
		MarketPlaceId != @CompanyFilesID
END
GO
