IF OBJECT_ID('LoadAllTheHmrcMarketplaces') IS NULL
	EXECUTE('CREATE PROCEDURE LoadAllTheHmrcMarketplaces AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON;
GO

ALTER PROCEDURE LoadAllTheHmrcMarketplaces
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		m.Id AS MarketplaceID
	FROM
		MP_CustomerMarketPlace m
		INNER JOIN MP_MarketplaceType mt
			ON m.MarketPlaceId = mt.Id
			AND mt.InternalId = 'AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA'
END
GO
