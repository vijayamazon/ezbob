IF EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = 'Customer' 
           AND  COLUMN_NAME = 'Disabled')
           BEGIN
	ALTER TABLE dbo.Customer DROP COLUMN Disabled
	END
	GO

ALTER TABLE dbo.Customer ADD
	Disabled int NOT NULL CONSTRAINT DF_Customer_Disabled DEFAULT 0
GO

