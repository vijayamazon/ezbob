DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL
BEGIN
	UPDATE ConfigurationVariables SET Value = 'https://localhost:44300'	WHERE Name = 'CustomerSite'
	UPDATE ConfigurationVariables SET Value = 'https://localhost:44300/Broker' WHERE Name = 'BrokerSite'
	UPDATE ConfigurationVariables SET Value = 'localhost:44300' WHERE Name = 'UnderwriterSite'
END

IF @Environment = 'QA'
BEGIN
	UPDATE ConfigurationVariables SET Value = 'https://192.168.120.11'	WHERE Name = 'CustomerSite'
	UPDATE ConfigurationVariables SET Value = 'https://192.168.120.11/Broker' WHERE Name = 'BrokerSite'
	UPDATE ConfigurationVariables SET Value = '192.168.120.11' WHERE Name = 'UnderwriterSite'
END

IF @Environment = 'UAT'
BEGIN
	UPDATE ConfigurationVariables SET Value = 'https://192.168.120.15'	WHERE Name = 'CustomerSite'
	UPDATE ConfigurationVariables SET Value = 'https://192.168.120.15/Broker' WHERE Name = 'BrokerSite'
	UPDATE ConfigurationVariables SET Value = '192.168.120.16' WHERE Name = 'UnderwriterSite'
END
GO
