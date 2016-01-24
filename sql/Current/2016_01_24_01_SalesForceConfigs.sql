DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL 
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SalesForceConsumerKey')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SalesForceConsumerKey', '3MVG954MqIw6FnnPNMtQquUEWgFTeZVdS_G43_vBVQFTsidIuZJQgJ17SJv3PwyxSXgBWUjva9Zyq1pBALdmO', 'Used for rest api')
	END
	
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SalesForceConsumerSecret')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SalesForceConsumerSecret', '1496232326147934946', 'Used for rest api')
	END
	
END

IF @Environment = 'QA' OR @Environment = 'UAT'
BEGIN

	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SalesForceConsumerKey')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SalesForceConsumerKey', '', 'Used for rest api')
	END
	
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SalesForceConsumerSecret')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SalesForceConsumerSecret', '', 'Used for rest api')
	END
	
END

IF @Environment = 'Prod'
BEGIN

	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SalesForceConsumerKey')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SalesForceConsumerKey', '', 'Used for rest api')
	END
	
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SalesForceConsumerSecret')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SalesForceConsumerSecret', '', 'Used for rest api')
	END

END
GO
