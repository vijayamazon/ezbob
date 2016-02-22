IF OBJECT_ID('LoadMarketplacesLastUpdateTime') IS NULL
	EXECUTE('CREATE PROCEDURE LoadMarketplacesLastUpdateTime AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadMarketplacesLastUpdateTime
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	-- Setting the isolation level to avoid deadlocks while 'waiting' in the main strategy
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

	SELECT
		MpID = m.Id,
		m.UpdatingStart,
		m.UpdatingEnd,
		mt.Name,
		LongUpdateTime = CONVERT(BIT, CASE -- eBay, Amazon, Pay Pal
			WHEN mt.InternalId IN ('A7120CB7-4C93-459B-9901-0E95E7281B59','A4920125-411F-4BB9-A52D-27E8A00D0A3B','3FA5E327-FCFD-483B-BA5A-DC1815747A28')
				THEN 1
				ELSE 0
			END
		)
	FROM
		MP_CustomerMarketplace m
		INNER JOIN MP_MarketplaceType mt ON m.MarketPlaceId = mt.Id
	WHERE
		m.CustomerId = @CustomerID
		AND
		m.Disabled = 0
END
GO
