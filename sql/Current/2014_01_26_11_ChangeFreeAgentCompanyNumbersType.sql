DECLARE @DataType NVARCHAR(128)
SELECT @DataType = DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS IC WHERE TABLE_NAME = 'MP_FreeAgentCompany' AND COLUMN_NAME = 'company_registration_number'
IF (@DataType = 'int')
BEGIN
	ALTER TABLE MP_FreeAgentCompany ALTER COLUMN company_registration_number NVARCHAR(20)
END
GO

DECLARE @DataType NVARCHAR(128)
SELECT @DataType = DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS IC WHERE TABLE_NAME = 'MP_FreeAgentCompany' AND COLUMN_NAME = 'sales_tax_registration_number'
IF (@DataType = 'int')
BEGIN
	ALTER TABLE MP_FreeAgentCompany ALTER COLUMN sales_tax_registration_number NVARCHAR(20)
END
GO
