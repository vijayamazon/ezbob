SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name LIKE 'CallCreditEnabled')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'CallCreditEnabled', '0', 'Boolean. CallCredit queries enabled or not.', 0
	)
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name LIKE 'CreditSafeEnabled')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES (
		'CreditSafeEnabled', '1', 'Boolean. CreditSafe queries enabled or not.', 0
	)
END
GO
