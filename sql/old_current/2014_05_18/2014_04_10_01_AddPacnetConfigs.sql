DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL OR @Environment = 'QA' OR @Environment = 'UAT'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PacnetSERVICE_TYPE')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PacnetSERVICE_TYPE', 'Testing', 'Pacnet service type')
	END
END

IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PacnetSERVICE_TYPE')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PacnetSERVICE_TYPE', 'Production', 'Pacnet service type')
	END
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PacnetRAVEN_SECRET')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PacnetRAVEN_SECRET', 'oranges are a good source of vitamin c', 'Pacnet raven secret')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PacnetRAVEN_GATEWAY')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PacnetRAVEN_GATEWAY', 'https://raven.pacnetservices.com/realtime', 'Pacnet raven gateway')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PacnetRAVEN_PREFIX')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PacnetRAVEN_PREFIX', 'EzBob', 'Pacnet raven prefix')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PacnetRAVEN_USERNAME')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PacnetRAVEN_USERNAME', 'orangemoney.wf', 'Pacnet raven user name')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='PacnetRAVEN_RAPIVERSION')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('PacnetRAVEN_RAPIVERSION', '2', 'Pacnet raven rapi version')
END
GO
