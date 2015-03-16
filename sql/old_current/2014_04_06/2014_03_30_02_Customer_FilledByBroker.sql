IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Customer') AND name = 'FilledByBroker')
	ALTER TABLE Customer ADD FilledByBroker BIT NOT NULL CONSTRAINT DF_Customer_FilledByBroker DEFAULT(0)
GO
