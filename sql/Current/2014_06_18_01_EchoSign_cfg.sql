IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'EchoSignUrl')
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted)
		VALUES ('EchoSignUrl', 'https://secure.echosign.com/services/EchoSignDocumentService18', 'EchoSign SOAP URL', 0)
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'EchoSignReminder')
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted)
		VALUES ('EchoSignReminder', 'none', 'EchoSign reminder to sign. Available values: daily or weekly (case insensitive). Any other value will be treated as "none".', 0)
GO

IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'EchoSignDeadline')
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted)
		VALUES ('EchoSignDeadline', '7', 'EchoSign signing deadline in days (integer). Can be empty (i.e. no deadline). Negative values are treated as "no deadline".', 0)
GO
