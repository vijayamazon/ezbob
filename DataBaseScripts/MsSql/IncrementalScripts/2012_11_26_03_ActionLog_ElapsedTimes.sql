ALTER TABLE dbo.MP_CustomerMarketplaceUpdatingActionLog ADD
	ElapsedAggregateData bigint NULL,
	ElapsedRetrieveDataFromDatabase bigint NULL,
	ElapsedRetrieveDataFromExternalService bigint NULL,
	ElapsedStoreAggregatedData bigint NULL,
	ElapsedStoreDataToDatabase bigint NULL