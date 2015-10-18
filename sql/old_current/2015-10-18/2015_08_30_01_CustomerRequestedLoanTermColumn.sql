IF NOT EXISTS (SELECT * FROM syscolumns WHERE id=object_id('CustomerRequestedLoan') AND name = 'Term')
BEGIN
	ALTER TABLE CustomerRequestedLoan ADD Term INT
END
GO

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE WHERE CONSTRAINT_NAME='PK_CustomerRequestedLoan' AND COLUMN_NAME = 'CustomerId')
BEGIN 
	ALTER TABLE CustomerRequestedLoan DROP CONSTRAINT PK_CustomerRequestedLoan
	ALTER TABLE CustomerRequestedLoan ADD CONSTRAINT PK_CustomerRequestedLoan PRIMARY KEY (Id)
END	
GO
