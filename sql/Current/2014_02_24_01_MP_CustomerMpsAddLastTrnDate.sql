IF NOT EXISTS(SELECT * FROM syscolumns WHERE id = OBJECT_ID('MP_CustomerMarketPlace') AND name = 'LastTransactionDate')
	ALTER TABLE MP_CustomerMarketPlace ADD LastTransactionDate DATETIME