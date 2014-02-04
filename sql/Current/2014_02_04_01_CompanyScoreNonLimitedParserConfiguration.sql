IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'CompanyScoreNonLimitedParserConfiguration')
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('CompanyScoreNonLimitedParserConfiguration', '[]', 'Experian parser configuration for non-limited company in Company Score tab in Underwriter')
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'DirectorInfoNonLimitedParserConfiguration')
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('DirectorInfoNonLimitedParserConfiguration', '[]', 'Experian parser configuration for extracting director information (non-limited company)')
GO

UPDATE ConfigurationVariables SET Description = 'Experian parser configuration for limited company in Company Score tab in Underwriter' WHERE Name = 'CompanyScoreParserConfiguration'
GO

UPDATE ConfigurationVariables SET Description = 'Experian parser configuration for extracting director information (limited company)' WHERE Name = 'DirectorInfoParserConfiguration'
GO
