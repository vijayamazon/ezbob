IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PostcodeAnywhereMaxBankAccountValidationAttempts')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PostcodeAnywhereMaxBankAccountValidationAttempts', '1', 'PostcodeAnywhere max bank account validation attempts')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PostcodeAnywhereKey')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PostcodeAnywhereKey', 'UW24-ZZ45-DF74-XP85', 'PostcodeAnywhere key')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PostcodeAnywhereEnabled')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PostcodeAnywhereEnabled', 'True', 'PostcodeAnywhere enabled')
END
GO
