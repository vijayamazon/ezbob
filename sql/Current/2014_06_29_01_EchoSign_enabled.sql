IF NOT EXISTS(SELECT * FROM ConfigurationVariables WHERE Name LIKE 'EchoSignEnabled%')
BEGIN
	INSERT INTO ConfigurationVariables (Name, Value, Description, IsEncrypted) VALUES
		('EchoSignEnabledUnderwriter', '0', 'Boolean. Is EchoSign enabled in underwriter.', 0),
		('EchoSignEnabledCustomer', '0', 'Boolean. Is EchoSign enabled in customer dashboard.', 0)
END
GO
