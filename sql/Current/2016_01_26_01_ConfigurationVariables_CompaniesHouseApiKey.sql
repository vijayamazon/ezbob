
IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='CompaniesHouseApiKey')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('CompaniesHouseApiKey', '40CdyyGYerf2q-cKZwgN0xVdlrwXNVhxwiMVxpG-', 'Companies house rest api authentication key')
END
	
GO
