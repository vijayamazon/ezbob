IF OBJECT_ID('LoadMarketplaceSecurityData') IS NULL
	EXECUTE('CREATE PROCEDURE LoadMarketplaceSecurityData AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON;
GO

ALTER PROCEDURE LoadMarketplaceSecurityData
@MarketplaceType UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		m.Id AS ID,
		m.SecurityData
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType mt
			ON m.MarketPlaceId = mt.Id
			AND mt.InternalId = @MarketplaceType
END
GO
