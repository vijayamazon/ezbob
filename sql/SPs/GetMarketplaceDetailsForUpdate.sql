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
		END)),
		LongUpdateTime = CONVERT(BIT, CASE -- eBay, Amazon, Pay Pal
			WHEN mt.InternalId IN ('A7120CB7-4C93-459B-9901-0E95E7281B59','A4920125-411F-4BB9-A52D-27E8A00D0A3B','3FA5E327-FCFD-483B-BA5A-DC1815747A28')
				THEN 1
				ELSE 0
			END
		)
	FROM
		MP_MarketplaceType mt
		INNER JOIN MP_CustomerMarketPlace m ON mt.Id = m.MarketPlaceId
	WHERE
		m.Id = @MarketplaceId
END
GO
