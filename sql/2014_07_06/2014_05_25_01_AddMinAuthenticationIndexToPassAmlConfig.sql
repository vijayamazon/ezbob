IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='MinAuthenticationIndexToPassAml')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('MinAuthenticationIndexToPassAml', '70', 'If authentication index is lower than this then the AML status will be warning')
END
GO
