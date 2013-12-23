IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'EzServiceConfiguration-dev')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description) VALUES (
		'EzServiceConfiguration-dev',
		'{ "SleepTimeout": 1000, "AdminPort": 7081, "ClientPort": 7082, "HostName": "localhost" }',
		'EzService configuration'
	)
END
GO

IF OBJECT_ID('EzServiceLoadConfiguration') IS NULL
BEGIN
	EXECUTE('CREATE PROCEDURE EzServiceLoadConfiguration AS SELECT 1')
END
GO

ALTER PROCEDURE EzServiceLoadConfiguration
@InstanceName NVARCHAR(32)
AS
	SELECT Value FROM ConfigurationVariables WHERE Name = 'EzServiceConfiguration-' + @InstanceName
GO
