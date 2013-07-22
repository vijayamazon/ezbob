IF COLUMNPROPERTY(OBJECT_ID('YodleeAccounts', 'U'), 'CustomerId', 'AllowsNull') = 0
BEGIN
	ALTER TABLE YodleeAccounts ALTER COLUMN CustomerId INT NULL
	ALTER TABLE YodleeAccounts ALTER COLUMN BankId INT NULL
END
GO
