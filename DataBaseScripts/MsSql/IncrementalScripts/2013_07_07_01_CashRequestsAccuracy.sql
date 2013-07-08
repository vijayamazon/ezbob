DECLARE @DataType NVARCHAR(128)
SELECT @DataType = DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS IC WHERE TABLE_NAME = 'CashRequests' AND COLUMN_NAME = 'SystemCalculatedSum'
IF (@DataType = 'decimal')
BEGIN
	ALTER TABLE CashRequests 
	ALTER COLUMN SystemCalculatedSum INT
END

SELECT @DataType = DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS IC WHERE TABLE_NAME = 'CashRequests' AND COLUMN_NAME = 'ManagerApprovedSum'
IF (@DataType = 'decimal')
BEGIN
	ALTER TABLE CashRequests 
	ALTER COLUMN ManagerApprovedSum INT
END
GO
