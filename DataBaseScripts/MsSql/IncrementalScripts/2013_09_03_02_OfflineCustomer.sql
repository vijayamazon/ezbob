IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Customer') AND name = 'IsOffline')
	ALTER TABLE Customer ADD IsOffline BIT NOT NULL CONSTRAINT DF_Customer_IsOffline DEFAULT (0)
GO
