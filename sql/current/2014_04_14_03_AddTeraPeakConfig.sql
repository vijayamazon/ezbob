IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='TeraPeakApiKey')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('TeraPeakApiKey', 'xdz8d8hw4cp5x9napc4s7tpq', 'TeraPeak api key')
END
GO
IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='TeraPeakUrl')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('TeraPeakUrl', 'http://api.terapeak.com/v1/research/xml/restricted', 'TeraPeak url')
END
GO
