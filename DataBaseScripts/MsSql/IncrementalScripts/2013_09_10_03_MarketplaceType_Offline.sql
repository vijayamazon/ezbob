IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('MP_MarketplaceType') AND name = 'IsOffline')
	ALTER TABLE MP_MarketplaceType ADD IsOffline BIT NOT NULL CONSTRAINT DF_MarketplaceType_Offline DEFAULT (0)
GO

UPDATE MP_MarketplaceType SET IsOffline = 1 WHERE Name IN ('HMRC', 'Yodlee')
GO
