IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'SalariesMultiplier' AND id = OBJECT_ID('MP_VatReturnSummary'))
	ALTER TABLE MP_VatReturnSummary ADD SalariesMultiplier DECIMAL(18, 6) NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'CustomerMarketplaceID' AND id = OBJECT_ID('MP_VatReturnSummary'))
	ALTER TABLE MP_VatReturnSummary ADD CustomerMarketplaceID INT NULL
GO
