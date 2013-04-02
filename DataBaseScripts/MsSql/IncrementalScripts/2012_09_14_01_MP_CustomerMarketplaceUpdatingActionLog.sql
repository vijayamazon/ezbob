ALTER TABLE dbo.MP_CustomerMarketplaceUpdatingActionLog ADD
	UpdatingTimePassInSeconds  AS (datediff(second,[UpdatingStart],[UpdatingEnd]))
