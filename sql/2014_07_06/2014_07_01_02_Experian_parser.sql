IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'DirectorDetailsParserConfiguration')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description, IsEncrypted) VALUES (
		'DirectorDetailsParserConfiguration', '[]', 'Experian parser configuration for extracting director details (limited company).', 0
	)
END
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'DirectorDetailsNonLimitedParserConfiguration')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description, IsEncrypted) VALUES (
		'DirectorDetailsNonLimitedParserConfiguration', '[]', 'Experian parser configuration for extracting director details (non-limited company).', 0
	)
END
GO
