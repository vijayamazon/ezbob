ALTER TABLE dbo.Customer ADD
	Details nvarchar(MAX) NULL,
	ValidFor datetime NULL
GO

IF EXISTS( SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = 'Customer' 
           AND  COLUMN_NAME = 'Customer')
           BEGIN
	ALTER TABLE dbo.Customer Drop COLUMN Customer
	END
	GO