IF OBJECT_ID('GetMarketplaceDetailsForUpdate') IS NULL
	EXECUTE('CREATE PROCEDURE GetMarketplaceDetailsForUpdate AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetMarketplaceDetailsForUpdate
@MarketplaceId INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		mt.Name,
		m.Disabled,
		m.DisplayName,
		FirstTime = CONVERT(BIT, (CASE
			WHEN EXISTS (SELECT Id FROM MP_CustomerMarketplaceUpdatingHistory WHERE CustomerMarketplaceId = @MarketplaceId)
				THEN 0
			ELSE
				1
		END))
	FROM 
		MP_MarketplaceType mt
		INNER JOIN MP_CustomerMarketPlace m
			ON mt.Id = m.MarketPlaceId
	WHERE 
		m.Id = @MarketplaceId
END
GO
