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

	-- Setting the isolation level to avoid deadlocks while 'waiting' in the main strategy
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	
	SELECT
		Id,
		dbo.udfGetLastMarketplaceStatus(Id) AS CurrentStatus
	FROM
		MP_CustomerMarketplace
	WHERE
		CustomerId = @CustomerId
END
GO
