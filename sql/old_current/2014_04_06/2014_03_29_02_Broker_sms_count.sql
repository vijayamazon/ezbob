IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'BrokerMaxPerNumber')
	INSERT INTO ConfigurationVariables (Name, Value, Description)
		VALUES ('BrokerMaxPerNumber', '3', 'Max number of SMSs to a single phone number when registering as a broker/restoring broker password.')
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'BrokerMaxPerPage')
	INSERT INTO ConfigurationVariables (Name, Value, Description)
		VALUES ('BrokerMaxPerPage', '10', 'Max number of SMSs when registering as a broker/restoring broker password for single page load.')
GO
