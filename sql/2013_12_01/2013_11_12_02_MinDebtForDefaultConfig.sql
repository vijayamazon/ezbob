IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='MinDectForDefault')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('MinDectForDefault', 100, 'Determines the minimal amount of debt for us to apply default for a loan when setting default to customer or loan')
END
GO
