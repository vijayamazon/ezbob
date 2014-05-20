IF OBJECT_ID('TMP_CustomerMarketplaces') IS NULL
	CREATE TABLE TMP_CustomerMarketplaces (
		BatchID UNIQUEIDENTIFIER NOT NULL,
		MarketplaceID INT NOT NULL,
		BackupTime DATETIME NOT NULL,
		OldData VARBINARY(MAX) NOT NULL,
		NewData VARBINARY(MAX) NOT NULL
	)
GO
