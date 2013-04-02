ALTER TABLE dbo.MP_CustomerMarketPlace ADD
	UpdatingTimePassInSeconds  AS (datediff(second,[UpdatingStart],[UpdatingEnd]))