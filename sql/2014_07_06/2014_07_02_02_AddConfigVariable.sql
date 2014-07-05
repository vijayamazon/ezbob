IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name='FinancialAccounts_Spare')
BEGIN
	INSERT INTO dbo.ConfigurationVariables	(	Name	, Value	, Description	, IsEncrypted	)
	VALUES	('FinancialAccounts_Spare', '1', 'if enabled this type of financial accounts will be shown in corresponding area on Credit Bureau tab ', NULL)
END 	
GO
