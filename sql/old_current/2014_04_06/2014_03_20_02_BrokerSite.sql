IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'BrokerSite')
	INSERT INTO ConfigurationVariables (Name, Value, Description)
		VALUES ('BrokerSite', 'https://app.ezbob.com/Broker', 'Broker site full URL, i.e. including protocol, host, and port. Used in emails.')
GO
