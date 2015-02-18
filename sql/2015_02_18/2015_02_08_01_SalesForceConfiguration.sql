DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL 
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SalesForceFakeMode')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SalesForceFakeMode', 'True', 'If True no acctual call to SF')
	END
	
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SalesForceUserName')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SalesForceUserName', 'yarons@ezbob.com.sandbox', 'SF login')
	END


	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SalesForcePassword')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SalesForcePassword', 'yaron123', 'SF password')
	END

	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SalesForceToken')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SalesForceToken', 'iaUmAG5GDkpXfpeqNEPi2rmt', 'Used for calls from non white list IPs')
	END
	
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SalesForceEnvironment')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SalesForceEnvironment', 'Sandbox', 'Sandbox or Prod')
	END
	
END

IF @Environment = 'QA' OR @Environment = 'UAT'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SalesForceFakeMode')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SalesForceFakeMode', 'True', 'if True no acctual call to SF')
	END
	
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SalesForceUserName')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SalesForceUserName', 'yarons@ezbob.com.sandbox', 'SF login')
	END


	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SalesForcePassword')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SalesForcePassword', 'yaron123', 'SF password')
	END

	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SalesForceToken')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SalesForceToken', 'iaUmAG5GDkpXfpeqNEPi2rmt', 'Used for calls from non white list IPs')
	END
	
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SalesForceEnvironment')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SalesForceEnvironment', 'Sandbox', 'Sandbox or Prod')
	END
	
END

IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SalesForceFakeMode')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SalesForceFakeMode', 'False', 'if True no acctual call to SF')
	END
	
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SalesForceUserName')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SalesForceUserName', '', 'SF login')
	END


	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SalesForcePassword')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SalesForcePassword', '', 'SF password')
	END

	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SalesForceToken')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SalesForceToken', '', 'Used for calls from non white list IPs')
	END
	
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='SalesForceEnvironment')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('SalesForceEnvironment', 'Prod', 'Sandbox or Prod')
	END
	
END
GO
