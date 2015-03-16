IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AmazonAskvilleLogin')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AmazonAskvilleLogin', 'nimrodk@ezbob.com', 'Amazon askville login')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AmazonAskvillePassword')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AmazonAskvillePassword', '1qazxsw2', 'Amazon askville password')
END
GO

