IF OBJECT_ID('GetYodleeMps') IS NULL
	EXECUTE('CREATE PROCEDURE GetYodleeMps AS SELECT 1')
GO

ALTER PROCEDURE GetYodleeMps
	(@CustomerId INT)
AS
BEGIN
	SELECT
		MP_CustomerMarketPlace.Id
	FROM
		MP_CustomerMarketPlace,
		MP_MarketplaceType
	WHERE
		MP_MarketplaceType.Name = 'Yodlee' AND
		MP_MarketplaceType.Id = MP_CustomerMarketPlace.MarketPlaceId AND
		MP_CustomerMarketPlace.CustomerId = @CustomerId
END
GO
