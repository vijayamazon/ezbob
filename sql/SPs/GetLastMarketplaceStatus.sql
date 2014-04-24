IF OBJECT_ID('GetLastMarketplaceStatus') IS NULL
	EXECUTE('CREATE PROCEDURE GetLastMarketplaceStatus AS SELECT 1')
GO

ALTER PROCEDURE GetLastMarketplaceStatus
@CustomerId INT,
@MarketplaceId INT
AS
BEGIN
	SET NOCOUNT ON;

	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

	SELECT dbo.udfGetLastMarketplaceStatus(@CustomerId, @MarketplaceId) AS CurrentStatus
END
GO