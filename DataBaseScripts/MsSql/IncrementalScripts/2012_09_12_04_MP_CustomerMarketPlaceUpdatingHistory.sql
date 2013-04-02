ALTER TABLE dbo.MP_CustomerMarketPlaceUpdatingHistory ADD
	UpdatingTimePassInSeconds  AS (datediff(second,[UpdatingStart],[UpdatingEnd]))
