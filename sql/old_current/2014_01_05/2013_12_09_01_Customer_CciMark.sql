IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('Customer') AND name = 'CciMark')
	ALTER TABLE Customer ADD CciMark BIT NOT NULL CONSTRAINT DF_Customer_CciMark DEFAULT(0)
GO
